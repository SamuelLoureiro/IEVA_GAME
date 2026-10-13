#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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

            var existingSampleTargets = GameObject.Find("_SampleTargets");
            if (existingSampleTargets != null)
            {
                Object.DestroyImmediate(existingSampleTargets);
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

            // Abduction Ground Pad (flat disc on the floor, NO solid vertical pillar!)
            var beamPad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beamPad.name = "AbductionBeamZone";
            beamPad.transform.SetParent(envRoot.transform);
            beamPad.transform.position = new Vector3(0f, 0.02f, 0f);
            beamPad.transform.localScale = new Vector3(8f, 0.04f, 8f);

            var beamCol = beamPad.GetComponent<CapsuleCollider>() ?? (Collider)beamPad.GetComponent<BoxCollider>();
            if (beamCol != null)
            {
                beamCol.isTrigger = true;
            }

            // 5. Instantiate ETPlayer
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player/ETPlayer.prefab");
            if (playerPrefab != null)
            {
                var playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerInstance.transform.position = new Vector3(0f, 0.1f, -7f);
                playerInstance.transform.rotation = Quaternion.identity;
            }

            // 6. Instantiate Sample Targets for early visual testing
            var targetsRoot = new GameObject("_SampleTargets");
            SpawnSampleTarget(targetsRoot, "Assets/_Project/Prefabs/Targets/Target_Chicken.prefab", new Vector3(-3f, 0.1f, 5f));
            SpawnSampleTarget(targetsRoot, "Assets/_Project/Prefabs/Targets/Target_Cow.prefab", new Vector3(4f, 0.1f, 6f));
            SpawnSampleTarget(targetsRoot, "Assets/_Project/Prefabs/Targets/Target_Pig.prefab", new Vector3(-6f, 0.1f, -4f));
            SpawnSampleTarget(targetsRoot, "Assets/_Project/Prefabs/Targets/Target_Human.prefab", new Vector3(7f, 0.1f, -5f));

            // 7. Ensure Lighting
            EnsureDirectionalLight();

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
    }
}
#endif
