using System.Collections;
using UnityEngine;

namespace VARG.Player
{
    [RequireComponent(typeof(LineRenderer))]
    public class ShrinkBeamFX : MonoBehaviour
    {
        [SerializeField] private float _beamDuration = 0.12f;
        [SerializeField] private Color _beamColor = new Color(0.2f, 1f, 0.5f, 0.9f);
        [SerializeField] private float _startWidth = 0.08f;
        [SerializeField] private float _endWidth = 0.04f;

        private LineRenderer _lineRenderer;
        private Coroutine _beamCoroutine;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            ConfigureLineRenderer();
        }

        private void ConfigureLineRenderer()
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = _startWidth;
            _lineRenderer.endWidth = _endWidth;
            _lineRenderer.startColor = _beamColor;
            _lineRenderer.endColor = _beamColor;
            _lineRenderer.enabled = false;

            if (_lineRenderer.sharedMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
                if (shader != null)
                {
                    _lineRenderer.material = new Material(shader) { color = _beamColor };
                }
            }
        }

        public void PlayBeam(Vector3 startPoint, Vector3 endPoint)
        {
            if (_beamCoroutine != null)
            {
                StopCoroutine(_beamCoroutine);
            }

            _beamCoroutine = StartCoroutine(BeamRoutine(startPoint, endPoint));
        }

        private IEnumerator BeamRoutine(Vector3 startPoint, Vector3 endPoint)
        {
            _lineRenderer.SetPosition(0, startPoint);
            _lineRenderer.SetPosition(1, endPoint);
            _lineRenderer.enabled = true;

            var elapsed = 0f;
            while (elapsed < _beamDuration)
            {
                elapsed += Time.deltaTime;
                var alpha = 1f - (elapsed / _beamDuration);
                var fadeColor = new Color(_beamColor.r, _beamColor.g, _beamColor.b, alpha);
                _lineRenderer.startColor = fadeColor;
                _lineRenderer.endColor = fadeColor;
                yield return null;
            }

            _lineRenderer.enabled = false;
            _beamCoroutine = null;
        }
    }
}
