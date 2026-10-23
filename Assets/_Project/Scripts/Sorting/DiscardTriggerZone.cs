using UnityEngine;

namespace VARG.Sorting
{
    public class DiscardTriggerZone : MonoBehaviour
    {
        [SerializeField] private DiscardBin _bin;

        private void Awake()
        {
            if (_bin == null)
            {
                _bin = GetComponentInParent<DiscardBin>();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (_bin != null)
            {
                _bin.ProcessTriggerStay(other);
            }
        }
    }
}
