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
        private readonly Dictionary<TargetType, int> _spawnedCountsByType = new();

        public IReadOnlyList<GameObject> SpawnedTargets => _spawnedTargets;
        public IReadOnlyDictionary<TargetType, int> SpawnedCountsByType => _spawnedCountsByType;

        private void Awake()
        {
            if (_spawnContainer == null)
            {
                var containerGo = new GameObject("_SpawnedTargets");
                _spawnContainer = containerGo.transform;
            }

            if (_fallbackLevelConfig == null)
            {
#if UNITY_EDITOR
                _fallbackLevelConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/_Project/Data/Levels/LevelConfig_1.asset");
#endif
            }
        }

        private void Start()
        {
            if (_spawnOnStart)
            {
                SpawnTargets();
            }
        }

        [ContextMenu("Clear Spawned Targets")]
        public void ClearSpawnedTargets()
        {
            for (var i = 0; i < _spawnedTargets.Count; i++)
            {
                if (_spawnedTargets[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(_spawnedTargets[i]);
                    }
                    else
                    {
                        DestroyImmediate(_spawnedTargets[i]);
                    }
                }
            }

            _spawnedTargets.Clear();
            _spawnedCountsByType.Clear();
        }

        [ContextMenu("Spawn Targets")]
        public void SpawnTargets()
        {
            ClearSpawnedTargets();
            CollectSpawnPointsIfEmpty();

            var mission = MissionManager.HasInstance ? MissionManager.Instance : Object.FindFirstObjectByType<MissionManager>();
            if (mission == null)
            {
                var systemsPrefab = Resources.Load<GameObject>("Systems/[Systems]");
                if (systemsPrefab != null)
                {
                    var systemsInstance = Instantiate(systemsPrefab);
                    mission = systemsInstance.GetComponentInChildren<MissionManager>() ?? Object.FindFirstObjectByType<MissionManager>();
                }
            }

            if (mission == null)
            {
                Debug.LogWarning("[TargetSpawner] MissionManager not found in scene. Cannot spawn mission-based targets.");
                return;
            }

            var levelConfig = GameManager.HasInstance ? GameManager.Instance.GetCurrentLevelConfig() : _fallbackLevelConfig;

            if (mission.RequiredCount == 0 && levelConfig != null)
            {
                mission.GenerateMission(levelConfig);
            }

            if (mission.RequiredCount == 0)
            {
                Debug.LogWarning("[TargetSpawner] Mission required order is empty. Cannot spawn targets.");
                return;
            }

            var extraSpawns = levelConfig != null ? levelConfig.ExtraSpawnsPerRequired : 1;
            var forbiddenSpawns = levelConfig != null ? levelConfig.ForbiddenSpawnCount : 2;

            // 1. Calculate counts according to GDD §R.2
            var requiredCounts = new Dictionary<TargetType, int>();
            for (var i = 0; i < mission.RequiredOrder.Count; i++)
            {
                var type = mission.RequiredOrder[i].Type;
                requiredCounts[type] = requiredCounts.GetValueOrDefault(type, 0) + 1;
            }

            var spawnQueue = new List<TargetType>();

            foreach (var kvp in requiredCounts)
            {
                var type = kvp.Key;
                var total = kvp.Value + extraSpawns;
                for (var i = 0; i < total; i++)
                {
                    spawnQueue.Add(type);
                }
            }

            var forbiddenType = mission.ForbiddenType;
            for (var i = 0; i < forbiddenSpawns; i++)
            {
                spawnQueue.Add(forbiddenType);
            }

            // 2. Shuffle queue and available spawn points
            Shuffle(spawnQueue);
            var shuffledPoints = new List<SpawnPoint>(_spawnPoints);
            Shuffle(shuffledPoints);

            // 3. Spawn targets
            for (var i = 0; i < spawnQueue.Count; i++)
            {
                var type = spawnQueue[i];
                var targetData = mission.GetTargetDataForType(type);
                if (targetData == null || targetData.Prefab == null)
                {
                    Debug.LogWarning($"[TargetSpawner] No valid prefab found for TargetType: {type}");
                    continue;
                }

                var spawnPoint = GetSpawnPoint(shuffledPoints, i);
                var spawnPos = spawnPoint != null ? spawnPoint.Position : transform.position;

                if (NavMesh.SamplePosition(spawnPos, out var navHit, 3f, NavMesh.AllAreas))
                {
                    spawnPos = navHit.position;
                }

                var randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
#if UNITY_EDITOR
                var instance = Application.isPlaying
                    ? Instantiate(targetData.Prefab, spawnPos, randomRotation, _spawnContainer)
                    : (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(targetData.Prefab, _spawnContainer);
                if (!Application.isPlaying && instance != null)
                {
                    instance.transform.position = spawnPos;
                    instance.transform.rotation = randomRotation;
                }
#else
                var instance = Instantiate(targetData.Prefab, spawnPos, randomRotation, _spawnContainer);
#endif
                if (instance == null) continue;

                var agent = instance.GetComponent<NavMeshAgent>();
                if (agent != null && Application.isPlaying)
                {
                    agent.Warp(spawnPos);
                }

                _spawnedTargets.Add(instance);
                _spawnedCountsByType[type] = _spawnedCountsByType.GetValueOrDefault(type, 0) + 1;
            }

            LogSpawnSummary(requiredCounts, extraSpawns, forbiddenType, forbiddenSpawns);
        }

        private void CollectSpawnPointsIfEmpty()
        {
            if (_spawnPoints.Count > 0) return;

            var points = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            _spawnPoints.AddRange(points);
        }

        private SpawnPoint GetSpawnPoint(List<SpawnPoint> points, int index)
        {
            if (points == null || points.Count == 0) return null;
            return points[index % points.Count];
        }

        private void LogSpawnSummary(Dictionary<TargetType, int> requiredCounts, int extraSpawns, TargetType forbiddenType, int forbiddenSpawns)
        {
            var summary = $"[TargetSpawner] Spawned {_spawnedTargets.Count} targets:";
            foreach (var kvp in _spawnedCountsByType)
            {
                var isForbidden = kvp.Key == forbiddenType;
                var note = isForbidden ? " (Forbidden)" : $" (Required: {requiredCounts.GetValueOrDefault(kvp.Key, 0)} + {extraSpawns})";
                summary += $" [{kvp.Key}: {kvp.Value}{note}]";
            }
            Debug.Log(summary);
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
