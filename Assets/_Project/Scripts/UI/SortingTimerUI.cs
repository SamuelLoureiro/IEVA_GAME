using TMPro;
using UnityEngine;
using VARG.Data;
using VARG.Mission;
using VARG.Sorting;

namespace VARG.UI
{
    public class SortingTimerUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _itemsCountText;

        [Header("Appearance")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _warningColor = new Color(1f, 0.28f, 0.28f);
        [SerializeField] private float _warningThreshold = 15f;

        private void OnEnable()
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.OnTimeChanged += HandleTimeChanged;
                TimerManager.Instance.OnPenaltyApplied += HandlePenaltyApplied;
            }

            if (SortingManager.Instance != null)
            {
                SortingManager.Instance.OnItemCountsChanged += HandleItemCountsChanged;
            }
        }

        private void OnDisable()
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.OnTimeChanged -= HandleTimeChanged;
                TimerManager.Instance.OnPenaltyApplied -= HandlePenaltyApplied;
            }

            if (SortingManager.Instance != null)
            {
                SortingManager.Instance.OnItemCountsChanged -= HandleItemCountsChanged;
            }
        }

        private void Start()
        {
            RefreshInitialState();
        }

        public void Configure(TextMeshProUGUI timeText, TextMeshProUGUI itemsCountText)
        {
            _timeText = timeText;
            _itemsCountText = itemsCountText;
        }

        private void RefreshInitialState()
        {
            if (TimerManager.Instance != null)
            {
                var remaining = TimerManager.Instance.GetRemaining(TimerPhase.Sorting);
                UpdateTimeDisplay(remaining);
            }

            if (SortingManager.Instance != null)
            {
                HandleItemCountsChanged(SortingManager.Instance.RemainingItems, SortingManager.Instance.TotalItems);
            }
        }

        private void HandleTimeChanged(TimerPhase phase, float remaining)
        {
            if (phase != TimerPhase.Sorting) return;
            UpdateTimeDisplay(remaining);
        }

        private void HandlePenaltyApplied(TimerPhase phase, float penalty)
        {
            if (phase != TimerPhase.Sorting) return;
            // Visual feedback on penalty
            if (_timeText != null)
            {
                _timeText.color = _warningColor;
            }
        }

        private void UpdateTimeDisplay(float remaining)
        {
            if (_timeText == null) return;

            var totalSeconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;

            _timeText.text = $"{minutes:00}:{seconds:00}";
            _timeText.color = remaining <= _warningThreshold ? _warningColor : _normalColor;
        }

        private void HandleItemCountsChanged(int remaining, int total)
        {
            if (_itemsCountText == null) return;
            _itemsCountText.text = $"Targets: {remaining}/{total}";
        }
    }
}
