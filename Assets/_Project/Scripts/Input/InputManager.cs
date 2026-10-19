using UnityEngine;
using VARG.Core;

namespace VARG.Input
{
    public class InputManager : Singleton<InputManager>
    {
        [Header("Mobile Ship Input (UDP + Gyro/Accel)")]
        [SerializeField] private int _udpPort = 5555;
        [SerializeField] private float _tiltDeadzone = 0.08f;
        [SerializeField] private float _maxTiltDegrees = 28f;
        [SerializeField] private float _steerSmoothing = 12f;
        [SerializeField] private Vector2 _axisSensitivity = Vector2.one;

        [Header("Shake Detector")]
        [SerializeField] private float _shakeThresholdG = 2.0f;
        [SerializeField] private int _shakeRequiredPeaks = 2;
        [SerializeField] private float _shakeWindowSeconds = 0.5f;

        private IPlayerInput _playerInput;
        private IShipInput _shipInput;
        private MobileShipInput _mobileShipInput;

        public IPlayerInput Player
        {
            get => _playerInput ??= new KeyboardMouseInput();
            private set => _playerInput = value;
        }

        public IShipInput Ship
        {
            get
            {
                if (_shipInput == null)
                {
                    InitializeMobileShipInput();
                }
                return _shipInput;
            }
            private set => _shipInput = value;
        }

        public MobileShipInput MobileShip => _mobileShipInput;

        protected override void Awake()
        {
            base.Awake();
            _playerInput ??= new KeyboardMouseInput();
            if (_shipInput == null)
            {
                InitializeMobileShipInput();
            }
        }

        private void Update()
        {
            _mobileShipInput?.Tick(Time.deltaTime);
        }

        protected override void OnDestroy()
        {
            if (_mobileShipInput != null)
            {
                _mobileShipInput.Dispose();
                _mobileShipInput = null;
            }

            base.OnDestroy();
        }

        public void SetPlayerInput(IPlayerInput input)
        {
            Player = input;
        }

        public void SetShipInput(IShipInput input)
        {
            Ship = input;
        }

        public void CalibrateShipGyro()
        {
            _mobileShipInput?.CalibrateNeutralPose();
        }

        private void InitializeMobileShipInput()
        {
            _mobileShipInput?.Dispose();
            _mobileShipInput = new MobileShipInput(
                _udpPort,
                _tiltDeadzone,
                _maxTiltDegrees,
                _steerSmoothing,
                _shakeThresholdG,
                _shakeRequiredPeaks,
                _shakeWindowSeconds,
                _axisSensitivity
            );
            _shipInput = _mobileShipInput;
        }
    }
}
