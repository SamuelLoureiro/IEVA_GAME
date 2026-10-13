using UnityEngine;

namespace VARG.Sorting
{
    public class DiscardBin : MonoBehaviour
    {
        [SerializeField] private Transform _destroyPoint;

        public Transform DestroyPoint => _destroyPoint != null ? _destroyPoint : transform;

        public void Configure(Transform destroyPoint)
        {
            _destroyPoint = destroyPoint;
        }
    }
}
