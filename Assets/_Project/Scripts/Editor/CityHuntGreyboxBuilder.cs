#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using VARG.Ship;
using VARG.Targets;
using VARG.UI;

namespace VARG.Editor
{
    public static class CityHuntGreyboxBuilder
    {
        [MenuItem("VARG/Build City Hunt Greybox")]
        public static void BuildCityHuntScene()
        {
            const string scenePath = "Assets/_Project/Scenes/03_CityHunt.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Clean previous environment setup if present
            var existingEnv = GameObject.Find("_Environment");
            if (existingEnv != null)
            {
                Object.DestroyImmediate(existingEnv);
            }

            var existingPlayer = GameObject.Find("ETPlayer");
            if (existingPlayer != null)
            {
                Object.DestroyImmediate(existingPlayer);
            }

            var existingCamera = GameObject.Find("Main Camera");
            if (existingCamera != null)
            {
                Object.DestroyImmediate(existingCamera);
            }

            var existingSampleTargets = GameObject.Find("_SampleTargets");
            if (existingSampleTargets != null)
            {
                Object.DestroyImmediate(existingSampleTargets);
            }

            var existingSpawner = GameObject.Find("TargetSpawner");
            if (existingSpawner != null)
            {
                Object.DestroyImmediate(existingSpawner);
            }

            var existingSpawnPoints = GameObject.Find("_SpawnPoints");
            if (existingSpawnPoints != null)
            {
                Object.DestroyImmediate(existingSpawnPoints);
            }

            var existingSpawned = GameObject.Find("_SpawnedTargets");
            if (existingSpawned != null)
            {
                Object.DestroyImmediate(existingSpawned);
            }

            var existingDebug = GameObject.Find("_DebugTools");
            if (existingDebug != null)
            {
                Object.DestroyImmediate(existingDebug);
            }

            var existingHud = GameObject.Find("HuntHUD");
            if (existingHud != null)
            {
                Object.DestroyImmediate(existingHud);
            }

            var envRoot = new GameObject("_Environment");

            // 1. Ground (80x80)
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(envRoot.transform);
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(80f, 1f, 80f);

            // 2. Boundary Walls (to keep player and AI inside the city)
            var boundariesRoot = new GameObject("Boundaries");
            boundariesRoot.transform.SetParent(envRoot.transform);

            CreateBoundaryWall(boundariesRoot, "Wall_North", new Vector3(0f, 3f, 40f), new Vector3(80f, 6f, 1f));
            CreateBoundaryWall(boundariesRoot, "Wall_South", new Vector3(0f, 3f, -40f), new Vector3(80f, 6f, 1f));
            CreateBoundaryWall(boundariesRoot, "Wall_East", new Vector3(40f, 3f, 0f), new Vector3(1f, 6f, 80f));
            CreateBoundaryWall(boundariesRoot, "Wall_West", new Vector3(-40f, 3f, 0f), new Vector3(1f, 6f, 80f));

            // 3. Buildings / Street Obstacles
            var buildingsRoot = new GameObject("Buildings");
            buildingsRoot.transform.SetParent(envRoot.transform);

            CreateBuilding(buildingsRoot, "House_NW", new Vector3(-18f, 3f, 18f), new Vector3(12f, 6f, 12f));
            CreateBuilding(buildingsRoot, "House_NE", new Vector3(18f, 3f, 18f), new Vector3(12f, 6f, 12f));
            CreateBuilding(buildingsRoot, "House_SW", new Vector3(-18f, 3f, -18f), new Vector3(12f, 6f, 12f));
            CreateBuilding(buildingsRoot, "House_SE", new Vector3(18f, 3f, -18f), new Vector3(12f, 6f, 12f));

            CreateBuilding(buildingsRoot, "Warehouse_West", new Vector3(-30f, 3.5f, 0f), new Vector3(10f, 7f, 20f));
            CreateBuilding(buildingsRoot, "Shop_East", new Vector3(30f, 2.5f, 0f), new Vector3(10f, 5f, 18f));

            // Small obstacles / fences
            CreateBuilding(buildingsRoot, "Fence_1", new Vector3(-7f, 0.75f, 9f), new Vector3(8f, 1.5f, 0.4f));
            CreateBuilding(buildingsRoot, "Fence_2", new Vector3(7f, 0.75f, -9f), new Vector3(8f, 1.5f, 0.4f));

            // 4. UFO / Mothership hovering in the sky (Y = 25)
            var shipRoot = new GameObject("AlienMothership");
            shipRoot.transform.SetParent(envRoot.transform);
            shipRoot.transform.position = new Vector3(0f, 25f, 0f);

            var saucerHull = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            saucerHull.name = "SaucerHull";
            saucerHull.transform.SetParent(shipRoot.transform);
            saucerHull.transform.localPosition = Vector3.zero;
            saucerHull.transform.localScale = new Vector3(14f, 1.2f, 14f);

            var saucerDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            saucerDome.name = "SaucerDome";
            saucerDome.transform.SetParent(shipRoot.transform);
            saucerDome.transform.localPosition = new Vector3(0f, 1f, 0f);
            saucerDome.transform.localScale = new Vector3(7f, 3.5f, 7f);

            // Abduction Spot Light shining down from the UFO
            var beamLightGo = new GameObject("AbductionSpotlight");
            beamLightGo.transform.SetParent(shipRoot.transform);
            beamLightGo.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            beamLightGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var beamLight = beamLightGo.AddComponent<Light>();
            beamLight.type = LightType.Spot;
            beamLight.range = 35f;
            beamLight.spotAngle = 35f;
            beamLight.color = new Color(0.2f, 1f, 0.5f);
            beamLight.intensity = 3f;

            // Abduction Beam Zone (flat visual disc on floor + proper vertical trigger collider + ShipBeamZone)
            var beamZoneGo = new GameObject("AbductionBeamZone");
            beamZoneGo.transform.SetParent(envRoot.transform);
            beamZoneGo.transform.position = Vector3.zero;

            var visualDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visualDisc.name = "BeamVisualDisc";
            visualDisc.transform.SetParent(beamZoneGo.transform);
            visualDisc.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            visualDisc.transform.localScale = new Vector3(8f, 0.04f, 8f);
            var discCol = visualDisc.GetComponent<Collider>();
            if (discCol != null) Object.DestroyImmediate(discCol);

            var beamCol = beamZoneGo.AddComponent<CapsuleCollider>();
            beamCol.isTrigger = true;
            beamCol.radius = 4f;
            beamCol.height = 6f;
            beamCol.center = new Vector3(0f, 3f, 0f);

            var shipBeam = beamZoneGo.AddComponent<ShipBeamZone>();
            var serializedBeam = new SerializedObject(shipBeam);
            serializedBeam.FindProperty("_spotlight").objectReferenceValue = beamLight;
            serializedBeam.FindProperty("_padRenderer").objectReferenceValue = visualDisc.GetComponent<Renderer>();
            serializedBeam.ApplyModifiedPropertiesWithoutUndo();

            // 5. Build NavMeshSurface for environment
            var navSurface = envRoot.AddComponent<NavMeshSurface>();
            navSurface.collectObjects = CollectObjects.Children;
            navSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            navSurface.BuildNavMesh();

            // 6. Instantiate ETPlayer
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player/ETPlayer.prefab");
            if (playerPrefab != null)
            {
                var playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerInstance.transform.position = new Vector3(0f, 0.1f, -7f);
                playerInstance.transform.rotation = Quaternion.identity;
            }

            // 7. Create 16 Spawn Points across city streets and plazas
            var spawnPointsRoot = new GameObject("_SpawnPoints");
            var spawnPointList = new List<SpawnPoint>();

            Vector3[] spawnPositions = new Vector3[]
            {
                new Vector3(-6f, 0.1f, 8f),
                new Vector3(6f, 0.1f, 8f),
                new Vector3(-8f, 0.1f, -8f),
                new Vector3(8f, 0.1f, -8f),

                new Vector3(-18f, 0.1f, 6f),
                new Vector3(18f, 0.1f, 6f),
                new Vector3(-18f, 0.1f, -6f),
                new Vector3(18f, 0.1f, -6f),

                new Vector3(-6f, 0.1f, 26f),
                new Vector3(6f, 0.1f, 26f),
                new Vector3(-28f, 0.1f, 18f),
                new Vector3(28f, 0.1f, 18f),

                new Vector3(-6f, 0.1f, -26f),
                new Vector3(6f, 0.1f, -26f),
                new Vector3(-28f, 0.1f, -18f),
                new Vector3(28f, 0.1f, -18f)
            };

            for (var i = 0; i < spawnPositions.Length; i++)
            {
                var spGo = new GameObject($"SpawnPoint_{i + 1}");
                spGo.transform.SetParent(spawnPointsRoot.transform);
                spGo.transform.position = spawnPositions[i];
                var sp = spGo.AddComponent<SpawnPoint>();
                spawnPointList.Add(sp);
            }

            // Create TargetSpawner
            var spawnerGo = new GameObject("TargetSpawner");
            var spawner = spawnerGo.AddComponent<TargetSpawner>();
            var spawnerSerialized = new SerializedObject(spawner);
            var spListProp = spawnerSerialized.FindProperty("_spawnPoints");
            spListProp.arraySize = spawnPointList.Count;
            for (var i = 0; i < spawnPointList.Count; i++)
            {
                spListProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPointList[i];
            }
            spawnerSerialized.ApplyModifiedPropertiesWithoutUndo();

            // 8. Ensure Lighting
            EnsureDirectionalLight();

            // 9. Hunt HUD Canvas with Targets Progress
            SetupHuntHUDInternal();

            // 10. Debug Tools for standalone testing (DebugMissionSeeder & DebugInventorySeeder)
            var debugGo = new GameObject("_DebugTools");
            var missionSeeder = debugGo.AddComponent<VARG.DebugTools.DebugMissionSeeder>();
            var missionSeederSerialized = new SerializedObject(missionSeeder);
            missionSeederSerialized.FindProperty("_seedOnStart").boolValue = true;
            missionSeederSerialized.FindProperty("_dontDestroyOnLoad").boolValue = false;
            missionSeederSerialized.ApplyModifiedPropertiesWithoutUndo();

            var invSeeder = debugGo.AddComponent<VARG.DebugTools.DebugInventorySeeder>();
            var invSeederSerialized = new SerializedObject(invSeeder);
            invSeederSerialized.FindProperty("_dontDestroyOnLoad").boolValue = false;
            invSeederSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[VARG] 03_CityHunt Greybox successfully built, configured, and saved!");
        }

