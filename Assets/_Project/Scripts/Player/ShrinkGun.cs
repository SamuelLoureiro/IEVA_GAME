using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Input;
using VARG.Targets;

namespace VARG.Player
{
    public class ShrinkGun : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor _interactor;
        [SerializeField] private ShrinkBeamFX _beamFX;
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private float _cooldown = 0.6f;
        [SerializeField] private float _maxRange = 25f;
        [SerializeField] private float _scareRadius = 8f;
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
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            _playerInput = InputManager.HasInstance ? InputManager.Instance.Player : new KeyboardMouseInput();
        }

        private void Update()
        {
            HandleFiring();
        }

        private void HandleFiring()
        {
            if (_playerInput == null || !_playerInput.FirePressed) return;
            if (Time.time < _nextFireTime) return;

            _nextFireTime = Time.time + _cooldown;
            Fire();
        }

        private void Fire()
        {
            PlayFireAudio();

            var camTransform = _interactor != null && _interactor.GetComponent<Camera>() != null
                ? _interactor.transform
                : Camera.main != null ? Camera.main.transform : transform;

            var origin = _muzzlePoint != null ? _muzzlePoint.position : camTransform.position + camTransform.forward * 0.3f - camTransform.up * 0.15f;
            var targetDetected = _interactor != null ? _interactor.CurrentTarget : null;

            if (targetDetected is Target target)
            {
                var hitPoint = _interactor.LastHit.point;
                if (hitPoint == Vector3.zero) hitPoint = target.transform.position;

                if (_beamFX != null)
                {
                    _beamFX.PlayBeam(origin, hitPoint);
                }

                target.StartShrinkCapture();
            }
            else if (_interactor != null && _interactor.LastHit.collider != null)
            {
                var hitPoint = _interactor.LastHit.point;
                if (_beamFX != null)
                {
                    _beamFX.PlayBeam(origin, hitPoint);
                }

                ScareNearbyTargets(hitPoint);
            }
            else
            {
                var endPoint = origin + camTransform.forward * _maxRange;
                if (_beamFX != null)
                {
                    _beamFX.PlayBeam(origin, endPoint);
                }
            }
        }

        private void ScareNearbyTargets(Vector3 center)
        {
            var colliders = Physics.OverlapSphere(center, _scareRadius);
            for (var i = 0; i < colliders.Length; i++)
            {
                var target = colliders[i].GetComponentInParent<Target>();
                if (target != null)
                {
                    var targetAi = target.GetComponent<TargetAI>();
                    if (targetAi != null)
                    {
                        targetAi.AlertDanger(center);
                    }
                }
            }
        }

        private void PlayFireAudio()
        {
            if (_audioSource == null) return;

            var clip = _fireClip != null ? _fireClip : ProceduralAudioUtility.GetLaserShootClip();
            _audioSource.pitch = Random.Range(1.05f, 1.25f);
            _audioSource.PlayOneShot(clip, 0.7f);
        }

        private void HandleCorrectCapture(TargetData data)
        {
            if (_audioSource == null) return;

            var clip = _captureSuccessClip != null ? _captureSuccessClip : ProceduralAudioUtility.GetCaptureSuccessClip();
            _audioSource.pitch = 1.0f;
            _audioSource.PlayOneShot(clip, 0.85f);
        }

        private void HandleOutOfOrderCapture(TargetData captured, TargetData expected)
        {
            PlayErrorSound();
        }

        private void HandleNotInListCapture(TargetData data)
        {
            PlayErrorSound();
        }

        private void PlayErrorSound()
        {
            if (_audioSource == null) return;

            var clip = _captureErrorClip != null ? _captureErrorClip : ProceduralAudioUtility.GetCaptureErrorClip();
            _audioSource.pitch = 1.0f;
            _audioSource.PlayOneShot(clip, 0.9f);
        }

        private void HandleMissionComplete()
        {
            if (_audioSource == null) return;

            var clip = _missionCompleteClip != null ? _missionCompleteClip : ProceduralAudioUtility.GetMissionCompleteClip();
            _audioSource.pitch = 1.0f;
            _audioSource.PlayOneShot(clip, 0.95f);
        }

        public void SetInputSource(IPlayerInput inputSource)
        {
            _playerInput = inputSource;
        }
    }
}
