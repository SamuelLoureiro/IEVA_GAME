using UnityEngine;

namespace VARG.Targets
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private float _radius = 1f;

        public Vector3 Position => transform.position;
        public float Radius => _radius;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.4f, 0.35f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.4f, 0.45f);
        }
    }
}
