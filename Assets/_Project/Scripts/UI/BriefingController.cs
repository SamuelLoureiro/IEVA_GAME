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

        public void Configure(
            DialogueBox dialogueBox,
            TargetShowcaseUI showcaseUI,
            TextMeshProUGUI levelTitleText,
            GameObject bossAvatar = null,
            Button spreadsheetButton = null,
            TextMeshProUGUI spreadsheetButtonText = null)
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
                _spreadsheetButtonText.text = "<b>✔ SPREADSHEET ACTIVE</b>";
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
                return "Atenção recruta! Missão customizada via planilha autorizada pela matriz! Memorize a sequência e não ouse capturar o alvo proibido!";
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
            if (_bossAvatar != null)
            {
                var rect = _bossAvatar.GetComponent<RectTransform>();
                if (rect != null)
                {
                    var startPos = rect.anchoredPosition;
                    var targetPos = new Vector2(-620f, -160f);
                    var startScale = rect.localScale;
                    var targetScale = new Vector3(0.55f, 0.55f, 1f);
                    var duration = 0.55f;
                    var elapsed = 0f;

                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                        rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                        rect.localScale = Vector3.Lerp(startScale, targetScale, t);
                        yield return null;
                    }

                    rect.anchoredPosition = targetPos;
                    rect.localScale = targetScale;
                }
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
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-24f, -16f);
            rect.sizeDelta = new Vector2(280f, 42f);

            var image = buttonObj.GetComponent<Image>();
            image.color = new Color(0.12f, 0.22f, 0.32f, 0.95f);

            var button = buttonObj.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.12f, 0.22f, 0.32f, 0.95f);
            colors.highlightedColor = new Color(0.18f, 0.38f, 0.55f, 1f);
            colors.pressedColor = new Color(0.10f, 0.60f, 0.45f, 1f);
            colors.selectedColor = new Color(0.18f, 0.38f, 0.55f, 1f);
            button.colors = colors;
            button.targetGraphic = image;

            var textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(buttonObj.transform, false);

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (_levelTitleText != null)
            {
                tmp.font = _levelTitleText.font;
                tmp.fontSharedMaterial = _levelTitleText.fontSharedMaterial;
            }
            tmp.text = "<b>📄 SPREADSHEET LEVEL</b>";
            tmp.fontSize = 15;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.40f, 0.88f, 0.95f, 1f);

            _spreadsheetButton = button;
            _spreadsheetButtonText = tmp;
            _spreadsheetButton.onClick.AddListener(OnLoadSpreadsheetLevelClicked);
        }

        public void OnLoadSpreadsheetLevelClicked()
        {
            if (SpreadsheetLevelLoader.TryLoadCustomLevel(out var levelData, out var statusMessage))
            {
                var customConfig = ScriptableObject.CreateInstance<LevelConfig>();
                customConfig.Setup(
                    levelData.TargetSequence.Count,
                    levelData.CaptureTime,
                    levelData.SortingTime,
                    levelData.ExtraSpawns,
                    levelData.ForbiddenSpawns,
                    levelData.FleeSpeedMultiplier
                );

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCustomSpreadsheetLevel(customConfig);
                }

                if (MissionManager.Instance != null)
                {
                    MissionManager.Instance.LoadCustomMission(levelData.TargetSequence, levelData.ForbiddenTarget);
                }

                if (_levelTitleText != null)
                {
                    _levelTitleText.text = "CAPTURE MISSION  |  CUSTOM SPREADSHEET LEVEL";
                }

                if (_spreadsheetButtonText != null)
                {
                    _spreadsheetButtonText.text = "<b>✔ SPREADSHEET LOADED</b>";
                }

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
                        var order = MissionManager.Instance != null ? MissionManager.Instance.RequiredOrder : null;
                        _showcaseUI.DisplayShowcase(order);

                        var count = MissionManager.Instance != null && MissionManager.Instance.RequiredCount > 0
                            ? MissionManager.Instance.RequiredCount
                            : levelData.TargetSequence.Count;
                        var memoryDuration = Mathf.Max(_minMemoryDuration, count * _secondsPerItem);
                        _showcaseUI.StartCountdown(memoryDuration, ProceedToHunt);
                    }

                    if (_dialogueBox != null)
                    {
                        var forbiddenStr = levelData.ForbiddenTarget.ToString().ToUpper();
                        var line = $"Planilha da matriz aprovada! Memorize os {levelData.TargetSequence.Count} alvos customizados! CUIDADO: {forbiddenStr} continua PROIBIDO!";
                        _dialogueBox.Say(_bossSpeakerName, line, null);
                    }
                }

                Debug.Log($"[BriefingController] {statusMessage}");
            }
            else
            {
                if (_dialogueBox != null)
                {
                    _dialogueBox.Say(_bossSpeakerName, $"Erro na planilha da matriz: {statusMessage}", null);
                }
                Debug.LogWarning($"[BriefingController] {statusMessage}");
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
