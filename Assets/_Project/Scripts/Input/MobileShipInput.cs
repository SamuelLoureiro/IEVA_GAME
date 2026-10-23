using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VARG.Input
{
    public class MobileShipInput : IShipInput, IDisposable
    {
        private const float Gravity = 9.80665f;
        private const float TimeoutSeconds = 1.5f;

        private readonly KeyboardShipInput _keyboardFallback = new();
        private readonly ShakeDetector _shakeDetector;
        private readonly byte[] _packetBuffer = new byte[256];

        private readonly float _deadzoneDegrees;
        private readonly float _maxTiltDegrees;
        private readonly float _steerSmoothing;
        private readonly Vector2 _axisSensitivity;

        private Socket _udpSocket;
        private Vector3 _accel = new(0f, 0f, -Gravity);
        private Vector3 _gyro;

        private float _pitchDeg;
        private float _rollDeg;
        private float _neutralPitchDeg;
        private float _neutralRollDeg;
        private bool _isCalibrated;
        private float _lastPacketTime = -100f;

        public Vector2 Steer { get; private set; }
        public bool DescendPressed { get; private set; }
        public bool CalibratePressed { get; private set; }
        public bool IsMobileConnected => (Time.realtimeSinceStartup - _lastPacketTime) <= TimeoutSeconds;

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
            _maxTiltDegrees = Mathf.Clamp(maxTiltDegrees, 10f, 65f);
            _deadzoneDegrees = Mathf.Clamp(deadzone, 0.02f, 0.4f) * _maxTiltDegrees;
            _steerSmoothing = Mathf.Max(1f, steerSmoothing);
            _axisSensitivity = axisSensitivity ?? Vector2.one;
            _shakeDetector = new ShakeDetector(shakeThresholdG, shakeRequiredPeaks, shakeWindowSeconds);

            try
            {
                _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    Blocking = false
                };
                _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpSocket.Bind(new IPEndPoint(IPAddress.Any, udpPort));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not bind UDP port {udpPort}: {ex.Message}");
            }
        }

        public void Tick(float deltaTime)
        {
            var shakeDetected = ReadPendingOscPackets();
            DescendPressed = shakeDetected || _keyboardFallback.DescendPressed;
            CalibratePressed = Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;

            if (CalibratePressed)
            {
                CalibrateNeutralPose();
            }

            var keyboardSteer = _keyboardFallback.Steer;
            if (!IsMobileConnected || keyboardSteer.sqrMagnitude > 0.01f)
            {
                Steer = keyboardSteer;
                return;
            }

            UpdateSensorFusion(deltaTime);
            var targetSteer = ComputeSteerFromTilt();
            Steer = Vector2.Lerp(Steer, targetSteer, _steerSmoothing * deltaTime);
        }

        public void CalibrateNeutralPose()
        {
            _neutralPitchDeg = _pitchDeg;
            _neutralRollDeg = _rollDeg;
            _isCalibrated = true;
        }

        public void Dispose()
        {
            try { _udpSocket?.Close(); } catch { }
            _udpSocket = null;
        }

        private bool ReadPendingOscPackets()
        {
            if (_udpSocket == null) return false;

            var shakeTriggered = false;
            while (_udpSocket.Available > 0)
            {
                try
                {
                    var length = _udpSocket.Receive(_packetBuffer);
                    if (length >= 12 && _packetBuffer[0] == (byte)'/')
                    {
                        _lastPacketTime = Time.realtimeSinceStartup;
                        if (ParseOscPacket(_packetBuffer, length) && _shakeDetector.ProcessSample(_accel / Gravity, Time.unscaledTime))
                        {
                            shakeTriggered = true;
                        }
                    }
                }
                catch (SocketException) { break; }
            }

            return shakeTriggered;
        }

        private bool ParseOscPacket(byte[] buffer, int length)
        {
            var addressEnd = Array.IndexOf(buffer, (byte)0, 0, length);
            var typeTagStart = (addressEnd + 4) & ~3;
            if (addressEnd <= 0 || typeTagStart + 8 > length || buffer[typeTagStart] != (byte)',') return false;

            var valOffset = typeTagStart + 4;
            var bits = (buffer[valOffset] << 24) | (buffer[valOffset + 1] << 16) | (buffer[valOffset + 2] << 8) | buffer[valOffset + 3];
            var value = BitConverter.Int32BitsToSingle(bits);

            var address = Encoding.ASCII.GetString(buffer, 0, addressEnd);
            var axis = address[address.Length - 1];

            if (address.Contains("accelerometer"))
            {
                if (axis == 'x') _accel.x = value;
                else if (axis == 'y') _accel.y = value;
                else if (axis == 'z') { _accel.z = value; return true; }
            }
            else if (address.Contains("gyroscope"))
            {
                if (axis == 'x') _gyro.x = value;
                else if (axis == 'y') _gyro.y = value;
                else if (axis == 'z') _gyro.z = value;
            }

            return false;
        }

        private void UpdateSensorFusion(float dt)
        {
            var norm = _accel.normalized;
            var zAbs = Mathf.Max(0.15f, Mathf.Abs(norm.z));

            var accelPitch = Mathf.Atan2(norm.x, Mathf.Sqrt(norm.y * norm.y + zAbs * zAbs)) * Mathf.Rad2Deg;
            var accelRoll = Mathf.Atan2(-norm.y, Mathf.Sqrt(norm.x * norm.x + zAbs * zAbs)) * Mathf.Rad2Deg;

            if (!_isCalibrated)
            {
                _pitchDeg = accelPitch;
                _rollDeg = accelRoll;
                CalibrateNeutralPose();
                return;
            }

            var gyroPitch = _pitchDeg + (_gyro.y * Mathf.Rad2Deg * dt);
            var gyroRoll = _rollDeg + (_gyro.x * Mathf.Rad2Deg * dt);

            _pitchDeg = Mathf.LerpAngle(gyroPitch, accelPitch, 0.2f);
            _rollDeg = Mathf.LerpAngle(gyroRoll, accelRoll, 0.2f);
        }

        private Vector2 ComputeSteerFromTilt()
        {
            var deltaRoll = Mathf.DeltaAngle(_neutralRollDeg, _rollDeg) * _axisSensitivity.x;
            var deltaPitch = Mathf.DeltaAngle(_neutralPitchDeg, _pitchDeg) * _axisSensitivity.y;

            var angles = new Vector2(deltaRoll, deltaPitch);
            var magnitude = angles.magnitude;
            if (magnitude <= _deadzoneDegrees) return Vector2.zero;

            var normalized = Mathf.Clamp01((magnitude - _deadzoneDegrees) / (_maxTiltDegrees - _deadzoneDegrees));
            return (angles / magnitude) * normalized;
        }
    }
}
