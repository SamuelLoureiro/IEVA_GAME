using System.Collections;
using TMPro;
using UnityEngine;

namespace VARG.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BossPopupUI : MonoBehaviour
    {
        private static BossPopupUI _instance;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private float _displayDuration = 3.2f;

        private Coroutine _displayCoroutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic() => _instance = null;

        public static void Show(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BossPopupUI>();
            }

            if (_instance == null)
            {
                var prefab = Resources.Load<GameObject>("UI/BossPopupUI");
                if (prefab == null) return;

                var canvasGo = new GameObject("BossDialogueCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
                var canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 500;

                var scaler = canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                var popupGo = Instantiate(prefab, canvasGo.transform, false);
                popupGo.name = "BossPopupUI";

                DontDestroyOnLoad(canvasGo);
                _instance = popupGo.GetComponent<BossPopupUI>();
            }

            _instance.DisplayLine(line);
        }

        private void Awake()
        {
            _instance = this;
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            if (_dialogueText == null)
            {
                var tmps = GetComponentsInChildren<TextMeshProUGUI>();
                for (var i = 0; i < tmps.Length; i++)
                {
                    if (tmps[i].name == "DialogueText")
                    {
                        _dialogueText = tmps[i];
                        break;
                    }
                }
                if (_dialogueText == null && tmps.Length > 0) _dialogueText = tmps[0];
            }
        }

        public void DisplayLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            gameObject.SetActive(true);
            if (_displayCoroutine != null) StopCoroutine(_displayCoroutine);
            _displayCoroutine = StartCoroutine(ShowRoutine(line));
        }

        private IEnumerator ShowRoutine(string line)
        {
            if (_dialogueText != null) _dialogueText.text = line;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(_displayDuration);

            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            _displayCoroutine = null;
        }
    }
}
