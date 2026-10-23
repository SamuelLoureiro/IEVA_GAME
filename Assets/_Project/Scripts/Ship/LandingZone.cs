using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using VARG.Core;
using VARG.Input;

namespace VARG.Ship
{
    [RequireComponent(typeof(Collider))]
    public class LandingZone : MonoBehaviour
    {
        private const string CityHuntSceneName = "03_CityHunt";

        [SerializeField] private float _maxLandingSpeed = 3.5f;
        [SerializeField] private TextMeshProUGUI _promptText;
        [SerializeField] private string _readyPromptMessage = "Press E / Shake phone to descend";
        [SerializeField] private string _slowDownPromptMessage = "Slow down over the city...";
        [SerializeField] private Light _zoneLight;
        [SerializeField] private Color _idleColor = new Color(0.2f, 0.7f, 1f);
        [SerializeField] private Color _readyColor = new Color(0.2f, 1f, 0.4f);

        private ShipController _shipInZone;
        private bool _isDescending;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            UpdateFeedback(false, false);
        }

        private void Update()
        {
            if (_isDescending || _shipInZone == null) return;

            var isReady = _shipInZone.CurrentSpeed <= _maxLandingSpeed;
            UpdateFeedback(true, isReady);

            if (isReady && InputManager.HasInstance && InputManager.Instance.Ship.DescendPressed)
            {
                DescendToCity();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isDescending) return;

            var ship = other.GetComponentInParent<ShipController>();
            if (ship != null)
            {
                _shipInZone = ship;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_shipInZone != null && other.GetComponentInParent<ShipController>() == _shipInZone)
            {
                _shipInZone = null;
                UpdateFeedback(false, false);
            }
        }

        private void DescendToCity()
        {
            _isDescending = true;
            UpdateFeedback(false, false);
            _shipInZone.StopImmediately();

            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.Load(CityHuntSceneName);
            }
            else
            {
                SceneManager.LoadScene(CityHuntSceneName);
            }
        }

        private void UpdateFeedback(bool inZone, bool isReady)
        {
            if (_zoneLight != null)
            {
                _zoneLight.color = isReady ? _readyColor : _idleColor;
            }

            if (_promptText != null)
            {
                _promptText.gameObject.SetActive(inZone);
                if (inZone)
                {
                    _promptText.text = isReady ? _readyPromptMessage : _slowDownPromptMessage;
                }
            }
        }
    }
}
