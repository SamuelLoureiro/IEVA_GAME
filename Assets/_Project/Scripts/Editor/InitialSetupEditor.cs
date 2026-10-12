#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Humor;
using VARG.Input;
using VARG.Inventory;
using VARG.Mission;

namespace VARG.Editor
{
    public static class InitialSetupEditor
    {
        [MenuItem("VARG/Setup Initial Project Assets")]
        public static void SetupProject()
        {
            SetupLayers();
            EnsureDirectories();
            CreateConfigs();
            CreateScenes();
            CreateSystemsPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VARG] Initial Project Assets, Scenes, Layers, and Systems Prefab created successfully!");
        }

        [MenuItem("VARG/Setup Layers and Tags")]
        public static void SetupLayers()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;

            var tagManager = new SerializedObject(assets[0]);
            var layersProp = tagManager.FindProperty("layers");
            if (layersProp == null) return;

            SetLayer(layersProp, 6, "Player");
            SetLayer(layersProp, 7, "Interactable");
            SetLayer(layersProp, 8, "Grabbable");
            SetLayer(layersProp, 9, "CageTrigger");

            tagManager.ApplyModifiedProperties();
            Debug.Log("[VARG] Configured Layers: Player (6), Interactable (7), Grabbable (8), CageTrigger (9)");
        }

        private static void SetLayer(SerializedProperty layersProp, int index, string layerName)
        {
            if (index >= layersProp.arraySize) return;
            var element = layersProp.GetArrayElementAtIndex(index);
            if (element != null && string.IsNullOrEmpty(element.stringValue))
            {
                element.stringValue = layerName;
            }
        }

        private static void EnsureDirectories()
        {
            EnsureDirectory("Assets/_Project/Data");
            EnsureDirectory("Assets/_Project/Data/Targets");
            EnsureDirectory("Assets/_Project/Data/Levels");
            EnsureDirectory("Assets/_Project/Scenes");
            EnsureDirectory("Assets/Resources/Systems");
        }

        private static void EnsureDirectory(string relativePath)
        {
            if (!AssetDatabase.IsValidFolder(relativePath))
            {
                var parent = Path.GetDirectoryName(relativePath)?.Replace("\\", "/");
                var folderName = Path.GetFileName(relativePath);
                if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
                {
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }

        private static void CreateScenes()
        {
            string[] sceneNames =
            {
                "00_Boot",
                "01_BossBriefing",
                "02_ShipFlight",
                "03_CityHunt",
                "04_ShipSorting",
                "05_Results"
            };

            var buildScenes = new EditorBuildSettingsScene[sceneNames.Length];

            for (var i = 0; i < sceneNames.Length; i++)
            {
                var path = $"Assets/_Project/Scenes/{sceneNames[i]}.unity";
                if (!File.Exists(path))
                {
                    var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                        UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                        UnityEditor.SceneManagement.NewSceneMode.Single);
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, path);
                }
                buildScenes[i] = new EditorBuildSettingsScene(path, true);
            }

            EditorBuildSettings.scenes = buildScenes;
        }

        private static void CreateConfigs()
        {
            // TargetData for the 4 targets
            CreateTargetData("Target_Chicken", TargetType.Chicken, "Galinha", 3f, 5f);
            CreateTargetData("Target_Cow", TargetType.Cow, "Vaca", 2f, 4f);
            CreateTargetData("Target_Pig", TargetType.Pig, "Porco", 2.5f, 4.5f);
            CreateTargetData("Target_Human", TargetType.Human, "Humano", 4f, 7f);
            // PenaltyConfig
            const string penaltyPath = "Assets/_Project/Data/PenaltyConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<PenaltyConfig>(penaltyPath) == null)
            {
                var penalty = ScriptableObject.CreateInstance<PenaltyConfig>();
                AssetDatabase.CreateAsset(penalty, penaltyPath);
            }

            // ScoringConfig
            const string scoringPath = "Assets/_Project/Data/ScoringConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<ScoringConfig>(scoringPath) == null)
            {
                var scoring = ScriptableObject.CreateInstance<ScoringConfig>();
                AssetDatabase.CreateAsset(scoring, scoringPath);
            }

            // GameConfig
            const string gameConfigPath = "Assets/_Project/Data/GameConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<GameConfig>(gameConfigPath) == null)
            {
                var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(gameConfig, gameConfigPath);
            }

            // Levels
            for (var i = 1; i <= 3; i++)
            {
                var levelPath = $"Assets/_Project/Data/Levels/LevelConfig_{i}.asset";
                if (AssetDatabase.LoadAssetAtPath<LevelConfig>(levelPath) == null)
                {
                    var level = ScriptableObject.CreateInstance<LevelConfig>();
                    AssetDatabase.CreateAsset(level, levelPath);
                }
            }

            CreateCampaignConfig();
        }

        private static void CreateTargetData(string assetName, TargetType type, string displayName, float walkSpeed, float fleeSpeed)
        {
            var path = $"Assets/_Project/Data/Targets/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<TargetData>(path) != null) return;

            var target = ScriptableObject.CreateInstance<TargetData>();
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty("_id").stringValue = assetName;
            serializedObject.FindProperty("_type").enumValueIndex = (int)type;
            serializedObject.FindProperty("_displayName").stringValue = displayName;
            serializedObject.FindProperty("_walkSpeed").floatValue = walkSpeed;
            serializedObject.FindProperty("_fleeSpeed").floatValue = fleeSpeed;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(target, path);
        }

        private static void CreateCampaignConfig()
        {
            const string campaignPath = "Assets/_Project/Data/CampaignConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<CampaignConfig>(campaignPath) != null) return;

            var campaign = ScriptableObject.CreateInstance<CampaignConfig>();
            AssetDatabase.CreateAsset(campaign, campaignPath);
        }

        private static void CreateSystemsPrefab()
        {
            const string prefabPath = "Assets/Resources/Systems/[Systems].prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

            var systemsGo = new GameObject("[Systems]");
            systemsGo.AddComponent<GameManager>();
            systemsGo.AddComponent<MissionManager>();
            systemsGo.AddComponent<InventoryManager>();
            systemsGo.AddComponent<TimerManager>();
            systemsGo.AddComponent<LivesManager>();
            systemsGo.AddComponent<BossDialogueManager>();
            systemsGo.AddComponent<SceneLoader>();
            systemsGo.AddComponent<InputManager>();

            PrefabUtility.SaveAsPrefabAsset(systemsGo, prefabPath);
            Object.DestroyImmediate(systemsGo);
            Debug.Log($"[VARG] [Systems] prefab created at {prefabPath}");
        }

        [MenuItem("VARG/Update [Systems] Prefab References")]
        public static void UpdateSystemsPrefab()
        {
            const string prefabPath = "Assets/Resources/Systems/[Systems].prefab";
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null)
            {
                Debug.LogWarning($"[VARG] Prefab not found at {prefabPath}");
                return;
            }

            var gameManager = root.GetComponent<GameManager>();
            if (gameManager != null)
            {
                var campaignConfig = AssetDatabase.LoadAssetAtPath<CampaignConfig>("Assets/_Project/Data/CampaignConfig.asset");
                var gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Project/Data/GameConfig.asset");
                var scoringConfig = AssetDatabase.LoadAssetAtPath<ScoringConfig>("Assets/_Project/Data/ScoringConfig.asset");
                var penaltyConfig = AssetDatabase.LoadAssetAtPath<PenaltyConfig>("Assets/_Project/Data/PenaltyConfig.asset");

                var serializedGm = new SerializedObject(gameManager);
                var campProp = serializedGm.FindProperty("_campaignConfig");
                var gameProp = serializedGm.FindProperty("_gameConfig");
                var scorProp = serializedGm.FindProperty("_scoringConfig");
                var penProp = serializedGm.FindProperty("_penaltyConfig");

                if (campProp != null) campProp.objectReferenceValue = campaignConfig;
                if (gameProp != null) gameProp.objectReferenceValue = gameConfig;
                if (scorProp != null) scorProp.objectReferenceValue = scoringConfig;
                if (penProp != null) penProp.objectReferenceValue = penaltyConfig;

                serializedGm.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[VARG] GameManager references on [Systems] prefab updated!");
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
