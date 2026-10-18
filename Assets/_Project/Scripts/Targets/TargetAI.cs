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
        [SerializeField] private float _tickInterval = 0.15f;

        [Header("Zigzag Evasion Settings")]
        [SerializeField] private bool _enableZigzag = true;
        [SerializeField] private float _baseZigzagAngle = 45f;
        [SerializeField] private float _zigzagMinDuration = 0.25f;
        [SerializeField] private float _zigzagMaxDuration = 0.5f;
        [SerializeField] private float _fleeWaypointDistance = 5.5f;

        [Header("Agility Settings")]
        [SerializeField] private float _wanderAcceleration = 12f;
        [SerializeField] private float _fleeAcceleration = 35f;
        [SerializeField] private float _fleeAngularSpeed = 999f;
        [SerializeField] private float _wanderAngularSpeed = 360f;

        [Header("Audio & Panic Feedback")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _customFleeClip;
        [SerializeField] private float _screamCooldown = 2.5f;

        private readonly float[] _testAngles = new float[9];
        private TargetAIState _state = TargetAIState.Idle;
        private Transform _playerTransform;
        private Vector3 _dangerSource;
        private float _stateTimer;
        private float _tickTimer;
        private float _nextZigzagSwitchTime;
        private float _zigzagSign = 1f;
        private float _currentZigzagAngle = 45f;
        private bool _isInitialized;
        private NavMeshPath _pathBuffer;
        private float _lastScreamTime = -10f;

        public TargetAIState CurrentState => _state;
        public AudioSource AudioSource => _audioSource;
        public AudioClip CustomFleeClip => _customFleeClip;

        private void Awake()
        {
            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
            }

            if (_target == null)
            {
                _target = GetComponent<Target>();
            }

            EnsureAudioSource();

            _pathBuffer = new NavMeshPath();
        }

        public void EnsureAudioSource()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            if (_audioSource != null)
            {
                _audioSource.spatialBlend = 1f;
                _audioSource.playOnAwake = false;
                _audioSource.minDistance = 2f;
                _audioSource.maxDistance = 25f;
                _audioSource.rolloffMode = AudioRolloffMode.Linear;
            }
        }

        private void Start()
        {
            InitializePlayerReference();
            ConfigureZigzagProfile();
            ApplyWalkSpeed();
            EnterIdleState();
            _isInitialized = true;
        }

        private void OnEnable()
        {
            if (_isInitialized && _state == TargetAIState.Captured)
            {
                ResetState();
            }
        }

        private void Update()
        {
            if (_state == TargetAIState.Captured) return;

            UpdateTimers();

            _tickTimer += Time.deltaTime;
            if (_tickTimer >= _tickInterval)
            {
                _tickTimer = 0f;
                TickDecision();
            }
        }

        private void ConfigureZigzagProfile()
        {
            var targetType = _target != null && _target.Data != null ? _target.Data.Type : TargetType.Chicken;
            switch (targetType)
            {
                case TargetType.Chicken:
                    _currentZigzagAngle = 55f;
                    _zigzagMinDuration = 0.2f;
                    _zigzagMaxDuration = 0.35f;
                    _fleeWaypointDistance = 4.5f;
                    break;
                case TargetType.Human:
                    _currentZigzagAngle = 45f;
                    _zigzagMinDuration = 0.3f;
                    _zigzagMaxDuration = 0.5f;
                    _fleeWaypointDistance = 6f;
                    break;
                case TargetType.Pig:
                    _currentZigzagAngle = 40f;
                    _zigzagMinDuration = 0.28f;
                    _zigzagMaxDuration = 0.45f;
                    _fleeWaypointDistance = 5f;
                    break;
                case TargetType.Cow:
                    _currentZigzagAngle = 30f;
                    _zigzagMinDuration = 0.45f;
                    _zigzagMaxDuration = 0.65f;
                    _fleeWaypointDistance = 6.5f;
                    break;
                default:
                    _currentZigzagAngle = _baseZigzagAngle;
                    break;
            }
        }

        private void InitializePlayerReference()
        {
            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                _playerTransform = playerObject.transform;
            }
        }

        private void UpdateTimers()
        {
            if (_stateTimer > 0f)
            {
                _stateTimer -= Time.deltaTime;
            }
        }

        private void TickDecision()
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            if (_playerTransform == null)
            {
                InitializePlayerReference();
            }

            var hasPlayerProximity = CheckPlayerProximity(out var playerPos);

            if (_state == TargetAIState.Flee)
            {
                if (hasPlayerProximity)
                {
                    _dangerSource = playerPos;
                    _stateTimer = _fleeDuration;

                    var shouldSwitchZigzag = _enableZigzag && (Time.time >= _nextZigzagSwitchTime || (!_agent.pathPending && _agent.remainingDistance < 1.2f));
                    if (shouldSwitchZigzag)
                    {
                        SwitchZigzagDirection();
                        UpdateFleeDestination();
                    }
                }
                else if (_stateTimer <= 0f)
                {
                    ApplyWalkSpeed();
                    EnterIdleState();
                }
                return;
            }

            if (hasPlayerProximity)
            {
                AlertDanger(playerPos);
                return;
            }

            switch (_state)
            {
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

        private bool CheckPlayerProximity(out Vector3 playerPos)
        {
            playerPos = Vector3.zero;
            if (_playerTransform == null) return false;

            var distSqr = (transform.position - _playerTransform.position).sqrMagnitude;
            if (distSqr <= _playerDetectRadius * _playerDetectRadius)
            {
                playerPos = _playerTransform.position;
                return true;
            }

            return false;
        }

        private void SwitchZigzagDirection()
        {
            _zigzagSign = -_zigzagSign;
            _nextZigzagSwitchTime = Time.time + Random.Range(_zigzagMinDuration, _zigzagMaxDuration);
        }

        private void EnterIdleState()
        {
            _state = TargetAIState.Idle;
            _stateTimer = Random.Range(_minIdleDuration, _maxIdleDuration);

            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.ResetPath();
                _agent.autoBraking = true;
                _agent.angularSpeed = _wanderAngularSpeed;
                _agent.acceleration = _wanderAcceleration;
            }
        }

        private void EnterWanderState()
        {
            _state = TargetAIState.Wander;
            ApplyWalkSpeed();

            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.autoBraking = true;
                _agent.angularSpeed = _wanderAngularSpeed;
                _agent.acceleration = _wanderAcceleration;
            }

            if (TryFindNavMeshPoint(transform.position, _wanderRadius, out var destination))
            {
                _agent.SetDestination(destination);
            }
            else
            {
                EnterIdleState();
            }
        }

        private void EnterFleeState(Vector3 dangerSource)
        {
            _state = TargetAIState.Flee;
            _dangerSource = dangerSource;
            _stateTimer = _fleeDuration;

            _zigzagSign = Random.value > 0.5f ? 1f : -1f;
            _nextZigzagSwitchTime = Time.time + Random.Range(_zigzagMinDuration, _zigzagMaxDuration);

            PlayFleeScream();

            ApplyFleeSpeed();
            UpdateFleeDestination();
        }

        private void UpdateFleeDestination()
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            _agent.autoBraking = false;
            _agent.angularSpeed = _fleeAngularSpeed;
            _agent.acceleration = _fleeAcceleration;

            var baseAway = (transform.position - _dangerSource);
            baseAway.y = 0f;

            if (baseAway.sqrMagnitude < 0.01f)
            {
                var rnd = Random.insideUnitCircle.normalized;
                baseAway = new Vector3(rnd.x, 0f, rnd.y);
            }
            else
            {
                baseAway.Normalize();
            }

            var primaryAngle = _enableZigzag ? (_zigzagSign * _currentZigzagAngle) : 0f;
            _testAngles[0] = primaryAngle;
            _testAngles[1] = primaryAngle + 20f * _zigzagSign;
            _testAngles[2] = primaryAngle - 20f * _zigzagSign;
            _testAngles[3] = 0f;
            _testAngles[4] = -primaryAngle;
            _testAngles[5] = primaryAngle + 45f * _zigzagSign;
            _testAngles[6] = primaryAngle - 45f * _zigzagSign;
            _testAngles[7] = 90f * _zigzagSign;
            _testAngles[8] = -90f * _zigzagSign;

            var bestDistanceToDanger = (transform.position - _dangerSource).sqrMagnitude;
            var bestTarget = Vector3.zero;
            var foundValidEscape = false;

            for (var i = 0; i < _testAngles.Length; i++)
            {
                var angle = _testAngles[i];
                var rot = Quaternion.Euler(0f, angle, 0f);
                var testDir = rot * baseAway;
                var testPos = transform.position + testDir * _fleeWaypointDistance;

                if (!NavMesh.SamplePosition(testPos, out var hit, 4f, NavMesh.AllAreas))
                {
                    continue;
                }

                var distToDangerSqr = (hit.position - _dangerSource).sqrMagnitude;
                if (distToDangerSqr <= bestDistanceToDanger * 0.9f)
                {
                    continue;
                }

                if (_agent.CalculatePath(hit.position, _pathBuffer) && _pathBuffer.status == NavMeshPathStatus.PathComplete)
                {
                    bestTarget = hit.position;
                    foundValidEscape = true;
                    break;
                }
            }

            if (foundValidEscape)
            {
                _agent.SetDestination(bestTarget);
            }
            else if (TryFindNavMeshPoint(transform.position + baseAway * _wanderRadius, 5f, out var fallbackPos))
            {
                _agent.SetDestination(fallbackPos);
            }
        }

        public void AlertDanger(Vector3 dangerSource)
        {
            if (_state == TargetAIState.Captured) return;

            if (_state == TargetAIState.Flee)
            {
                _dangerSource = dangerSource;
                _stateTimer = _fleeDuration;
                SwitchZigzagDirection();
                UpdateFleeDestination();
                PlayFleeScream();
                return;
            }

            EnterFleeState(dangerSource);
        }

        private void PlayFleeScream()
        {
            if (Time.time - _lastScreamTime < _screamCooldown) return;
            _lastScreamTime = Time.time;

            if (_audioSource != null && _customFleeClip != null)
            {
                _audioSource.pitch = Random.Range(0.95f, 1.05f);
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

        public void ResetState()
        {
            _state = TargetAIState.Idle;
            if (_agent != null)
            {
                _agent.enabled = true;
            }
            ApplyWalkSpeed();
            EnterIdleState();
        }

        private void ApplyWalkSpeed()
        {
            if (_agent == null) return;
            var walkSpeed = _target != null && _target.Data != null ? _target.Data.WalkSpeed : 3f;
            _agent.speed = walkSpeed;
        }

        private void ApplyFleeSpeed()
        {
            if (_agent == null) return;

            var baseFleeSpeed = _target != null && _target.Data != null ? _target.Data.FleeSpeed : 6f;
            var fleeMultiplier = 1f;

            if (GameManager.HasInstance)
            {
                var levelConfig = GameManager.Instance.GetCurrentLevelConfig();
                if (levelConfig != null)
                {
                    fleeMultiplier = levelConfig.FleeSpeedMultiplier;
                }
            }

            _agent.speed = baseFleeSpeed * fleeMultiplier;
        }

        private static bool TryFindNavMeshPoint(Vector3 center, float range, out Vector3 result)
        {
            for (var i = 0; i < 6; i++)
            {
                var randomDirection = Random.insideUnitSphere * range;
                randomDirection.y = 0f;
                randomDirection += center;

                if (NavMesh.SamplePosition(randomDirection, out var hit, 4f, NavMesh.AllAreas))
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
