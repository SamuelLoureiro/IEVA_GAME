using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Input;

namespace VARG.UI
{
    public class DialogueBox : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private GameObject _continueIndicator;
        [SerializeField] private Button _advanceButton;

        private IPlayerInput _playerInput;
        private Action _onDialogueComplete;
        private bool _isLineFinished;

        public bool IsLineFinished => _isLineFinished;

        private void Awake()
        {
            if (_advanceButton != null)
            {
                _advanceButton.onClick.AddListener(HandleAdvanceRequested);
            }
        }

        private void Start()
        {
            _playerInput = InputManager.HasInstance ? InputManager.Instance.Player : new KeyboardMouseInput();
        }

        private void Update()
        {
            if (_playerInput == null) return;

            if (_playerInput.InteractPressed || _playerInput.FirePressed)
            {
                HandleAdvanceRequested();
            }
        }

        public void Say(string speakerName, string text, Action onComplete = null)
        {
            _isLineFinished = false;
            _onDialogueComplete = onComplete;

            if (_speakerNameText != null)
            {
                _speakerNameText.text = speakerName ?? string.Empty;
            }

            if (_dialogueText != null)
            {
                _dialogueText.text = text ?? string.Empty;
            }

            _isLineFinished = true;
            if (_continueIndicator != null)
            {
                _continueIndicator.SetActive(true);
            }
        }

        public void HandleAdvanceRequested()
        {
            if (_isLineFinished)
            {
                var callback = _onDialogueComplete;
                _onDialogueComplete = null;
                callback?.Invoke();
            }
        }
    }
}
