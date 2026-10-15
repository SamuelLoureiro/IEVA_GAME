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
        [SerializeField] private Renderer _padRenderer;
        [SerializeField] private Color _activeColor = new Color(0.2f, 1f, 0.5f);
        [SerializeField] private Color _deniedColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private float _feedbackDuration = 0.6f;

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _deniedClip;
        [SerializeField] private AudioClip _ascendClip;

        private float _lastDeniedTime = -10f;
        private bool _isTransitioning;
        private Coroutine _flashRoutine;
        private Color _originalLightColor;
        private Color _originalPadColor;
        private bool _hasOriginalColors;
        private MaterialPropertyBlock _propBlock;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public bool IsTransitioning => _isTransitioning;

        private void Awake()
        {
            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }

            _propBlock = new MaterialPropertyBlock();
            CacheOriginalColors();
        }

        private void OnEnable()
        {
            GameEvents.OnMissionComplete += HandleMissionComplete;
        }

        private void OnDisable()
        {
            GameEvents.OnMissionComplete -= HandleMissionComplete;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other)) return;
            EvaluateBeamEntry(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_isTransitioning) return;
            if (!IsPlayer(other)) return;

            if (MissionManager.HasInstance && MissionManager.Instance.IsComplete)
            {
                EvaluateBeamEntry(other);
            }
        }

        private bool IsPlayer(Collider other)
        {
            return other.CompareTag(_playerTag) || other.GetComponentInParent<ETPlayerController>() != null;
        }

        private void EvaluateBeamEntry(Collider playerCollider)
        {
            if (_isTransitioning) return;

            var isComplete = MissionManager.HasInstance && MissionManager.Instance.IsComplete;

            if (isComplete)
            {
                AscendToShip(playerCollider);
            }
            else
            {
                DenyAscent();
            }
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
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("04_ShipSorting");
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

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }
            _flashRoutine = StartCoroutine(FlashDeniedFeedbackRoutine());
        }

        private void HandleMissionComplete()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
                RestoreColors();
            }
        }

        private void CacheOriginalColors()
        {
            _originalLightColor = _spotlight != null ? _spotlight.color : _activeColor;

            if (_padRenderer != null && _padRenderer.sharedMaterial != null)
            {
                if (_padRenderer.sharedMaterial.HasProperty(BaseColorId))
                {
                    _originalPadColor = _padRenderer.sharedMaterial.GetColor(BaseColorId);
                }
                else if (_padRenderer.sharedMaterial.HasProperty(ColorId))
                {
                    _originalPadColor = _padRenderer.sharedMaterial.GetColor(ColorId);
                }
                else
                {
                    _originalPadColor = _activeColor;
                }
            }
            else
            {
                _originalPadColor = _activeColor;
            }

            _hasOriginalColors = true;
        }

        private IEnumerator FlashDeniedFeedbackRoutine()
        {
            ApplyColor(_deniedColor);
            yield return new WaitForSeconds(_feedbackDuration);
            RestoreColors();
            _flashRoutine = null;
        }

        private void ApplyColor(Color color)
        {
            if (_spotlight != null)
            {
                _spotlight.color = color;
            }

            if (_padRenderer != null)
            {
                _padRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(BaseColorId, color);
                _propBlock.SetColor(ColorId, color);
                _padRenderer.SetPropertyBlock(_propBlock);
            }
        }

        private void RestoreColors()
        {
            if (!_hasOriginalColors) return;

            if (_spotlight != null)
            {
                _spotlight.color = _originalLightColor;
            }

            if (_padRenderer != null)
            {
                _padRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(BaseColorId, _originalPadColor);
                _propBlock.SetColor(ColorId, _originalPadColor);
                _padRenderer.SetPropertyBlock(_propBlock);
            }
        }
    }
}
