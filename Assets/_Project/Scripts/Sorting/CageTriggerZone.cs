using UnityEngine;

namespace VARG.Sorting
{
    public class CageTriggerZone : MonoBehaviour
    {
        [SerializeField] private Cage _cage;

        private void Awake()
        {
            if (_cage == null)
            {
                _cage = GetComponentInParent<Cage>();
            }
        }

        public void Configure(Cage cage)
        {
            _cage = cage;
        }

        private void OnTriggerStay(Collider other)
        {
            if (_cage != null)
            {
                _cage.ProcessTriggerStay(other);
            }
        }
    }
}
