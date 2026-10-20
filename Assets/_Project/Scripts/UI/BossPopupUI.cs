using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Humor;

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

        private void OnEnable()
        {
            _instance = this;
            BossDialogueManager.OnBossSpokeStatic += Show;
        }

        private void OnDisable()
        {
            BossDialogueManager.OnBossSpokeStatic -= Show;
        }

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

                var canvasGo = new GameObject("BossDialogueCanvas", typeof(Canvas), typeof(CanvasScaler));
                var canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 500;

                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                var popupGo = Instantiate(prefab, canvasGo.transform, false);
                popupGo.name = "BossPopupUI";

                var rect = popupGo.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    rect.anchoredPosition = new Vector2(0f, 35f);
                    rect.sizeDelta = new Vector2(580f, 115f);
                    rect.localScale = Vector3.one;
                }

                var cg = popupGo.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.blocksRaycasts = false;
                    cg.interactable = false;
                }

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

            SetupBossAvatar();
        }

        private void SetupBossAvatar()
        {
            Transform avatarContainer = transform.Find("AvatarContainer");
            if (avatarContainer == null)
            {
                for (var i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    if (child.name.Contains("Avatar"))
                    {
                        avatarContainer = child;
                        break;
                    }
                }
            }

            if (avatarContainer == null || avatarContainer.Find("AlienHead") != null) return;

            var bgImages = avatarContainer.GetComponentsInChildren<Image>();
            for (var i = 0; i < bgImages.Length; i++)
            {
                bgImages[i].color = new Color(0.12f, 0.18f, 0.25f, 0.95f);
            }

            var headGo = new GameObject("AlienHead", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            headGo.transform.SetParent(avatarContainer, false);
            var headRect = headGo.GetComponent<RectTransform>();
            headRect.anchorMin = new Vector2(0.5f, 0.5f);
            headRect.anchorMax = new Vector2(0.5f, 0.5f);
            headRect.pivot = new Vector2(0.5f, 0.5f);
            headRect.anchoredPosition = new Vector2(0f, 6f);
            headRect.sizeDelta = new Vector2(50f, 44f);
            var headImg = headGo.GetComponent<Image>();
            headImg.color = new Color(0.25f, 0.85f, 0.50f, 1f);
            headImg.raycastTarget = false;

            CreateEye(headGo.transform, new Vector2(-12f, 2f), 12f);
            CreateEye(headGo.transform, new Vector2(12f, 2f), -12f);

            var collarGo = new GameObject("Collar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            collarGo.transform.SetParent(avatarContainer, false);
            var collarRect = collarGo.GetComponent<RectTransform>();
            collarRect.anchorMin = new Vector2(0.5f, 0.5f);
            collarRect.anchorMax = new Vector2(0.5f, 0.5f);
            collarRect.pivot = new Vector2(0.5f, 0.5f);
            collarRect.anchoredPosition = new Vector2(0f, -20f);
            collarRect.sizeDelta = new Vector2(30f, 12f);
            var collarImg = collarGo.GetComponent<Image>();
            collarImg.color = Color.white;
            collarImg.raycastTarget = false;

            var tieGo = new GameObject("Tie", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            tieGo.transform.SetParent(collarGo.transform, false);
            var tieRect = tieGo.GetComponent<RectTransform>();
            tieRect.anchorMin = new Vector2(0.5f, 0.5f);
            tieRect.anchorMax = new Vector2(0.5f, 0.5f);
            tieRect.pivot = new Vector2(0.5f, 0.5f);
            tieRect.anchoredPosition = new Vector2(0f, -6f);
            tieRect.sizeDelta = new Vector2(8f, 18f);
            var tieImg = tieGo.GetComponent<Image>();
            tieImg.color = new Color(0.85f, 0.18f, 0.18f, 1f);
            tieImg.raycastTarget = false;
        }

        private static void CreateEye(Transform parent, Vector2 pos, float zRot)
        {
            var eyeGo = new GameObject("Eye", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            eyeGo.transform.SetParent(parent, false);
            var eyeRect = eyeGo.GetComponent<RectTransform>();
            eyeRect.anchorMin = new Vector2(0.5f, 0.5f);
            eyeRect.anchorMax = new Vector2(0.5f, 0.5f);
            eyeRect.pivot = new Vector2(0.5f, 0.5f);
            eyeRect.anchoredPosition = pos;
            eyeRect.sizeDelta = new Vector2(10f, 16f);
            eyeRect.localEulerAngles = new Vector3(0f, 0f, zRot);
            var eyeImg = eyeGo.GetComponent<Image>();
            eyeImg.color = new Color(0.05f, 0.06f, 0.09f, 1f);
            eyeImg.raycastTarget = false;

            var shineGo = new GameObject("Shine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shineGo.transform.SetParent(eyeGo.transform, false);
            var shineRect = shineGo.GetComponent<RectTransform>();
            shineRect.anchorMin = new Vector2(0.5f, 0.5f);
            shineRect.anchorMax = new Vector2(0.5f, 0.5f);
            shineRect.pivot = new Vector2(0.5f, 0.5f);
            shineRect.anchoredPosition = new Vector2(2f, 3f);
            shineRect.sizeDelta = new Vector2(3f, 5f);
            var shineImg = shineGo.GetComponent<Image>();
            shineImg.color = Color.white;
            shineImg.raycastTarget = false;
        }

        public void DisplayLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            SetupBossAvatar();
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
