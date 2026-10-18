using UnityEngine;
using UnityEngine.AI;

namespace VARG.Targets
{
    public class PigAnimationController : MonoBehaviour
    {
        private static readonly int RunStateHash = Animator.StringToHash("Run Forward In Place");
        private static readonly int IdleStateHash = Animator.StringToHash("Idle");

        [SerializeField] private float _moveThreshold = 0.1f;
        [SerializeField] private float _transitionDuration = 0.15f;

        private Animator _animator;
        private NavMeshAgent _agent;
        private Rigidbody _rb;
        private bool _wasMoving;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _agent = GetComponentInParent<NavMeshAgent>();
            _rb = GetComponentInParent<Rigidbody>();
        }

        private void Update()
        {
            if (_animator == null) return;

            var isMoving = CheckIsMoving();
            if (isMoving != _wasMoving)
            {
                _wasMoving = isMoving;
                _animator.CrossFade(isMoving ? RunStateHash : IdleStateHash, _transitionDuration);
            }
        }

        private bool CheckIsMoving()
        {
            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                return _agent.velocity.sqrMagnitude > (_moveThreshold * _moveThreshold);
            }

            if (_rb != null)
            {
                return _rb.linearVelocity.sqrMagnitude > (_moveThreshold * _moveThreshold);
            }

            return false;
        }
    }
}
