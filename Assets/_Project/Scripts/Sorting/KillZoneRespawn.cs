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

            var targetPos = _respawnPoint != null ? _respawnPoint.position : Vector3.up;

            var cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                other.transform.position = targetPos;
                cc.enabled = true;
                return;
            }

            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.position = targetPos;
            }
            else
            {
                other.transform.position = targetPos;
            }
        }
    }
}
