using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Input;
using VARG.Targets;

namespace VARG.Player
{
    [RequireComponent(typeof(AudioSource))]
    public class ShrinkGun : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor _interactor;
        [SerializeField] private ShrinkBeamFX _beamFX;
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private float _cooldown = 0.6f;
        [SerializeField] private float _maxRange = 25f;

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _fireClip;
        [SerializeField] private AudioClip _captureSuccessClip;
        [SerializeField] private AudioClip _captureErrorClip;
        [SerializeField] private AudioClip _missionCompleteClip;

        private IPlayerInput _playerInput;
        private float _nextFireTime;

        private void OnEnable()
        {
            GameEvents.OnCorrectCapture += HandleCorrectCapture;
            GameEvents.OnOutOfOrderCapture += HandleOutOfOrderCapture;
            GameEvents.OnNotInListCapture += HandleNotInListCapture;
            GameEvents.OnMissionComplete += HandleMissionComplete;
        }

        private void OnDisable()
        {
            GameEvents.OnCorrectCapture -= HandleCorrectCapture;
            GameEvents.OnOutOfOrderCapture -= HandleOutOfOrderCapture;
            GameEvents.OnNotInListCapture -= HandleNotInListCapture;
            GameEvents.OnMissionComplete -= HandleMissionComplete;
        }

        private void Start()
        {
            if (_interactor == null)
            {
                _interactor = GetComponentInParent<PlayerInteractor>() ?? Object.FindFirstObjectByType<PlayerInteractor>();
            }

            if (_beamFX == null)
            {
                _beamFX = GetComponentInChildren<ShrinkBeamFX>() ?? GetComponentInParent<ShrinkBeamFX>();
            }

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            _playerInput = InputManager.HasInstance ? InputManager.Instance.Player : new KeyboardMouseInput();
        }

        private void Update()
        {
            if (_playerInput == null || !_playerInput.FirePressed) return;
            if (Time.time < _nextFireTime) return;

            _nextFireTime = Time.time + _cooldown;
            Fire();
        }

        private void Fire()
        {
            PlayClip(_fireClip, 0.7f);

            var camTransform = _interactor != null ? _interactor.transform : transform;
            var origin = _muzzlePoint != null
                ? _muzzlePoint.position
                : camTransform.position + camTransform.forward * 0.3f - camTransform.up * 0.15f;

            var endPoint = origin + camTransform.forward * _maxRange;

            if (_interactor != null && _interactor.CurrentTarget is Target target)
            {
                endPoint = _interactor.LastHit.point != Vector3.zero
                    ? _interactor.LastHit.point
                    : target.transform.position;

                target.StartShrinkCapture();
            }
            else if (_interactor != null && _interactor.LastHit.collider != null)
            {
                endPoint = _interactor.LastHit.point;
            }

            if (_beamFX != null)
            {
                _beamFX.PlayBeam(origin, endPoint);
            }
        }

        private void HandleCorrectCapture(TargetData data) => PlayClip(_captureSuccessClip, 0.85f);

        private void HandleOutOfOrderCapture(TargetData captured, TargetData expected) => PlayClip(_captureErrorClip, 0.9f);

        private void HandleNotInListCapture(TargetData data) => PlayClip(_captureErrorClip, 0.9f);

        private void HandleMissionComplete() => PlayClip(_missionCompleteClip, 0.95f);

        private void PlayClip(AudioClip clip, float volume)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip, volume);
            }
        }
    }
}
