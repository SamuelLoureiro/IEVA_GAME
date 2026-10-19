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

        [Header("Colors")]
        [SerializeField] private float _lowTimeThreshold = 15f;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _lowTimeColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private bool _pulseOnLowTime = true;

        [Header("Popups")]
        [SerializeField] private GameObject _popupPrefab;
        [SerializeField] private Transform _popupContainer;

        [Header("Settings")]
        [SerializeField] private bool _autoStartCaptureTimer = true;
        [SerializeField] private float _defaultCaptureDuration = 90f;

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
        }

        private void Start()
        {
            if (_autoStartCaptureTimer && TimerManager.HasInstance && !TimerManager.Instance.IsRunning(TimerPhase.Capture))
            {
                var dur = GameManager.HasInstance && GameManager.Instance.GetCurrentLevelConfig() != null
                    ? GameManager.Instance.GetCurrentLevelConfig().CaptureTime
                    : _defaultCaptureDuration;
                TimerManager.Instance.StartTimer(TimerPhase.Capture, dur > 0 ? dur : _defaultCaptureDuration);
            }
            RefreshAll();
        }

        private void HandleTimeChanged(TimerPhase phase, float remaining)
        {
            if (phase == TimerPhase.Capture) UpdateTimerText(remaining);
        }

        private void HandlePenaltyApplied(TimerPhase phase, float seconds)
        {
            if (_popupPrefab != null)
            {
                var container = _popupContainer != null ? _popupContainer : transform;
                var popup = Instantiate(_popupPrefab, container).GetComponent<TimeDeltaPopup>();
                if (popup != null) popup.Show($"-{seconds:0}s");
            }
            RefreshDebtDisplay();
        }

        private void HandleTimeLowEvent(TimerPhase phase)
        {
            if (phase == TimerPhase.Capture && _timerText != null) _timerText.color = _lowTimeColor;
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
            var clamped = Mathf.Max(0f, remaining);
            _timerText.text = $"{Mathf.FloorToInt(clamped / 60f):00}:{Mathf.FloorToInt(clamped % 60f):00}";
            _timerText.color = (remaining <= _lowTimeThreshold && remaining > 0f) ? _lowTimeColor : _normalColor;
        }

        public void RefreshDebtDisplay()
        {
            if (_debtText == null) return;
            var debt = TimerManager.HasInstance ? TimerManager.Instance.SortingDebt : 0f;
            _debtText.gameObject.SetActive(debt > 0f);
            if (debt > 0f) _debtText.text = $"Debt: -{debt:0}s";
        }
    }
}
