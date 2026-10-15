using UnityEngine;
using VARG.Data;

namespace VARG.Sorting
{
    public class SortableItem : MonoBehaviour
    {
        [SerializeField] private TargetData _data;
        private CapturedItem _capturedItem;
        private bool _isHeld;
        private bool _isSorted;
        private float _lastReleaseTime;

        public TargetData Data
        {
            get => _data != null ? _data : _capturedItem.Data;
            set => _data = value;
        }

        public TargetType Type => Data != null ? Data.Type : TargetType.Chicken;

        public CapturedItem Item => _capturedItem;

        public bool IsSorted => _isSorted;
        public float TimeSinceReleased => Time.time - _lastReleaseTime;

        public bool IsHeld
        {
            get => _isHeld;
            set
            {
                if (_isHeld && !value)
                {
                    _lastReleaseTime = Time.time;
                }
                _isHeld = value;
            }
        }

        public void MarkSorted()
        {
            _isSorted = true;
            _isHeld = false;
        }

        public void Initialize(CapturedItem capturedItem)
        {
            _capturedItem = capturedItem;
            if (capturedItem.Data != null)
            {
                _data = capturedItem.Data;
            }
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
