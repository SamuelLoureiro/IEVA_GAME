using UnityEngine;
using VARG.Data;

namespace VARG.Sorting
{
    public class Cage : MonoBehaviour
    {
        [SerializeField] private TargetType _acceptedType;
        [SerializeField] private Transform _snapPoint;
        [SerializeField] private Transform _ejectPoint;
        [SerializeField] private float _ejectForce = 7f;

        public TargetType AcceptedType => _acceptedType;
        public Transform SnapPoint => _snapPoint != null ? _snapPoint : transform;
        public Transform EjectPoint => _ejectPoint != null ? _ejectPoint : transform;
        public float EjectForce => _ejectForce;

        public void Configure(TargetType acceptedType, Transform snapPoint, Transform ejectPoint)
        {
            _acceptedType = acceptedType;
            _snapPoint = snapPoint;
            _ejectPoint = ejectPoint;
        }
    }
}
