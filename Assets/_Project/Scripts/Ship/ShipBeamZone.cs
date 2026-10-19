using System.Collections;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Mission;
using VARG.Player;

namespace VARG.Ship
{
    [RequireComponent(typeof(Collider))]
    public class ShipBeamZone : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private string _playerTag = "Player";

        [Header("Cooldown & Control")]
        [SerializeField] private float _deniedCooldown = 2f;
        [SerializeField] private bool _freezePlayerOnAscent = true;

        [Header("Visual Feedback")]
        [SerializeField] private Light _spotlight;
        [SerializeField] private Color _activeColor = new Color(0.2f, 1f, 0.5f);
        [SerializeField] private Color _deniedColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private float _feedbackDuration = 0.6f;

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _deniedClip;
        [SerializeField] private AudioClip _ascendClip;

        private float _lastDeniedTime = -10f;
        private bool _isTransitioning;

        private void Awake()
        {
            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isTransitioning || !IsPlayer(other)) return;

            var isComplete = MissionManager.HasInstance && MissionManager.Instance.IsComplete;
            if (isComplete)
            {
                AscendToShip(other);
            }
            else
            {
                DenyAscent();
            }
        }

        private bool IsPlayer(Collider other)
        {
            return other.CompareTag(_playerTag) || other.GetComponentInParent<ETPlayerController>() != null;
        }

        private void AscendToShip(Collider playerCollider)
        {
            _isTransitioning = true;

            if (_freezePlayerOnAscent && playerCollider != null)
            {
                var controller = playerCollider.GetComponentInParent<ETPlayerController>();
                if (controller != null)
                {
                    controller.CanMove = false;
                }
            }

            if (_audioSource != null && _ascendClip != null)
            {
                _audioSource.PlayOneShot(_ascendClip);
            }

            if (TimerManager.HasInstance)
            {
                TimerManager.Instance.StopTimer(TimerPhase.Capture);
            }

            if (GameManager.HasInstance)
            {
                GameManager.Instance.GoToSorting();
            }
        }

        private void DenyAscent()
        {
            if (Time.time - _lastDeniedTime < _deniedCooldown) return;

            _lastDeniedTime = Time.time;
            GameEvents.TriggerBeamDenied();

            if (_audioSource != null && _deniedClip != null)
            {
                _audioSource.PlayOneShot(_deniedClip);
            }

            if (_spotlight != null)
            {
                StopAllCoroutines();
                StartCoroutine(FlashLightRoutine());
            }
        }

        private IEnumerator FlashLightRoutine()
        {
            _spotlight.color = _deniedColor;
            yield return new WaitForSeconds(_feedbackDuration);
            _spotlight.color = _activeColor;
        }
    }
}
