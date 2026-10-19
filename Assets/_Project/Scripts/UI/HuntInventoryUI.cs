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

        [Header("Visual Feedback")]
        [SerializeField] private Color _normalTextColor = Color.white;
        [SerializeField] private Color _completedTextColor = new Color(0.2f, 1f, 0.4f);

        private void OnEnable()
        {
            GameEvents.OnCorrectCapture += HandleCorrectCapture;
            GameEvents.OnMissionComplete += HandleMissionComplete;
            if (InventoryManager.HasInstance) InventoryManager.Instance.OnInventoryCleared += RefreshDisplay;
            RefreshDisplay();
        }

        private void OnDisable()
        {
            GameEvents.OnCorrectCapture -= HandleCorrectCapture;
            GameEvents.OnMissionComplete -= HandleMissionComplete;
            if (InventoryManager.HasInstance) InventoryManager.Instance.OnInventoryCleared -= RefreshDisplay;
        }

        private void Start() => RefreshDisplay();

        private void HandleCorrectCapture(TargetData data) => RefreshDisplay();
        private void HandleMissionComplete() => RefreshDisplay();

        public void RefreshDisplay()
        {
            if (_progressText == null) return;

            var current = MissionManager.HasInstance ? MissionManager.Instance.CurrentIndex : 0;
            var total = MissionManager.HasInstance ? MissionManager.Instance.RequiredCount : 0;
            var isComplete = MissionManager.HasInstance && MissionManager.Instance.IsComplete;

            _progressText.text = isComplete
                ? $"Targets: {current}/{total} (Return to Ship!)"
                : $"Targets: {current}/{total}";
            _progressText.color = isComplete ? _completedTextColor : _normalTextColor;
        }
    }
}
