using UnityEngine;

namespace VARG.Ship
{
    public class ShipFollowCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 _positionOffset = new Vector3(0f, 16f, -18f);
        [SerializeField] private Vector3 _lookAtOffset = new Vector3(0f, -2f, 4f);
        [SerializeField] private float _positionSmoothTime = 0.18f;
        [SerializeField] private float _rotationSmoothSpeed = 10f;

        private Vector3 _currentVelocity;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        private void Start()
        {
            if (_target == null)
            {
                var ship = Object.FindFirstObjectByType<ShipController>();
                if (ship != null)
                {
                    _target = ship.transform;
                }
            }

            if (_target != null)
            {
                transform.position = _target.position + _positionOffset;
                transform.LookAt(_target.position + _lookAtOffset);
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            var desiredPosition = _target.position + _positionOffset;
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _currentVelocity,
                _positionSmoothTime
            );

            var lookTarget = _target.position + _lookAtOffset;
            var direction = lookTarget - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                var targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _rotationSmoothSpeed * Time.deltaTime
                );
            }
        }
    }
}
