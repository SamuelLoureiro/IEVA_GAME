using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Core;
using VARG.Data;
using VARG.Inventory;
using VARG.Mission;

namespace VARG.UI
{
    public class HuntInventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Image _panelBackground;

        [Header("Text Formatting")]
        [SerializeField] private string _progressFormat = "Targets: {0}/{1}";
        [SerializeField] private string _completedFormat = "Targets: {0}/{1} (Return to Ship!)";

        [Header("Visual Feedback")]
        [SerializeField] private Color _normalTextColor = Color.white;
        [SerializeField] private Color _completedTextColor = new Color(0.2f, 1f, 0.4f);
        [SerializeField] private float _punchScale = 1.25f;
        [SerializeField] private float _punchDuration = 0.2f;

        private Coroutine _punchRoutine;
        private Vector3 _originalScale = Vector3.one;

        private void Awake()
        {
            if (_progressText != null)
            {
                _originalScale = _progressText.transform.localScale;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnCorrectCapture += HandleCorrectCapture;
            GameEvents.OnMissionComplete += HandleMissionComplete;

            if (InventoryManager.HasInstance)
            {
                InventoryManager.Instance.OnInventoryCleared += RefreshDisplay;
            }

            RefreshDisplay();
        }

        private void OnDisable()
        {
            GameEvents.OnCorrectCapture -= HandleCorrectCapture;
            GameEvents.OnMissionComplete -= HandleMissionComplete;

            if (InventoryManager.HasInstance)
            {
                InventoryManager.Instance.OnInventoryCleared -= RefreshDisplay;
            }

            if (_punchRoutine != null)
            {
                StopCoroutine(_punchRoutine);
                _punchRoutine = null;
                if (_progressText != null)
                {
                    _progressText.transform.localScale = _originalScale;
                }
            }
        }

        private void Start()
        {
            RefreshDisplay();
        }

        private void HandleCorrectCapture(TargetData data)
        {
            RefreshDisplay();
            TriggerPunchAnimation();
        }

        private void HandleMissionComplete()
        {
            RefreshDisplay();
            TriggerPunchAnimation();
        }

        public void RefreshDisplay()
        {
            if (_progressText == null) return;

            var current = 0;
            var total = 0;
            var isComplete = false;

            if (MissionManager.HasInstance)
            {
                current = MissionManager.Instance.CurrentIndex;
                total = MissionManager.Instance.RequiredCount;
                isComplete = MissionManager.Instance.IsComplete;
            }

            if (isComplete)
            {
                _progressText.text = string.Format(_completedFormat, current, total);
                _progressText.color = _completedTextColor;
            }
            else
            {
                _progressText.text = string.Format(_progressFormat, current, total);
                _progressText.color = _normalTextColor;
            }
        }

        private void TriggerPunchAnimation()
        {
            if (_progressText == null || !gameObject.activeInHierarchy) return;

            if (_punchRoutine != null)
            {
                StopCoroutine(_punchRoutine);
            }

            _punchRoutine = StartCoroutine(PunchScaleRoutine());
        }

        private IEnumerator PunchScaleRoutine()
        {
            var targetTransform = _progressText.transform;
            var punchVector = _originalScale * _punchScale;
            var halfDuration = _punchDuration * 0.5f;

            var elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / halfDuration);
                targetTransform.localScale = Vector3.Lerp(_originalScale, punchVector, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / halfDuration);
                targetTransform.localScale = Vector3.Lerp(punchVector, _originalScale, t);
                yield return null;
            }

            targetTransform.localScale = _originalScale;
            _punchRoutine = null;
        }
    }
}
