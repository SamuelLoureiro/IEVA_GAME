using UnityEngine;

namespace VARG.Sorting
{
    public class KillZoneRespawn : MonoBehaviour
    {
        [SerializeField] private Transform _respawnPoint;

        public Transform RespawnPoint
        {
            get => _respawnPoint;
            set => _respawnPoint = value;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;

            var targetPosition = _respawnPoint != null ? _respawnPoint.position : Vector3.up;

            var characterController = other.GetComponent<CharacterController>();
            if (characterController != null)
            {
                var grab = other.GetComponent<GrabController>();
                if (grab != null)
                {
                    grab.DropHeld();
                }

                characterController.enabled = false;
                other.transform.position = targetPosition;
                characterController.enabled = true;
                return;
            }

            var sortable = other.GetComponentInParent<SortableItem>();
            if (sortable != null && sortable.IsHeld)
            {
                var grab = FindFirstObjectByType<GrabController>();
                if (grab != null)
                {
                    grab.DropHeld();
                }
            }

            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = targetPosition + Vector3.up * 0.25f;
            }
            else
            {
                other.transform.position = targetPosition + Vector3.up * 0.25f;
            }
        }
    }
}
