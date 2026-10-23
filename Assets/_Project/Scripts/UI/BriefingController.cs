using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Core;
using VARG.Data;
using VARG.Mission;

namespace VARG.UI
{
    public class BriefingController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private DialogueBox _dialogueBox;
        [SerializeField] private TargetShowcaseUI _showcaseUI;
        [SerializeField] private TextMeshProUGUI _levelTitleText;
        [SerializeField] private GameObject _bossAvatar;

        [Header("Spreadsheet Level")]
        [SerializeField] private Button _spreadsheetButton;
        [SerializeField] private TextMeshProUGUI _spreadsheetButtonText;

        [Header("Settings")]
        [SerializeField] private float _secondsPerItem = 3.0f;
        [SerializeField] private float _minMemoryDuration = 8.0f;
        [SerializeField] private string _bossSpeakerName = "COMMANDER ZORP";

        private bool _isBossInCorner;

        private void Start()
        {
            EnsureSpreadsheetButton();
            InitializeBriefing();
        }

        private void InitializeBriefing()
        {
            var currentLevel = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1;
            var levelConfig = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevelConfig() : null;

            if (MissionManager.Instance != null && MissionManager.Instance.RequiredCount == 0)
            {
                MissionManager.Instance.GenerateMission(levelConfig);
            }

            if (_levelTitleText != null)
            {
                _levelTitleText.text = GameManager.Instance != null && GameManager.Instance.IsCustomSpreadsheetLevel
                    ? "CAPTURE MISSION  |  CUSTOM SPREADSHEET LEVEL"
                    : $"CAPTURE MISSION  |  LEVEL {currentLevel}";
            }

            if (_spreadsheetButtonText != null && GameManager.Instance != null && GameManager.Instance.IsCustomSpreadsheetLevel)
            {
                _spreadsheetButtonText.text = "<b>[SPREADSHEET ACTIVE]</b>";
            }

            if (_showcaseUI != null)
            {
                _showcaseUI.gameObject.SetActive(false);
            }

            var openingLine = GetBossOpeningLine(currentLevel);

            if (_dialogueBox != null)
            {
                _dialogueBox.Say(_bossSpeakerName, openingLine, OnBossFinishedSpeaking);
            }
            else
            {
                OnBossFinishedSpeaking();
            }
        }

