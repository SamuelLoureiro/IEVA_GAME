using UnityEngine;
using VARG.Input;

namespace VARG.Sorting
{
    [RequireComponent(typeof(CharacterController))]
    public class ETPlayerSortingController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _gravity = -15f;

        [Header("Look")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _mouseSensitivity = 1.8f;
        [SerializeField] private float _minPitch = -85f;
        [SerializeField] private float _maxPitch = 85f;

        private CharacterController _characterController;
        private IPlayerInput _fallbackInput;
        private float _verticalVelocity;
        private float _currentPitch;

        public Transform CameraTransform => _cameraTransform;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _fallbackInput = new KeyboardMouseInput();
        }

        private void OnEnable()
        {
            LockCursor(true);
        }

        private void OnDisable()
        {
            LockCursor(false);
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
            HandleCursorToggle();
        }

        public void Configure(Transform cameraTransform, float moveSpeed = 3.5f)
        {
            _cameraTransform = cameraTransform;
            _moveSpeed = moveSpeed;
        }

        private void HandleLook()
        {
            if (_cameraTransform == null || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            var lookDelta = ReadLookInput();
            var yaw = lookDelta.x * _mouseSensitivity * 0.1f;
            var pitch = lookDelta.y * _mouseSensitivity * 0.1f;

            transform.Rotate(Vector3.up * yaw);

            _currentPitch -= pitch;
            _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch);
            _cameraTransform.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            var moveInput = ReadMoveInput();

            var moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
            var motion = moveDirection * _moveSpeed;

            if (_characterController.isGrounded)
            {
                _verticalVelocity = -1f;
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }

            motion.y = _verticalVelocity;
            _characterController.Move(motion * Time.deltaTime);
        }

        private void HandleCursorToggle()
        {
            if (GetInput().PausePressed)
            {
                var isLocked = Cursor.lockState == CursorLockMode.Locked;
                LockCursor(!isLocked);
            }
        }

        private Vector2 ReadMoveInput()
        {
            return GetInput().Move;
        }

        private Vector2 ReadLookInput()
        {
            return GetInput().Look;
        }

        private IPlayerInput GetInput()
        {
            if (InputManager.Instance != null && InputManager.Instance.Player != null)
            {
                return InputManager.Instance.Player;
            }
            return _fallbackInput;
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
