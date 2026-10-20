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
        [SerializeField] private PenaltyConfig _penaltyConfig;

        [Header("Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _acceptClip;
        [SerializeField] private AudioClip _ejectClip;
        [SerializeField] private ParticleSystem _acceptParticles;

        private readonly List<SortableItem> _lockedItems = new();

        public TargetType AcceptedType => _acceptedType;
        public Transform SnapPoint => _snapPoint != null ? _snapPoint : transform;
        public Transform EjectPoint => _ejectPoint != null ? _ejectPoint : transform;
        public float EjectForce => _ejectForce;
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
            _ejectPoint = ejectPoint;
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
                    snap.x + UnityEngine.Random.Range(-0.2f, 0.2f),
                    snap.y + 0.9f,
                    snap.z + UnityEngine.Random.Range(-0.2f, 0.2f)
                );
                rb.linearVelocity = new Vector3(0f, -2.0f, 0f);
            }

            item.transform.SetParent(transform, true);
            StartCoroutine(SettleItemRoutine(item));

            if (_audioSource != null && _acceptClip != null) _audioSource.PlayOneShot(_acceptClip);
            if (_acceptParticles != null) _acceptParticles.Play();

            GameEvents.TriggerCorrectCage(item.Data);
            if (SortingManager.Instance != null) SortingManager.Instance.NotifyItemResolved(item);
        }

        private System.Collections.IEnumerator SettleItemRoutine(SortableItem item)
        {
            yield return new WaitForSeconds(1.2f);
            if (item != null)
            {
                var rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        private void EjectItem(SortableItem item)
        {
            TeleportToCenter(item);

            if (_audioSource != null && _ejectClip != null) _audioSource.PlayOneShot(_ejectClip);

            var penalty = _penaltyConfig != null ? _penaltyConfig.WrongCagePenalty : 5f;
            if (TimerManager.Instance != null) TimerManager.Instance.ApplyPenalty(TimerPhase.Sorting, penalty);

            GameEvents.TriggerWrongCage(item.Data);
        }

        private static void TeleportToCenter(SortableItem item)
        {
            if (item == null) return;

            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            var anchor = GameObject.Find("SpawnAnchor");
            var center = anchor != null ? anchor.transform.position : new Vector3(0f, 0.35f, 1.2f);

            item.transform.position = new Vector3(
                center.x + Random.Range(-0.4f, 0.4f),
                Mathf.Max(center.y + 0.35f, 0.45f),
                center.z + Random.Range(-0.4f, 0.4f)
            );
            item.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        public void ConfigureFeedback(AudioClip acceptClip, AudioClip ejectClip, ParticleSystem acceptParticles, ParticleSystem ejectParticles)
        {
            _acceptClip = acceptClip;
            _ejectClip = ejectClip;
            _acceptParticles = acceptParticles;
        }
    }
}
