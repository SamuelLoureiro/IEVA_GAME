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

        [Header("Colors")]
        [SerializeField] private Color _activeLifeColor = new Color(0.2f, 1f, 0.5f);
        [SerializeField] private Color _lostLifeColor = new Color(0.3f, 0.3f, 0.3f, 0.35f);

        private void OnEnable()
        {
            if (LivesManager.HasInstance) LivesManager.Instance.OnLivesChanged += HandleLivesChanged;
            GameEvents.OnLifeLost += HandleLifeLost;
            RefreshLivesDisplay();
        }

        private void OnDisable()
        {
            if (LivesManager.HasInstance) LivesManager.Instance.OnLivesChanged -= HandleLivesChanged;
            GameEvents.OnLifeLost -= HandleLifeLost;
        }

        private void Start() => RefreshLivesDisplay();

        private void HandleLivesChanged(int lives) => RefreshLivesDisplay();
        private void HandleLifeLost(int livesLeft) => RefreshLivesDisplay();

        public void RefreshLivesDisplay()
        {
            var currentLives = LivesManager.HasInstance ? LivesManager.Instance.Lives : LivesManager.DefaultStartingLives;

            if (_livesText != null) _livesText.text = $"Lives: {currentLives}";

            if (_lifeIcons != null)
            {
                for (var i = 0; i < _lifeIcons.Length; i++)
                {
                    if (_lifeIcons[i] != null) _lifeIcons[i].color = i < currentLives ? _activeLifeColor : _lostLifeColor;
                }
            }
        }
    }
}
