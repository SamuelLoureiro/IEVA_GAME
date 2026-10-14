#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VARG.Data;

namespace VARG.Editor
{
    public static class LevelConfigVerification
    {
        [MenuItem("VARG/Verify A15 - Level Config Balancing")]
        public static void RunVerification()
        {
            var passedCount = 0;
            var totalCount = 4;

            Debug.Log("[Verification A15] Starting Level Config Balancing verification...");

            LevelConfigBalancer.ApplyBalance();

            var lvl1 = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/_Project/Data/Levels/LevelConfig_1.asset");
            var lvl2 = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/_Project/Data/Levels/LevelConfig_2.asset");
            var lvl3 = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/_Project/Data/Levels/LevelConfig_3.asset");
            var campaign = AssetDatabase.LoadAssetAtPath<CampaignConfig>("Assets/_Project/Data/CampaignConfig.asset");

            // Test 1: Level 1 parameters (Introductory difficulty)
            if (lvl1 != null && lvl1.ListSize == 3 && Mathf.Approximately(lvl1.CaptureTime, 90f) && Mathf.Approximately(lvl1.SortingTime, 45f) && Mathf.Approximately(lvl1.FleeSpeedMultiplier, 1.0f))
            {
                passedCount++;
                Debug.Log($"[Verification A15] Test 1 PASSED: Level 1 parameters match GDD §R.2 (ListSize: {lvl1.ListSize}, Capture: {lvl1.CaptureTime}s, Sorting: {lvl1.SortingTime}s, FleeMultiplier: {lvl1.FleeSpeedMultiplier}).");
            }
            else
            {
                Debug.LogError("[Verification A15] Test 1 FAILED: Level 1 values do not match expected.");
            }

            // Test 2: Level 2 parameters (Moderate escalation)
            if (lvl2 != null && lvl2.ListSize == 4 && Mathf.Approximately(lvl2.CaptureTime, 100f) && Mathf.Approximately(lvl2.SortingTime, 55f) && Mathf.Approximately(lvl2.FleeSpeedMultiplier, 1.15f))
            {
                passedCount++;
                Debug.Log($"[Verification A15] Test 2 PASSED: Level 2 parameters match GDD §R.2 (ListSize: {lvl2.ListSize}, Capture: {lvl2.CaptureTime}s, Sorting: {lvl2.SortingTime}s, FleeMultiplier: {lvl2.FleeSpeedMultiplier}).");
            }
            else
            {
                Debug.LogError("[Verification A15] Test 2 FAILED: Level 2 values do not match expected.");
            }

            // Test 3: Level 3 parameters (High difficulty & agility)
            if (lvl3 != null && lvl3.ListSize == 5 && Mathf.Approximately(lvl3.CaptureTime, 110f) && Mathf.Approximately(lvl3.SortingTime, 65f) && lvl3.ExtraSpawnsPerRequired == 2 && lvl3.ForbiddenSpawnCount == 3 && Mathf.Approximately(lvl3.FleeSpeedMultiplier, 1.30f))
            {
                passedCount++;
                Debug.Log($"[Verification A15] Test 3 PASSED: Level 3 parameters match GDD §R.2 (ListSize: {lvl3.ListSize}, Capture: {lvl3.CaptureTime}s, Sorting: {lvl3.SortingTime}s, Extra: {lvl3.ExtraSpawnsPerRequired}, Forbidden: {lvl3.ForbiddenSpawnCount}, FleeMultiplier: {lvl3.FleeSpeedMultiplier}).");
            }
            else
            {
                Debug.LogError("[Verification A15] Test 3 FAILED: Level 3 values do not match expected.");
            }

            // Test 4: CampaignConfig linking
            if (campaign != null && campaign.Levels != null && campaign.Levels.Length == 3 && campaign.Targets != null && campaign.Targets.Length == 4)
            {
                passedCount++;
                Debug.Log("[Verification A15] Test 4 PASSED: CampaignConfig links all 3 Levels and 4 Targets.");
            }
            else
            {
                Debug.LogError("[Verification A15] Test 4 FAILED: CampaignConfig levels or targets array missing.");
            }

            Debug.Log($"[Verification A15] Summary: {passedCount}/{totalCount} tests PASSED.");
            if (passedCount == totalCount)
            {
                Debug.Log(">>> [A15 VERIFICATION SUCCESS] All criteria satisfied! <<<");
            }
            else
            {
                Debug.LogError(">>> [A15 VERIFICATION FAILURE] Some tests failed. <<<");
            }
        }
    }
}
#endif
