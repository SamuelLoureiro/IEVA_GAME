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
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.name == "00_Boot")
            {
                if (FindFirstObjectByType<MainMenuController>() == null)
                {
                    var go = new GameObject("MainMenuController");
                    go.AddComponent<MainMenuController>();
                }
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            EnsureMenuUI();
            SetupListeners();

            // Ensure CustomLevel.csv exists in background so players have it ready to edit
            SpreadsheetLevelLoader.EnsureTemplateExists(out _);
        }

        private void OnDestroy()
        {
            if (_playCampaignButton != null) _playCampaignButton.onClick.RemoveListener(PlayCampaign);
            if (_playCustomButton != null) _playCustomButton.onClick.RemoveListener(PlayCustomSpreadsheet);
            if (_openSpreadsheetButton != null) _openSpreadsheetButton.onClick.RemoveListener(OpenSpreadsheetFile);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(QuitGame);
        }

        public void Configure(
            Button playCampaignButton,
            Button playCustomButton,
            Button openSpreadsheetButton,
            Button quitButton,
            TextMeshProUGUI statusText,
            TextMeshProUGUI titleText = null,
            TextMeshProUGUI subtitleText = null)
        {
            _playCampaignButton = playCampaignButton;
            _playCustomButton = playCustomButton;
            _openSpreadsheetButton = openSpreadsheetButton;
            _quitButton = quitButton;
            _statusText = statusText;
            _titleText = titleText;
            _subtitleText = subtitleText;
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
                GameManager.Instance.ClearCustomSpreadsheetLevel();
                GameManager.Instance.StartLevel();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load("01_BossBriefing");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("01_BossBriefing");
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

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCustomSpreadsheetLevel(customConfig);
                }

                if (MissionManager.Instance != null)
                {
                    MissionManager.Instance.LoadCustomMission(levelData.TargetSequence, levelData.ForbiddenTarget);
                }

                SetStatus("Spreadsheet loaded! Starting mission...", new Color(0.35f, 0.85f, 1f));

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartLevel();
                }
                else if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.Load("01_BossBriefing");
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("01_BossBriefing");
                }
            }
            else
            {
                SetStatus(statusMessage, new Color(1f, 0.45f, 0.45f));
                Debug.LogWarning($"[MainMenuController] {statusMessage}");
            }
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

            var canvas = FindFirstObjectByType<Canvas>() ?? new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            }

            var bg = CreatePanel("Background", canvas.transform, Vector2.zero, Vector2.one, new Color(0.04f, 0.06f, 0.09f, 1f));
            _titleText = CreateText("Title", bg.transform, "PROJECT V.A.R.G.", 52, new Vector2(0.5f, 0.80f), new Vector2(1000, 70));
            _subtitleText = CreateText("Subtitle", bg.transform, "METHODICAL ABDUCTION  |  MISSIONS & SPREADSHEETS", 20, new Vector2(0.5f, 0.73f), new Vector2(1000, 40));

            var container = CreatePanel("ButtonContainer", bg.transform, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), Color.clear);
            container.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 320);
            var layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;

            _playCampaignButton = CreateMenuButton("PlayCampaignBtn", container.transform, "<b>▶ PLAY CAMPAIGN</b>", new Color(0.10f, 0.75f, 0.45f));
            _playCustomButton = CreateMenuButton("PlayCustomBtn", container.transform, "<b>📄 PLAY CUSTOM (SPREADSHEET)</b>", new Color(0.15f, 0.45f, 0.85f));
            _openSpreadsheetButton = CreateMenuButton("OpenCsvBtn", container.transform, "<b>📁 OPEN SPREADSHEET (EXCEL)</b>", new Color(0.18f, 0.28f, 0.38f));
            _quitButton = CreateMenuButton("QuitBtn", container.transform, "<b>QUIT</b>", new Color(0.45f, 0.15f, 0.18f));

            _statusText = CreateText("StatusText", bg.transform, "Select a game mode to begin.", 18, new Vector2(0.5f, 0.16f), new Vector2(900, 50));
        }

        private static Button CreateMenuButton(string name, Transform parent, string label, Color color)
        {
            var btnObj = CreatePanel(name, parent, Vector2.zero, Vector2.one, color);
            var layoutElem = btnObj.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 56f;
            layoutElem.preferredWidth = 460f;

            var btn = btnObj.AddComponent<Button>();
            CreateText("BtnText", btnObj.transform, label, 20, Vector2.zero, Vector2.zero);
            return btn;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, Vector2 anchor, Vector2 sizeDelta)
        {
            var textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(parent, false);
            var rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = sizeDelta;
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }
    }
}
