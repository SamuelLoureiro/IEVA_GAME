#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VARG.Data;

namespace VARG.Editor
{
    public static class LevelConfigBalancer
    {
        [MenuItem("VARG/Apply Level Balance (Levels 1-3)")]
        public static void ApplyBalance()
        {
            const string lvl1Path = "Assets/_Project/Data/Levels/LevelConfig_1.asset";
            const string lvl2Path = "Assets/_Project/Data/Levels/LevelConfig_2.asset";
            const string lvl3Path = "Assets/_Project/Data/Levels/LevelConfig_3.asset";
            const string campaignPath = "Assets/_Project/Data/CampaignConfig.asset";

            var lvl1 = AssetDatabase.LoadAssetAtPath<LevelConfig>(lvl1Path);
            var lvl2 = AssetDatabase.LoadAssetAtPath<LevelConfig>(lvl2Path);
            var lvl3 = AssetDatabase.LoadAssetAtPath<LevelConfig>(lvl3Path);
            var campaign = AssetDatabase.LoadAssetAtPath<CampaignConfig>(campaignPath);

            if (lvl1 == null || lvl2 == null || lvl3 == null)
            {
                Debug.LogError("[LevelConfigBalancer] One or more LevelConfig assets were not found in Assets/_Project/Data/Levels/!");
                return;
            }

            // Level 1: Standard intro level (GDD §R.2)
            ConfigureLevel(lvl1, listSize: 3, captureTime: 90f, sortingTime: 45f, extraSpawns: 1, forbiddenSpawns: 2, fleeMultiplier: 1.0f);

            // Level 2: Escalating quota and agility
            ConfigureLevel(lvl2, listSize: 4, captureTime: 100f, sortingTime: 55f, extraSpawns: 1, forbiddenSpawns: 2, fleeMultiplier: 1.15f);

            // Level 3: Challenging quota, more evasive targets, high penalty risk
            ConfigureLevel(lvl3, listSize: 5, captureTime: 110f, sortingTime: 65f, extraSpawns: 2, forbiddenSpawns: 3, fleeMultiplier: 1.30f);

            // Ensure CampaignConfig has references to all 3 levels and 4 targets
            if (campaign != null)
            {
                var chicken = AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Chicken.asset");
                var cow = AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Cow.asset");
                var human = AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Human.asset");
                var pig = AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Pig.asset");

                var serializedCampaign = new SerializedObject(campaign);

                var levelsProp = serializedCampaign.FindProperty("_levels");
                levelsProp.arraySize = 3;
                levelsProp.GetArrayElementAtIndex(0).objectReferenceValue = lvl1;
                levelsProp.GetArrayElementAtIndex(1).objectReferenceValue = lvl2;
                levelsProp.GetArrayElementAtIndex(2).objectReferenceValue = lvl3;

                var targetsProp = serializedCampaign.FindProperty("_targets");
                targetsProp.arraySize = 4;
                targetsProp.GetArrayElementAtIndex(0).objectReferenceValue = chicken;
                targetsProp.GetArrayElementAtIndex(1).objectReferenceValue = cow;
                targetsProp.GetArrayElementAtIndex(2).objectReferenceValue = human;
                targetsProp.GetArrayElementAtIndex(3).objectReferenceValue = pig;

                serializedCampaign.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(campaign);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[LevelConfigBalancer] Levels 1-3 and CampaignConfig successfully balanced and saved according to GDD §R.2!");
        }

        private static void ConfigureLevel(LevelConfig config, int listSize, float captureTime, float sortingTime, int extraSpawns, int forbiddenSpawns, float fleeMultiplier)
        {
            var serialized = new SerializedObject(config);

            serialized.FindProperty("_listSize").intValue = listSize;
            serialized.FindProperty("_captureTime").floatValue = captureTime;
            serialized.FindProperty("_sortingTime").floatValue = sortingTime;
            serialized.FindProperty("_extraSpawnsPerRequired").intValue = extraSpawns;
            serialized.FindProperty("_forbiddenSpawnCount").intValue = forbiddenSpawns;
            serialized.FindProperty("_fleeSpeedMultiplier").floatValue = fleeMultiplier;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }
    }
}
#endif
