using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace VARG.Targets
{
    public class Shrinkable : MonoBehaviour
    {
        [SerializeField] private float _shrinkDuration = 0.5f;
        [SerializeField] private AnimationCurve _shrinkCurve;

        private Vector3 _initialScale;
        private bool _isShrinking;

        public bool IsShrinking => _isShrinking;

        private void Awake()
        {
            _initialScale = transform.localScale;

            if (_shrinkCurve == null || _shrinkCurve.length == 0)
            {
                _shrinkCurve = CreateDefaultBoingCurve();
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

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            var colliders = GetComponentsInChildren<Collider>();
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
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

            onShrinkComplete?.Invoke();
            gameObject.SetActive(false);
        }

        public void ResetState()
        {
            _isShrinking = false;
            transform.localScale = _initialScale;

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = true;

            var colliders = GetComponentsInChildren<Collider>();
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = true;
            }
        }
    }
}
