using System;
using UnityEngine;

namespace VARG.Data
{
    [Serializable]
    public struct CapturedItem : IEquatable<CapturedItem>
    {
        [SerializeField] private TargetData _data;
        [SerializeField] private CaptureResult _result;

        public TargetData Data => _data;
        public CaptureResult Result => _result;

        public CapturedItem(TargetData data, CaptureResult result)
        {
            _data = data;
            _result = result;
        }

        public bool Equals(CapturedItem other)
        {
            return Equals(_data, other._data) && _result == other._result;
        }

        public override bool Equals(object obj)
        {
            return obj is CapturedItem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_data, (int)_result);
        }
    }
}
