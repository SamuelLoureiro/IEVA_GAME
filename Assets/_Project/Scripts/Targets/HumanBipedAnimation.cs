using UnityEngine;
using UnityEngine.AI;

namespace VARG.Targets
{
    public class HumanBipedAnimation : MonoBehaviour
    {
        [Header("Bones / Pivots")]
        [SerializeField] private Transform _torso;
        [SerializeField] private Transform _headPivot;
        [SerializeField] private Transform _armPivotL;
        [SerializeField] private Transform _armPivotR;
        [SerializeField] private Transform _legPivotL;
        [SerializeField] private Transform _legPivotR;

        [Header("Animation Settings")]
        [SerializeField] private float _walkFrequency = 10f;
        [SerializeField] private float _fleeFrequency = 16f;
        [SerializeField] private float _maxLegAngle = 38f;
        [SerializeField] private float _maxArmAngle = 32f;
        [SerializeField] private float _bobHeight = 0.05f;
        [SerializeField] private float _moveThreshold = 0.1f;

        private NavMeshAgent _agent;
        private Rigidbody _rb;
        private TargetAI _ai;

        private float _cycle;
        private Vector3 _initialTorsoPos;
        private Quaternion _initialArmRotL;
        private Quaternion _initialArmRotR;
        private Quaternion _initialLegRotL;
        private Quaternion _initialLegRotR;
        private Quaternion _initialHeadRot;

        private void Awake()
        {
            CacheReferences();
        }

        private void Start()
        {
            CacheReferences();
            if (_torso != null) _initialTorsoPos = _torso.localPosition;
            if (_armPivotL != null) _initialArmRotL = _armPivotL.localRotation;
            if (_armPivotR != null) _initialArmRotR = _armPivotR.localRotation;
            if (_legPivotL != null) _initialLegRotL = _legPivotL.localRotation;
            if (_legPivotR != null) _initialLegRotR = _legPivotR.localRotation;
            if (_headPivot != null) _initialHeadRot = _headPivot.localRotation;
        }

        private void CacheReferences()
        {
            if (_agent == null) _agent = GetComponentInParent<NavMeshAgent>();
            if (_rb == null) _rb = GetComponentInParent<Rigidbody>();
            if (_ai == null) _ai = GetComponentInParent<TargetAI>();

            if (_torso == null) _torso = transform.Find("Torso");
            if (_torso != null)
            {
                if (_headPivot == null) _headPivot = _torso.Find("HeadPivot");
                if (_armPivotL == null) _armPivotL = _torso.Find("ArmPivotL");
                if (_armPivotR == null) _armPivotR = _torso.Find("ArmPivotR");
            }

            if (_legPivotL == null) _legPivotL = transform.Find("LegPivotL");
            if (_legPivotR == null) _legPivotR = transform.Find("LegPivotR");
        }

        private void Update()
        {
            var speed = GetMovementSpeed();
            var isMoving = speed > _moveThreshold;
            var isFleeing = _ai != null && _ai.CurrentState == TargetAIState.Flee;

            var freq = isFleeing ? _fleeFrequency : _walkFrequency;
            if (isMoving)
            {
                _cycle += Time.deltaTime * freq;
            }
            else
            {
                _cycle += Time.deltaTime * 2f;
            }

            if (isMoving)
            {
                AnimateWalkRun(isFleeing);
            }
            else
            {
                AnimateIdle();
            }
        }

        private float GetMovementSpeed()
        {
            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                return _agent.velocity.magnitude;
            }

            if (_rb != null)
            {
                return _rb.linearVelocity.magnitude;
            }

            return 0f;
        }

        private void AnimateWalkRun(bool isFleeing)
        {
            var sin = Mathf.Sin(_cycle);
            var cos = Mathf.Cos(_cycle);

            // Legs swing opposite to each other
            var legAngleL = sin * _maxLegAngle;
            var legAngleR = -sin * _maxLegAngle;

            if (_legPivotL != null)
            {
                _legPivotL.localRotation = _initialLegRotL * Quaternion.Euler(legAngleL, 0f, 0f);
            }
            if (_legPivotR != null)
            {
                _legPivotR.localRotation = _initialLegRotR * Quaternion.Euler(legAngleR, 0f, 0f);
            }

            // Arms: When fleeing, comical flailing up in panic! When walking, normal opposing arm swing
            if (isFleeing)
            {
                var flailL = 135f + Mathf.Sin(_cycle * 1.5f) * 25f;
                var flailR = 135f + Mathf.Cos(_cycle * 1.5f) * 25f;
                if (_armPivotL != null) _armPivotL.localRotation = Quaternion.Euler(flailL, 0f, 15f);
                if (_armPivotR != null) _armPivotR.localRotation = Quaternion.Euler(flailR, 0f, -15f);
            }
            else
            {
                var armAngleL = -sin * _maxArmAngle;
                var armAngleR = sin * _maxArmAngle;
                if (_armPivotL != null) _armPivotL.localRotation = _initialArmRotL * Quaternion.Euler(armAngleL, 0f, 0f);
                if (_armPivotR != null) _armPivotR.localRotation = _initialArmRotR * Quaternion.Euler(armAngleR, 0f, 0f);
            }

            // Torso bobbing on footsteps
            if (_torso != null)
            {
                var bob = Mathf.Abs(cos) * _bobHeight;
                _torso.localPosition = _initialTorsoPos + new Vector3(0f, bob, 0f);
            }

            // Head subtle sway
            if (_headPivot != null)
            {
                var headTilt = sin * (isFleeing ? 6f : 3f);
                _headPivot.localRotation = _initialHeadRot * Quaternion.Euler(0f, 0f, headTilt);
            }
        }

        private void AnimateIdle()
        {
            // Smoothly return limbs to rest
            if (_legPivotL != null)
            {
                _legPivotL.localRotation = Quaternion.Slerp(_legPivotL.localRotation, _initialLegRotL, Time.deltaTime * 6f);
            }
            if (_legPivotR != null)
            {
                _legPivotR.localRotation = Quaternion.Slerp(_legPivotR.localRotation, _initialLegRotR, Time.deltaTime * 6f);
            }
            if (_armPivotL != null)
            {
                _armPivotL.localRotation = Quaternion.Slerp(_armPivotL.localRotation, _initialArmRotL, Time.deltaTime * 6f);
            }
            if (_armPivotR != null)
            {
                _armPivotR.localRotation = Quaternion.Slerp(_armPivotR.localRotation, _initialArmRotR, Time.deltaTime * 6f);
            }

            // Subtle breathing bob
            if (_torso != null)
            {
                var breath = Mathf.Sin(_cycle) * (_bobHeight * 0.25f);
                _torso.localPosition = _initialTorsoPos + new Vector3(0f, breath, 0f);
            }

            if (_headPivot != null)
            {
                _headPivot.localRotation = Quaternion.Slerp(_headPivot.localRotation, _initialHeadRot, Time.deltaTime * 4f);
            }
        }
    }
}
