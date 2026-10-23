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
        [SerializeField] private PenaltyConfig _penaltyConfig;

        [Header("Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _acceptClip;
        [SerializeField] private AudioClip _ejectClip;

        private readonly List<SortableItem> _lockedItems = new();

        public TargetType AcceptedType => _acceptedType;
        public Transform SnapPoint => _snapPoint != null ? _snapPoint : transform;
        public int LockedCount => _lockedItems.Count;

        private void Awake()
        {
            if (_penaltyConfig == null) _penaltyConfig = Resources.Load<PenaltyConfig>("Data/PenaltyConfig");
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        }

        public void ProcessTriggerStay(Collider other)
        {
            var item = other.GetComponentInParent<SortableItem>();
            if (item == null || item.IsSorted || item.IsHeld) return;

            var isForbidden = MissionManager.Instance != null && item.Type == MissionManager.Instance.ForbiddenType;
            var quota = MissionManager.Instance != null ? MissionManager.Instance.GetRequiredCountForType(_acceptedType) : int.MaxValue;
            var isCorrect = item.Type == _acceptedType && !isForbidden && _lockedItems.Count < quota;

            if (isCorrect)
            {
                AcceptItem(item);
            }
            else
            {
                EjectItem(item);
            }
        }

        private void OnTriggerStay(Collider other) => ProcessTriggerStay(other);

        private void AcceptItem(SortableItem item)
        {
            item.MarkSorted();
            _lockedItems.Add(item);

            var grabbable = item.GetComponent<Grabbable>();
            if (grabbable != null) grabbable.enabled = false;

            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                var snap = SnapPoint.position;
                item.transform.position = new Vector3(
                    snap.x + Random.Range(-0.2f, 0.2f),
                    snap.y + 0.9f,
                    snap.z + Random.Range(-0.2f, 0.2f)
                );
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            item.transform.SetParent(transform, true);

            if (_audioSource != null && _acceptClip != null) _audioSource.PlayOneShot(_acceptClip);

            GameEvents.TriggerCorrectCage(item.Data);
            if (SortingManager.Instance != null) SortingManager.Instance.NotifyItemResolved(item);
        }

        private void EjectItem(SortableItem item)
        {
            item.ReturnToCenter();

            if (_audioSource != null && _ejectClip != null) _audioSource.PlayOneShot(_ejectClip);

            var penalty = _penaltyConfig != null ? _penaltyConfig.WrongCagePenalty : 5f;
            if (TimerManager.Instance != null) TimerManager.Instance.ApplyPenalty(TimerPhase.Sorting, penalty);

            GameEvents.TriggerWrongCage(item.Data);
        }
    }
}
