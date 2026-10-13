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

        private readonly List<SortableItem> _lockedItems = new();
        private readonly Dictionary<SortableItem, float> _ejectedCooldowns = new();

        public TargetType AcceptedType => _acceptedType;
        public Transform SnapPoint => _snapPoint != null ? _snapPoint : transform;
        public Transform EjectPoint => _ejectPoint != null ? _ejectPoint : transform;
        public float EjectForce => _ejectForce;

        private void Awake()
        {
            if (_penaltyConfig == null)
            {
                _penaltyConfig = Resources.Load<PenaltyConfig>("Data/PenaltyConfig");
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

            if (item.IsHeld) return;

            if (item.TimeSinceReleased < _settleDelay) return;

            if (_ejectedCooldowns.TryGetValue(item, out var readyTime) && Time.time < readyTime)
            {
                return;
            }

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
            var isValidDeposit = isCorrectType && !isForbidden;

            if (isValidDeposit)
            {
                AcceptItem(item);
            }
            else
            {
                EjectItem(item);
            }
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
            item.MarkSorted();
            _lockedItems.Add(item);

            var grabbable = item.GetComponent<Grabbable>();
            if (grabbable != null)
            {
                grabbable.enabled = false;
            }

            var highlight = item.GetComponent<HighlightOnHover>();
            if (highlight != null)
            {
                highlight.SetHighlight(false);
                highlight.enabled = false;
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
            _ejectedCooldowns[item] = Time.time + 1.2f;

            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                var targetEject = EjectPoint != null ? EjectPoint.position : (transform.position + Vector3.up * 1.8f - transform.forward * 1.5f);
                var ejectDir = (targetEject - item.transform.position).normalized + Vector3.up * 0.6f;
                ejectDir.Normalize();
                rb.AddForce(ejectDir * _ejectForce, ForceMode.Impulse);
            }

            var penalty = _penaltyConfig != null ? _penaltyConfig.WrongCagePenalty : 5f;
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.ApplyPenalty(TimerPhase.Sorting, penalty);
            }

            GameEvents.TriggerWrongCage(item.Data);
        }
    }
}
