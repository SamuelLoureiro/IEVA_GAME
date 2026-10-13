using UnityEngine;
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
        [SerializeField] private AudioSource _audioSource;

        private IPlayerInput _playerInput;
        private float _nextFireTime;

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

            _audioSource.pitch = Random.Range(1.1f, 1.3f);
            _audioSource.Play();
        }

        public void SetInputSource(IPlayerInput inputSource)
        {
            _playerInput = inputSource;
        }
    }
}
