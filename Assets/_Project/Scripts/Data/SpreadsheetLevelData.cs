using System.Collections.Generic;
using VARG.Data;

namespace VARG.Data
{
    public class SpreadsheetLevelData
    {
        public List<TargetType> TargetSequence { get; set; } = new();
        public TargetType ForbiddenTarget { get; set; } = TargetType.Human;
        public float CaptureTime { get; set; } = 90f;
        public float SortingTime { get; set; } = 45f;
        public int ExtraSpawns { get; set; } = 2;
        public int ForbiddenSpawns { get; set; } = 2;
        public float FleeSpeedMultiplier { get; set; } = 1f;

        public bool IsValid => TargetSequence != null && TargetSequence.Count > 0;
    }
}
