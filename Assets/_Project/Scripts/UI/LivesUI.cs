using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Core;
using VARG.Mission;

namespace VARG.UI
{
    public class LivesUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private Image[] _lifeIcons;

        [Header("Colors & Animation")]
        [SerializeField] private Color _activeLifeColor = new Color(0.2f, 1f, 0.5f);
        [SerializeField] private Color _lostLifeColor = new Color(0.3f, 0.3f, 0.3f, 0.35f);
        [SerializeField] private Color _flashColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private float _flashDuration = 0.5f;

        private Coroutine _flashRoutine;

        private void OnEnable()
        {
            if (LivesManager.HasInstance)
            {
                LivesManager.Instance.OnLivesChanged += HandleLivesChanged;
            }

            GameEvents.OnLifeLost += HandleLifeLost;

            RefreshLivesDisplay();
        }

        private void OnDisable()
        {
            if (LivesManager.HasInstance)
            {
                LivesManager.Instance.OnLivesChanged -= HandleLivesChanged;
            }

            GameEvents.OnLifeLost -= HandleLifeLost;

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }
        }

        private void Start()
        {
            RefreshLivesDisplay();
        }

        private void HandleLivesChanged(int lives)
        {
            RefreshLivesDisplay();
        }

        private void HandleLifeLost(int livesLeft)
        {
            RefreshLivesDisplay();
            TriggerLifeLostFlash(livesLeft);
        }

        public void RefreshLivesDisplay()
        {
            var currentLives = LivesManager.HasInstance ? LivesManager.Instance.Lives : LivesManager.DefaultStartingLives;

            if (_livesText != null)
            {
                _livesText.text = $"Lives: {currentLives}";
            }

            if (_lifeIcons != null)
            {
                for (var i = 0; i < _lifeIcons.Length; i++)
                {
                    if (_lifeIcons[i] == null) continue;
                    _lifeIcons[i].color = i < currentLives ? _activeLifeColor : _lostLifeColor;
                }
            }
        }

        private void TriggerLifeLostFlash(int lostLifeIndex)
        {
            if (!gameObject.activeInHierarchy) return;

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(FlashLifeLostRoutine(lostLifeIndex));
        }

        private IEnumerator FlashLifeLostRoutine(int iconIndex)
        {
            Image targetIcon = null;
            if (_lifeIcons != null && iconIndex >= 0 && iconIndex < _lifeIcons.Length)
            {
                targetIcon = _lifeIcons[iconIndex];
            }

            var elapsed = 0f;
            var halfDuration = _flashDuration * 0.5f;

            if (targetIcon != null)
            {
                targetIcon.color = _flashColor;
            }

            if (_livesText != null)
            {
                _livesText.color = _flashColor;
            }

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / halfDuration);

                if (targetIcon != null)
                {
                    targetIcon.color = Color.Lerp(_flashColor, _lostLifeColor, t);
                }

                if (_livesText != null)
                {
                    _livesText.color = Color.Lerp(_flashColor, Color.white, t);
                }

                yield return null;
            }

            if (targetIcon != null)
            {
                targetIcon.color = _lostLifeColor;
            }

            if (_livesText != null)
            {
                _livesText.color = Color.white;
            }

            _flashRoutine = null;
        }
    }
}
