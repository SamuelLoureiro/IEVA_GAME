using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using VARG.Player;

namespace VARG.Targets
{
    [RequireComponent(typeof(AudioSource))]
    public class Shrinkable : MonoBehaviour
    {
        [SerializeField] private float _shrinkDuration = 0.5f;
        [SerializeField] private AnimationCurve _shrinkCurve;

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _boingClip;
        [SerializeField] private AudioClip _popClip;

        private Vector3 _initialScale;
        private bool _isShrinking;

        public bool IsShrinking => _isShrinking;

        private void Awake()
        {
            _initialScale = transform.localScale;
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        public void StartShrink(Action onShrinkComplete)
        {
            if (_isShrinking) return;
            StartCoroutine(ShrinkRoutine(onShrinkComplete));
        }

        private IEnumerator ShrinkRoutine(Action onShrinkComplete)
        {
            _isShrinking = true;

            if (TryGetComponent<TargetAI>(out var targetAi))
            {
                targetAi.SetCaptured();
            }

            if (TryGetComponent<NavMeshAgent>(out var agent))
            {
                agent.enabled = false;
            }

            var colliders = GetComponentsInChildren<Collider>();
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            if (_audioSource != null && _boingClip != null)
            {
                _audioSource.PlayOneShot(_boingClip, 0.8f);
            }

            var elapsed = 0f;
            var hasCurve = _shrinkCurve != null && _shrinkCurve.length > 0;

            while (elapsed < _shrinkDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / _shrinkDuration);
                var scaleFactor = hasCurve ? _shrinkCurve.Evaluate(t) : Mathf.Lerp(1f, 0.05f, t);
                transform.localScale = _initialScale * scaleFactor;
                yield return null;
            }

            _isShrinking = false;

            if (CaptureFeedbackFX.Instance != null)
            {
                CaptureFeedbackFX.Instance.PlayCapturePoof(transform.position, Color.white);
            }

            if (_popClip != null)
            {
                AudioSource.PlayClipAtPoint(_popClip, transform.position, 0.85f);
            }

            onShrinkComplete?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
