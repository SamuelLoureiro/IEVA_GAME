using UnityEngine;

namespace VARG.Data
{
    [CreateAssetMenu(fileName = "ScoringConfig", menuName = "VARG/Data/ScoringConfig")]
    public class ScoringConfig : ScriptableObject
    {
        [SerializeField] private int _pointsPerSecond = 10;
        [SerializeField] private int _pointsPerLife = 100;

        public int PointsPerSecond => _pointsPerSecond;
        public int PointsPerLife => _pointsPerLife;
    }
}
