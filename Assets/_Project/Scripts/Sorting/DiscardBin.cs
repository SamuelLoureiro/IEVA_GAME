using System.Collections;
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

        public Transform DestroyPoint => _destroyPoint != null ? _destroyPoint : transform;

        private void Awake()
        {
            if (_penaltyConfig == null)
            {
                _penaltyConfig = Resources.Load<PenaltyConfig>("Data/PenaltyConfig");
            }
        }

        public void Configure(Transform destroyPoint)
        {
            _destroyPoint = destroyPoint;
        }

        public void ProcessTriggerStay(Collider other)
        {
            var item = other.GetComponentInParent<SortableItem>();
            if (item == null || item.IsSorted) return;

            if (item.IsHeld) return;

            if (item.TimeSinceReleased < _settleDelay) return;

            ValidateDiscard(item);
        }

        private void OnTriggerStay(Collider other)
        {
            ProcessTriggerStay(other);
        }

        private void ValidateDiscard(SortableItem item)
        {
            item.MarkSorted();

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

            var isForbidden = IsItemForbidden(item);

            if (isForbidden)
            {
                GameEvents.TriggerCorrectDiscard(item.Data);
            }
            else
            {
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
