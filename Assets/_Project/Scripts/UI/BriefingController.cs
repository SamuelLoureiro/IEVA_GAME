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

        private void Start()
        {
            if (_spreadsheetButton != null) _spreadsheetButton.onClick.AddListener(OnLoadSpreadsheetLevelClicked);
            InitializeBriefing();
        }

        public void Configure(DialogueBox dialogueBox, TargetShowcaseUI showcaseUI, TextMeshProUGUI levelTitleText,
            GameObject bossAvatar = null, Button spreadsheetButton = null, TextMeshProUGUI spreadsheetButtonText = null)
        {
            _dialogueBox = dialogueBox;
            _showcaseUI = showcaseUI;
            _levelTitleText = levelTitleText;
            _bossAvatar = bossAvatar;
            _spreadsheetButton = spreadsheetButton;
            _spreadsheetButtonText = spreadsheetButtonText;
        }

        private void InitializeBriefing()
        {
            var level = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1;
            var config = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevelConfig() : null;

            if (MissionManager.Instance != null && MissionManager.Instance.RequiredCount == 0)
            {
                MissionManager.Instance.GenerateMission(config);
            }

            if (_levelTitleText != null)
            {
                _levelTitleText.text = GameManager.Instance != null && GameManager.Instance.IsCustomSpreadsheetLevel
                    ? "CAPTURE MISSION  |  CUSTOM SPREADSHEET LEVEL"
                    : $"CAPTURE MISSION  |  LEVEL {level}";
            }

            if (_dialogueBox != null)
            {
                var forbidden = MissionManager.Instance != null ? MissionManager.Instance.ForbiddenType.ToString().ToUpper() : "HUMAN";
                var text = $"Rookie! Memorize the target order! Do NOT capture {forbidden}!";
                _dialogueBox.Say(_bossSpeakerName, text, StartShowcasePhase);
            }
            else
            {
                StartShowcasePhase();
            }
        }

        private void StartShowcasePhase()
        {
            if (_showcaseUI != null)
            {
                var order = MissionManager.Instance != null ? MissionManager.Instance.RequiredOrder : null;
                var count = order != null ? order.Count : 3;
                var duration = Mathf.Max(_minMemoryDuration, count * _secondsPerItem);

                _showcaseUI.DisplayShowcase(order);
                _showcaseUI.StartCountdown(duration, ProceedToHunt);
            }
        }

        public void OnLoadSpreadsheetLevelClicked()
        {
            if (!SpreadsheetLevelLoader.TryLoadCustomLevel(out var levelData, out var status))
            {
                if (_dialogueBox != null) _dialogueBox.Say(_bossSpeakerName, $"CSV Error: {status}");
                return;
            }

            var customConfig = ScriptableObject.CreateInstance<LevelConfig>();
            customConfig.Setup(levelData.TargetSequence.Count, levelData.CaptureTime, levelData.SortingTime,
                levelData.ExtraSpawns, levelData.ForbiddenSpawns, levelData.FleeSpeedMultiplier);

            GameManager.Instance?.SetCustomSpreadsheetLevel(customConfig);
            MissionManager.Instance?.LoadCustomMission(levelData.TargetSequence, levelData.ForbiddenTarget);

            if (_levelTitleText != null) _levelTitleText.text = "CAPTURE MISSION  |  CUSTOM SPREADSHEET LEVEL";
            if (_spreadsheetButtonText != null) _spreadsheetButtonText.text = "✔ CSV ACTIVE";

            StartShowcasePhase();
        }

        public void ProceedToHunt()
        {
            if (_showcaseUI != null) _showcaseUI.HideShowcase();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToHunt();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("03_CityHunt");
            }
        }
    }
}
