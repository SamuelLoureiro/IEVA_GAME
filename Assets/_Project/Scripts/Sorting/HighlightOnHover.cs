using UnityEngine;

namespace VARG.Sorting
{
    public class HighlightOnHover : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Color _highlightTint = new Color(0.3f, 0.3f, 0.3f, 0f);

        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;
        private Color _originalColor = Color.white;
        private bool _isHighlighted;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _propBlock = new MaterialPropertyBlock();

            if (_renderer != null && _renderer.sharedMaterial != null)
            {
                var sharedMat = _renderer.sharedMaterial;
                if (sharedMat.HasProperty(BaseColorId))
                {
                    _originalColor = sharedMat.GetColor(BaseColorId);
                }
                else if (sharedMat.HasProperty(ColorId))
                {
                    _originalColor = sharedMat.GetColor(ColorId);
                }
            }
        }

        public void SetHighlight(bool enable)
        {
            if (_renderer == null || _isHighlighted == enable) return;

            _isHighlighted = enable;
            if (enable)
            {
                var targetColor = _originalColor + _highlightTint;
                _renderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(BaseColorId, targetColor);
                _propBlock.SetColor(ColorId, targetColor);
                _renderer.SetPropertyBlock(_propBlock);
            }
            else
            {
                _renderer.SetPropertyBlock(null);
            }
        }

        private void OnDisable()
        {
            SetHighlight(false);
        }
    }
}
