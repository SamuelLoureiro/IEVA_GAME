using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VARG.Input;

namespace VARG.UI
{
    public class GyroCalibrationUI : MonoBehaviour
    {
        private const string SceneName = "02_ShipFlight";

        [Header("UI References (Optional - created automatically if null)")]
        [SerializeField] private RectTransform _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _controlsText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _calibrationFeedbackText;

        [Header("Settings")]
        [SerializeField] private float _feedbackDuration = 2.0f;

        private Coroutine _feedbackCoroutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            CheckAndSpawn(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            CheckAndSpawn(scene);
        }

        private static void CheckAndSpawn(UnityEngine.SceneManagement.Scene scene)
        {
            if (scene.name != SceneName) return;
            if (FindFirstObjectByType<GyroCalibrationUI>() != null) return;

            Canvas targetCanvas = null;
            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            for (var i = 0; i < canvases.Length; i++)
            {
                var c = canvases[i];
                if (c.gameObject.scene == scene && c.name != "BossDialogueCanvas")
                {
                    targetCanvas = c;
                    break;
                }
            }

            if (targetCanvas != null)
            {
                var uiObj = new GameObject("GyroCalibrationUI");
                uiObj.transform.SetParent(targetCanvas.transform, false);
                uiObj.AddComponent<GyroCalibrationUI>();
            }
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneChanged;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneChanged;
        }

        private void OnSceneChanged(UnityEngine.SceneManagement.Scene nextScene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (nextScene.name != SceneName)
            {
                Destroy(gameObject);
            }
        }

        private void Awake()
        {
            EnsureUI();
        }

        private void Update()
        {
            UpdateStatus();

            if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            {
                TriggerCalibration();
            }
        }

        public void TriggerCalibration()
        {
            InputManager.Instance?.CalibrateShipGyro();

            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(ShowCalibrationFeedbackRoutine());
        }

        private void UpdateStatus()
        {
            if (_statusText == null) return;

            var mobileConnected = InputManager.HasInstance &&
                                  InputManager.Instance.MobileShip != null &&
                                  InputManager.Instance.MobileShip.IsMobileConnected;

            if (mobileConnected)
            {
                _statusText.text = "<b>INPUT:</b> PHONE (UDP GYRO) <color=#46FF85>[CONNECTED]</color>";
            }
            else
            {
                _statusText.text = "<b>INPUT:</b> KEYBOARD (WASD) | UDP:5555 LISTENING";
            }
        }

        private IEnumerator ShowCalibrationFeedbackRoutine()
        {
            if (_calibrationFeedbackText != null)
            {
                _calibrationFeedbackText.gameObject.SetActive(true);
                _calibrationFeedbackText.text = "<b>[ C ] GYROSCOPE CALIBRATED!</b> (Neutral position saved)";
                _calibrationFeedbackText.color = new Color(0.27f, 1f, 0.52f, 1f);
            }

            yield return new WaitForSeconds(_feedbackDuration);

            if (_calibrationFeedbackText != null)
            {
                _calibrationFeedbackText.gameObject.SetActive(false);
            }
        }

        private void EnsureUI()
        {
            if (_panelRoot != null) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var panelObj = new GameObject("Panel_FlightControls", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObj.transform.SetParent(transform, false);

            _panelRoot = panelObj.GetComponent<RectTransform>();
            _panelRoot.anchorMin = new Vector2(0f, 1f);
            _panelRoot.anchorMax = new Vector2(0f, 1f);
            _panelRoot.pivot = new Vector2(0f, 1f);
            _panelRoot.anchoredPosition = new Vector2(28f, -28f);
            _panelRoot.sizeDelta = new Vector2(400f, 160f);

            var panelBg = panelObj.GetComponent<Image>();
            panelBg.color = new Color(0.05f, 0.08f, 0.12f, 0.90f);

            _titleText = CreateText("Text_Title", panelObj.transform, "=CONTROLS_OVERVIEW()", 16, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.25f, 0.85f, 1f));
            _titleText.rectTransform.anchoredPosition = new Vector2(16f, -14f);
            _titleText.rectTransform.sizeDelta = new Vector2(368f, 24f);

            var controlsMsg =
                "<b>WASD / Arrows:</b> Steer ship (or Phone Tilt)\n" +
                "<b>[ C ]:</b> Calibrate Gyro (Neutral angle)\n" +
                "<b>[ E ]:</b> Descend over City (or Phone Shake)";

            _controlsText = CreateText("Text_Controls", panelObj.transform, controlsMsg, 13, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
            _controlsText.rectTransform.anchoredPosition = new Vector2(16f, -42f);
            _controlsText.rectTransform.sizeDelta = new Vector2(368f, 60f);

            _statusText = CreateText("Text_Status", panelObj.transform, "INPUT: KEYBOARD", 12, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.65f, 0.75f, 0.85f));
            _statusText.rectTransform.anchoredPosition = new Vector2(16f, -104f);
            _statusText.rectTransform.sizeDelta = new Vector2(368f, 22f);

            _calibrationFeedbackText = CreateText("Text_Feedback", panelObj.transform, "", 12, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.27f, 1f, 0.52f));
            _calibrationFeedbackText.rectTransform.anchoredPosition = new Vector2(16f, -130f);
            _calibrationFeedbackText.rectTransform.sizeDelta = new Vector2(368f, 22f);
            _calibrationFeedbackText.gameObject.SetActive(false);
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, TextAlignmentOptions align, FontStyles style, Color color)
        {
            var textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(parent, false);

            var rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);

            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.lineSpacing = 8f;
            return tmp;
        }
    }
}
