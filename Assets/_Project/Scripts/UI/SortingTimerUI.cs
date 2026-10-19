using TMPro;
using UnityEngine;
using VARG.Data;
using VARG.Mission;
using VARG.Sorting;

namespace VARG.UI
{
    public class SortingTimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _itemsCountText;

        public void Configure(TextMeshProUGUI timeText, TextMeshProUGUI itemsCountText)
        {
            _timeText = timeText;
            _itemsCountText = itemsCountText;
        }

        private void Update()
        {
            if (_timeText != null && TimerManager.Instance != null)
            {
                var remaining = Mathf.Max(0f, TimerManager.Instance.GetRemaining(TimerPhase.Sorting));
                _timeText.text = $"{(int)remaining / 60:00}:{(int)remaining % 60:00}";
            }

            if (_itemsCountText != null && SortingManager.Instance != null)
            {
                _itemsCountText.text = $"Targets: {SortingManager.Instance.RemainingItems}/{SortingManager.Instance.TotalItems}";
            }
        }
    }
}
