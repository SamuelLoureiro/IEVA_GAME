#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using VARG.UI;

namespace VARG.Editor
{
    public static class ResultsSceneBuilder
    {
        [MenuItem("VARG/Build Results Scene (05_Results)")]
        public static void BuildScene()
        {
            const string scenePath = "Assets/_Project/Scenes/05_Results.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Clean previous UI if any
            var existingCanvas = GameObject.Find("ResultsCanvas");
            if (existingCanvas != null) Object.DestroyImmediate(existingCanvas);

            var existingEventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (existingEventSystem != null) Object.DestroyImmediate(existingEventSystem.gameObject);

            // 1. Setup Camera background
            var cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.05f, 0.07f, 0.1f);
            }

            // 2. Setup EventSystem
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            // 3. Setup Canvas
            var canvasGo = new GameObject("ResultsCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

            // 4. Main Panel
            var windowGo = new GameObject("Panel_ResultsWindow");
            windowGo.transform.SetParent(canvasGo.transform, false);

            var windowRect = windowGo.AddComponent<RectTransform>();
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.anchoredPosition = Vector2.zero;
            windowRect.sizeDelta = new Vector2(860f, 620f);

            var windowImg = windowGo.AddComponent<Image>();
            windowImg.color = new Color(0.07f, 0.09f, 0.13f, 0.95f);

            // Header Banner
            var headerGo = new GameObject("HeaderBanner");
            headerGo.transform.SetParent(windowGo.transform, false);
            var headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, 110f);
            var headerImg = headerGo.AddComponent<Image>();
            headerImg.color = new Color(0.1f, 0.13f, 0.19f, 1f);

