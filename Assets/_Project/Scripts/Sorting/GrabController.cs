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
        [SerializeField] private float _holdDistance = 2.0f;
        [SerializeField] private float _followForce = 18f;
        [SerializeField] private float _throwForce = 14f;

        [Header("Visuals")]
        [SerializeField] private TractorBeamVisual _beamVisual;

        private IPlayerInput _fallbackInput;
        private Grabbable _heldGrabbable;

        public bool IsHolding => _heldGrabbable != null;
        public Grabbable HeldGrabbable => _heldGrabbable;

        private void Awake()
        {
            if (_playerCamera == null)
            {
                _playerCamera = GetComponentInChildren<Camera>() ?? Camera.main;
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
            UpdateGrabbing(input);
            UpdateBeamVisual();
        }

        private void FixedUpdate()
        {
            if (_heldGrabbable != null && _playerCamera != null)
            {
                var targetPosition = _playerCamera.transform.position + _playerCamera.transform.forward * _holdDistance;
                _heldGrabbable.FollowTarget(targetPosition, _followForce);
            }
        }

        private void UpdateGrabbing(IPlayerInput input)
        {
            if (_heldGrabbable == null)
            {
                if (input.GrabHeld)
                {
                    var ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
                    if (Physics.Raycast(ray, out var hit, _raycastDistance, _grabbableLayer))
                    {
                        var grabbable = hit.collider.GetComponentInParent<Grabbable>();
                        if (grabbable != null && !grabbable.IsHeld)
                        {
                            _heldGrabbable = grabbable;
                            _heldGrabbable.OnGrabbed();
                        }
                    }
                }
            }
            else
            {
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
                var desiredHoldPos = _playerCamera.transform.position + _playerCamera.transform.forward * _holdDistance;
                _beamVisual.UpdateBeam(_heldGrabbable.transform.position, desiredHoldPos, targetType, mass, _heldGrabbable.transform.position.y);
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
            DropHeld();
        }
    }
}
