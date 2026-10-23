using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Core;
using VARG.Data;

namespace VARG.UI
{
    public class ResultsScreen : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;

        [Header("Metrics")]
        [SerializeField] private TextMeshProUGUI _captureTimeText;
        [SerializeField] private TextMeshProUGUI _sortingTimeText;
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private TextMeshProUGUI _errorsText;
        [SerializeField] private TextMeshProUGUI _scoreText;

        [Header("Bars")]
        [SerializeField] private Image _captureTimeBar;
        [SerializeField] private Image _sortingTimeBar;
        [SerializeField] private Image _livesBar;
        [SerializeField] private Image _accuracyBar;

        [Header("Buttons")]
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        public LevelResult CurrentResult { get; private set; }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_mainMenuButton == null && _retryButton != null)
            {
                _mainMenuButton = Instantiate(_retryButton, _retryButton.transform.parent);
                _mainMenuButton.name = "Button_MainMenu";
                var label = _mainMenuButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = "MAIN MENU";
            }

            if (_nextLevelButton != null) _nextLevelButton.onClick.AddListener(() => GameManager.Instance?.NextLevel());
            if (_retryButton != null) _retryButton.onClick.AddListener(() => GameManager.Instance?.RetryLevel());
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());

            DisplayResults();
        }

        public void DisplayResults()
        {
            var hasResult = GameManager.HasInstance && GameManager.Instance.LastResult.Score >= 0;
            CurrentResult = hasResult ? GameManager.Instance.LastResult : new LevelResult(52f, 28f, 4, 1, 1200);

            var isVictory = GameManager.HasInstance ? GameManager.Instance.IsVictory : CurrentResult.RemainingLives > 0;
            var level = GameManager.HasInstance ? GameManager.Instance.CurrentLevel : 1;
            var isCustom = GameManager.HasInstance && GameManager.Instance.IsCustomSpreadsheetLevel;

            if (_titleText != null)
            {
                _titleText.text = isVictory ? "MISSION COMPLETE!" : "MISSION FAILED!";
                _titleText.color = isVictory ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.25f, 0.25f);
            }

            if (_subtitleText != null)
            {
                if (isCustom)
                {
                    _subtitleText.text = isVictory
                        ? "Custom Spreadsheet Mission Complete! Quota Achieved!"
                        : "Custom Spreadsheet Mission - Failed";
                }
                else
                {
                    _subtitleText.text = isVictory
                        ? (level >= GameManager.MaxLevel ? "Campaign Complete! You are the Master of Abduction!" : $"Level {level} Performance Summary")
                        : $"Level {level} - Mission Failed";
                }
            }

            if (_captureTimeText != null) _captureTimeText.text = FormatTime(CurrentResult.RemainingCaptureTime);
            if (_sortingTimeText != null) _sortingTimeText.text = FormatTime(CurrentResult.RemainingSortingTime);
            if (_livesText != null) _livesText.text = $"{CurrentResult.RemainingLives} / 5";
            if (_errorsText != null) _errorsText.text = $"{CurrentResult.ErrorsCount}";
            if (_scoreText != null) _scoreText.text = $"{CurrentResult.Score:N0} PTS";

            if (_captureTimeBar != null) _captureTimeBar.fillAmount = Mathf.Clamp01(CurrentResult.RemainingCaptureTime / 90f);
            if (_sortingTimeBar != null) _sortingTimeBar.fillAmount = Mathf.Clamp01(CurrentResult.RemainingSortingTime / 45f);
            if (_livesBar != null) _livesBar.fillAmount = Mathf.Clamp01(CurrentResult.RemainingLives / 5f);
            if (_accuracyBar != null) _accuracyBar.fillAmount = Mathf.Max(0f, 1f - (CurrentResult.ErrorsCount * 0.25f));

            var showNextLevel = isVictory && !isCustom;
            if (_nextLevelButton != null) _nextLevelButton.gameObject.SetActive(showNextLevel);
            if (_retryButton != null) _retryButton.gameObject.SetActive(true);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);

            UpdateButtonPositions(showNextLevel);
        }

        private void UpdateButtonPositions(bool showNextLevel)
        {
            if (_mainMenuButton == null || _retryButton == null) return;

            var menuRect = _mainMenuButton.GetComponent<RectTransform>();
            var retryRect = _retryButton.GetComponent<RectTransform>();

            if (showNextLevel && _nextLevelButton != null)
            {
                var nextRect = _nextLevelButton.GetComponent<RectTransform>();
                if (menuRect != null) menuRect.anchoredPosition = new Vector2(-235f, 0f);
                if (retryRect != null) retryRect.anchoredPosition = new Vector2(0f, 0f);
                if (nextRect != null) nextRect.anchoredPosition = new Vector2(235f, 0f);
            }
            else
            {
                if (menuRect != null) menuRect.anchoredPosition = new Vector2(-120f, 0f);
                if (retryRect != null) retryRect.anchoredPosition = new Vector2(120f, 0f);
            }
        }

        private static string FormatTime(float sec)
        {
            var c = Mathf.Max(0f, sec);
            return $"{Mathf.FloorToInt(c / 60f):00}:{Mathf.FloorToInt(c % 60f):00}";
        }
    }
}
