using System;
using UnityEngine;

namespace VARG.Data
{
    [Serializable]
    public struct CapturedItem
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
    }
}
