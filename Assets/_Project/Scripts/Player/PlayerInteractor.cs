using System;
using UnityEngine;
using VARG.Input;

namespace VARG.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _range = 15f;
        [SerializeField] private LayerMask _interactableLayer = ~0;

        private IPlayerInput _playerInput;
        private IInteractable _currentTarget;
        private RaycastHit _lastHit;

        public IInteractable CurrentTarget => _currentTarget;
        public bool HasTarget => _currentTarget != null;
        public RaycastHit LastHit => _lastHit;

        public event Action<IInteractable> OnTargetChanged;

        private void Start()
        {
            if (_playerCamera == null)
            {
                _playerCamera = GetComponent<Camera>();
                if (_playerCamera == null)
                {
                    _playerCamera = Camera.main;
                }
            }

            _playerInput = InputManager.HasInstance ? InputManager.Instance.Player : new KeyboardMouseInput();

            var interactableLayerIndex = LayerMask.NameToLayer("Interactable");
            if (interactableLayerIndex != -1)
            {
                _interactableLayer = 1 << interactableLayerIndex;
            }
        }

        private void Update()
        {
            PerformRaycast();
            HandleInteractionInput();
        }

        private void PerformRaycast()
        {
            if (_playerCamera == null) return;

            var ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            IInteractable detectedInteractable = null;

            if (Physics.Raycast(ray, out _lastHit, _range, _interactableLayer))
            {
                detectedInteractable = _lastHit.collider.GetComponentInParent<IInteractable>();
                if (detectedInteractable != null && !detectedInteractable.CanInteract)
                {
                    detectedInteractable = null;
                }
            }

            if (!ReferenceEquals(_currentTarget, detectedInteractable))
            {
                _currentTarget = detectedInteractable;
                OnTargetChanged?.Invoke(_currentTarget);
            }
        }

        private void HandleInteractionInput()
        {
            if (_playerInput == null || _currentTarget == null) return;

            if (_playerInput.InteractPressed)
            {
                _currentTarget.Interact(gameObject);
            }
        }

        public void SetInputSource(IPlayerInput inputSource)
        {
            _playerInput = inputSource;
        }
    }
}
