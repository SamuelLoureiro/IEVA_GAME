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

        private void Awake()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
            }

            var canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 100;
            }

            if (_timeText == null || _itemsCountText == null)
            {
                var tmps = GetComponentsInChildren<TextMeshProUGUI>();
                for (var i = 0; i < tmps.Length; i++)
                {
                    if (tmps[i].name.Contains("Timer") && _timeText == null) _timeText = tmps[i];
                    else if ((tmps[i].name.Contains("Count") || tmps[i].name.Contains("Target")) && _itemsCountText == null) _itemsCountText = tmps[i];
                }
            }

            EnsureLivesUI();
        }

        private void EnsureLivesUI()
        {
            if (FindFirstObjectByType<LivesUI>() != null) return;

            var panelGo = new GameObject("Panel_Lives", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            panelGo.transform.SetParent(transform, false);

            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(35f, -25f);
            panelRect.sizeDelta = new Vector2(180f, 50f);

            var bg = panelGo.GetComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.06f, 0.08f, 0.11f, 0.88f);

            var textGo = new GameObject("Text_Lives", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(panelGo.transform, false);

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 0f);
            textRect.offsetMax = new Vector2(-16f, 0f);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = new Color(0.2f, 1f, 0.5f);
            tmp.text = "LIVES: 5";

            var livesUI = panelGo.AddComponent<LivesUI>();
            livesUI.Configure(tmp);
        }

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
                _timeText.color = remaining <= 15f && remaining > 0f ? new Color(1f, 0.25f, 0.25f) : Color.white;
            }

            if (_itemsCountText != null && SortingManager.Instance != null)
            {
                _itemsCountText.text = $"Targets: {SortingManager.Instance.RemainingItems}/{SortingManager.Instance.TotalItems}";
            }
        }
    }
}
