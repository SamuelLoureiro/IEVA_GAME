#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VARG.Targets;

namespace VARG.Editor
{
    [InitializeOnLoad]
    public static class PigModelSetupUtility
    {
        private const string PigModelPrefabPath = "Assets/Free Cube Pig Cute Pro Series/Prefabs/Full/Pig No Root.prefab";
        private const string EyesMaterialPath = "Assets/Free Cube Pig Cute Pro Series/Materials/Eyes W 01.mat";
        private const string CowMaterialPath = "Assets/Free Cube Pig Cute Pro Series/Materials/Cow.mat";
        private const string HornMaterialPath = "Assets/Free Cube Pig Cute Pro Series/Materials/Horn.mat";
        private const string ChickenMaterialPath = "Assets/Free Cube Pig Cute Pro Series/Materials/Chicken.mat";
        private const string BeakMaterialPath = "Assets/Free Cube Pig Cute Pro Series/Materials/Beak_Orange.mat";
        private const string CombMaterialPath = "Assets/Free Cube Pig Cute Pro Series/Materials/Comb_Red.mat";

        private const string TargetPigPrefabPath = "Assets/_Project/Prefabs/Targets/Target_Pig.prefab";
        private const string MiniPigPrefabPath = "Assets/_Project/Prefabs/Targets/Mini_Pig.prefab";

        private const string TargetCowPrefabPath = "Assets/_Project/Prefabs/Targets/Target_Cow.prefab";
        private const string MiniCowPrefabPath = "Assets/_Project/Prefabs/Targets/Mini_Cow.prefab";

        private const string TargetChickenPrefabPath = "Assets/_Project/Prefabs/Targets/Target_Chicken.prefab";
        private const string MiniChickenPrefabPath = "Assets/_Project/Prefabs/Targets/Mini_Chicken.prefab";

        [InitializeOnLoadMethod]
        private static void AutoInit()
        {
            EditorApplication.update += RunOnceOnUpdate;
        }

        private static void RunOnceOnUpdate()
        {
            EditorApplication.update -= RunOnceOnUpdate;
            SetupAllAnimals(silent: true);
        }

        [MenuItem("VARG/Art/Setup All Targets (Pig + Cow + Chicken + Human)")]
        [MenuItem("Tools/VARG/Setup All Targets (Pig + Cow + Chicken + Human)")]
        [MenuItem("Window/VARG/Setup All Targets (Pig + Cow + Chicken + Human)")]
        public static void SetupAllAnimalsMenu()
        {
            SetupAllAnimals(silent: false);
        }

        public static void SetupAllAnimals(bool silent = false)
        {
            SetupPigModel(silent: true);
            SetupCowModel(silent: true);
            SetupChickenModel(silent: true);
            HumanModelSetupUtility.SetupHumanModel(silent: true);
            AssetDatabase.SaveAssets();

            Debug.Log("[AnimalModelSetup] Cute Cube Pig, Cow, Chicken, and Human configured successfully!");
            if (!silent)
            {
                EditorUtility.DisplayDialog("VARG", "All 4 targets (Pig, Cow, Chicken, and Human) configured successfully!", "OK");
            }
        }

        [MenuItem("VARG/Art/Measure Target Ground Alignment")]
        public static void MeasureAllTargets()
        {
            MeasureOne("Pig", TargetPigPrefabPath);
            MeasureOne("Cow", TargetCowPrefabPath);
            MeasureOne("Chicken", TargetChickenPrefabPath);
            MeasureOne("Human", "Assets/_Project/Prefabs/Targets/Target_Human.prefab");
        }

        private static void MeasureOne(string name, string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;
            var inst = Object.Instantiate(prefab);
            inst.transform.position = Vector3.zero;

            var renderers = inst.GetComponentsInChildren<Renderer>();
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var r in renderers)
            {
                if (!r.enabled) continue;
                min = Vector3.Min(min, r.bounds.min);
                max = Vector3.Max(max, r.bounds.max);
            }
            var agent = inst.GetComponent<UnityEngine.AI.NavMeshAgent>();
            var col = inst.GetComponent<Collider>();
            var elev = (agent != null ? agent.baseOffset : 0f) + min.y;
            Debug.Log($"[MEASURE] {name}: Visual min.y={min.y:F3}, max.y={max.y:F3}, agent.baseOffset={(agent != null ? agent.baseOffset : 0f):F3} => FEET ELEVATION FROM NAVMESH: {elev:F3}m (Collider min.y={(col != null ? col.bounds.min.y : 0f):F3})");
            Object.DestroyImmediate(inst);
        }

