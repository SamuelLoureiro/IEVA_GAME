using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Core;
using VARG.Data;
using VARG.Mission;

namespace VARG.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _playCampaignButton;
        [SerializeField] private Button _playCustomButton;
        [SerializeField] private Button _openSpreadsheetButton;
        [SerializeField] private Button _quitButton;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureMainMenuInBootScene()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            SpawnIfBootScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            SpawnIfBootScene(scene);
        }

        private static void SpawnIfBootScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (scene.name == "00_Boot" && FindFirstObjectByType<MainMenuController>() == null)
            {
                var go = new GameObject("MainMenuController");
                go.AddComponent<MainMenuController>();
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            EnsureMenuUI();
            SetupListeners();

            SpreadsheetLevelLoader.EnsureTemplateExists(out _);
        }

        private void OnDestroy()
        {
            if (_playCampaignButton != null) _playCampaignButton.onClick.RemoveListener(PlayCampaign);
            if (_playCustomButton != null) _playCustomButton.onClick.RemoveListener(PlayCustomSpreadsheet);
            if (_openSpreadsheetButton != null) _openSpreadsheetButton.onClick.RemoveListener(OpenSpreadsheetFile);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(QuitGame);
        }

        private void SetupListeners()
        {
            if (_playCampaignButton != null)
            {
                _playCampaignButton.onClick.RemoveListener(PlayCampaign);
                _playCampaignButton.onClick.AddListener(PlayCampaign);
            }

            if (_playCustomButton != null)
            {
                _playCustomButton.onClick.RemoveListener(PlayCustomSpreadsheet);
                _playCustomButton.onClick.AddListener(PlayCustomSpreadsheet);
            }

            if (_openSpreadsheetButton != null)
            {
                _openSpreadsheetButton.onClick.RemoveListener(OpenSpreadsheetFile);
                _openSpreadsheetButton.onClick.AddListener(OpenSpreadsheetFile);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(QuitGame);
                _quitButton.onClick.AddListener(QuitGame);
            }
        }

        public void PlayCampaign()
        {
            SetStatus("Starting Campaign...", new Color(0.35f, 0.90f, 0.55f));
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCampaign();
            }
            else
            {
                StartBriefing();
            }
        }

        public void PlayCustomSpreadsheet()
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

                GameManager.Instance?.SetCustomSpreadsheetLevel(customConfig);
                MissionManager.Instance?.LoadCustomMission(levelData.TargetSequence, levelData.ForbiddenTarget);

                SetStatus("Spreadsheet loaded! Starting mission...", new Color(0.35f, 0.85f, 1f));
                StartBriefing();
            }
            else
            {
                SetStatus(statusMessage, new Color(1f, 0.45f, 0.45f));
            }
        }

        private void StartBriefing()
        {
            if (GameManager.Instance != null) GameManager.Instance.StartLevel();
            else if (SceneLoader.Instance != null) SceneLoader.Instance.Load("01_BossBriefing");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("01_BossBriefing");
        }

        public void OpenSpreadsheetFile()
        {
            var path = SpreadsheetLevelLoader.GetDefaultFilePath();
            SpreadsheetLevelLoader.EnsureTemplateExists(out path);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.OpenWithDefaultApp(path);
#else
            Application.OpenURL("file://" + path);
#endif

            SetStatus("'CustomLevel.csv' opened in external editor! Edit, save, and click 'PLAY CUSTOM'.", new Color(0.40f, 0.88f, 0.98f));
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetStatus(string message, Color color)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
                _statusText.color = color;
            }
        }

        private void EnsureMenuUI()
        {
            if (_playCampaignButton != null) return;

            var canvasObj = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSys = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            }

            var bgObj = CreatePanel("Background", canvasObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.04f, 0.06f, 0.09f, 1f));

            _titleText = CreateText("Title", bgObj.transform, "PROJECT V.A.R.G.", 52, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.92f, 0.96f, 1f));
            _titleText.rectTransform.anchorMin = new Vector2(0.5f, 0.80f);
            _titleText.rectTransform.anchorMax = new Vector2(0.5f, 0.80f);
            _titleText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _titleText.rectTransform.sizeDelta = new Vector2(1000, 70);

            _subtitleText = CreateText("Subtitle", bgObj.transform, "METHODICAL ABDUCTION  |  MISSIONS & SPREADSHEETS", 20, TextAlignmentOptions.Center, FontStyles.Normal, new Color(0.40f, 0.75f, 0.90f));
            _subtitleText.rectTransform.anchorMin = new Vector2(0.5f, 0.73f);
            _subtitleText.rectTransform.anchorMax = new Vector2(0.5f, 0.73f);
            _subtitleText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _subtitleText.rectTransform.sizeDelta = new Vector2(1000, 40);

            var btnContainer = CreatePanel("ButtonContainer", bgObj.transform, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480, 320), Color.clear);
            var layout = btnContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            _playCampaignButton = CreateMenuButton("PlayCampaignBtn", btnContainer.transform, "<b>PLAY CAMPAIGN</b>", new Color(0.10f, 0.75f, 0.45f), new Color(0.18f, 0.92f, 0.58f));
            _playCustomButton = CreateMenuButton("PlayCustomBtn", btnContainer.transform, "<b>PLAY CUSTOM (SPREADSHEET)</b>", new Color(0.15f, 0.45f, 0.85f), new Color(0.25f, 0.65f, 1f));
            _openSpreadsheetButton = CreateMenuButton("OpenCsvBtn", btnContainer.transform, "<b>OPEN SPREADSHEET (EXCEL)</b>", new Color(0.18f, 0.28f, 0.38f), new Color(0.28f, 0.42f, 0.58f));
            _quitButton = CreateMenuButton("QuitBtn", btnContainer.transform, "<b>QUIT</b>", new Color(0.45f, 0.15f, 0.18f), new Color(0.65f, 0.22f, 0.25f));

            _statusText = CreateText("StatusText", bgObj.transform, "Select a game mode to begin.", 18, TextAlignmentOptions.Center, FontStyles.Italic, new Color(0.70f, 0.80f, 0.90f));
            _statusText.rectTransform.anchorMin = new Vector2(0.5f, 0.16f);
            _statusText.rectTransform.anchorMax = new Vector2(0.5f, 0.16f);
            _statusText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _statusText.rectTransform.sizeDelta = new Vector2(900, 50);
        }

        private static Button CreateMenuButton(string name, Transform parent, string label, Color normalColor, Color highlightColor)
        {
            var btnObj = CreatePanel(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 56), normalColor);
            var layoutElem = btnObj.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 56f;
            layoutElem.preferredWidth = 460f;

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor = normalColor * 0.8f;
            colors.selectedColor = highlightColor;
            btn.colors = colors;

            CreateText("BtnText", btnObj.transform, label, 20, TextAlignmentOptions.Center, FontStyles.Bold, Color.white);
            return btn;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (parent != null) panel.transform.SetParent(parent, false);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            if (size != Vector2.zero) rect.sizeDelta = size;
            else { rect.offsetMin = rect.offsetMax = Vector2.zero; }

            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, TextAlignmentOptions align, FontStyles style, Color color)
        {
            var textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(parent, false);

            var rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.fontStyle = style;
            tmp.color = color;
            return tmp;
        }
    }
}
