using UnityEngine;
using VARG.Data;

namespace VARG.Sorting
{
    public class SortableItem : MonoBehaviour
    {
        [SerializeField] private TargetData _data;
        private bool _isHeld;

        public TargetData Data
        {
            get => _data;
            set => _data = value;
        }

        public bool IsHeld
        {
            get => _isHeld;
            set => _isHeld = value;
        }
    }
}
