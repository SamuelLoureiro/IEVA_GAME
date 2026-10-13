using UnityEngine;
using UnityEngine.UI;
using VARG.Player;

namespace VARG.UI
{
    public class CrosshairUI : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor _interactor;
        [SerializeField] private Image _crosshairImage;
        [SerializeField] private Color _defaultColor = new Color(0.2f, 1f, 0.4f, 0.75f);
        [SerializeField] private Color _targetDetectedColor = new Color(1f, 0.15f, 0.15f, 0.95f);

        private void Start()
        {
            if (_interactor == null)
            {
                _interactor = Object.FindFirstObjectByType<PlayerInteractor>();
            }

            if (_interactor != null)
            {
                _interactor.OnTargetChanged += HandleTargetChanged;
            }

            ResetCrosshair();
        }

        private void OnDestroy()
        {
            if (_interactor != null)
            {
                _interactor.OnTargetChanged -= HandleTargetChanged;
            }
        }

        private void HandleTargetChanged(IInteractable target)
        {
            if (_crosshairImage == null) return;

            _crosshairImage.color = target != null ? _targetDetectedColor : _defaultColor;
        }

        private void ResetCrosshair()
        {
            if (_crosshairImage != null)
            {
                _crosshairImage.color = _defaultColor;
            }
        }

        public void BindInteractor(PlayerInteractor interactor)
        {
            if (_interactor != null)
            {
                _interactor.OnTargetChanged -= HandleTargetChanged;
            }

            _interactor = interactor;

            if (_interactor != null)
            {
                _interactor.OnTargetChanged += HandleTargetChanged;
            }

            ResetCrosshair();
        }
    }
}
