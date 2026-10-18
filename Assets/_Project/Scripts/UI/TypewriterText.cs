using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace VARG.UI
{
    public class TypewriterText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textComponent;
        [SerializeField] private float _charactersPerSecond = 35f;

        private Coroutine _typeCoroutine;
        private string _targetText = string.Empty;
        private Action _onComplete;
        private bool _isTyping;

        public bool IsTyping => _isTyping;

        private void Awake()
        {
            if (_textComponent == null)
            {
                _textComponent = GetComponent<TextMeshProUGUI>();
            }
        }

        public void ShowText(string fullText, Action onComplete = null)
        {
            if (_typeCoroutine != null)
            {
                StopCoroutine(_typeCoroutine);
            }

            _targetText = fullText ?? string.Empty;
            _onComplete = onComplete;
            _typeCoroutine = StartCoroutine(TypeTextRoutine());
        }

        public void Skip()
        {
            if (!_isTyping) return;

            if (_typeCoroutine != null)
            {
                StopCoroutine(_typeCoroutine);
                _typeCoroutine = null;
            }

            if (_textComponent != null)
            {
                _textComponent.text = _targetText;
                _textComponent.maxVisibleCharacters = int.MaxValue;
            }

            _isTyping = false;
            var callback = _onComplete;
            _onComplete = null;
            callback?.Invoke();
        }

        private IEnumerator TypeTextRoutine()
        {
            _isTyping = true;
            if (_textComponent != null)
            {
                _textComponent.text = _targetText;
                _textComponent.maxVisibleCharacters = 0;
            }

            var delay = 1f / Mathf.Max(1f, _charactersPerSecond);
            var currentLength = 0;

            while (currentLength < _targetText.Length)
            {
                currentLength++;
                if (_textComponent != null)
                {
                    _textComponent.maxVisibleCharacters = currentLength;
                }

                yield return new WaitForSeconds(delay);
            }

            if (_textComponent != null)
            {
                _textComponent.maxVisibleCharacters = int.MaxValue;
            }

            _isTyping = false;
            _typeCoroutine = null;
            var callback = _onComplete;
            _onComplete = null;
            callback?.Invoke();
        }
    }
}
