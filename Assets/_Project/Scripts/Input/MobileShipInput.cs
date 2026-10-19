using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VARG.Input
{
    public class MobileShipInput : IShipInput, IDisposable
    {
        private const float StandardGravity = 9.80665f;
        private const float ConnectionTimeoutSeconds = 1.5f;
        private const int AllThreeAxesMask = 0b111;

        private readonly KeyboardShipInput _keyboardFallback;
        private readonly ShakeDetector _shakeDetector;
        private readonly object _sensorLock = new object();

        private readonly int _udpPort;
        private readonly float _deadzoneDegrees;
        private readonly float _maxTiltDegrees;
        private readonly float _steerSmoothing;
        private readonly Vector2 _axisSensitivity;

        private Socket _udpSocket;
        private Thread _receiveThread;
        private volatile bool _isRunning;

        private Vector3 _oscAccelRaw = new Vector3(0f, 0f, -StandardGravity);
        private Vector3 _oscGyroRaw;
        private int _accelAxesMask;
        private int _gyroAxesMask;

        private Vector3 _latestAccelG = new Vector3(0f, 0f, -1f);
        private Vector3 _latestGyroRad;
        private bool _hasAccel;
        private bool _hasGyro;
        private bool _hasNewSample;

        private float _filteredPitchDeg;
        private float _filteredRollDeg;
        private float _neutralPitchDeg;
        private float _neutralRollDeg;
        private bool _hasCalibratedNeutral;
        private float _lastPacketTime = -100f;
        private float _lastSampleTime = -1f;

        private Vector2 _smoothedSteer;
        private bool _descendTriggered;
        private bool _calibratePressed;

        public Vector2 Steer { get; private set; }
        public bool DescendPressed => _descendTriggered || _keyboardFallback.DescendPressed;
        public bool CalibratePressed => _calibratePressed;
        public bool IsMobileConnected => (Time.realtimeSinceStartup - _lastPacketTime) <= ConnectionTimeoutSeconds;

        public MobileShipInput(
            int udpPort = 5555,
            float deadzone = 0.08f,
            float maxTiltDegrees = 25f,
            float steerSmoothing = 14f,
            float shakeThresholdG = 1.75f,
            int shakeRequiredPeaks = 2,
            float shakeWindowSeconds = 0.6f,
            Vector2? axisSensitivity = null)
        {
            _udpPort = udpPort;
            _maxTiltDegrees = Mathf.Clamp(maxTiltDegrees, 10f, 65f);
            _deadzoneDegrees = Mathf.Clamp(deadzone, 0.02f, 0.4f) * _maxTiltDegrees;
            _steerSmoothing = Mathf.Max(1f, steerSmoothing);
            _axisSensitivity = axisSensitivity ?? Vector2.one;

            _keyboardFallback = new KeyboardShipInput();
            _shakeDetector = new ShakeDetector(shakeThresholdG, shakeRequiredPeaks, shakeWindowSeconds);

            StartUdpReceiver();
        }

        public void Tick(float deltaTime)
        {
            _descendTriggered = false;
            _calibratePressed = false;

            if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            {
                _calibratePressed = true;
                CalibrateNeutralPose();
            }

            bool hasSample;
            Vector3 accelG;
            Vector3 gyroRad;
            bool hasAccel;
            bool hasGyro;

            lock (_sensorLock)
            {
                hasSample = _hasNewSample;
                accelG = _latestAccelG;
                gyroRad = _latestGyroRad;
                hasAccel = _hasAccel;
                hasGyro = _hasGyro;
                _hasNewSample = false;
            }

            if (hasSample)
            {
                var now = Time.realtimeSinceStartup;
                var sampleDt = _lastSampleTime > 0f ? Mathf.Clamp(now - _lastSampleTime, 0.002f, 0.1f) : deltaTime;
                _lastSampleTime = now;
                _lastPacketTime = now;

                UpdatePitchAndRoll(accelG, gyroRad, hasAccel, hasGyro, sampleDt);

                if (hasAccel && _shakeDetector.ProcessSample(accelG, Time.unscaledTime))
                {
                    _descendTriggered = true;
                }
            }

            var keyboardSteer = _keyboardFallback.Steer;
            if (!IsMobileConnected || keyboardSteer.sqrMagnitude > 0.01f)
            {
                _smoothedSteer = Vector2.Lerp(_smoothedSteer, keyboardSteer, _steerSmoothing * deltaTime);
                Steer = keyboardSteer.sqrMagnitude > 0.01f ? keyboardSteer : _smoothedSteer;
                return;
            }

            var targetSteer = ComputeSteerFromPitchAndRoll();
            _smoothedSteer = Vector2.Lerp(_smoothedSteer, targetSteer, _steerSmoothing * deltaTime);
            Steer = _smoothedSteer;
        }

        public void CalibrateNeutralPose()
        {
            _neutralPitchDeg = _filteredPitchDeg;
            _neutralRollDeg = _filteredRollDeg;
            _hasCalibratedNeutral = true;
        }

        public void Dispose()
        {
            _isRunning = false;

            if (_udpSocket != null)
            {
                try { _udpSocket.Close(); } catch { }
                _udpSocket = null;
            }

            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                _receiveThread.Join(150);
                _receiveThread = null;
            }
        }

        private void UpdatePitchAndRoll(Vector3 accelG, Vector3 gyroRad, bool hasAccel, bool hasGyro, float dt)
        {
            if (!hasAccel || accelG.sqrMagnitude < 0.04f) return;

            var norm = accelG.normalized;
            var zAbs = Mathf.Max(0.15f, Mathf.Abs(norm.z));

            // MultiSense OSC uses Android's landscape sensor frame, so X maps to Portrait Pitch and -Y to Portrait Roll.
            var accelPitchDeg = Mathf.Atan2(norm.x, Mathf.Sqrt((norm.y * norm.y) + (zAbs * zAbs))) * Mathf.Rad2Deg;
            var accelRollDeg = Mathf.Atan2(-norm.y, Mathf.Sqrt((norm.x * norm.x) + (zAbs * zAbs))) * Mathf.Rad2Deg;

            if (!_hasCalibratedNeutral)
            {
                _filteredPitchDeg = accelPitchDeg;
                _filteredRollDeg = accelRollDeg;
                CalibrateNeutralPose();
                return;
            }

            if (hasGyro)
            {
                var predictedPitch = _filteredPitchDeg + (gyroRad.y * Mathf.Rad2Deg * dt);
                var predictedRoll = _filteredRollDeg + (gyroRad.x * Mathf.Rad2Deg * dt);

                var isNearOneG = accelG.sqrMagnitude > 0.65f && accelG.sqrMagnitude < 1.45f;
                var accelWeight = isNearOneG ? 0.22f : 0.05f;

                _filteredPitchDeg = Mathf.LerpAngle(predictedPitch, accelPitchDeg, accelWeight);
                _filteredRollDeg = Mathf.LerpAngle(predictedRoll, accelRollDeg, accelWeight);
            }
            else
            {
                var smooth = Mathf.Clamp01(dt * 14f);
                _filteredPitchDeg = Mathf.LerpAngle(_filteredPitchDeg, accelPitchDeg, smooth);
                _filteredRollDeg = Mathf.LerpAngle(_filteredRollDeg, accelRollDeg, smooth);
            }
        }

        private Vector2 ComputeSteerFromPitchAndRoll()
        {
            if (!_hasCalibratedNeutral) return Vector2.zero;

            var deltaRoll = Mathf.DeltaAngle(_neutralRollDeg, _filteredRollDeg) * _axisSensitivity.x;
            var deltaPitch = Mathf.DeltaAngle(_neutralPitchDeg, _filteredPitchDeg) * _axisSensitivity.y;

            var rawAngles = new Vector2(deltaRoll, deltaPitch);
            var magnitude = rawAngles.magnitude;

            if (magnitude <= _deadzoneDegrees)
            {
                return Vector2.zero;
            }

            var range = Mathf.Max(5f, _maxTiltDegrees - _deadzoneDegrees);
            var normalized = Mathf.Clamp01((magnitude - _deadzoneDegrees) / range);
            return (rawAngles / magnitude) * normalized;
        }

        private void StartUdpReceiver()
        {
            try
            {
                _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpSocket.ReceiveTimeout = 400;
                _udpSocket.Bind(new IPEndPoint(IPAddress.Any, _udpPort));
                _isRunning = true;

                _receiveThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name = "VARG_OscReceiver"
                };
                _receiveThread.Start();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MobileShipInput] Could not bind UDP port {_udpPort}: {ex.Message}");
            }
        }

        private void ReceiveLoop()
        {
            var buffer = new byte[512];
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (_isRunning && _udpSocket != null)
            {
                try
                {
                    var length = _udpSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                    if (length >= 12 && buffer[0] == (byte)'/')
                    {
                        ParseOscFloatMessage(buffer, length);
                    }
                }
                catch (SocketException) { }
                catch (ObjectDisposedException) { break; }
            }
        }

        private void ParseOscFloatMessage(byte[] buffer, int length)
        {
            var addressEnd = Array.IndexOf(buffer, (byte)0, 0, length);
            if (addressEnd <= 0) return;

            var typeTagStart = (addressEnd + 4) & ~3;
            if (typeTagStart + 8 > length || buffer[typeTagStart] != (byte)',' || buffer[typeTagStart + 1] != (byte)'f')
            {
                return;
            }

            var valueOffset = typeTagStart + 4;
            var intBits = (buffer[valueOffset] << 24)
                        | (buffer[valueOffset + 1] << 16)
                        | (buffer[valueOffset + 2] << 8)
                        | buffer[valueOffset + 3];
            var value = BitConverter.Int32BitsToSingle(intBits);

            var address = Encoding.ASCII.GetString(buffer, 0, addressEnd);
            var axisChar = address[address.Length - 1];

            if (address.Contains("accelerometer"))
            {
                if (axisChar == 'x') { _oscAccelRaw.x = value; _accelAxesMask |= 0b001; }
                else if (axisChar == 'y') { _oscAccelRaw.y = value; _accelAxesMask |= 0b010; }
                else if (axisChar == 'z') { _oscAccelRaw.z = value; _accelAxesMask |= 0b100; }

                if (_accelAxesMask == AllThreeAxesMask)
                {
                    lock (_sensorLock)
                    {
                        _latestAccelG = _oscAccelRaw / StandardGravity;
                        _hasAccel = true;
                        _hasNewSample = true;
                    }
                }
            }
            else if (address.Contains("gyroscope"))
            {
                if (axisChar == 'x') { _oscGyroRaw.x = value; _gyroAxesMask |= 0b001; }
                else if (axisChar == 'y') { _oscGyroRaw.y = value; _gyroAxesMask |= 0b010; }
                else if (axisChar == 'z') { _oscGyroRaw.z = value; _gyroAxesMask |= 0b100; }

                if (_gyroAxesMask == AllThreeAxesMask)
                {
                    lock (_sensorLock)
                    {
                        _latestGyroRad = _oscGyroRaw;
                        _hasGyro = true;
                        _hasNewSample = true;
                    }
                }
            }
        }
    }
}
