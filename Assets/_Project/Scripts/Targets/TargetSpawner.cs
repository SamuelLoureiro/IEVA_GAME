using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VARG.Core;
using VARG.Data;
using VARG.Mission;

namespace VARG.Targets
{
    public class TargetSpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private List<SpawnPoint> _spawnPoints = new();
        [SerializeField] private LevelConfig _fallbackLevelConfig;
        [SerializeField] private Transform _spawnContainer;
        [SerializeField] private bool _spawnOnStart = true;

        private readonly List<GameObject> _spawnedTargets = new();

        private void Awake()
        {
            if (_spawnContainer == null)
            {
                _spawnContainer = new GameObject("_SpawnedTargets").transform;
            }
        }

        private void Start()
        {
            if (_spawnOnStart)
            {
                SpawnTargets();
            }
        }

        public void ClearSpawnedTargets()
        {
            for (var i = 0; i < _spawnedTargets.Count; i++)
            {
                if (_spawnedTargets[i] != null)
                {
                    Destroy(_spawnedTargets[i]);
                }
            }

            _spawnedTargets.Clear();
        }

        public void SpawnTargets()
        {
            ClearSpawnedTargets();

            if (_spawnPoints.Count == 0)
            {
                _spawnPoints.AddRange(FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None));
            }

            if (!MissionManager.HasInstance) return;
            var mission = MissionManager.Instance;

            var levelConfig = GameManager.HasInstance ? GameManager.Instance.GetCurrentLevelConfig() : _fallbackLevelConfig;
            if (mission.RequiredCount == 0 && levelConfig != null)
            {
                mission.GenerateMission(levelConfig);
            }

            if (mission.RequiredCount == 0) return;

            var extraSpawns = levelConfig != null ? levelConfig.ExtraSpawnsPerRequired : 1;
            var forbiddenSpawns = levelConfig != null ? levelConfig.ForbiddenSpawnCount : 2;

            var requiredCounts = new Dictionary<TargetType, int>();
            for (var i = 0; i < mission.RequiredOrder.Count; i++)
            {
                var type = mission.RequiredOrder[i].Type;
                requiredCounts[type] = requiredCounts.GetValueOrDefault(type, 0) + 1;
            }

            var spawnQueue = new List<TargetType>();
            foreach (var kvp in requiredCounts)
            {
                var count = kvp.Value + extraSpawns;
                for (var i = 0; i < count; i++)
                {
                    spawnQueue.Add(kvp.Key);
                }
            }

            for (var i = 0; i < forbiddenSpawns; i++)
            {
                spawnQueue.Add(mission.ForbiddenType);
            }

            Shuffle(spawnQueue);
            Shuffle(_spawnPoints);

            for (var i = 0; i < spawnQueue.Count; i++)
            {
                var targetData = mission.GetTargetDataForType(spawnQueue[i]);
                if (targetData == null || targetData.Prefab == null) continue;

                var spawnPos = _spawnPoints.Count > 0
                    ? _spawnPoints[i % _spawnPoints.Count].Position
                    : transform.position;

                if (NavMesh.SamplePosition(spawnPos, out var navHit, 3f, NavMesh.AllAreas))
                {
                    spawnPos = navHit.position;
                }

                var rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                var instance = Instantiate(targetData.Prefab, spawnPos, rotation, _spawnContainer);

                if (instance.TryGetComponent<NavMeshAgent>(out var agent))
                {
                    agent.Warp(spawnPos);
                }

                _spawnedTargets.Add(instance);
            }
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var rnd = Random.Range(i, list.Count);
                (list[i], list[rnd]) = (list[rnd], list[i]);
            }
        }
    }
}
