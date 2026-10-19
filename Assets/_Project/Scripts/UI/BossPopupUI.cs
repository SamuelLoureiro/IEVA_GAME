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
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private float _displayDuration = 3.5f;

        [Header("Boss Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _voiceClip;

        private Coroutine _displayCoroutine;

        private void OnEnable()
        {
            BossDialogueManager.OnBossSpokeStatic += Show;
        }

        private void OnDisable()
        {
            BossDialogueManager.OnBossSpokeStatic -= Show;
        }

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();

            if (_dialogueText == null)
            {
                var tmps = GetComponentsInChildren<TextMeshProUGUI>();
                for (var i = 0; i < tmps.Length; i++)
                {
                    if (tmps[i] != _speakerNameText)
                    {
                        _dialogueText = tmps[i];
                        break;
                    }
                }
            }
        }

        public void Show(string line, BossExpression expression)
        {
            if (string.IsNullOrEmpty(line)) return;

            if (_voiceClip != null)
            {
                if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.PlayOneShot(_voiceClip, 0.85f);
            }

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