        private static void CreateBoundaryWall(GameObject parent, string name, Vector3 pos, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent.transform);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
        }

        private static void CreateBuilding(GameObject parent, string name, Vector3 pos, Vector3 scale)
        {
            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = name;
            building.transform.SetParent(parent.transform);
            building.transform.position = pos;
            building.transform.localScale = scale;

            var obstacle = building.AddComponent<NavMeshObstacle>();
            obstacle.size = Vector3.one;
            obstacle.carving = true;
        }

        private static void SpawnSampleTarget(GameObject parent, string prefabPath, Vector3 pos)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent.transform);
            instance.transform.position = pos;
        }

        private static void EnsureDirectionalLight()
        {
            var light = Object.FindFirstObjectByType<Light>();
            if (light != null) return;

            var lightGo = new GameObject("Directional Light");
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.95f, 0.85f);
            l.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        [MenuItem("VARG/Setup Ship Beam Zone in City Hunt")]
        public static void SetupShipBeamZoneOnly()
        {
            const string scenePath = "Assets/_Project/Scenes/03_CityHunt.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var envRoot = GameObject.Find("_Environment");
            if (envRoot == null)
            {
                Debug.LogWarning("[CityHuntGreyboxBuilder] Environment root not found in 03_CityHunt.");
                return;
            }

            var beamLight = GameObject.Find("AbductionSpotlight")?.GetComponent<Light>();

            var existingBeam = GameObject.Find("AbductionBeamZone");
            if (existingBeam != null)
            {
                Object.DestroyImmediate(existingBeam);
            }

            var beamZoneGo = new GameObject("AbductionBeamZone");
            beamZoneGo.transform.SetParent(envRoot.transform);
            beamZoneGo.transform.position = Vector3.zero;

            var visualDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visualDisc.name = "BeamVisualDisc";
            visualDisc.transform.SetParent(beamZoneGo.transform);
            visualDisc.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            visualDisc.transform.localScale = new Vector3(8f, 0.04f, 8f);
            var discCol = visualDisc.GetComponent<Collider>();
            if (discCol != null) Object.DestroyImmediate(discCol);

            var beamCol = beamZoneGo.AddComponent<CapsuleCollider>();
            beamCol.isTrigger = true;
            beamCol.radius = 4f;
            beamCol.height = 6f;
            beamCol.center = new Vector3(0f, 3f, 0f);

            var shipBeam = beamZoneGo.AddComponent<ShipBeamZone>();
            var serializedBeam = new SerializedObject(shipBeam);
            if (beamLight != null)
            {
                serializedBeam.FindProperty("_spotlight").objectReferenceValue = beamLight;
            }
            serializedBeam.FindProperty("_padRenderer").objectReferenceValue = visualDisc.GetComponent<Renderer>();
            serializedBeam.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[CityHuntGreyboxBuilder] ShipBeamZone configured successfully in 03_CityHunt!");
        }

        [MenuItem("VARG/Setup Hunt HUD in City Hunt")]
        public static void SetupHuntHUDOnly()
        {
            const string scenePath = "Assets/_Project/Scenes/03_CityHunt.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            SetupHuntHUDInternal();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[CityHuntGreyboxBuilder] Hunt HUD configured successfully in 03_CityHunt!");
        }

        private static void SetupHuntHUDInternal()
        {
            var existingHud = GameObject.Find("HuntHUD");
            if (existingHud != null)
            {
                Object.DestroyImmediate(existingHud);
            }

            var hudGo = new GameObject("HuntHUD");
            var canvas = hudGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = hudGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            hudGo.AddComponent<GraphicRaycaster>();

            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            var popupPrefab = EnsureTimeDeltaPopupPrefab(fontAsset);

            // 1. Lives Panel (Top-Left)
            var livesPanelGo = new GameObject("Panel_Lives");
            livesPanelGo.transform.SetParent(hudGo.transform, false);

            var livesRect = livesPanelGo.AddComponent<RectTransform>();
            livesRect.anchorMin = new Vector2(0f, 1f);
            livesRect.anchorMax = new Vector2(0f, 1f);
            livesRect.pivot = new Vector2(0f, 1f);
            livesRect.anchoredPosition = new Vector2(30f, -30f);
            livesRect.sizeDelta = new Vector2(240f, 60f);

            var livesBg = livesPanelGo.AddComponent<Image>();
            livesBg.color = new Color(0.08f, 0.1f, 0.14f, 0.75f);

            var livesLabelGo = new GameObject("Text_LivesLabel");
            livesLabelGo.transform.SetParent(livesPanelGo.transform, false);
            var livesLabelRect = livesLabelGo.AddComponent<RectTransform>();
            livesLabelRect.anchorMin = new Vector2(0f, 0f);
            livesLabelRect.anchorMax = new Vector2(0f, 1f);
            livesLabelRect.anchoredPosition = new Vector2(50f, 0f);
            livesLabelRect.sizeDelta = new Vector2(80f, 0f);

            var livesTmp = livesLabelGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) livesTmp.font = fontAsset;
            livesTmp.text = "Lives: 5";
            livesTmp.fontSize = 20;
            livesTmp.fontStyle = FontStyles.Bold;
            livesTmp.alignment = TextAlignmentOptions.MidlineLeft;
            livesTmp.color = Color.white;

            var iconsContainerGo = new GameObject("Icons_Container");
            iconsContainerGo.transform.SetParent(livesPanelGo.transform, false);
            var iconsContainerRect = iconsContainerGo.AddComponent<RectTransform>();
            iconsContainerRect.anchorMin = new Vector2(0f, 0.5f);
            iconsContainerRect.anchorMax = new Vector2(1f, 0.5f);
            iconsContainerRect.anchoredPosition = new Vector2(50f, 0f);
            iconsContainerRect.sizeDelta = new Vector2(-110f, 24f);

            var iconsLayout = iconsContainerGo.AddComponent<HorizontalLayoutGroup>();
            iconsLayout.spacing = 6f;
            iconsLayout.childAlignment = TextAnchor.MiddleCenter;
            iconsLayout.childForceExpandWidth = false;
            iconsLayout.childForceExpandHeight = false;

            var lifeIcons = new Image[5];
            for (var i = 0; i < 5; i++)
            {
                var iconGo = new GameObject($"LifeIcon_{i + 1}");
                iconGo.transform.SetParent(iconsContainerGo.transform, false);
                var iconRect = iconGo.AddComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(16f, 16f);
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.color = new Color(0.2f, 1f, 0.5f);
                lifeIcons[i] = iconImg;
            }

            var livesUi = livesPanelGo.AddComponent<LivesUI>();
            var serializedLivesUi = new SerializedObject(livesUi);
            serializedLivesUi.FindProperty("_livesText").objectReferenceValue = livesTmp;
            var iconsProp = serializedLivesUi.FindProperty("_lifeIcons");
            iconsProp.arraySize = 5;
            for (var i = 0; i < 5; i++)
            {
                iconsProp.GetArrayElementAtIndex(i).objectReferenceValue = lifeIcons[i];
            }
            serializedLivesUi.ApplyModifiedPropertiesWithoutUndo();

            // 2. Timer Panel (Top-Center)
            var timerPanelGo = new GameObject("Panel_Timer");
            timerPanelGo.transform.SetParent(hudGo.transform, false);

            var timerRect = timerPanelGo.AddComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 1f);
            timerRect.anchorMax = new Vector2(0.5f, 1f);
            timerRect.pivot = new Vector2(0.5f, 1f);
            timerRect.anchoredPosition = new Vector2(0f, -30f);
            timerRect.sizeDelta = new Vector2(220f, 75f);

            var timerBg = timerPanelGo.AddComponent<Image>();
            timerBg.color = new Color(0.08f, 0.1f, 0.14f, 0.75f);

            var timerTextGo = new GameObject("Text_Timer");
            timerTextGo.transform.SetParent(timerPanelGo.transform, false);
            var timerTextRect = timerTextGo.AddComponent<RectTransform>();
            timerTextRect.anchorMin = new Vector2(0f, 0.35f);
            timerTextRect.anchorMax = new Vector2(1f, 1f);
            timerTextRect.sizeDelta = Vector2.zero;

            var timerTmp = timerTextGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) timerTmp.font = fontAsset;
            timerTmp.text = "01:30";
            timerTmp.fontSize = 32;
            timerTmp.fontStyle = FontStyles.Bold;
            timerTmp.alignment = TextAlignmentOptions.Center;
            timerTmp.color = Color.white;

            var debtTextGo = new GameObject("Text_Debt");
            debtTextGo.transform.SetParent(timerPanelGo.transform, false);
            var debtTextRect = debtTextGo.AddComponent<RectTransform>();
            debtTextRect.anchorMin = new Vector2(0f, 0f);
            debtTextRect.anchorMax = new Vector2(1f, 0.35f);
            debtTextRect.sizeDelta = Vector2.zero;

            var debtTmp = debtTextGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) debtTmp.font = fontAsset;
            debtTmp.text = "Debt: -10s";
            debtTmp.fontSize = 16;
            debtTmp.fontStyle = FontStyles.Bold;
            debtTmp.alignment = TextAlignmentOptions.Center;
            debtTmp.color = new Color(1f, 0.45f, 0.2f);
            debtTextGo.SetActive(false);

            var popupContainerGo = new GameObject("Transform_PopupContainer");
            popupContainerGo.transform.SetParent(timerPanelGo.transform, false);
            var popupContainerRect = popupContainerGo.AddComponent<RectTransform>();
            popupContainerRect.anchorMin = new Vector2(0.5f, 0f);
            popupContainerRect.anchorMax = new Vector2(0.5f, 0f);
            popupContainerRect.anchoredPosition = new Vector2(0f, -25f);
            popupContainerRect.sizeDelta = Vector2.zero;

            var timerUi = timerPanelGo.AddComponent<TimerUI>();
            var serializedTimerUi = new SerializedObject(timerUi);
            serializedTimerUi.FindProperty("_timerText").objectReferenceValue = timerTmp;
            serializedTimerUi.FindProperty("_debtText").objectReferenceValue = debtTmp;
            serializedTimerUi.FindProperty("_timerBackground").objectReferenceValue = timerBg;
            serializedTimerUi.FindProperty("_popupPrefab").objectReferenceValue = popupPrefab;
            serializedTimerUi.FindProperty("_popupContainer").objectReferenceValue = popupContainerGo.transform;
            serializedTimerUi.FindProperty("_autoStartCaptureTimer").boolValue = true;
            serializedTimerUi.FindProperty("_defaultCaptureDuration").floatValue = 90f;
            serializedTimerUi.ApplyModifiedPropertiesWithoutUndo();

            // 3. Targets Progress Panel (Top-Right)
            var targetPanelGo = new GameObject("Panel_TargetsProgress");
            targetPanelGo.transform.SetParent(hudGo.transform, false);

            var panelRect = targetPanelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-30f, -30f);
            panelRect.sizeDelta = new Vector2(360f, 60f);

            var panelImg = targetPanelGo.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.1f, 0.14f, 0.75f);

            var textGo = new GameObject("Text_Progress");
            textGo.transform.SetParent(targetPanelGo.transform, false);

            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(20f, 0f);
            textRect.offsetMax = new Vector2(-20f, 0f);

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) tmp.font = fontAsset;
            tmp.text = "Targets: 0/0";
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Midline;
            tmp.color = Color.white;

            var huntUi = targetPanelGo.AddComponent<HuntInventoryUI>();
            var serializedHuntUi = new SerializedObject(huntUi);
            serializedHuntUi.FindProperty("_progressText").objectReferenceValue = tmp;
            serializedHuntUi.FindProperty("_panelBackground").objectReferenceValue = panelImg;
            serializedHuntUi.FindProperty("_progressFormat").stringValue = "Targets: {0}/{1}";
            serializedHuntUi.FindProperty("_completedFormat").stringValue = "Targets: {0}/{1} (Return to Ship!)";
            serializedHuntUi.FindProperty("_normalTextColor").colorValue = Color.white;
            serializedHuntUi.FindProperty("_completedTextColor").colorValue = new Color(0.2f, 1f, 0.4f);
            serializedHuntUi.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject EnsureTimeDeltaPopupPrefab(TMP_FontAsset fontAsset)
        {
            const string prefabFolder = "Assets/_Project/Prefabs/UI";
            const string prefabPath = "Assets/_Project/Prefabs/UI/TimeDeltaPopup.prefab";

            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null) return existingPrefab;

            if (!AssetDatabase.IsValidFolder(prefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
            }

            var popupGo = new GameObject("TimeDeltaPopup");
            var rect = popupGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 40f);

            var canvasGroup = popupGo.AddComponent<CanvasGroup>();

            var textGo = new GameObject("DeltaText");
            textGo.transform.SetParent(popupGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) tmp.font = fontAsset;
            tmp.text = "-10s";
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.25f, 0.25f);

            var popupComp = popupGo.AddComponent<TimeDeltaPopup>();
            var serializedPopup = new SerializedObject(popupComp);
            serializedPopup.FindProperty("_deltaText").objectReferenceValue = tmp;
            serializedPopup.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
            serializedPopup.FindProperty("_penaltyColor").colorValue = new Color(1f, 0.25f, 0.25f);
            serializedPopup.FindProperty("_floatDistance").floatValue = 50f;
            serializedPopup.FindProperty("_duration").floatValue = 1.2f;
            serializedPopup.ApplyModifiedPropertiesWithoutUndo();

            var saved = PrefabUtility.SaveAsPrefabAsset(popupGo, prefabPath);
            Object.DestroyImmediate(popupGo);
            AssetDatabase.SaveAssets();

            return saved;
        }
    }
}
#endif
