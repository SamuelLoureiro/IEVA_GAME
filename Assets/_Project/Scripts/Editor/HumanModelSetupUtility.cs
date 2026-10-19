#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VARG.Targets;

namespace VARG.Editor
{
    [InitializeOnLoad]
    public static class HumanModelSetupUtility
    {
        private const string TargetHumanPrefabPath = "Assets/_Project/Prefabs/Targets/Target_Human.prefab";
        private const string MiniHumanPrefabPath = "Assets/_Project/Prefabs/Targets/Mini_Human.prefab";

        private const string SkinMatPath = "Assets/_Project/Materials/Mat_HumanSkin.mat";
        private const string HairMatPath = "Assets/_Project/Materials/Mat_HumanHair.mat";
        private const string ShirtMatPath = "Assets/_Project/Materials/Mat_HumanShirt.mat";
        private const string PantsMatPath = "Assets/_Project/Materials/Mat_HumanPants.mat";
        private const string ShoesMatPath = "Assets/_Project/Materials/Mat_HumanShoes.mat";
        private const string EyesMatPath = "Assets/Free Cube Pig Cute Pro Series/Materials/Eyes W 01.mat";

        [InitializeOnLoadMethod]
        private static void AutoInit()
        {
            EditorApplication.update += RunOnceOnUpdate;
        }

        private static void RunOnceOnUpdate()
        {
            EditorApplication.update -= RunOnceOnUpdate;
            SetupHumanModel(silent: true);
        }

        [MenuItem("VARG/Art/Setup Human Model")]
        [MenuItem("Tools/VARG/Setup Human Model")]
        [MenuItem("Window/VARG/Setup Human Model")]
        public static void SetupHumanModelMenu()
        {
            SetupHumanModel(silent: false);
        }

        public static void SetupHumanModel(bool silent = false)
        {
            var skinMat = GetOrCreateMaterial(SkinMatPath, new Color(1.0f, 0.835f, 0.647f));
            var hairMat = GetOrCreateMaterial(HairMatPath, new Color(0.24f, 0.15f, 0.10f));
            var shirtMat = GetOrCreateMaterial(ShirtMatPath, new Color(0.08f, 0.53f, 0.82f));
            var pantsMat = GetOrCreateMaterial(PantsMatPath, new Color(0.10f, 0.14f, 0.49f));
            var shoesMat = GetOrCreateMaterial(ShoesMatPath, new Color(0.15f, 0.18f, 0.20f));
            var eyesMat = AssetDatabase.LoadAssetAtPath<Material>(EyesMatPath);

            SetupTargetHuman(skinMat, hairMat, shirtMat, pantsMat, shoesMat, eyesMat);
            SetupMiniHuman(skinMat, hairMat, shirtMat, pantsMat, shoesMat, eyesMat);

            AssetDatabase.SaveAssets();
            Debug.Log("[HumanModelSetup] Cute Cube Human configured on Target_Human and Mini_Human!");
            if (!silent)
            {
                EditorUtility.DisplayDialog("VARG", "Cute Cube Human configured on Target_Human and Mini_Human!", "OK");
            }
        }

        private static void SetupTargetHuman(Material skin, Material hair, Material shirt, Material pants, Material shoes, Material eyes)
        {
            var root = PrefabUtility.LoadPrefabContents(TargetHumanPrefabPath);

            // Disable root primitive cylinder renderer
            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;

            var oldModel = root.transform.Find("Model_Human");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            var modelObj = BuildHumanGameObject(skin, hair, shirt, pants, shoes, eyes, 1.0f);
            modelObj.transform.SetParent(root.transform, false);
            modelObj.transform.localPosition = new Vector3(0f, -0.7f, 0f);
            modelObj.transform.localRotation = Quaternion.identity;
            modelObj.transform.localScale = Vector3.one * 1.0f;

            var collider = root.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                collider.center = new Vector3(0f, 0.15f, 0f);
                collider.height = 1.7f;
                collider.radius = 0.45f;
            }

