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

        private void Awake()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }

            _lineRenderer.positionCount = 2;
            _lineRenderer.enabled = false;
        }

        public void UpdateBeam(Vector3 currentItemPos)
        {
            if (_lineRenderer == null) return;

            var origin = _muzzlePoint != null ? _muzzlePoint.position : transform.position;
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, origin);
            _lineRenderer.SetPosition(1, currentItemPos);
        }

        public void HideBeam()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }
        }

        private void OnDisable()
        {
            HideBeam();
        }
    }
}
