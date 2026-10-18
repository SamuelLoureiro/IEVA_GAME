using System.Collections;
using TMPro;
using UnityEngine;

namespace VARG.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TimeDeltaPopup : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI _deltaText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float _floatDistance = 50f;
        [SerializeField] private float _duration = 1.2f;
        [SerializeField] private Color _penaltyColor = new Color(1f, 0.25f, 0.25f);

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void Show(float seconds, string suffix = "s")
        {
            Show($"-{seconds:0}{suffix}");
        }

        public void Show(string text)
        {
            if (_deltaText != null)
            {
                _deltaText.text = text;
                _deltaText.color = _penaltyColor;
            }

            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(AnimateRoutine());
        }

        private IEnumerator AnimateRoutine()
        {
            var elapsed = 0f;
            var startPos = _rectTransform != null ? _rectTransform.anchoredPosition : Vector2.zero;
            var targetPos = startPos + Vector2.up * _floatDistance;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / _duration);

                if (_rectTransform != null)
                {
                    _rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                }

                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1f - Mathf.Pow(t, 2f);
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
