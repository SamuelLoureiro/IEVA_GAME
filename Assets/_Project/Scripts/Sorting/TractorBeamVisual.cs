using UnityEngine;
using VARG.Data;

namespace VARG.Sorting
{
    [RequireComponent(typeof(LineRenderer))]
    public class TractorBeamVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _muzzlePoint;

        [Header("Beam Parameters")]
        [SerializeField] private int _segments = 18;
        [SerializeField] private float _wobbleSpeed = 14f;
        [SerializeField] private float _wobbleStrength = 0.025f;

        private Vector3[] _curvePoints;
        private float _currentLiftFactor;

        private void Awake()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }

            _curvePoints = new Vector3[_segments];
            _lineRenderer.positionCount = _segments;
            _lineRenderer.enabled = false;
            _currentLiftFactor = 0f;
        }

        public void Configure(LineRenderer lineRenderer, Transform muzzlePoint)
        {
            _lineRenderer = lineRenderer;
            _muzzlePoint = muzzlePoint;

            if (_curvePoints == null || _curvePoints.Length != _segments)
            {
                _curvePoints = new Vector3[_segments];
            }

            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = _segments;
                _lineRenderer.enabled = false;
            }

            _currentLiftFactor = 0f;
        }

        public void UpdateBeam(Vector3 currentItemPos, Vector3 desiredHoldPos, TargetType targetType, float mass, float initialGrabY)
        {
            if (_lineRenderer == null) return;

            var origin = _muzzlePoint != null ? _muzzlePoint.position : transform.position;
            var distance = Vector3.Distance(origin, currentItemPos);
            var maxArchHeight = CalculateArchHeight(targetType, distance);

            var upwardPull = Mathf.Max(0f, desiredHoldPos.y - currentItemPos.y);
            var heightAbovePickup = Mathf.Max(0f, currentItemPos.y - initialGrabY);
            var targetLift = Mathf.Clamp01(Mathf.Max(upwardPull * 1.6f, heightAbovePickup * 1.3f));

            _currentLiftFactor = Mathf.MoveTowards(_currentLiftFactor, targetLift, Time.deltaTime * 6f);

            var wobble = Mathf.Sin(Time.time * _wobbleSpeed) * _wobbleStrength * _currentLiftFactor;
            var midPoint = Vector3.Lerp(origin, currentItemPos, 0.5f);
            midPoint += Vector3.up * ((maxArchHeight * _currentLiftFactor) + wobble);

            for (var i = 0; i < _segments; i++)
            {
                var t = (float)i / (_segments - 1);
                _curvePoints[i] = EvaluateQuadraticBezier(origin, midPoint, currentItemPos, t);
            }

            _lineRenderer.enabled = true;
            _lineRenderer.SetPositions(_curvePoints);
        }

        public void HideBeam()
        {
            _currentLiftFactor = 0f;
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }
        }

        private float CalculateArchHeight(TargetType type, float distance)
        {
            var weight = type switch
            {
                TargetType.Cow => 0.65f,
                TargetType.Pig => 0.38f,
                TargetType.Human => 0.38f,
                TargetType.Chicken => 0.10f,
                _ => 0.30f
            };

            var distanceFactor = Mathf.Clamp(distance / 2.5f, 0.65f, 1.35f);
            return weight * distanceFactor;
        }

        private static Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            var u = 1f - t;
            return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
        }

        private void OnDisable()
        {
            HideBeam();
        }
    }
}
