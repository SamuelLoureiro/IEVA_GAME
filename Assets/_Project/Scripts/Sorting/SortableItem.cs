using UnityEngine;
using VARG.Data;

namespace VARG.Sorting
{
    public class SortableItem : MonoBehaviour
    {
        [SerializeField] private TargetData _data;
        private CapturedItem _capturedItem;
        private bool _isHeld;

        public TargetData Data
        {
            get => _data != null ? _data : _capturedItem.Data;
            set => _data = value;
        }

        public CapturedItem Item => _capturedItem;

        public bool IsHeld
        {
            get => _isHeld;
            set => _isHeld = value;
        }

        public void Initialize(CapturedItem capturedItem)
        {
            _capturedItem = capturedItem;
            if (capturedItem.Data != null)
            {
                _data = capturedItem.Data;
            }
        }
    }
}
