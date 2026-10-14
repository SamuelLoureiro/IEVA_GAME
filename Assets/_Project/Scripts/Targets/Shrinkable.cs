using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using VARG.Player;

namespace VARG.Targets
{
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
        public AudioSource AudioSource => _audioSource;

        private void Awake()
        {
            _initialScale = transform.localScale;

            if (_shrinkCurve == null || _shrinkCurve.length == 0)
            {
                _shrinkCurve = CreateDefaultBoingCurve();
            }

            EnsureAudioSource();
        }

        public void EnsureAudioSource()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (_audioSource != null)
            {
                _audioSource.spatialBlend = 1f;
                _audioSource.playOnAwake = false;
                _audioSource.minDistance = 2f;
                _audioSource.maxDistance = 25f;
            }
        }

        private static AnimationCurve CreateDefaultBoingCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f, 0f, 2f),
                new Keyframe(0.2f, 1.25f, 0f, -2f),
                new Keyframe(0.6f, 0.4f, -2f, -1f),
                new Keyframe(1f, 0.05f, 0f, 0f)
            );
        }

        public void StartShrink(Action onShrinkComplete)
        {
            if (_isShrinking) return;

            StartCoroutine(ShrinkRoutine(onShrinkComplete));
        }

        private IEnumerator ShrinkRoutine(Action onShrinkComplete)
        {
            _isShrinking = true;

            var targetAi = GetComponent<TargetAI>();
            if (targetAi != null) targetAi.SetCaptured();

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            var colliders = GetComponentsInChildren<Collider>();
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            if (_audioSource != null)
            {
                var boing = _boingClip != null ? _boingClip : ProceduralAudioUtility.GetBoingShrinkClip();
                _audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                _audioSource.PlayOneShot(boing, 0.8f);
            }

            var elapsed = 0f;
            while (elapsed < _shrinkDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / _shrinkDuration);
                var curveMultiplier = _shrinkCurve.Evaluate(t);
                transform.localScale = _initialScale * curveMultiplier;
                yield return null;
            }

            transform.localScale = _initialScale * 0.05f;
            _isShrinking = false;

            if (CaptureFeedbackFX.Instance != null)
            {
                CaptureFeedbackFX.Instance.PlayCapturePoof(transform.position, Color.white);
            }

            var pop = _popClip != null ? _popClip : ProceduralAudioUtility.GetCapturePopClip();
            if (pop != null)
            {
                AudioSource.PlayClipAtPoint(pop, transform.position, 0.85f);
            }

            onShrinkComplete?.Invoke();
            gameObject.SetActive(false);
        }

        public void ResetState()
        {
            _isShrinking = false;
            transform.localScale = _initialScale;

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = true;

            var targetAi = GetComponent<TargetAI>();
            if (targetAi != null) targetAi.ResetState();

            var colliders = GetComponentsInChildren<Collider>();
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = true;
            }
        }
    }
}
