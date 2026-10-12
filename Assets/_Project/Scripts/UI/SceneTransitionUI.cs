using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VARG.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SceneTransitionUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private float _fadeDuration = 0.3f;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        public IEnumerator FadeIn()
        {
            _canvasGroup.blocksRaycasts = true;
            UpdateProgress(0f);
            SetStatus("Salvando planilha...");

            var elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }

        public void UpdateProgress(float progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = Mathf.Clamp01(progress);
            }
        }

        public void SetStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
            }
        }

        public IEnumerator FadeOut()
        {
            UpdateProgress(1f);
            var elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / _fadeDuration));
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