        private string GetBossOpeningLine(int level)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsCustomSpreadsheetLevel)
            {
                return "Attention recruit! Custom mission via spreadsheet authorized by headquarters! Memorize the sequence and do not dare capture the forbidden target!";
            }

            if (level == 1)
            {
                return "Attention, rookie! Here are the targets you must capture on Earth and their exact order. Memorize the sequence before deploying the ship! Do NOT capture the forbidden target under any circumstances!";
            }

            return $"New mission authorized for Level {level}! The target quota has increased. Memorize the sequence carefully and don't waste my time!";
        }

        private void OnBossFinishedSpeaking()
        {
            StartCoroutine(TransitionBossToCornerRoutine());
        }

        private System.Collections.IEnumerator TransitionBossToCornerRoutine()
        {
            if (_bossAvatar != null && _bossAvatar.TryGetComponent<RectTransform>(out var rect))
            {
                rect.anchoredPosition = new Vector2(-620f, -160f);
                rect.localScale = new Vector3(0.55f, 0.55f, 1f);
            }

            _isBossInCorner = true;

            if (_dialogueBox != null)
            {
                var forbiddenStr = MissionManager.Instance != null ? MissionManager.Instance.ForbiddenType.ToString().ToUpper() : "HUMAN";
                var prompt = $"Here is your target sequence. Memorize the exact order! Watch out: {forbiddenStr} is FORBIDDEN!";
                _dialogueBox.Say(_bossSpeakerName, prompt, OnStage2Finished);
            }

            if (_showcaseUI != null)
            {
                var order = MissionManager.Instance != null ? MissionManager.Instance.RequiredOrder : null;
                _showcaseUI.DisplayShowcase(order);
            }

            yield break;
        }

        private void EnsureSpreadsheetButton()
        {
            if (_spreadsheetButton != null)
            {
                _spreadsheetButton.onClick.RemoveListener(OnLoadSpreadsheetLevelClicked);
                _spreadsheetButton.onClick.AddListener(OnLoadSpreadsheetLevelClicked);
                return;
            }

            var canvas = _levelTitleText != null ? _levelTitleText.canvas : FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var buttonObj = new GameObject("SpreadsheetButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(canvas.transform, false);

            var rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-24f, -16f);
            rect.sizeDelta = new Vector2(280f, 42f);

            buttonObj.GetComponent<Image>().color = new Color(0.12f, 0.22f, 0.32f, 0.95f);
            _spreadsheetButton = buttonObj.GetComponent<Button>();

            var textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(buttonObj.transform, false);

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            _spreadsheetButtonText = textObj.GetComponent<TextMeshProUGUI>();
            _spreadsheetButtonText.text = "<b>[SPREADSHEET LEVEL]</b>";
            _spreadsheetButtonText.fontSize = 15;
            _spreadsheetButtonText.alignment = TextAlignmentOptions.Center;
            _spreadsheetButtonText.color = new Color(0.40f, 0.88f, 0.95f, 1f);

            _spreadsheetButton.onClick.AddListener(OnLoadSpreadsheetLevelClicked);
        }

        public void OnLoadSpreadsheetLevelClicked()
        {
            if (!SpreadsheetLevelLoader.TryLoadCustomLevel(out var levelData, out var statusMessage))
            {
                _dialogueBox?.Say(_bossSpeakerName, $"Spreadsheet error: {statusMessage}", null);
                return;
            }

            var customConfig = ScriptableObject.CreateInstance<LevelConfig>();
            customConfig.Setup(
                levelData.TargetSequence.Count,
                levelData.CaptureTime,
                levelData.SortingTime,
                levelData.ExtraSpawns,
                levelData.ForbiddenSpawns,
                levelData.FleeSpeedMultiplier
            );

            GameManager.Instance?.SetCustomSpreadsheetLevel(customConfig);
            MissionManager.Instance?.LoadCustomMission(levelData.TargetSequence, levelData.ForbiddenTarget);

            if (_levelTitleText != null) _levelTitleText.text = "CAPTURE MISSION  |  CUSTOM SPREADSHEET LEVEL";
            if (_spreadsheetButtonText != null) _spreadsheetButtonText.text = "<b>[SPREADSHEET LOADED]</b>";

            if (!_isBossInCorner)
            {
                StopAllCoroutines();
                StartCoroutine(TransitionBossToCornerRoutine());
            }
            else
            {
                if (_showcaseUI != null)
                {
                    _showcaseUI.gameObject.SetActive(true);
                    _showcaseUI.DisplayShowcase(MissionManager.Instance?.RequiredOrder);
                    var count = MissionManager.Instance != null && MissionManager.Instance.RequiredCount > 0
                        ? MissionManager.Instance.RequiredCount
                        : levelData.TargetSequence.Count;
                    _showcaseUI.StartCountdown(Mathf.Max(_minMemoryDuration, count * _secondsPerItem), ProceedToHunt);
                }

                _dialogueBox?.Say(_bossSpeakerName, $"Spreadsheet approved! Memorize the {levelData.TargetSequence.Count} targets! {levelData.ForbiddenTarget.ToString().ToUpper()} remains FORBIDDEN!", null);
            }
        }

        private void OnStage2Finished()
        {
            var levelConfig = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevelConfig() : null;
            var count = MissionManager.Instance != null && MissionManager.Instance.RequiredCount > 0
                ? MissionManager.Instance.RequiredCount
                : (levelConfig != null ? levelConfig.ListSize : 3);
            var memoryDuration = Mathf.Max(_minMemoryDuration, count * _secondsPerItem);

            if (_dialogueBox != null)
            {
                _dialogueBox.Say(_bossSpeakerName, "Memorize the sequence now! Deploy when ready, or time will run out!", null);
            }

            if (_showcaseUI != null)
            {
                _showcaseUI.StartCountdown(memoryDuration, ProceedToHunt);
            }
        }

        public void ProceedToHunt()
        {
            if (_showcaseUI != null)
            {
                _showcaseUI.HideShowcase();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToHunt();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load("03_CityHunt");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("03_CityHunt");
            }
        }
    }
}
