using UnityEngine;

namespace VARG.UI
{
    public class SpinAndBob : MonoBehaviour
    {
        [SerializeField] private float _spinSpeed = 50f;
        [SerializeField] private float _bobSpeed = 2.2f;
        [SerializeField] private float _bobHeight = 0.08f;

        private Vector3 _basePosition;
        private float _timeOffset;

        public void Initialize(Vector3 basePosition, float timeOffset = 0f)
        {
            _basePosition = basePosition;
            _timeOffset = timeOffset;
            transform.position = basePosition;
        }

        private void Start()
        {
            if (_basePosition == Vector3.zero)
            {
                _basePosition = transform.position;
            }
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, _spinSpeed * Time.deltaTime, Space.World);
            var bob = Mathf.Sin((Time.time + _timeOffset) * _bobSpeed) * _bobHeight;
            transform.position = _basePosition + Vector3.up * bob;
        }
    }
}
