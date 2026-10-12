using UnityEngine;
using VARG.Input;

namespace VARG.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class ETPlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _gravity = -15f;
        [SerializeField] private float _groundedDownwardForce = -2f;

        private CharacterController _characterController;
        private IPlayerInput _playerInput;
        private float _verticalVelocity;
        private bool _canMove = true;

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Mathf.Max(0f, value);
        }

        public bool CanMove
        {
            get => _canMove;
            set => _canMove = value;
        }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            _playerInput = InputManager.HasInstance ? InputManager.Instance.Player : new KeyboardMouseInput();
        }

        private void Update()
        {
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            if (!_canMove) return;

            var inputVector = _playerInput != null ? _playerInput.Move : Vector2.zero;
            var currentTransform = transform;
            var horizontalVelocity = (currentTransform.forward * inputVector.y + currentTransform.right * inputVector.x) * _moveSpeed;

            if (_characterController.isGrounded)
            {
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = _groundedDownwardForce;
                }
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }

            var motion = horizontalVelocity;
            motion.y = _verticalVelocity;

            _characterController.Move(motion * Time.deltaTime);
        }

        public void SetInputSource(IPlayerInput inputSource)
        {
            _playerInput = inputSource;
        }
    }
}
