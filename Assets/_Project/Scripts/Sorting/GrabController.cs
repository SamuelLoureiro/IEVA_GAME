using UnityEngine;
using VARG.Input;

namespace VARG.Sorting
{
    public class GrabController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _playerCamera;

        [Header("Grabbing Settings")]
        [SerializeField] private LayerMask _grabbableLayer;
        [SerializeField] private float _raycastDistance = 4.5f;
        [SerializeField] private float _minHoldDistance = 1.2f;
        [SerializeField] private float _maxHoldDistance = 3.5f;
        [SerializeField] private float _defaultHoldDistance = 2.0f;
        [SerializeField] private float _scrollSensitivity = 0.35f;
        [SerializeField] private float _followForce = 18f;
        [SerializeField] private float _throwForce = 14f;

        [Header("Visuals")]
        [SerializeField] private TractorBeamVisual _beamVisual;

        private IPlayerInput _fallbackInput;
        private Grabbable _hoveredGrabbable;
        private Grabbable _heldGrabbable;
        private float _currentHoldDistance;
        private float _initialGrabY;

        public bool IsHolding => _heldGrabbable != null;
        public Grabbable HeldGrabbable => _heldGrabbable;

        private void Awake()
        {
            if (_playerCamera == null)
            {
                _playerCamera = GetComponentInChildren<Camera>();
                if (_playerCamera == null)
                {
                    _playerCamera = Camera.main;
                }
            }

            if (_grabbableLayer.value == 0)
            {
                var layer = LayerMask.NameToLayer("Grabbable");
                _grabbableLayer = layer != -1 ? 1 << layer : ~0;
            }

            if (_beamVisual == null)
            {
                _beamVisual = GetComponentInChildren<TractorBeamVisual>();
            }

            _fallbackInput = new KeyboardMouseInput();
            _currentHoldDistance = _defaultHoldDistance;
        }

        public void Configure(Camera playerCamera, TractorBeamVisual beamVisual = null)
        {
            _playerCamera = playerCamera;
            if (beamVisual != null)
            {
                _beamVisual = beamVisual;
            }
        }

        private void Update()
        {
            if (_playerCamera == null) return;

            if (_heldGrabbable != null && (!_heldGrabbable.gameObject.activeInHierarchy || !_heldGrabbable.enabled))
            {
                DropHeld();
            }

            var input = GetInput();
            UpdateHover();
            UpdateGrabbing(input);
            UpdateBeamVisual();
        }

        private void FixedUpdate()
        {
            if (_heldGrabbable != null && _playerCamera != null)
            {
                var targetPosition = _playerCamera.transform.position + _playerCamera.transform.forward * _currentHoldDistance;
                _heldGrabbable.FollowTarget(targetPosition, _followForce);
            }
        }

        private void UpdateHover()
        {
            if (_heldGrabbable != null)
            {
                ClearHover();
                return;
            }

            var ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit, _raycastDistance, _grabbableLayer))
            {
                var grabbable = hit.collider.GetComponentInParent<Grabbable>();
                if (grabbable != _hoveredGrabbable)
                {
                    ClearHover();
                    _hoveredGrabbable = grabbable;
                    if (_hoveredGrabbable != null)
                    {
                        _hoveredGrabbable.SetHighlight(true);
                    }
                }
            }
            else
            {
                ClearHover();
            }
        }

        private void ClearHover()
        {
            if (_hoveredGrabbable != null)
            {
                _hoveredGrabbable.SetHighlight(false);
                _hoveredGrabbable = null;
            }
        }

        private void UpdateGrabbing(IPlayerInput input)
        {
            if (_heldGrabbable == null)
            {
                if (input.GrabHeld && _hoveredGrabbable != null)
                {
                    _heldGrabbable = _hoveredGrabbable;
                    ClearHover();

                    var initialDistance = Vector3.Distance(_playerCamera.transform.position, _heldGrabbable.transform.position);
                    _currentHoldDistance = Mathf.Clamp(initialDistance, _minHoldDistance, _maxHoldDistance);
                    _initialGrabY = _heldGrabbable.transform.position.y;

                    _heldGrabbable.OnGrabbed();
                }
            }
            else
            {
                var scroll = input.HoldDistanceDelta;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    var delta = Mathf.Sign(scroll) * _scrollSensitivity;
                    _currentHoldDistance = Mathf.Clamp(_currentHoldDistance + delta, _minHoldDistance, _maxHoldDistance);
                }

                if (input.ThrowPressed)
                {
                    var impulse = _playerCamera.transform.forward * _throwForce;
                    _heldGrabbable.OnReleased(impulse);
                    _heldGrabbable = null;
                }
                else if (!input.GrabHeld)
                {
                    _heldGrabbable.OnReleased(Vector3.zero);
                    _heldGrabbable = null;
                }
            }
        }

        public void DropHeld()
        {
            if (_heldGrabbable != null)
            {
                _heldGrabbable.OnReleased(Vector3.zero);
                _heldGrabbable = null;
            }

            if (_beamVisual != null)
            {
                _beamVisual.HideBeam();
            }
        }

        private void UpdateBeamVisual()
        {
            if (_beamVisual == null) return;

            if (_heldGrabbable != null && _playerCamera != null)
            {
                var sortable = _heldGrabbable.SortableItem;
                var targetType = (sortable != null && sortable.Data != null) ? sortable.Data.Type : VARG.Data.TargetType.Chicken;
                var mass = _heldGrabbable.Rigidbody != null ? _heldGrabbable.Rigidbody.mass : 1f;
                var desiredHoldPos = _playerCamera.transform.position + _playerCamera.transform.forward * _currentHoldDistance;
                _beamVisual.UpdateBeam(_heldGrabbable.transform.position, desiredHoldPos, targetType, mass, _initialGrabY);
            }
            else
            {
                _beamVisual.HideBeam();
            }
        }

        private IPlayerInput GetInput()
        {
            if (InputManager.Instance != null && InputManager.Instance.Player != null)
            {
                return InputManager.Instance.Player;
            }
            return _fallbackInput;
        }

        private void OnDisable()
        {
            ClearHover();
            DropHeld();
        }
    }
}
