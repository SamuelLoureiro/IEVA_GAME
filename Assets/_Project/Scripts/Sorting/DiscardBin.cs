using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Mission;

namespace VARG.Sorting
{
    public class DiscardBin : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform _destroyPoint;
        [SerializeField] private float _settleDelay = 0.20f;
        [SerializeField] private PenaltyConfig _penaltyConfig;

        [Header("Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _shredClip;
        [SerializeField] private AudioClip _wrongDiscardClip;
        [SerializeField] private ParticleSystem _shredParticles;

        private readonly HashSet<SortableItem> _rejectedUntilGrabbed = new();
        private Cage[] _cages;

        public Transform DestroyPoint => _destroyPoint != null ? _destroyPoint : transform;

        private void Awake()
        {
            if (_penaltyConfig == null)
            {
                _penaltyConfig = Resources.Load<PenaltyConfig>("Data/PenaltyConfig");
            }

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            _cages = FindObjectsByType<Cage>(FindObjectsSortMode.None);
        }

        public void Configure(Transform destroyPoint)
        {
            _destroyPoint = destroyPoint;
        }

        public void ProcessTriggerStay(Collider other)
        {
            var item = other.GetComponentInParent<SortableItem>();
            if (item == null || item.IsSorted) return;

            if (item.IsHeld)
            {
                _rejectedUntilGrabbed.Remove(item);
                return;
            }

            if (_rejectedUntilGrabbed.Contains(item)) return;

            if (item.TimeSinceReleased < _settleDelay) return;

            ValidateDiscard(item);
        }

        private void OnTriggerStay(Collider other)
        {
            ProcessTriggerStay(other);
        }

        private void ValidateDiscard(SortableItem item)
        {
            var isValidDiscard = IsValidDiscard(item);

            if (isValidDiscard)
            {
                AcceptDiscard(item);
            }
            else
            {
                EjectDiscard(item);
            }
        }

        private void AcceptDiscard(SortableItem item)
        {
            _rejectedUntilGrabbed.Remove(item);
            item.MarkSorted();

            var grabbable = item.GetComponent<Grabbable>();
            if (grabbable != null)
            {
                grabbable.enabled = false;
            }


            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                var vy = Mathf.Min(rb.linearVelocity.y, 0.5f);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.35f, vy, rb.linearVelocity.z * 0.35f);
                rb.angularVelocity *= 0.5f;
                StartCoroutine(SettleRoutine(rb, item.transform));
            }

            if (_audioSource != null && _shredClip != null)
            {
                _audioSource.PlayOneShot(_shredClip);
            }

            if (_shredParticles != null)
            {
                _shredParticles.Play();
            }

            GameEvents.TriggerCorrectDiscard(item.Data);

            if (SortingManager.Instance != null)
            {
                SortingManager.Instance.NotifyItemResolved(item);
            }
        }

        private void EjectDiscard(SortableItem item)
        {
            _rejectedUntilGrabbed.Add(item);

            var safeEjectPos = transform.TransformPoint(new Vector3(0f, 0.7f, -2.0f));

            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                item.transform.position = safeEjectPos;

                var launchDir = (-transform.forward * 0.82f + Vector3.up * 0.55f).normalized;
                rb.AddForce(launchDir * 8.5f, ForceMode.VelocityChange);
            }

            if (_audioSource != null && _wrongDiscardClip != null)
            {
                _audioSource.PlayOneShot(_wrongDiscardClip);
            }

            var penalty = _penaltyConfig != null ? _penaltyConfig.WrongDiscardPenalty : 5f;
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.ApplyPenalty(TimerPhase.Sorting, penalty);
            }

            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.LoseLife();
            }

            GameEvents.TriggerWrongDiscard(item.Data);
        }

        public void ConfigureFeedback(AudioClip shredClip, AudioClip wrongDiscardClip, ParticleSystem shredParticles)
        {
            _shredClip = shredClip;
            _wrongDiscardClip = wrongDiscardClip;
            _shredParticles = shredParticles;
        }

        private IEnumerator SettleRoutine(Rigidbody rb, Transform itemTransform)
        {
            var elapsed = 0f;
            const float timeout = 3.0f;
            while (elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
                if (rb == null) yield break;
                if (elapsed > 0.4f && rb.linearVelocity.sqrMagnitude < 0.04f)
                {
                    break;
                }
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                if (itemTransform != null)
                {
                    itemTransform.SetParent(transform, true);
                }
            }
        }

        private bool IsValidDiscard(SortableItem item)
        {
            if (IsItemForbidden(item))
            {
                return true;
            }

            if (MissionManager.Instance == null || MissionManager.Instance.RequiredCount == 0)
            {
                return false;
            }

            var required = MissionManager.Instance.GetRequiredCountForType(item.Type);
            if (required == 0)
            {
                return true;
            }

            var cagedCount = 0;
            if (_cages == null || _cages.Length == 0)
            {
                _cages = FindObjectsByType<Cage>(FindObjectsSortMode.None);
            }

            if (_cages != null)
            {
                for (var i = 0; i < _cages.Length; i++)
                {
                    if (_cages[i] != null && _cages[i].AcceptedType == item.Type)
                    {
                        cagedCount += _cages[i].LockedCount;
                    }
                }
            }

            var availableCount = SortingManager.Instance != null
                ? SortingManager.Instance.GetUnsortedCountForType(item.Type)
                : 0;

            return (cagedCount + availableCount) > required;
        }

        private bool IsItemForbidden(SortableItem item)
        {
            if (MissionManager.Instance != null && MissionManager.Instance.RequiredCount > 0)
            {
                return item.Type == MissionManager.Instance.ForbiddenType;
            }
            return item.Item.Result == CaptureResult.NotInList;
        }
    }
}
