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
        [SerializeField] private PenaltyConfig _penaltyConfig;

        [Header("Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _shredClip;
        [SerializeField] private AudioClip _wrongDiscardClip;
        [SerializeField] private ParticleSystem _shredParticles;

        public Transform DestroyPoint => _destroyPoint != null ? _destroyPoint : transform;

        private void Awake()
        {
            if (_penaltyConfig == null) _penaltyConfig = Resources.Load<PenaltyConfig>("Data/PenaltyConfig");
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        }

        public void Configure(Transform destroyPoint) => _destroyPoint = destroyPoint;

        public void ProcessTriggerStay(Collider other)
        {
            var item = other.GetComponentInParent<SortableItem>();
            if (item == null || item.IsSorted || item.IsHeld) return;

            if (IsValidDiscard(item))
            {
                AcceptDiscard(item);
            }
            else
            {
                EjectDiscard(item);
            }
        }

        private void OnTriggerStay(Collider other) => ProcessTriggerStay(other);

        private void AcceptDiscard(SortableItem item)
        {
            item.MarkSorted();

            var grabbable = item.GetComponent<Grabbable>();
            if (grabbable != null) grabbable.enabled = false;

            Destroy(item.gameObject, 0.2f);

            if (_audioSource != null && _shredClip != null) _audioSource.PlayOneShot(_shredClip);
            if (_shredParticles != null) _shredParticles.Play();

            GameEvents.TriggerCorrectDiscard(item.Data);
            if (SortingManager.Instance != null) SortingManager.Instance.NotifyItemResolved(item);
        }

        private void EjectDiscard(SortableItem item)
        {
            var rb = item.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = -transform.forward * 8.5f + Vector3.up * 2f;

            if (_audioSource != null && _wrongDiscardClip != null) _audioSource.PlayOneShot(_wrongDiscardClip);

            var penalty = _penaltyConfig != null ? _penaltyConfig.WrongDiscardPenalty : 5f;
            if (TimerManager.Instance != null) TimerManager.Instance.ApplyPenalty(TimerPhase.Sorting, penalty);
            if (LivesManager.Instance != null) LivesManager.Instance.LoseLife();

            GameEvents.TriggerWrongDiscard(item.Data);
        }

        public void ConfigureFeedback(AudioClip shredClip, AudioClip wrongDiscardClip, ParticleSystem shredParticles)
        {
            _shredClip = shredClip;
            _wrongDiscardClip = wrongDiscardClip;
            _shredParticles = shredParticles;
        }

        private bool IsValidDiscard(SortableItem item)
        {
            if (MissionManager.Instance != null && item.Type == MissionManager.Instance.ForbiddenType) return true;
            if (MissionManager.Instance == null || MissionManager.Instance.RequiredCount == 0) return false;

            var required = MissionManager.Instance.GetRequiredCountForType(item.Type);
            if (required == 0) return true;

            var cages = FindObjectsByType<Cage>(FindObjectsSortMode.None);
            var cagedCount = 0;
            for (var i = 0; i < cages.Length; i++)
            {
                if (cages[i].AcceptedType == item.Type) cagedCount += cages[i].LockedCount;
            }

            var availableCount = SortingManager.Instance != null ? SortingManager.Instance.GetUnsortedCountForType(item.Type) : 0;
            return (cagedCount + availableCount) > required;
        }
    }
}
