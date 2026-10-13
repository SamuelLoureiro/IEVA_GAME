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
                characterController.enabled = false;
                other.transform.position = targetPosition;
                characterController.enabled = true;
                return;
            }

            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = targetPosition;
            }
            else
            {
                other.transform.position = targetPosition;
            }
        }
    }
}
