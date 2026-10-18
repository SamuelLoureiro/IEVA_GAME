using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using VARG.Data;
using VARG.Player;

namespace VARG.Sorting
{
    public class SortingEscapeeAI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Grabbable _grabbable;
        [SerializeField] private SortableItem _sortableItem;
        [SerializeField] private AudioSource _audioSource;

        [Header("Flee Settings")]
        [SerializeField] private float _fleeSpeed = 4.5f;
        [SerializeField] private float _fleeAngularSpeed = 999f;
        [SerializeField] private float _fleeDuration = 8.0f;
        [SerializeField] private float _repathInterval = 0.2f;
        [SerializeField] private float _zigzagAngle = 45f;

        private Transform _playerTransform;
        private bool _isEscaping;
        private float _escapeTimer;
        private float _repathTimer;
        private float _zigzagSign = 1f;
        private float _nextZigzagTime;
        private Coroutine _escapeRoutine;

        public bool IsEscaping => _isEscaping;

        private void Awake()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_grabbable == null) _grabbable = GetComponent<Grabbable>();
            if (_sortableItem == null) _sortableItem = GetComponent<SortableItem>();
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();

            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
                if (_agent == null)
                {
                    _agent = gameObject.AddComponent<NavMeshAgent>();
                }
            }

            ConfigureAgent();
        }

        private void ConfigureAgent()
        {
            if (_agent == null) return;
            _agent.speed = _fleeSpeed;
            _agent.angularSpeed = _fleeAngularSpeed;
            _agent.acceleration = 35f;
            _agent.radius = 0.22f;
            _agent.height = 0.45f;
            _agent.stoppingDistance = 0.1f;
            _agent.autoBraking = false;
            _agent.enabled = false;
        }

        private void Start()
        {
            AcquirePlayerReference();
        }

        private void AcquirePlayerReference()
        {
            if (_playerTransform != null) return;
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
            }
        }

        public void TriggerEscape()
        {
            StopEscape();
            _isEscaping = true;
            _escapeTimer = _fleeDuration;
            PlayPanicScream();
            _escapeRoutine = StartCoroutine(ActivateAgentAfterLanding());
        }

        private void PlayPanicScream()
        {
            if (_audioSource == null) return;
            var type = _sortableItem != null ? _sortableItem.Type : TargetType.Chicken;

            var clip = ProceduralAudioUtility.GetFleeVoiceClip(type);
            if (clip != null)
            {
                _audioSource.pitch = Random.Range(0.95f, 1.05f);
                _audioSource.PlayOneShot(clip, 0.85f);
            }
        }

        private IEnumerator ActivateAgentAfterLanding()
        {
            yield return new WaitForSeconds(0.35f);

            var elapsed = 0f;
            const float maxWait = 1.2f;
            while (elapsed < maxWait && _rb != null)
            {
                if (transform.position.y <= 0.35f && _rb.linearVelocity.y <= 0.1f)
                {
                    break;
                }
                yield return new WaitForSeconds(0.05f);
                elapsed += 0.05f;
            }

            if (_grabbable != null && _grabbable.IsHeld)
            {
                _isEscaping = false;
                yield break;
            }

            if (_sortableItem != null && _sortableItem.IsSorted)
            {
                _isEscaping = false;
                yield break;
            }

            if (NavMesh.SamplePosition(transform.position, out var hit, 3.0f, NavMesh.AllAreas))
            {
                if (_rb != null)
                {
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                    _rb.isKinematic = true;
                }

                if (_agent != null)
                {
                    _agent.enabled = true;
                    _agent.Warp(hit.position);
                    UpdateFleeDestination();
                }
            }
            else
            {
                _isEscaping = false;
            }
        }

        private void Update()
        {
            if (!_isEscaping) return;

            if ((_grabbable != null && _grabbable.IsHeld) || (_sortableItem != null && _sortableItem.IsSorted))
            {
                StopEscape();
                return;
            }

            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            _escapeTimer -= Time.deltaTime;
            if (_escapeTimer <= 0f)
            {
                StopEscape();
                return;
            }

            _repathTimer += Time.deltaTime;
            if (_repathTimer >= _repathInterval)
            {
                _repathTimer = 0f;
                UpdateFleeDestination();
            }
        }

        private void UpdateFleeDestination()
        {
            AcquirePlayerReference();

            var fleeDir = _playerTransform != null
                ? (transform.position - _playerTransform.position).normalized
                : transform.forward;

            if (fleeDir.sqrMagnitude < 0.001f)
            {
                fleeDir = transform.forward;
            }

            if (Time.time >= _nextZigzagTime)
            {
                _zigzagSign *= -1f;
                _nextZigzagTime = Time.time + Random.Range(0.25f, 0.5f);
            }

            var zigzagRot = Quaternion.Euler(0f, _zigzagAngle * _zigzagSign, 0f);
            var targetDir = zigzagRot * fleeDir;
            var targetPos = transform.position + targetDir * 4.5f;

            if (!NavMesh.SamplePosition(targetPos, out var hit, 3.5f, NavMesh.AllAreas))
            {
                targetDir = Quaternion.Euler(0f, -_zigzagAngle * _zigzagSign, 0f) * fleeDir;
                targetPos = transform.position + targetDir * 2.5f;
                NavMesh.SamplePosition(targetPos, out hit, 3.5f, NavMesh.AllAreas);
            }

            if (hit.hit && _agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.SetDestination(hit.position);
            }
        }

        public void StopEscape()
        {
            if (_escapeRoutine != null)
            {
                StopCoroutine(_escapeRoutine);
                _escapeRoutine = null;
            }

            _isEscaping = false;

            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.ResetPath();
                _agent.enabled = false;
            }

            if (_rb != null)
            {
                _rb.isKinematic = false;
            }
        }
    }
}
