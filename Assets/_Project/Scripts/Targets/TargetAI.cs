using UnityEngine;
using UnityEngine.AI;
using VARG.Core;
using VARG.Data;

namespace VARG.Targets
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AudioSource))]
    public class TargetAI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Target _target;

        [Header("AI Distances and Timers")]
        [SerializeField] private float _wanderRadius = 12f;
        [SerializeField] private float _minIdleDuration = 1f;
        [SerializeField] private float _maxIdleDuration = 2.5f;
        [SerializeField] private float _playerDetectRadius = 14f;
        [SerializeField] private float _fleeDuration = 3.5f;
        [SerializeField] private float _fleeWaypointDistance = 6f;
        [SerializeField] private float _tickInterval = 0.2f;

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _customFleeClip;
        [SerializeField] private float _screamCooldown = 2.5f;

        private TargetAIState _state = TargetAIState.Idle;
        private Transform _playerTransform;
        private float _stateTimer;
        private float _tickTimer;
        private float _lastScreamTime = -10f;

        public TargetAIState CurrentState => _state;

        private void Awake()
        {
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
            if (_target == null) _target = GetComponent<Target>();
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                _playerTransform = playerObject.transform;
            }

            EnterIdleState();
        }

        private void Update()
        {
            if (_state == TargetAIState.Captured) return;

            if (_stateTimer > 0f)
            {
                _stateTimer -= Time.deltaTime;
            }

            _tickTimer += Time.deltaTime;
            if (_tickTimer >= _tickInterval)
            {
                _tickTimer = 0f;
                TickDecision();
            }
        }

        private void TickDecision()
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            if (IsPlayerNear())
            {
                EnterFleeState();
                return;
            }

            switch (_state)
            {
                case TargetAIState.Flee:
                    if (_stateTimer <= 0f)
                    {
                        EnterIdleState();
                    }
                    break;

                case TargetAIState.Idle:
                    if (_stateTimer <= 0f)
                    {
                        EnterWanderState();
                    }
                    break;

                case TargetAIState.Wander:
                    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        EnterIdleState();
                    }
                    break;
            }
        }

        private bool IsPlayerNear()
        {
            if (_playerTransform == null) return false;

            var distanceSqr = (transform.position - _playerTransform.position).sqrMagnitude;
            return distanceSqr <= _playerDetectRadius * _playerDetectRadius;
        }

        private void EnterIdleState()
        {
            _state = TargetAIState.Idle;
            _stateTimer = Random.Range(_minIdleDuration, _maxIdleDuration);
            _agent.ResetPath();
        }

        private void EnterWanderState()
        {
            _state = TargetAIState.Wander;
            _agent.speed = _target != null && _target.Data != null ? _target.Data.WalkSpeed : 3f;

            if (TryFindNavMeshPoint(transform.position, _wanderRadius, out var destination))
            {
                _agent.SetDestination(destination);
            }
            else
            {
                EnterIdleState();
            }
        }

        private void EnterFleeState()
        {
            if (_state != TargetAIState.Flee)
            {
                PlayFleeScream();
            }

            _state = TargetAIState.Flee;
            _stateTimer = _fleeDuration;
            _agent.speed = GetFleeSpeed();

            var awayDir = (transform.position - _playerTransform.position);
            awayDir.y = 0f;
            awayDir = awayDir.sqrMagnitude > 0.01f ? awayDir.normalized : transform.forward;

            var targetPos = transform.position + awayDir * _fleeWaypointDistance;
            if (NavMesh.SamplePosition(targetPos, out var hit, 4f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }
        }

        private float GetFleeSpeed()
        {
            var baseSpeed = _target != null && _target.Data != null ? _target.Data.FleeSpeed : 6f;
            var multiplier = 1f;

            if (GameManager.HasInstance)
            {
                var levelConfig = GameManager.Instance.GetCurrentLevelConfig();
                if (levelConfig != null)
                {
                    multiplier = levelConfig.FleeSpeedMultiplier;
                }
            }

            return baseSpeed * multiplier;
        }

        private void PlayFleeScream()
        {
            if (Time.time - _lastScreamTime < _screamCooldown) return;
            _lastScreamTime = Time.time;

            if (_audioSource != null && _customFleeClip != null)
            {
                _audioSource.PlayOneShot(_customFleeClip, 0.85f);
            }
        }

        public void SetCaptured()
        {
            _state = TargetAIState.Captured;
            if (_agent != null)
            {
                if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
                {
                    _agent.ResetPath();
                }
                _agent.enabled = false;
            }
        }

        private static bool TryFindNavMeshPoint(Vector3 center, float range, out Vector3 result)
        {
            for (var i = 0; i < 5; i++)
            {
                var randomOffset = Random.insideUnitSphere * range;
                randomOffset.y = 0f;

                if (NavMesh.SamplePosition(center + randomOffset, out var hit, 4f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            result = center;
            return false;
        }
    }
}