        [MenuItem("VARG/Art/Setup Chicken Model")]
        [MenuItem("Tools/VARG/Setup Chicken Model")]
        [MenuItem("Window/VARG/Setup Chicken Model")]
        public static void SetupChickenModelMenu()
        {
            SetupChickenModel(silent: false);
        }

        public static void SetupChickenModel(bool silent = false)
        {
            var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PigModelPrefabPath);
            if (modelPrefab == null)
            {
                Debug.LogError($"[AnimalModelSetup] Could not find '{PigModelPrefabPath}'!");
                return;
            }

            var chickenMat = AssetDatabase.LoadAssetAtPath<Material>(ChickenMaterialPath);
            var eyesMat = AssetDatabase.LoadAssetAtPath<Material>(EyesMaterialPath);
            var beakMat = AssetDatabase.LoadAssetAtPath<Material>(BeakMaterialPath);
            var combMat = AssetDatabase.LoadAssetAtPath<Material>(CombMaterialPath);

            SetupTargetChicken(modelPrefab, chickenMat, eyesMat, beakMat, combMat);
            SetupMiniChicken(modelPrefab, chickenMat, eyesMat, beakMat, combMat);

            AssetDatabase.SaveAssets();
            Debug.Log("[AnimalModelSetup] Cute Cube Chicken configured on Target_Chicken and Mini_Chicken!");
            if (!silent)
            {
                EditorUtility.DisplayDialog("VARG", "Cute Cube Chicken configured on Target_Chicken and Mini_Chicken!", "OK");
            }
        }

        [MenuItem("VARG/Art/Setup Cow Model")]
        [MenuItem("Tools/VARG/Setup Cow Model")]
        [MenuItem("Window/VARG/Setup Cow Model")]
        public static void SetupCowModelMenu()
        {
            SetupCowModel(silent: false);
        }

        public static void SetupCowModel(bool silent = false)
        {
            var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PigModelPrefabPath);
            if (modelPrefab == null)
            {
                Debug.LogError($"[AnimalModelSetup] Could not find '{PigModelPrefabPath}'!");
                return;
            }

            var cowMat = AssetDatabase.LoadAssetAtPath<Material>(CowMaterialPath);
            var eyesMat = AssetDatabase.LoadAssetAtPath<Material>(EyesMaterialPath);
            var hornMat = AssetDatabase.LoadAssetAtPath<Material>(HornMaterialPath);

            SetupTargetCow(modelPrefab, cowMat, eyesMat, hornMat);
            SetupMiniCow(modelPrefab, cowMat, eyesMat, hornMat);

            AssetDatabase.SaveAssets();
            Debug.Log("[AnimalModelSetup] Cute Cube Cow configured on Target_Cow and Mini_Cow!");
            if (!silent)
            {
                EditorUtility.DisplayDialog("VARG", "Cute Cube Cow configured on Target_Cow and Mini_Cow!", "OK");
            }
        }

        [MenuItem("VARG/Art/Setup Pig Model")]
        [MenuItem("Tools/VARG/Setup Pig Model")]
        [MenuItem("Window/VARG/Setup Pig Model")]
        public static void SetupPigModelMenu()
        {
            SetupPigModel(silent: false);
        }

        public static void SetupPigModel(bool silent = false)
        {
            var pigModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PigModelPrefabPath);
            if (pigModelPrefab == null)
            {
                Debug.LogError($"[AnimalModelSetup] Could not find '{PigModelPrefabPath}'!");
                return;
            }

            SetupTargetPig(pigModelPrefab);
            SetupMiniPig(pigModelPrefab);

            AssetDatabase.SaveAssets();
            Debug.Log("[AnimalModelSetup] Cute Cube Pig configured on Target_Pig and Mini_Pig!");
            if (!silent)
            {
                EditorUtility.DisplayDialog("VARG", "Cute Cube Pig setup successfully on Target_Pig and Mini_Pig!", "OK");
            }
        }

        [MenuItem("VARG/Art/Revert All Animals (Back to Primitives)")]
        [MenuItem("Tools/VARG/Revert All Animals")]
        [MenuItem("Window/VARG/Revert All Animals")]
        public static void RevertAllAnimals()
        {
            RevertPrefab(TargetPigPrefabPath, "Model_Pig");
            RevertPrefab(MiniPigPrefabPath, "Model_Pig");
            RevertPrefab(TargetCowPrefabPath, "Model_Cow");
            RevertPrefab(MiniCowPrefabPath, "Model_Cow");
            RevertPrefab(TargetChickenPrefabPath, "Model_Chicken");
            RevertPrefab(MiniChickenPrefabPath, "Model_Chicken");

            AssetDatabase.SaveAssets();
            Debug.Log("[AnimalModelSetup] Reverted all animal prefabs back to default primitives.");
            EditorUtility.DisplayDialog("VARG", "Reverted all animals back to default primitives.", "OK");
        }

        private static void SetupTargetPig(GameObject modelPrefab)
        {
            var root = PrefabUtility.LoadPrefabContents(TargetPigPrefabPath);
            root.transform.localScale = Vector3.one;

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;

            var oldModel = root.transform.Find("Model_Pig");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            modelInstance.name = "Model_Pig";
            modelInstance.transform.localPosition = new Vector3(0f, -0.30f, 0f);
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one * 0.9f;

            if (modelInstance.GetComponent<PigAnimationController>() == null)
            {
                modelInstance.AddComponent<PigAnimationController>();
            }

            var collider = root.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.center = new Vector3(0f, 0.17f, 0f);
                collider.size = new Vector3(0.9f, 0.95f, 1.1f);
            }

            var agent = root.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.radius = 0.45f;
                agent.height = 0.9f;
                agent.baseOffset = 0.306f;
            }

            EditorUtility.SetDirty(root.transform);
            EditorUtility.SetDirty(root);
            PrefabUtility.SaveAsPrefabAsset(root, TargetPigPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void SetupMiniPig(GameObject modelPrefab)
        {
            var root = PrefabUtility.LoadPrefabContents(MiniPigPrefabPath);
            root.transform.localScale = Vector3.one * 0.35f;

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;

            var oldModel = root.transform.Find("Model_Pig");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            modelInstance.name = "Model_Pig";
            modelInstance.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one * 0.55f;

            if (modelInstance.GetComponent<PigAnimationController>() == null)
            {
                modelInstance.AddComponent<PigAnimationController>();
            }

            var collider = root.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.center = new Vector3(0f, 0.1f, 0f);
                collider.size = new Vector3(0.5f, 0.5f, 0.65f);
            }

            EditorUtility.SetDirty(root.transform);
            EditorUtility.SetDirty(root);
            PrefabUtility.SaveAsPrefabAsset(root, MiniPigPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void SetupTargetCow(GameObject modelPrefab, Material cowMat, Material eyesMat, Material hornMat)
        {
            var root = PrefabUtility.LoadPrefabContents(TargetCowPrefabPath);
            root.transform.localScale = Vector3.one;

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;

            var oldModel = root.transform.Find("Model_Cow");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            modelInstance.name = "Model_Cow";
            modelInstance.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one * 1.8f;

            ApplyMaterials(modelInstance, cowMat, eyesMat);

            var mainTransform = FindMainTransform(modelInstance.transform);
            AddHorns(mainTransform, hornMat);

            if (modelInstance.GetComponent<CowFeaturesSetup>() == null)
            {
                modelInstance.AddComponent<CowFeaturesSetup>();
            }

            if (modelInstance.GetComponent<PigAnimationController>() == null)
            {
                modelInstance.AddComponent<PigAnimationController>();
            }

            var collider = root.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.center = new Vector3(0f, 0.29f, 0f);
                collider.size = new Vector3(1.5f, 1.80f, 2.2f);
            }

            var agent = root.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.radius = 0.75f;
                agent.height = 1.8f;
                agent.baseOffset = 0.612f;
            }

            EditorUtility.SetDirty(root.transform);
            EditorUtility.SetDirty(root);
            PrefabUtility.SaveAsPrefabAsset(root, TargetCowPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void SetupMiniCow(GameObject modelPrefab, Material cowMat, Material eyesMat, Material hornMat)
        {
            var root = PrefabUtility.LoadPrefabContents(MiniCowPrefabPath);
            root.transform.localScale = Vector3.one * 0.35f;

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;

            var oldModel = root.transform.Find("Model_Cow");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            modelInstance.name = "Model_Cow";
            modelInstance.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one * 0.75f;

            ApplyMaterials(modelInstance, cowMat, eyesMat);

            var mainTransform = FindMainTransform(modelInstance.transform);
            AddHorns(mainTransform, hornMat);

            if (modelInstance.GetComponent<CowFeaturesSetup>() == null)
            {
                modelInstance.AddComponent<CowFeaturesSetup>();
            }

            if (modelInstance.GetComponent<PigAnimationController>() == null)
            {
                modelInstance.AddComponent<PigAnimationController>();
            }

            var collider = root.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.center = new Vector3(0f, 0.2f, 0f);
                collider.size = new Vector3(0.7f, 0.75f, 0.95f);
            }

            EditorUtility.SetDirty(root.transform);
            EditorUtility.SetDirty(root);
            PrefabUtility.SaveAsPrefabAsset(root, MiniCowPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void SetupTargetChicken(GameObject modelPrefab, Material chickenMat, Material eyesMat, Material beakMat, Material combMat)
        {
            var root = PrefabUtility.LoadPrefabContents(TargetChickenPrefabPath);
            root.transform.localScale = Vector3.one;

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;

            var oldModel = root.transform.Find("Model_Chicken");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            modelInstance.name = "Model_Chicken";
            modelInstance.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one * 0.45f;
            ApplyMaterials(modelInstance, chickenMat, eyesMat);

            var mainTransform = FindMainTransform(modelInstance.transform);
            AddChickenFeatures(mainTransform, modelInstance.transform, beakMat, combMat);
            RetractUnusedChickenParts(modelInstance.transform);

            var biped = modelInstance.GetComponent<ChickenBipedSetup>();
            if (biped == null)
            {
                biped = modelInstance.AddComponent<ChickenBipedSetup>();
            }
            if (beakMat != null)
            {
                biped.SetFootMaterial(beakMat);
            }

            if (modelInstance.GetComponent<PigAnimationController>() == null)
            {
                modelInstance.AddComponent<PigAnimationController>();
            }

            var sphere = root.GetComponent<SphereCollider>();
            if (sphere != null)
            {
                sphere.center = new Vector3(0f, 0.10f, 0f);
                sphere.radius = 0.25f;
            }

            var agent = root.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.radius = 0.25f;
                agent.height = 0.5f;
                agent.baseOffset = 0.154f;
            }

            EditorUtility.SetDirty(root.transform);
            EditorUtility.SetDirty(root);
            PrefabUtility.SaveAsPrefabAsset(root, TargetChickenPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void SetupMiniChicken(GameObject modelPrefab, Material chickenMat, Material eyesMat, Material beakMat, Material combMat)
        {
            var root = PrefabUtility.LoadPrefabContents(MiniChickenPrefabPath);
            root.transform.localScale = Vector3.one * 0.35f;

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;

            var oldModel = root.transform.Find("Model_Chicken");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            modelInstance.name = "Model_Chicken";
            modelInstance.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one * 0.30f;

            ApplyMaterials(modelInstance, chickenMat, eyesMat);

            var mainTransform = FindMainTransform(modelInstance.transform);
            AddChickenFeatures(mainTransform, modelInstance.transform, beakMat, combMat);
            RetractUnusedChickenParts(modelInstance.transform);

            var miniBiped = modelInstance.GetComponent<ChickenBipedSetup>();
            if (miniBiped == null)
            {
                miniBiped = modelInstance.AddComponent<ChickenBipedSetup>();
            }
            if (beakMat != null)
            {
                miniBiped.SetFootMaterial(beakMat);
            }

            if (modelInstance.GetComponent<PigAnimationController>() == null)
            {
                modelInstance.AddComponent<PigAnimationController>();
            }

            var sphere = root.GetComponent<SphereCollider>();
            if (sphere != null)
            {
                sphere.center = new Vector3(0f, 0.08f, 0f);
                sphere.radius = 0.16f;
            }

            EditorUtility.SetDirty(root.transform);
            EditorUtility.SetDirty(root);
            PrefabUtility.SaveAsPrefabAsset(root, MiniChickenPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ApplyMaterials(GameObject modelInstance, Material bodyMat, Material eyesMat)
        {
            var renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r == null) continue;

                if (r.name.Contains("Face"))
                {
                    if (eyesMat != null)
                    {
                        r.sharedMaterial = eyesMat;
                    }
                }
                else
                {
                    if (bodyMat != null)
                    {
                        r.sharedMaterial = bodyMat;
                    }
                }
            }
        }

        private static Transform FindMainTransform(Transform modelRoot)
        {
            foreach (var t in modelRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "+ Main") return t;
            }
            foreach (var t in modelRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "RigHub") return t;
            }
            return modelRoot;
        }

        private static void AddHorns(Transform mainTransform, Material hornMat)
        {
            var existingHornL = mainTransform.Find("Horn_L");
            if (existingHornL != null) Object.DestroyImmediate(existingHornL.gameObject);
            var existingHornR = mainTransform.Find("Horn_R");
            if (existingHornR != null) Object.DestroyImmediate(existingHornR.gameObject);

            // Left horn positioned directly on the skull where the left ear was
            var hornL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hornL.name = "Horn_L";
            Object.DestroyImmediate(hornL.GetComponent<Collider>());
            hornL.transform.SetParent(mainTransform, false);
            hornL.transform.localPosition = new Vector3(-0.24f, 1.02f, 0.16f);
            hornL.transform.localRotation = Quaternion.Euler(15f, 0f, -25f);
            hornL.transform.localScale = new Vector3(0.10f, 0.28f, 0.10f);
            if (hornMat != null) hornL.GetComponent<Renderer>().sharedMaterial = hornMat;

            // Right horn positioned directly on the skull where the right ear was
            var hornR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hornR.name = "Horn_R";
            Object.DestroyImmediate(hornR.GetComponent<Collider>());
            hornR.transform.SetParent(mainTransform, false);
            hornR.transform.localPosition = new Vector3(0.24f, 1.02f, 0.16f);
            hornR.transform.localRotation = Quaternion.Euler(15f, 0f, 25f);
            hornR.transform.localScale = new Vector3(0.10f, 0.28f, 0.10f);
            if (hornMat != null) hornR.GetComponent<Renderer>().sharedMaterial = hornMat;
        }

        private static void AddChickenFeatures(Transform mainTransform, Transform modelRoot, Material beakMat, Material combMat)
        {
            var existingBeak = mainTransform.Find("Beak_Orange");
            if (existingBeak != null) Object.DestroyImmediate(existingBeak.gameObject);
            var existingWattle = mainTransform.Find("Wattle_Red");
            if (existingWattle != null) Object.DestroyImmediate(existingWattle.gameObject);
            var existingComb = mainTransform.Find("Comb_Red");
            if (existingComb != null) Object.DestroyImmediate(existingComb.gameObject);

            // Orange 3D Beak protruding from the face at mouth/snout level
            var beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beak.name = "Beak_Orange";
            Object.DestroyImmediate(beak.GetComponent<Collider>());
            beak.transform.SetParent(mainTransform, false);
            beak.transform.localPosition = new Vector3(0f, 0.60f, 0.52f);
            beak.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            beak.transform.localScale = new Vector3(0.24f, 0.16f, 0.28f);
            if (beakMat != null) beak.GetComponent<Renderer>().sharedMaterial = beakMat;

            // Red Wattle hanging underneath the beak
            var wattle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wattle.name = "Wattle_Red";
            Object.DestroyImmediate(wattle.GetComponent<Collider>());
            wattle.transform.SetParent(mainTransform, false);
            wattle.transform.localPosition = new Vector3(0f, 0.46f, 0.48f);
            wattle.transform.localScale = new Vector3(0.09f, 0.16f, 0.12f);
            if (combMat != null) wattle.GetComponent<Renderer>().sharedMaterial = combMat;

            // Red Comb with 3 lobes resting directly on top of the head surface (Y = 1.05f)
            var combContainer = new GameObject("Comb_Red");
            combContainer.transform.SetParent(mainTransform, false);
            combContainer.transform.localPosition = new Vector3(0f, 1.05f, 0.15f);

            var combCenter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            combCenter.name = "Lobe_Center";
            Object.DestroyImmediate(combCenter.GetComponent<Collider>());
            combCenter.transform.SetParent(combContainer.transform, false);
            combCenter.transform.localPosition = new Vector3(0f, 0.14f, 0.00f);
            combCenter.transform.localScale = new Vector3(0.12f, 0.28f, 0.24f);
            if (combMat != null) combCenter.GetComponent<Renderer>().sharedMaterial = combMat;

            var combFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            combFront.name = "Lobe_Front";
            Object.DestroyImmediate(combFront.GetComponent<Collider>());
            combFront.transform.SetParent(combContainer.transform, false);
            combFront.transform.localPosition = new Vector3(0f, 0.10f, 0.16f);
            combFront.transform.localScale = new Vector3(0.12f, 0.20f, 0.18f);
            if (combMat != null) combFront.GetComponent<Renderer>().sharedMaterial = combMat;

            var combBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            combBack.name = "Lobe_Back";
            Object.DestroyImmediate(combBack.GetComponent<Collider>());
            combBack.transform.SetParent(combContainer.transform, false);
            combBack.transform.localPosition = new Vector3(0f, 0.08f, -0.15f);
            combBack.transform.localScale = new Vector3(0.12f, 0.16f, 0.18f);
            if (combMat != null) combBack.GetComponent<Renderer>().sharedMaterial = combMat;

            // Orange chicken feet attached to the two active walking legs
            foreach (var t in modelRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "RigR1Leg1" || t.name == "RigL2Leg1")
                {
                    AddChickenFoot(t, beakMat);
                }
            }
        }

        private static void AddChickenFoot(Transform legBone, Material footMat)
        {
            var existingFoot = legBone.Find("Foot_Orange");
            if (existingFoot != null) Object.DestroyImmediate(existingFoot.gameObject);

            var foot = new GameObject("Foot_Orange");
            foot.transform.SetParent(legBone, false);
            foot.transform.localPosition = new Vector3(-0.25f, 0.04f, 0f);
            foot.transform.localRotation = Quaternion.identity;

            var footBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            footBase.name = "FootBase";
            Object.DestroyImmediate(footBase.GetComponent<Collider>());
            footBase.transform.SetParent(foot.transform, false);
            footBase.transform.localPosition = Vector3.zero;
            footBase.transform.localScale = new Vector3(0.06f, 0.16f, 0.14f);
            if (footMat != null) footBase.GetComponent<Renderer>().sharedMaterial = footMat;

            var toeCenter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toeCenter.name = "ToeCenter";
            Object.DestroyImmediate(toeCenter.GetComponent<Collider>());
            toeCenter.transform.SetParent(foot.transform, false);
            toeCenter.transform.localPosition = new Vector3(-0.01f, 0.12f, 0f);
            toeCenter.transform.localScale = new Vector3(0.05f, 0.12f, 0.05f);
            if (footMat != null) toeCenter.GetComponent<Renderer>().sharedMaterial = footMat;

            var toeLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toeLeft.name = "ToeLeft";
            Object.DestroyImmediate(toeLeft.GetComponent<Collider>());
            toeLeft.transform.SetParent(foot.transform, false);
            toeLeft.transform.localPosition = new Vector3(-0.01f, 0.10f, -0.06f);
            toeLeft.transform.localRotation = Quaternion.Euler(0f, -22f, 0f);
            toeLeft.transform.localScale = new Vector3(0.05f, 0.10f, 0.04f);
            if (footMat != null) toeLeft.GetComponent<Renderer>().sharedMaterial = footMat;

            var toeRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toeRight.name = "ToeRight";
            Object.DestroyImmediate(toeRight.GetComponent<Collider>());
            toeRight.transform.SetParent(foot.transform, false);
            toeRight.transform.localPosition = new Vector3(-0.01f, 0.10f, 0.06f);
            toeRight.transform.localRotation = Quaternion.Euler(0f, 22f, 0f);
            toeRight.transform.localScale = new Vector3(0.05f, 0.10f, 0.04f);
            if (footMat != null) toeRight.GetComponent<Renderer>().sharedMaterial = footMat;
        }

        private static void RetractUnusedChickenParts(Transform modelInstance)
        {
            foreach (var t in modelInstance.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "RigEarL")
                {
                    t.localScale = Vector3.zero;
                    t.localPosition = new Vector3(0.20f, 0.15f, 0.00f);
                }
                else if (t.name == "RigEarR")
                {
                    t.localScale = Vector3.zero;
                    t.localPosition = new Vector3(0.20f, 0.15f, 0.00f);
                }
                else if (t.name == "RigNose")
                {
                    t.localScale = Vector3.zero;
                    t.localPosition = new Vector3(0.03f, 0.10f, 0.00f);
                }
                else if (t.name == "RigTail1" || t.name == "RigL1Leg1" || t.name == "RigR2Leg1")
                {
                    t.localScale = Vector3.zero;
                }
            }
        }

        private static void RevertPrefab(string prefabPath, string modelName)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = true;

            var oldModel = root.transform.Find(modelName);
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