            var titleGo = new GameObject("Text_Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0f, 15f);
            titleRect.sizeDelta = new Vector2(800f, 45f);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) titleTmp.font = fontAsset;
            titleTmp.text = "MISSION COMPLETE!";
            titleTmp.fontSize = 36;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.2f, 1f, 0.4f);

            var subtitleGo = new GameObject("Text_Subtitle");
            subtitleGo.transform.SetParent(headerGo.transform, false);
            var subtitleRect = subtitleGo.AddComponent<RectTransform>();
            subtitleRect.anchoredPosition = new Vector2(0f, -25f);
            subtitleRect.sizeDelta = new Vector2(800f, 25f);
            var subtitleTmp = subtitleGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) subtitleTmp.font = fontAsset;
            subtitleTmp.text = "Level 1 Performance Summary";
            subtitleTmp.fontSize = 18;
            subtitleTmp.alignment = TextAlignmentOptions.Center;
            subtitleTmp.color = new Color(0.6f, 0.7f, 0.82f);

            // Left Column: Numerical Stats (X: -210, Y: -30)
            var statsColGo = new GameObject("Column_Stats");
            statsColGo.transform.SetParent(windowGo.transform, false);
            var statsRect = statsColGo.AddComponent<RectTransform>();
            statsRect.anchoredPosition = new Vector2(-210f, -30f);
            statsRect.sizeDelta = new Vector2(360f, 320f);

            var captureRow = CreateStatRow(statsColGo, "Row_Capture", "Capture Time:", "01:15", 90f, fontAsset);
            var sortingRow = CreateStatRow(statsColGo, "Row_Sorting", "Sorting Time:", "00:30", 40f, fontAsset);
            var livesRow = CreateStatRow(statsColGo, "Row_Lives", "Lives Left:", "5 / 5", -10f, fontAsset);
            var errorsRow = CreateStatRow(statsColGo, "Row_Errors", "Total Errors:", "0", -60f, fontAsset);

            // Score Highlight Panel
            var scorePanelGo = new GameObject("Panel_Score");
            scorePanelGo.transform.SetParent(statsColGo.transform, false);
            var scorePanelRect = scorePanelGo.AddComponent<RectTransform>();
            scorePanelRect.anchoredPosition = new Vector2(0f, -125f);
            scorePanelRect.sizeDelta = new Vector2(340f, 55f);
            var scoreBg = scorePanelGo.AddComponent<Image>();
            scoreBg.color = new Color(0.12f, 0.16f, 0.23f);

            var scoreLabelGo = new GameObject("Text_ScoreLabel");
            scoreLabelGo.transform.SetParent(scorePanelGo.transform, false);
            var scoreLabelRect = scoreLabelGo.AddComponent<RectTransform>();
            scoreLabelRect.anchoredPosition = new Vector2(-75f, 0f);
            scoreLabelRect.sizeDelta = new Vector2(150f, 30f);
            var scoreLabelTmp = scoreLabelGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) scoreLabelTmp.font = fontAsset;
            scoreLabelTmp.text = "TOTAL SCORE:";
            scoreLabelTmp.fontSize = 18;
            scoreLabelTmp.fontStyle = FontStyles.Bold;
            scoreLabelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            scoreLabelTmp.color = Color.white;

            var scoreValGo = new GameObject("Text_ScoreValue");
            scoreValGo.transform.SetParent(scorePanelGo.transform, false);
            var scoreValRect = scoreValGo.AddComponent<RectTransform>();
            scoreValRect.anchoredPosition = new Vector2(75f, 0f);
            scoreValRect.sizeDelta = new Vector2(150f, 30f);
            var scoreValTmp = scoreValGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) scoreValTmp.font = fontAsset;
            scoreValTmp.text = "1,450 PTS";
            scoreValTmp.fontSize = 24;
            scoreValTmp.fontStyle = FontStyles.Bold;
            scoreValTmp.alignment = TextAlignmentOptions.MidlineRight;
            scoreValTmp.color = new Color(1f, 0.85f, 0.2f);

            // Right Column: Excel-style Bar Chart (X: 200, Y: -30)
            var chartColGo = new GameObject("Column_Chart");
            chartColGo.transform.SetParent(windowGo.transform, false);
            var chartRect = chartColGo.AddComponent<RectTransform>();
            chartRect.anchoredPosition = new Vector2(200f, -30f);
            chartRect.sizeDelta = new Vector2(380f, 320f);

            var chartTitleGo = new GameObject("Text_ChartTitle");
            chartTitleGo.transform.SetParent(chartColGo.transform, false);
            var chartTitleRect = chartTitleGo.AddComponent<RectTransform>();
            chartTitleRect.anchoredPosition = new Vector2(0f, 130f);
            chartTitleRect.sizeDelta = new Vector2(360f, 25f);
            var chartTitleTmp = chartTitleGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) chartTitleTmp.font = fontAsset;
            chartTitleTmp.text = "PERFORMANCE METRICS";
            chartTitleTmp.fontSize = 16;
            chartTitleTmp.fontStyle = FontStyles.Bold;
            chartTitleTmp.color = new Color(0.7f, 0.8f, 0.9f);

            var captureBarImg = CreateChartBar(chartColGo, "Bar_Capture", "Capture Speed", new Color(0.23f, 0.51f, 0.96f), 75f, fontAsset);
            var sortingBarImg = CreateChartBar(chartColGo, "Bar_Sorting", "Sorting Speed", new Color(0.02f, 0.71f, 0.83f), 20f, fontAsset);
            var livesBarImg = CreateChartBar(chartColGo, "Bar_Lives", "Survival", new Color(0.06f, 0.73f, 0.51f), -35f, fontAsset);
            var accuracyBarImg = CreateChartBar(chartColGo, "Bar_Accuracy", "Accuracy", new Color(0.96f, 0.62f, 0.04f), -90f, fontAsset);

            // Bottom Action Buttons
            var buttonsRowGo = new GameObject("Row_Buttons");
            buttonsRowGo.transform.SetParent(windowGo.transform, false);
            var buttonsRowRect = buttonsRowGo.AddComponent<RectTransform>();
            buttonsRowRect.anchorMin = new Vector2(0f, 0f);
            buttonsRowRect.anchorMax = new Vector2(1f, 0f);
            buttonsRowRect.pivot = new Vector2(0.5f, 0f);
            buttonsRowRect.anchoredPosition = new Vector2(0f, 30f);
            buttonsRowRect.sizeDelta = new Vector2(0f, 60f);

            var retryBtn = CreateButton(buttonsRowGo, "Button_Retry", "TRY AGAIN", new Vector2(-120f, 0f), new Color(0.2f, 0.25f, 0.35f), Color.white, fontAsset);
            var nextBtn = CreateButton(buttonsRowGo, "Button_NextLevel", "NEXT LEVEL", new Vector2(120f, 0f), new Color(0.12f, 0.72f, 0.35f), Color.black, fontAsset);

            // Attach & Configure ResultsScreen
            var resultsComp = windowGo.AddComponent<ResultsScreen>();
            var serialized = new SerializedObject(resultsComp);
            serialized.FindProperty("_titleText").objectReferenceValue = titleTmp;
            serialized.FindProperty("_subtitleText").objectReferenceValue = subtitleTmp;
            serialized.FindProperty("_captureTimeText").objectReferenceValue = captureRow;
            serialized.FindProperty("_sortingTimeText").objectReferenceValue = sortingRow;
            serialized.FindProperty("_livesText").objectReferenceValue = livesRow;
            serialized.FindProperty("_errorsText").objectReferenceValue = errorsRow;
            serialized.FindProperty("_scoreText").objectReferenceValue = scoreValTmp;

            serialized.FindProperty("_captureTimeBar").objectReferenceValue = captureBarImg;
            serialized.FindProperty("_sortingTimeBar").objectReferenceValue = sortingBarImg;
            serialized.FindProperty("_livesBar").objectReferenceValue = livesBarImg;
            serialized.FindProperty("_accuracyBar").objectReferenceValue = accuracyBarImg;

            serialized.FindProperty("_nextLevelButton").objectReferenceValue = nextBtn;
            serialized.FindProperty("_retryButton").objectReferenceValue = retryBtn;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ResultsSceneBuilder] 05_Results scene successfully built and configured!");
        }

        private static TextMeshProUGUI CreateStatRow(GameObject parent, string name, string label, string defaultValue, float yOffset, TMP_FontAsset font)
        {
            var rowGo = new GameObject(name);
            rowGo.transform.SetParent(parent.transform, false);
            var rect = rowGo.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, yOffset);
            rect.sizeDelta = new Vector2(340f, 30f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rowGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(-80f, 0f);
            labelRect.sizeDelta = new Vector2(160f, 30f);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            if (font != null) labelTmp.font = font;
            labelTmp.text = label;
            labelTmp.fontSize = 16;
            labelTmp.color = new Color(0.65f, 0.75f, 0.85f);
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var valGo = new GameObject("Value");
            valGo.transform.SetParent(rowGo.transform, false);
            var valRect = valGo.AddComponent<RectTransform>();
            valRect.anchoredPosition = new Vector2(80f, 0f);
            valRect.sizeDelta = new Vector2(160f, 30f);
            var valTmp = valGo.AddComponent<TextMeshProUGUI>();
            if (font != null) valTmp.font = font;
            valTmp.text = defaultValue;
            valTmp.fontSize = 18;
            valTmp.fontStyle = FontStyles.Bold;
            valTmp.color = Color.white;
            valTmp.alignment = TextAlignmentOptions.MidlineRight;

            return valTmp;
        }

        private static Image CreateChartBar(GameObject parent, string name, string label, Color barColor, float yOffset, TMP_FontAsset font)
        {
            var rowGo = new GameObject(name);
            rowGo.transform.SetParent(parent.transform, false);
            var rect = rowGo.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, yOffset);
            rect.sizeDelta = new Vector2(360f, 40f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rowGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(0f, 12f);
            labelRect.sizeDelta = new Vector2(360f, 18f);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            if (font != null) labelTmp.font = font;
            labelTmp.text = label;
            labelTmp.fontSize = 14;
            labelTmp.color = new Color(0.6f, 0.7f, 0.8f);

            var trackGo = new GameObject("Track");
            trackGo.transform.SetParent(rowGo.transform, false);
            var trackRect = trackGo.AddComponent<RectTransform>();
            trackRect.anchoredPosition = new Vector2(0f, -8f);
            trackRect.sizeDelta = new Vector2(360f, 14f);
            var trackImg = trackGo.AddComponent<Image>();
            trackImg.color = new Color(0.12f, 0.16f, 0.22f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(trackGo.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = barColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 0.5f;

            return fillImg;
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 pos, Color btnColor, Color textColor, TMP_FontAsset font)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent.transform, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(210f, 48f);

            var img = btnGo.AddComponent<Image>();
            img.color = btnColor;

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = Color.Lerp(btnColor, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(btnColor, Color.black, 0.2f);
            btn.colors = colors;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = label;
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;

            return btn;
        }
    }
}
#endif
