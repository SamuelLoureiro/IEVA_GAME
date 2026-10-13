using UnityEngine;

namespace VARG.Sorting
{
    public class HighlightOnHover : MonoBehaviour
    {
        [SerializeField] private Color _highlightTint = new Color(0.3f, 0.3f, 0.3f, 0f);

        private Renderer _renderer;
        private Material _material;
        private Color _originalColor;
        private bool _isHighlighted;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null)
            {
                _material = _renderer.material;
                _originalColor = _material.color;
            }
        }

        public void SetHighlight(bool enable)
        {
            if (_material == null || _isHighlighted == enable) return;

            _isHighlighted = enable;
            _material.color = enable ? _originalColor + _highlightTint : _originalColor;
        }

        private void OnDisable()
        {
            SetHighlight(false);
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
            }
        }
    }
}
