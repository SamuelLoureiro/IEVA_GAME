using UnityEngine;
using VARG.Input;

namespace VARG.Ship
{
    [RequireComponent(typeof(Rigidbody))]
    public class ShipController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _accelerationForce = 45f;
        [SerializeField] private float _maxSpeed = 18f;
        [SerializeField] private float _linearDamping = 4f;
        [SerializeField] private float _flightHeight = 12f;
        [SerializeField] private Vector2 _boundsExtents = new Vector2(80f, 80f);

        [Header("Visual Tilt")]
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private float _maxTiltAngle = 20f;
        [SerializeField] private float _tiltSmoothSpeed = 10f;
        [SerializeField] private float _idleSpinSpeed = 25f;

        private Rigidbody _rigidbody;
        private Vector2 _steerInput;
        private float _currentYaw;

        public bool CanControl { get; set; } = true;
        public float CurrentSpeed => _rigidbody != null ? _rigidbody.linearVelocity.magnitude : 0f;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.linearDamping = _linearDamping;
            _rigidbody.angularDamping = 10f;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

            if (_visualRoot != null)
            {
                _currentYaw = _visualRoot.localEulerAngles.y;
            }
        }

        private void Start()
        {
            var position = transform.position;
            position.y = _flightHeight;
            transform.position = position;
        }

        private void Update()
        {
            var shipInput = CanControl && InputManager.HasInstance ? InputManager.Instance.Ship : null;
            _steerInput = shipInput != null ? Vector2.ClampMagnitude(shipInput.Steer, 1f) : Vector2.zero;

            UpdateVisualTilt(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (CanControl && _steerInput.sqrMagnitude > 0.0001f)
            {
                var force = new Vector3(_steerInput.x, 0f, _steerInput.y) * _accelerationForce;
                _rigidbody.AddForce(force, ForceMode.Acceleration);
            }

            if (_rigidbody.linearVelocity.sqrMagnitude > _maxSpeed * _maxSpeed)
            {
                _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _maxSpeed;
            }

            ClampPositionToBounds();
        }

        public void StopImmediately()
        {
            CanControl = false;
            _steerInput = Vector2.zero;
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }
        }

        private void ClampPositionToBounds()
        {
            var pos = _rigidbody.position;
            var clampedX = Mathf.Clamp(pos.x, -_boundsExtents.x, _boundsExtents.x);
            var clampedZ = Mathf.Clamp(pos.z, -_boundsExtents.y, _boundsExtents.y);

            if (!Mathf.Approximately(pos.x, clampedX) || !Mathf.Approximately(pos.z, clampedZ))
            {
                _rigidbody.position = new Vector3(clampedX, _flightHeight, clampedZ);
            }
        }

        private void UpdateVisualTilt(float deltaTime)
        {
            if (_visualRoot == null || _maxSpeed <= 0.01f) return;

            var velocityRatio = _rigidbody.linearVelocity / _maxSpeed;
            var pitch = Mathf.Clamp(velocityRatio.z, -1f, 1f) * _maxTiltAngle;
            var roll = -Mathf.Clamp(velocityRatio.x, -1f, 1f) * _maxTiltAngle;
            _currentYaw = (_currentYaw + _idleSpinSpeed * deltaTime) % 360f;

            var targetRotation = Quaternion.Euler(pitch, _currentYaw, roll);
            _visualRoot.localRotation = Quaternion.Slerp(_visualRoot.localRotation, targetRotation, _tiltSmoothSpeed * deltaTime);
        }
    }
}
