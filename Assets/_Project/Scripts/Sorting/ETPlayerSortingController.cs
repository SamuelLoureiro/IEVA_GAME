using UnityEngine;
using UnityEngine.InputSystem;
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
        private float _verticalVelocity;
        private float _currentPitch;

        public Transform CameraTransform => _cameraTransform;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
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
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                var isLocked = Cursor.lockState == CursorLockMode.Locked;
                LockCursor(!isLocked);
            }
        }

        private Vector2 ReadMoveInput()
        {
            if (InputManager.Instance != null && InputManager.Instance.Player != null)
            {
                return InputManager.Instance.Player.Move;
            }

            var input = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            }

            return input;
        }

        private Vector2 ReadLookInput()
        {
            if (InputManager.Instance != null && InputManager.Instance.Player != null)
            {
                return InputManager.Instance.Player.Look;
            }

            var mouse = Mouse.current;
            return mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