            PrefabUtility.SaveAsPrefabAsset(root, TargetHumanPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void SetupMiniHuman(Material skin, Material hair, Material shirt, Material pants, Material shoes, Material eyes)
        {
            var root = PrefabUtility.LoadPrefabContents(MiniHumanPrefabPath);

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;

            var oldModel = root.transform.Find("Model_Human");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            var modelObj = BuildHumanGameObject(skin, hair, shirt, pants, shoes, eyes, 0.45f);
            modelObj.transform.SetParent(root.transform, false);
            modelObj.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            modelObj.transform.localRotation = Quaternion.identity;
            modelObj.transform.localScale = Vector3.one * 0.45f;

            var collider = root.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                collider.center = new Vector3(0f, 0.12f, 0f);
                collider.height = 0.75f;
                collider.radius = 0.22f;
            }

            PrefabUtility.SaveAsPrefabAsset(root, MiniHumanPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        public static GameObject BuildHumanGameObject(Material skin, Material hair, Material shirt, Material pants, Material shoes, Material eyes, float baseScale)
        {
            var root = new GameObject("Model_Human");

            // 1. Torso Root (Pivot at hip level Y = 0.68)
            var torso = new GameObject("Torso");
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0f, 0.68f, 0f);

            var shirtBody = CreateCube("ShirtBody", torso.transform,
                new Vector3(0f, 0.26f, 0f),
                new Vector3(0.48f, 0.52f, 0.30f),
                shirt);

            // 2. Head Pivot (Pivot at neck Y = 0.52 on Torso)
            var headPivot = new GameObject("HeadPivot");
            headPivot.transform.SetParent(torso.transform, false);
            headPivot.transform.localPosition = new Vector3(0f, 0.52f, 0f);

            var headMesh = CreateCube("HeadMesh", headPivot.transform,
                new Vector3(0f, 0.26f, 0f),
                new Vector3(0.52f, 0.52f, 0.52f),
                skin);

            // Hair elements
            CreateCube("HairTop", headPivot.transform,
                new Vector3(0f, 0.51f, -0.02f),
                new Vector3(0.56f, 0.16f, 0.56f),
                hair);

            CreateCube("HairBack", headPivot.transform,
                new Vector3(0f, 0.30f, -0.24f),
                new Vector3(0.56f, 0.38f, 0.12f),
                hair);

            CreateCube("HairSideL", headPivot.transform,
                new Vector3(-0.24f, 0.32f, -0.02f),
                new Vector3(0.08f, 0.32f, 0.52f),
                hair);

            CreateCube("HairSideR", headPivot.transform,
                new Vector3(0.24f, 0.32f, -0.02f),
                new Vector3(0.08f, 0.32f, 0.52f),
                hair);

            CreateCube("HairFringe", headPivot.transform,
                new Vector3(0f, 0.46f, 0.24f),
                new Vector3(0.48f, 0.12f, 0.10f),
                hair);

            // Face details
            CreateCube("Nose", headPivot.transform,
                new Vector3(0f, 0.20f, 0.28f),
                new Vector3(0.08f, 0.06f, 0.06f),
                skin);

            var eyesObj = CreateCube("Eyes", headPivot.transform,
                new Vector3(0f, 0.28f, 0.265f),
                new Vector3(0.38f, 0.18f, 0.01f),
                eyes);

            // 3. Arms (Pivots at shoulder level Y = 0.46 on Torso)
            var armPivotL = new GameObject("ArmPivotL");
            armPivotL.transform.SetParent(torso.transform, false);
            armPivotL.transform.localPosition = new Vector3(-0.30f, 0.46f, 0f);

            CreateCube("SleeveL", armPivotL.transform,
                new Vector3(0f, -0.12f, 0f),
                new Vector3(0.14f, 0.24f, 0.16f),
                shirt);

            CreateCube("HandL", armPivotL.transform,
                new Vector3(0f, -0.32f, 0f),
                new Vector3(0.12f, 0.20f, 0.14f),
                skin);

            var armPivotR = new GameObject("ArmPivotR");
            armPivotR.transform.SetParent(torso.transform, false);
            armPivotR.transform.localPosition = new Vector3(0.30f, 0.46f, 0f);

            CreateCube("SleeveR", armPivotR.transform,
                new Vector3(0f, -0.12f, 0f),
                new Vector3(0.14f, 0.24f, 0.16f),
                shirt);

            CreateCube("HandR", armPivotR.transform,
                new Vector3(0f, -0.32f, 0f),
                new Vector3(0.12f, 0.20f, 0.14f),
                skin);

            // 4. Legs (Pivots at hip level Y = 0.68 on Root)
            var legPivotL = new GameObject("LegPivotL");
            legPivotL.transform.SetParent(root.transform, false);
            legPivotL.transform.localPosition = new Vector3(-0.12f, 0.68f, 0f);

            CreateCube("PantsL", legPivotL.transform,
                new Vector3(0f, -0.26f, 0f),
                new Vector3(0.16f, 0.52f, 0.18f),
                pants);

            CreateCube("ShoeL", legPivotL.transform,
                new Vector3(0f, -0.55f, 0.03f),
                new Vector3(0.17f, 0.14f, 0.24f),
                shoes);

            var legPivotR = new GameObject("LegPivotR");
            legPivotR.transform.SetParent(root.transform, false);
            legPivotR.transform.localPosition = new Vector3(0.12f, 0.68f, 0f);

            CreateCube("PantsR", legPivotR.transform,
                new Vector3(0f, -0.26f, 0f),
                new Vector3(0.16f, 0.52f, 0.18f),
                pants);

            CreateCube("ShoeR", legPivotR.transform,
                new Vector3(0f, -0.55f, 0.03f),
                new Vector3(0.17f, 0.14f, 0.24f),
                shoes);

            // 5. Animation Component
            root.AddComponent<HumanBipedAnimation>();

            return root;
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            var col = cube.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;

            if (mat != null)
            {
                cube.GetComponent<Renderer>().sharedMaterial = mat;
            }

            return cube;
        }

        private static Material GetOrCreateMaterial(string path, Color color)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader)
                {
                    color = color
                };
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }
    }
}
#endif
