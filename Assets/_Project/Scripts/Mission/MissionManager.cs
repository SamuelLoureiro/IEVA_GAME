using System.Collections.Generic;
using UnityEngine;
using VARG.Core;
using VARG.Data;

namespace VARG.Mission
{
    public class MissionManager : Singleton<MissionManager>
    {
        private readonly List<TargetData> _requiredOrder = new();

        public IReadOnlyList<TargetData> RequiredOrder => _requiredOrder;
        public TargetType ForbiddenType { get; private set; }
        public int CurrentIndex { get; private set; }
        public int RequiredCount => _requiredOrder.Count;
        public bool IsComplete => CurrentIndex >= RequiredCount && RequiredCount > 0;

        public void GenerateMission(LevelConfig level)
        {
        }

        public bool IsForbidden(TargetType targetType)
        {
            return targetType == ForbiddenType;
        }

        public CaptureResult ValidateCapture(TargetData captured)
        {
            return CaptureResult.Correct;
        }

        public void SetMissionDebug(List<TargetData> order, TargetType forbidden)
        {
            _requiredOrder.Clear();
            _requiredOrder.AddRange(order);
            ForbiddenType = forbidden;
            CurrentIndex = 0;
        }
    }
}
