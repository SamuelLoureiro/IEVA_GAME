using UnityEngine;

namespace VARG.Sorting
{
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        private Rigidbody _rb;
        private SortableItem _sortableItem;
        private HighlightOnHover _highlighter;

        public bool IsHeld { get; private set; }
        public Rigidbody Rigidbody => _rb;
        public SortableItem SortableItem => _sortableItem;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _sortableItem = GetComponent<SortableItem>();
            _highlighter = GetComponent<HighlightOnHover>();
        }

        public void OnGrabbed()
        {
            IsHeld = true;
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            if (_sortableItem != null)
            {
                _sortableItem.IsHeld = true;
            }

            SetHighlight(false);
        }

        public void OnReleased(Vector3 impulse = default)
        {
            IsHeld = false;
            _rb.useGravity = true;

            if (_sortableItem != null)
            {
                _sortableItem.IsHeld = false;
            }

            if (impulse != Vector3.zero)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.AddForce(impulse, ForceMode.Impulse);
            }
            else if (_rb.linearVelocity.sqrMagnitude > 4f)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * 2f;
            }
        }

        public void FollowTarget(Vector3 targetPosition, float followForce)
        {
            var delta = targetPosition - _rb.position;
            _rb.linearVelocity = delta * followForce;
            _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);
        }

        public void SetHighlight(bool enable)
        {
            if (_highlighter != null && !IsHeld)
            {
                _highlighter.SetHighlight(enable);
            }
        }
    }
}
