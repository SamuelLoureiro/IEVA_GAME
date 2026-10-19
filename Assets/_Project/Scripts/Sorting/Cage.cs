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
        [SerializeField] private float _ejectForce = 8.5f;
        [SerializeField] private PenaltyConfig _penaltyConfig;

        [Header("Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _acceptClip;
        [SerializeField] private AudioClip _ejectClip;
        [SerializeField] private ParticleSystem _acceptParticles;

        private readonly List<SortableItem> _lockedItems = new();

        public TargetType AcceptedType => _acceptedType;
        public Transform SnapPoint => _snapPoint != null ? _snapPoint : transform;
        public int LockedCount => _lockedItems.Count;

        private void Awake()
        {
            if (_penaltyConfig == null) _penaltyConfig = Resources.Load<PenaltyConfig>("Data/PenaltyConfig");
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        }

        public void Configure(TargetType acceptedType, Transform snapPoint, Transform ejectPoint)
        {
            _acceptedType = acceptedType;
            _snapPoint = snapPoint;
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
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
            }

            item.transform.position = SnapPoint.position;
            item.transform.SetParent(transform, true);

            if (_audioSource != null && _acceptClip != null) _audioSource.PlayOneShot(_acceptClip);
            if (_acceptParticles != null) _acceptParticles.Play();

            GameEvents.TriggerCorrectCage(item.Data);
            if (SortingManager.Instance != null) SortingManager.Instance.NotifyItemResolved(item);
        }

        private void EjectItem(SortableItem item)
        {
            var rb = item.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = -transform.forward * _ejectForce + Vector3.up * 2f;

            if (_audioSource != null && _ejectClip != null) _audioSource.PlayOneShot(_ejectClip);

            var penalty = _penaltyConfig != null ? _penaltyConfig.WrongCagePenalty : 5f;
            if (TimerManager.Instance != null) TimerManager.Instance.ApplyPenalty(TimerPhase.Sorting, penalty);

            GameEvents.TriggerWrongCage(item.Data);
        }

        public void ConfigureFeedback(AudioClip acceptClip, AudioClip ejectClip, ParticleSystem acceptParticles, ParticleSystem ejectParticles)
        {
            _acceptClip = acceptClip;
            _ejectClip = ejectClip;
            _acceptParticles = acceptParticles;
        }
    }
}
