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
        [Header("Components")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Image _avatarImage;

        [Header("Expressions (Optional Sprites)")]
        [SerializeField] private Sprite _neutralSprite;
        [SerializeField] private Sprite _annoyedSprite;
        [SerializeField] private Sprite _angrySprite;
        [SerializeField] private Sprite _furiousSprite;
        [SerializeField] private Sprite _smugSprite;

        [Header("Settings")]
        [SerializeField] private float _displayDuration = 3.5f;
        [SerializeField] private float _fadeDuration = 0.2f;

        private Coroutine _displayCoroutine;
        private Coroutine _fadeCoroutine;

        private static BossPopupUI _activeInstance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents()
        {
            _activeInstance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterGlobalListener()
        {
            BossDialogueManager.OnBossSpokeStatic -= HandleGlobalBossSpoke;
            BossDialogueManager.OnBossSpokeStatic += HandleGlobalBossSpoke;
        }

        private static void HandleGlobalBossSpoke(string line, BossExpression expression)
        {
            if (string.IsNullOrEmpty(line)) return;

            if (_activeInstance == null)
            {
                _activeInstance = Object.FindFirstObjectByType<BossPopupUI>();
                if (_activeInstance == null)
                {
                    var prefab = Resources.Load<GameObject>("UI/BossPopupUI");
                    if (prefab != null)
                    {
                        var canvas = Object.FindFirstObjectByType<Canvas>();
                        var go = canvas != null ? Object.Instantiate(prefab, canvas.transform) : Object.Instantiate(prefab);
                        _activeInstance = go.GetComponent<BossPopupUI>();
                    }
                }
            }

            if (_activeInstance != null)
            {
                _activeInstance.Show(line, expression);
            }
        }

        private void Awake()
        {
            _activeInstance = this;

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void OnDestroy()
        {
            if (_activeInstance == this)
            {
                _activeInstance = null;
            }
        }

        public void Show(string line, BossExpression expression)
        {
            if (string.IsNullOrEmpty(line)) return;

            if (_displayCoroutine != null)
            {
                StopCoroutine(_displayCoroutine);
            }

            _displayCoroutine = StartCoroutine(ShowDialogueRoutine(line, expression));
        }

        private IEnumerator ShowDialogueRoutine(string line, BossExpression expression)
        {
            UpdateAvatar(expression);

            if (_speakerNameText != null)
            {
                _speakerNameText.text = GetSpeakerTitle(expression);
            }

            if (_dialogueText != null)
            {
                _dialogueText.text = line;
            }

            yield return StartFade(1f);
            yield return new WaitForSeconds(_displayDuration);
            yield return StartFade(0f);
            _displayCoroutine = null;
        }

        private void UpdateAvatar(BossExpression expression)
        {
            if (_avatarImage == null) return;

            var sprite = expression switch
            {
                BossExpression.Neutral => _neutralSprite,
                BossExpression.Annoyed => _annoyedSprite,
                BossExpression.Angry => _angrySprite,
                BossExpression.Furious => _furiousSprite,
                BossExpression.Smug => _smugSprite,
                _ => _neutralSprite
            };

            if (sprite != null)
            {
                _avatarImage.sprite = sprite;
                _avatarImage.enabled = true;
            }
        }

        private static string GetSpeakerTitle(BossExpression expression)
        {
            return expression switch
            {
                BossExpression.Furious => "COMANDANTE VARG [FURIOSO]",
                BossExpression.Angry => "COMANDANTE VARG [IRRITADO]",
                BossExpression.Annoyed => "COMANDANTE VARG",
                BossExpression.Smug => "COMANDANTE VARG [SARCÁSTICO]",
                _ => "COMANDANTE VARG"
            };
        }

        private Coroutine StartFade(float targetAlpha)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
            return _fadeCoroutine;
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            if (_canvasGroup == null) yield break;

            var startAlpha = _canvasGroup.alpha;
            var elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _canvasGroup.blocksRaycasts = targetAlpha > 0.5f;
            _fadeCoroutine = null;
        }
    }
}
