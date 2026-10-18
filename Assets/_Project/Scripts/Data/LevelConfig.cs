using UnityEngine;

namespace VARG.Data
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "VARG/Data/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private int _listSize = 3;
        [SerializeField] private float _captureTime = 90f;
        [SerializeField] private float _sortingTime = 45f;
        [SerializeField] private int _extraSpawnsPerRequired = 1;
        [SerializeField] private int _forbiddenSpawnCount = 2;
        [SerializeField] private float _fleeSpeedMultiplier = 1f;

        public int ListSize => _listSize;
        public float CaptureTime => _captureTime;
        public float SortingTime => _sortingTime;
        public int ExtraSpawnsPerRequired => _extraSpawnsPerRequired;
        public int ForbiddenSpawnCount => _forbiddenSpawnCount;
        public float FleeSpeedMultiplier => _fleeSpeedMultiplier;

        public void Setup(int listSize, float captureTime, float sortingTime, int extraSpawns = 1, int forbiddenSpawns = 2, float fleeMultiplier = 1f)
        {
            _listSize = listSize;
            _captureTime = captureTime;
            _sortingTime = sortingTime;
            _extraSpawnsPerRequired = extraSpawns;
            _forbiddenSpawnCount = forbiddenSpawns;
            _fleeSpeedMultiplier = fleeMultiplier;
        }
    }
}
