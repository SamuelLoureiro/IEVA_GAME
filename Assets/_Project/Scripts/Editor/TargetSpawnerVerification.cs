#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using VARG.Data;
using VARG.Mission;
using VARG.Targets;

namespace VARG.Editor
{
    public static class TargetSpawnerVerification
    {
        [MenuItem("VARG/Verify A08 - Target Spawner Logic")]
        public static void RunVerification()
        {
            var passedCount = 0;
            var totalCount = 4;

            // Test 1: Check TargetSpawner component and SpawnPoints in scene
            var spawner = Object.FindFirstObjectByType<TargetSpawner>();
            var spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

            if (spawner != null && spawnPoints.Length >= 4)
            {
                passedCount++;
                Debug.Log($"[Verification A08] Test 1 PASSED: Found TargetSpawner and {spawnPoints.Length} SpawnPoints in scene.");
            }
            else
            {
                Debug.LogError($"[Verification A08] Test 1 FAILED: Spawner found: {spawner != null}, SpawnPoints: {spawnPoints.Length}");
            }

            // Test 2: Generate controlled test mission and run Spawner
            var missionManager = MissionManager.Instance;
            if (missionManager == null)
            {
                var missionGo = new GameObject("Test_MissionManager");
                missionManager = missionGo.AddComponent<MissionManager>();
            }

            var levelConfig = ScriptableObject.CreateInstance<LevelConfig>();
            var serializedLevel = new SerializedObject(levelConfig);
            serializedLevel.FindProperty("_listSize").intValue = 3;
            serializedLevel.FindProperty("_extraSpawnsPerRequired").intValue = 1;
            serializedLevel.FindProperty("_forbiddenSpawnCount").intValue = 2;
            serializedLevel.ApplyModifiedPropertiesWithoutUndo();

            missionManager.GenerateMission(levelConfig);

            // Execute Spawn
            spawner.SpawnTargets();

            // Calculate expected counts
            var expectedCounts = new Dictionary<TargetType, int>();
            for (var i = 0; i < missionManager.RequiredOrder.Count; i++)
            {
                var type = missionManager.RequiredOrder[i].Type;
                expectedCounts[type] = expectedCounts.GetValueOrDefault(type, 0) + 1;
            }

            var expectedTotal = 0;
            var expectedByTargetType = new Dictionary<TargetType, int>();
            foreach (var kvp in expectedCounts)
            {
                var count = kvp.Value + levelConfig.ExtraSpawnsPerRequired;
                expectedByTargetType[kvp.Key] = count;
                expectedTotal += count;
            }

            expectedByTargetType[missionManager.ForbiddenType] = levelConfig.ForbiddenSpawnCount;
            expectedTotal += levelConfig.ForbiddenSpawnCount;

            var spawnedCounts = spawner.SpawnedCountsByType;
            var countsMatch = true;

            foreach (var kvp in expectedByTargetType)
            {
                var actual = spawnedCounts.GetValueOrDefault(kvp.Key, 0);
                if (actual != kvp.Value)
                {
                    countsMatch = false;
                    Debug.LogError($"[Verification A08] Count mismatch for {kvp.Key}: Expected {kvp.Value}, Got {actual}");
                }
            }

            if (countsMatch && spawner.SpawnedTargets.Count == expectedTotal)
            {
                passedCount++;
                Debug.Log($"[Verification A08] Test 2 PASSED: Spawned target counts ({spawner.SpawnedTargets.Count}) match GDD §R.2 rule exactly (k + extra per type + forbidden).");
            }
            else
            {
                Debug.LogError($"[Verification A08] Test 2 FAILED: Total spawned {spawner.SpawnedTargets.Count}, expected {expectedTotal}");
            }

            // Test 3: Verify components on spawned targets
            var allComponentsValid = true;
            for (var i = 0; i < spawner.SpawnedTargets.Count; i++)
            {
                var targetGo = spawner.SpawnedTargets[i];
                if (targetGo == null) continue;

                var targetComp = targetGo.GetComponent<Target>();
                var aiComp = targetGo.GetComponent<TargetAI>();
                var agentComp = targetGo.GetComponent<NavMeshAgent>();
                var shrinkComp = targetGo.GetComponent<Shrinkable>();

                if (targetComp == null || aiComp == null || agentComp == null || shrinkComp == null)
                {
                    allComponentsValid = false;
                    Debug.LogError($"[Verification A08] Missing components on {targetGo.name}");
                }
            }

            if (allComponentsValid)
            {
                passedCount++;
                Debug.Log("[Verification A08] Test 3 PASSED: All spawned targets have Target, TargetAI, NavMeshAgent and Shrinkable.");
            }

            // Test 4: Clear targets
            spawner.ClearSpawnedTargets();
            if (spawner.SpawnedTargets.Count == 0 && spawner.SpawnedCountsByType.Count == 0)
            {
                passedCount++;
                Debug.Log("[Verification A08] Test 4 PASSED: ClearSpawnedTargets cleans up all instances.");
            }
            else
            {
                Debug.LogError($"[Verification A08] Test 4 FAILED: Targets remaining: {spawner.SpawnedTargets.Count}");
            }

            Object.DestroyImmediate(levelConfig);

            if (passedCount == totalCount)
            {
                Debug.Log($"<color=green>[Verification A08] ALL {totalCount} TESTS PASSED for A08 TargetSpawner!</color>");
            }
            else
            {
                Debug.LogError($"[Verification A08] {passedCount}/{totalCount} tests passed.");
            }
        }
    }
}
#endif
