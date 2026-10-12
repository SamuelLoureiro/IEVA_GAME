using UnityEngine;
using VARG.Input;

namespace VARG.Player
{
    public class ETCameraLook : MonoBehaviour
    {
        [SerializeField] private Transform _playerBody;
        [SerializeField] private float _mouseSensitivity = 1.5f;
        [SerializeField] private float _minPitch = -85f;
        [SerializeField] private float _maxPitch = 85f;
        [SerializeField] private bool _lockCursorOnStart = true;

        private IPlayerInput _playerInput;
        private float _currentPitch;
        private bool _isLookActive = true;

        public bool IsLookActive
        {
            get => _isLookActive;
            set
            {
                _isLookActive = value;
                UpdateCursorLock(value);
            }
        }

        private void Start()
        {
            if (_playerBody == null && transform.parent != null)
            {
                _playerBody = transform.parent;
            }

            _playerInput = InputManager.HasInstance ? InputManager.Instance.Player : new KeyboardMouseInput();

            if (_lockCursorOnStart)
            {
                UpdateCursorLock(true);
            }
        }

        private void Update()
        {
            ApplyLook();
        }

        private void ApplyLook()
        {
            if (!_isLookActive || _playerInput == null) return;

            var lookDelta = _playerInput.Look * _mouseSensitivity;

            _currentPitch -= lookDelta.y;
            _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch);

            transform.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);

            if (_playerBody != null)
            {
                _playerBody.Rotate(Vector3.up * lookDelta.x);
            }
        }

        public void UpdateCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void SetInputSource(IPlayerInput inputSource)
        {
            _playerInput = inputSource;
        }
    }
}
