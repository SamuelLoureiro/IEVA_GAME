using UnityEngine;
using VARG.Data;

namespace VARG.Sorting
{
    public class SortableItem : MonoBehaviour
    {
        [SerializeField] private TargetData _data;
        private CapturedItem _capturedItem;
        private bool _isSorted;

        public TargetData Data
        {
            get => _data != null ? _data : _capturedItem.Data;
            set => _data = value;
        }

        public TargetType Type => Data != null ? Data.Type : TargetType.Chicken;
        public CapturedItem Item => _capturedItem;
        public bool IsSorted => _isSorted;
        public bool IsHeld { get; set; }

        public void MarkSorted()
        {
            _isSorted = true;
            IsHeld = false;
        }

        public void Initialize(CapturedItem capturedItem)
        {
            _capturedItem = capturedItem;
            if (capturedItem.Data != null)
            {
                _data = capturedItem.Data;
            }
        }

        public void ReturnToCenter()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            transform.position = new Vector3(
                Random.Range(-0.4f, 0.4f),
                0.7f,
                1.2f + Random.Range(-0.4f, 0.4f)
            );
            transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        private void OnDestroy()
        {
            if (!_isSorted && SortingManager.Instance != null)
            {
                SortingManager.Instance.UnregisterItem(this);
            }
        }
    }
}
