#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VARG.UI;

namespace VARG.Editor
{
    public static class MainMenuBuilder
    {
        private const string BootScenePath = "Assets/_Project/Scenes/00_Boot.unity";

        [MenuItem("VARG/Build Main Menu Scene (00_Boot)")]
        public static void BuildMainMenuScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != BootScenePath)
            {
                var open = EditorUtility.DisplayDialog(
                    "Switch Scene",
                    $"The Main Menu must be built in '{BootScenePath}'. Open it now?",
                    "Open & Build",
                    "Cancel"
                );

                if (!open) return;
                activeScene = EditorSceneManager.OpenScene(BootScenePath);
            }

            CleanScene(activeScene);

            BuildCamera();
            BuildLighting();
            BuildMenuStage();

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("[MainMenuBuilder] Main Menu scene (00_Boot) built and saved successfully!");
            EditorUtility.DisplayDialog("VARG", "Main Menu (00_Boot) built successfully!", "OK");
        }

        private static void CleanScene(Scene scene)
        {
            foreach (var rootObj in scene.GetRootGameObjects())
            {
                Object.DestroyImmediate(rootObj);
            }
        }

        private static void BuildCamera()
        {
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";

            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.06f, 0.09f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;

            camObj.transform.position = new Vector3(0f, 0f, -10f);
            camObj.AddComponent<AudioListener>();
        }

        private static void BuildLighting()
        {
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void BuildMenuStage()
        {
            var canvasObj = new GameObject("MainMenuCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            var eventSys = new GameObject("EventSystem");
            eventSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSys.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Fullscreen Background
            var bgObj = CreatePanel("Background", canvasObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.04f, 0.06f, 0.09f, 1f));

            // Alien Decorative Frame
            var frameObj = CreatePanel("CyberFrame", bgObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640, 720), new Color(0.07f, 0.11f, 0.16f, 0.95f));
            CreatePanel("CyberBorder", frameObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.18f, 0.45f, 0.65f, 0.35f));

            // Title Header
            var title = CreateText("Title", frameObj.transform, "PROJECT V.A.R.G.", 46, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.92f, 0.96f, 1f));
            title.rectTransform.anchorMin = new Vector2(0.5f, 0.88f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 0.88f);
            title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            title.rectTransform.sizeDelta = new Vector2(600, 60);

            var subtitle = CreateText("Subtitle", frameObj.transform, "ABDUÇÃO METÓDICA  |  NÍVEIS E PLANILHAS", 16, TextAlignmentOptions.Center, FontStyles.Normal, new Color(0.40f, 0.75f, 0.90f));
            subtitle.rectTransform.anchorMin = new Vector2(0.5f, 0.80f);
            subtitle.rectTransform.anchorMax = new Vector2(0.5f, 0.80f);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            subtitle.rectTransform.sizeDelta = new Vector2(600, 30);

            // Button container
            var btnContainer = CreatePanel("ButtonContainer", frameObj.transform, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 320), Color.clear);
            var layout = btnContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            var playCampaignBtn = CreateMenuButton("PlayCampaignBtn", btnContainer.transform, "<b>▶ JOGAR CAMPANHA</b>  <size=15>[ ENTER ]</size>", new Color(0.10f, 0.75f, 0.45f), new Color(0.18f, 0.92f, 0.58f));
            var playCustomBtn = CreateMenuButton("PlayCustomBtn", btnContainer.transform, "<b>📄 JOGAR CUSTOM (PLANILHA)</b>  <size=15>[ C ]</size>", new Color(0.15f, 0.48f, 0.85f), new Color(0.25f, 0.68f, 1f));
            var openCsvBtn = CreateMenuButton("OpenCsvBtn", btnContainer.transform, "<b>📁 ABRIR PLANILHA (EXCEL)</b>  <size=15>[ E ]</size>", new Color(0.16f, 0.24f, 0.34f), new Color(0.24f, 0.38f, 0.52f));
            var quitBtn = CreateMenuButton("QuitBtn", btnContainer.transform, "<b>SAIR</b>  <size=15>[ ESC ]</size>", new Color(0.45f, 0.15f, 0.18f), new Color(0.65f, 0.22f, 0.25f));

            // Status message
            var status = CreateText("StatusText", frameObj.transform, "Selecione um modo para iniciar.", 16, TextAlignmentOptions.Center, FontStyles.Italic, new Color(0.70f, 0.80f, 0.90f));
            status.rectTransform.anchorMin = new Vector2(0.5f, 0.12f);
            status.rectTransform.anchorMax = new Vector2(0.5f, 0.12f);
            status.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            status.rectTransform.sizeDelta = new Vector2(560, 40);

            // Controller
            var controllerObj = new GameObject("MainMenuController");
            var controller = controllerObj.AddComponent<MainMenuController>();
            controller.Configure(playCampaignBtn, playCustomBtn, openCsvBtn, quitBtn, status, title, subtitle);
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

            var text = CreateText("BtnText", btnObj.transform, label, 20, TextAlignmentOptions.Center, FontStyles.Bold, Color.white);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

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

            if (size != Vector2.zero)
            {
                rect.sizeDelta = size;
            }
            else
            {
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            var img = panel.GetComponent<Image>();
            img.color = color;
            return panel;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, TextAlignmentOptions align, FontStyles style, Color color)
        {
            var textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(parent, false);

            var rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

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
#endif
