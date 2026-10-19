using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Mission;

namespace VARG.Sorting
{
    public class Cage : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private TargetType _acceptedType;
        [SerializeField] private Transform _snapPoint;
        [SerializeField] private Transform _ejectPoint;
        [SerializeField] private float _ejectForce = 8.5f;
        [SerializeField] private float _settleDelay = 0.20f;
        [SerializeField] private PenaltyConfig _penaltyConfig;

        [Header("Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _acceptClip;
        [SerializeField] private AudioClip _ejectClip;
        [SerializeField] private ParticleSystem _acceptParticles;
        [SerializeField] private ParticleSystem _ejectParticles;

        private readonly List<SortableItem> _lockedItems = new();
        private readonly HashSet<SortableItem> _rejectedUntilGrabbed = new();

        public TargetType AcceptedType => _acceptedType;
        public Transform SnapPoint => _snapPoint != null ? _snapPoint : transform;
        public Transform EjectPoint => _ejectPoint != null ? _ejectPoint : transform;
        public float EjectForce => _ejectForce;
        public int LockedCount => _lockedItems.Count;

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
        }

        public void Configure(TargetType acceptedType, Transform snapPoint, Transform ejectPoint)
        {
            _acceptedType = acceptedType;
            _snapPoint = snapPoint;
            _ejectPoint = ejectPoint;
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

            ValidateItem(item);
        }

        private void OnTriggerStay(Collider other)
        {
            ProcessTriggerStay(other);
        }

        private void ValidateItem(SortableItem item)
        {
            var isForbidden = IsItemForbidden(item);
            var isCorrectType = item.Type == _acceptedType;
            var isWithinQuota = _lockedItems.Count < GetRequiredQuota();
            var isValidDeposit = isCorrectType && !isForbidden && isWithinQuota;

            if (isValidDeposit)
            {
                AcceptItem(item);
            }
            else
            {
                EjectItem(item);
            }
        }

        private int GetRequiredQuota()
        {
            if (MissionManager.Instance != null && MissionManager.Instance.RequiredCount > 0)
            {
                return MissionManager.Instance.GetRequiredCountForType(_acceptedType);
            }
            return int.MaxValue;
        }

        private bool IsItemForbidden(SortableItem item)
        {
            if (MissionManager.Instance != null && MissionManager.Instance.RequiredCount > 0)
            {
                return item.Type == MissionManager.Instance.ForbiddenType;
            }
            return item.Item.Result == CaptureResult.NotInList;
        }

        private void AcceptItem(SortableItem item)
        {
            _rejectedUntilGrabbed.Remove(item);
            item.MarkSorted();
            _lockedItems.Add(item);

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

            if (_audioSource != null && _acceptClip != null)
            {
                _audioSource.PlayOneShot(_acceptClip);
            }

            if (_acceptParticles != null)
            {
                _acceptParticles.Play();
            }

            GameEvents.TriggerCorrectCage(item.Data);

            if (SortingManager.Instance != null)
            {
                SortingManager.Instance.NotifyItemResolved(item);
            }
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

        private void EjectItem(SortableItem item)
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
                rb.AddForce(launchDir * _ejectForce, ForceMode.VelocityChange);
            }

            if (_audioSource != null && _ejectClip != null)
            {
                _audioSource.PlayOneShot(_ejectClip);
            }

            if (_ejectParticles != null)
            {
                _ejectParticles.transform.position = safeEjectPos;
                _ejectParticles.Play();
            }

            var penalty = _penaltyConfig != null ? _penaltyConfig.WrongCagePenalty : 5f;
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.ApplyPenalty(TimerPhase.Sorting, penalty);
            }


            GameEvents.TriggerWrongCage(item.Data);
        }

        public void ConfigureFeedback(AudioClip acceptClip, AudioClip ejectClip, ParticleSystem acceptParticles, ParticleSystem ejectParticles)
        {
            _acceptClip = acceptClip;
            _ejectClip = ejectClip;
            _acceptParticles = acceptParticles;
            _ejectParticles = ejectParticles;
        }
    }
}
