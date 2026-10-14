using System.Collections;
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

        [Header("Metrics Texts")]
        [SerializeField] private TextMeshProUGUI _captureTimeText;
        [SerializeField] private TextMeshProUGUI _sortingTimeText;
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private TextMeshProUGUI _errorsText;
        [SerializeField] private TextMeshProUGUI _scoreText;

        [Header("Chart Bars")]
        [SerializeField] private Image _captureTimeBar;
        [SerializeField] private Image _sortingTimeBar;
        [SerializeField] private Image _livesBar;
        [SerializeField] private Image _accuracyBar;

        [Header("Buttons")]
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _retryButton;

        [Header("Settings & Colors")]
        [SerializeField] private Color _victoryTitleColor = new Color(0.2f, 1f, 0.4f);
        [SerializeField] private Color _defeatTitleColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private string _victoryTitle = "MISSION COMPLETE!";
        [SerializeField] private string _defeatTitle = "MISSION FAILED!";
        [SerializeField] private float _chartAnimDuration = 0.8f;

        private Coroutine _barAnimRoutine;

        public LevelResult CurrentResult { get; private set; }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SetupButtonListeners();
            DisplayResults();
        }

        private void OnDestroy()
        {
            if (_nextLevelButton != null)
            {
                _nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            }

            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveListener(OnRetryClicked);
            }

            if (_barAnimRoutine != null)
            {
                StopCoroutine(_barAnimRoutine);
                _barAnimRoutine = null;
            }
        }

        public void DisplayResults()
        {
            var hasResult = GameManager.HasInstance && GameManager.Instance.LastResult.Score >= 0;
            CurrentResult = hasResult ? GameManager.Instance.LastResult : GetSampleResult();

            var isVictory = CurrentResult.RemainingLives > 0;

            if (_titleText != null)
            {
                _titleText.text = isVictory ? _victoryTitle : _defeatTitle;
                _titleText.color = isVictory ? _victoryTitleColor : _defeatTitleColor;
            }

            if (_subtitleText != null)
            {
                var levelNumber = GameManager.HasInstance ? GameManager.Instance.CurrentLevel : 1;
                _subtitleText.text = $"Level {levelNumber} Performance Summary";
            }

            if (_captureTimeText != null)
            {
                _captureTimeText.text = FormatTime(CurrentResult.RemainingCaptureTime);
            }

            if (_sortingTimeText != null)
            {
                _sortingTimeText.text = FormatTime(CurrentResult.RemainingSortingTime);
            }

            if (_livesText != null)
            {
                _livesText.text = $"{CurrentResult.RemainingLives} / 5";
            }

            if (_errorsText != null)
            {
                _errorsText.text = $"{CurrentResult.ErrorsCount}";
            }

            if (_scoreText != null)
            {
                _scoreText.text = $"{CurrentResult.Score:N0} PTS";
            }

            // Calculate fill percentages for the chart
            var captureFill = Mathf.Clamp01(CurrentResult.RemainingCaptureTime / 90f);
            var sortingFill = Mathf.Clamp01(CurrentResult.RemainingSortingTime / 45f);
            var livesFill = Mathf.Clamp01(CurrentResult.RemainingLives / 5f);
            var accuracyFill = Mathf.Max(0f, 1f - (CurrentResult.ErrorsCount * 0.25f));

            if (gameObject.activeInHierarchy)
            {
                if (_barAnimRoutine != null) StopCoroutine(_barAnimRoutine);
                _barAnimRoutine = StartCoroutine(AnimateBarsRoutine(captureFill, sortingFill, livesFill, accuracyFill));
            }
            else
            {
                SetBarsFillImmediate(captureFill, sortingFill, livesFill, accuracyFill);
            }

            // Buttons visibility
            if (_nextLevelButton != null)
            {
                _nextLevelButton.gameObject.SetActive(isVictory);
            }

            if (_retryButton != null)
            {
                _retryButton.gameObject.SetActive(true);
            }
        }

        private void SetupButtonListeners()
        {
            if (_nextLevelButton != null)
            {
                _nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }

            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(OnRetryClicked);
            }
        }

        private void OnNextLevelClicked()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.NextLevel();
            }
        }

        private void OnRetryClicked()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.RetryLevel();
            }
        }

        private void SetBarsFillImmediate(float captureFill, float sortingFill, float livesFill, float accuracyFill)
        {
            if (_captureTimeBar != null) _captureTimeBar.fillAmount = captureFill;
            if (_sortingTimeBar != null) _sortingTimeBar.fillAmount = sortingFill;
            if (_livesBar != null) _livesBar.fillAmount = livesFill;
            if (_accuracyBar != null) _accuracyBar.fillAmount = accuracyFill;
        }

        private IEnumerator AnimateBarsRoutine(float targetCapture, float targetSorting, float targetLives, float targetAccuracy)
        {
            var elapsed = 0f;

            SetBarsFillImmediate(0f, 0f, 0f, 0f);

            while (elapsed < _chartAnimDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / _chartAnimDuration);
                var smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (_captureTimeBar != null) _captureTimeBar.fillAmount = targetCapture * smoothT;
                if (_sortingTimeBar != null) _sortingTimeBar.fillAmount = targetSorting * smoothT;
                if (_livesBar != null) _livesBar.fillAmount = targetLives * smoothT;
                if (_accuracyBar != null) _accuracyBar.fillAmount = targetAccuracy * smoothT;

                yield return null;
            }

            SetBarsFillImmediate(targetCapture, targetSorting, targetLives, targetAccuracy);
            _barAnimRoutine = null;
        }

        private static string FormatTime(float totalSeconds)
        {
            var clamped = Mathf.Max(0f, totalSeconds);
            var minutes = Mathf.FloorToInt(clamped / 60f);
            var seconds = Mathf.FloorToInt(clamped % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        private static LevelResult GetSampleResult()
        {
            return new LevelResult(52f, 28f, 4, 1, 1200);
        }
    }
}
