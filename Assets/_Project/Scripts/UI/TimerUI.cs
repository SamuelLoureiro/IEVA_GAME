using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Core;
using VARG.Data;
using VARG.Mission;

namespace VARG.UI
{
    public class TimerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _debtText;
        [SerializeField] private Image _timerBackground;

        [Header("Colors & Low Time")]
        [SerializeField] private float _lowTimeThreshold = 15f;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _lowTimeColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private bool _pulseOnLowTime = true;

        [Header("Popups")]
        [SerializeField] private GameObject _popupPrefab;
        [SerializeField] private Transform _popupContainer;

        [Header("Auto Start Capture Timer (A12)")]
        [SerializeField] private bool _autoStartCaptureTimer = true;
        [SerializeField] private float _defaultCaptureDuration = 90f;

        private Coroutine _pulseRoutine;
        private bool _isLowTime;

        private void OnEnable()
        {
            if (TimerManager.HasInstance)
            {
                TimerManager.Instance.OnTimeChanged += HandleTimeChanged;
                TimerManager.Instance.OnPenaltyApplied += HandlePenaltyApplied;
            }

            GameEvents.OnTimeLow += HandleTimeLowEvent;

            RefreshAll();
        }

        private void OnDisable()
        {
            if (TimerManager.HasInstance)
            {
                TimerManager.Instance.OnTimeChanged -= HandleTimeChanged;
                TimerManager.Instance.OnPenaltyApplied -= HandlePenaltyApplied;
            }

            GameEvents.OnTimeLow -= HandleTimeLowEvent;

            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
                _pulseRoutine = null;
            }
        }

        private void Start()
        {
            InitializeTimerIfNeeded();
            RefreshAll();
        }

        private void InitializeTimerIfNeeded()
        {
            if (!_autoStartCaptureTimer || !TimerManager.HasInstance) return;

            var timerManager = TimerManager.Instance;
            if (!timerManager.IsRunning(TimerPhase.Capture))
            {
                var duration = _defaultCaptureDuration;
                if (GameManager.HasInstance)
                {
                    var levelConfig = GameManager.Instance.GetCurrentLevelConfig();
                    if (levelConfig != null && levelConfig.CaptureTime > 0f)
                    {
                        duration = levelConfig.CaptureTime;
                    }
                }

                timerManager.StartTimer(TimerPhase.Capture, duration);
            }
        }

        private void HandleTimeChanged(TimerPhase phase, float remaining)
        {
            if (phase != TimerPhase.Capture) return;

            UpdateTimerText(remaining);
        }

        private void HandlePenaltyApplied(TimerPhase phase, float seconds)
        {
            SpawnPenaltyPopup(phase, seconds);
            RefreshDebtDisplay();
        }

        private void HandleTimeLowEvent(TimerPhase phase)
        {
            if (phase == TimerPhase.Capture)
            {
                SetLowTimeState(true);
            }
        }

        public void RefreshAll()
        {
            var remaining = TimerManager.HasInstance ? TimerManager.Instance.GetRemaining(TimerPhase.Capture) : _defaultCaptureDuration;
            UpdateTimerText(remaining);
            RefreshDebtDisplay();
        }

        private void UpdateTimerText(float remaining)
        {
            if (_timerText == null) return;

            _timerText.text = FormatTime(remaining);

            var isLow = remaining <= _lowTimeThreshold && remaining > 0f;
            SetLowTimeState(isLow);
        }

        private void SetLowTimeState(bool isLow)
        {
            if (_isLowTime == isLow) return;
            _isLowTime = isLow;

            if (_isLowTime)
            {
                if (_timerText != null) _timerText.color = _lowTimeColor;
                if (_pulseOnLowTime && _pulseRoutine == null && gameObject.activeInHierarchy)
                {
                    _pulseRoutine = StartCoroutine(PulseRoutine());
                }
            }
            else
            {
                if (_timerText != null) _timerText.color = _normalColor;
                if (_pulseRoutine != null)
                {
                    StopCoroutine(_pulseRoutine);
                    _pulseRoutine = null;
                    if (_timerText != null) _timerText.transform.localScale = Vector3.one;
                }
            }
        }

        public void RefreshDebtDisplay()
        {
            if (_debtText == null) return;

            var debt = TimerManager.HasInstance ? TimerManager.Instance.SortingDebt : 0f;
            if (debt > 0f)
            {
                _debtText.gameObject.SetActive(true);
                _debtText.text = $"Debt: -{debt:0}s";
            }
            else
            {
                _debtText.gameObject.SetActive(false);
            }
        }

        private void SpawnPenaltyPopup(TimerPhase phase, float seconds)
        {
            if (_popupPrefab == null) return;

            var parent = _popupContainer != null ? _popupContainer : transform;
            var popupInstance = Instantiate(_popupPrefab, parent);
            var popup = popupInstance.GetComponent<TimeDeltaPopup>();
            if (popup != null)
            {
                var label = phase == TimerPhase.Sorting ? $"-{seconds:0}s (Debt)" : $"-{seconds:0}s";
                popup.Show(label);
            }
        }

        private static string FormatTime(float totalSeconds)
        {
            var clamped = Mathf.Max(0f, totalSeconds);
            var minutes = Mathf.FloorToInt(clamped / 60f);
            var seconds = Mathf.FloorToInt(clamped % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        private IEnumerator PulseRoutine()
        {
            var target = _timerText != null ? _timerText.transform : transform;
            var baseScale = Vector3.one;
            var targetScale = baseScale * 1.15f;

            while (_isLowTime)
            {
                var elapsed = 0f;
                var pulseDuration = 0.5f;

                while (elapsed < pulseDuration)
                {
                    elapsed += Time.deltaTime;
                    var t = Mathf.PingPong(elapsed * 2f, 1f);
                    target.localScale = Vector3.Lerp(baseScale, targetScale, t);
                    yield return null;
                }
            }

            target.localScale = baseScale;
            _pulseRoutine = null;
        }
    }
}
