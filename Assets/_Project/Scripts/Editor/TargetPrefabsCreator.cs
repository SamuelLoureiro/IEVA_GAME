#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using VARG.Data;
using VARG.Sorting;
using VARG.Targets;

namespace VARG.Editor
{
    public static class TargetPrefabsCreator
    {
        [MenuItem("VARG/Create Target and Mini Prefabs")]
        public static void CreateAllPrefabs()
        {
            EnsureFolder("Assets/_Project/Prefabs/Targets");
            TuneTargetSpeeds();

            var interactableLayer = LayerMask.NameToLayer("Interactable");
            var grabbableLayer = LayerMask.NameToLayer("Grabbable");

            var pyramidMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Project/Meshes/Pyramid.asset");

            CreateTargetPair("Chicken", PrimitiveType.Sphere, null, new Vector3(0.8f, 0.8f, 0.8f), new Vector3(0.35f, 0.35f, 0.35f), interactableLayer, grabbableLayer);
            CreateTargetPair("Cow", PrimitiveType.Cube, pyramidMesh, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(0.4f, 0.4f, 0.4f), interactableLayer, grabbableLayer);
            CreateTargetPair("Pig", PrimitiveType.Cube, null, new Vector3(1f, 1f, 1f), new Vector3(0.35f, 0.35f, 0.35f), interactableLayer, grabbableLayer);
            CreateTargetPair("Human", PrimitiveType.Cylinder, null, new Vector3(0.8f, 1.4f, 0.8f), new Vector3(0.25f, 0.45f, 0.25f), interactableLayer, grabbableLayer);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VARG] Created all 8 Target and Mini prefabs and tuned target speeds!");
        }

        private static void TuneTargetSpeeds()
        {
            SetTargetDataSpeed("Target_Chicken", 3.5f, 8.0f);
            SetTargetDataSpeed("Target_Cow", 2.5f, 6.2f);
            SetTargetDataSpeed("Target_Pig", 3.0f, 7.0f);
            SetTargetDataSpeed("Target_Human", 4.0f, 8.5f);
        }

        private static void SetTargetDataSpeed(string assetName, float walkSpeed, float fleeSpeed)
        {
            var path = $"Assets/_Project/Data/Targets/{assetName}.asset";
            var data = AssetDatabase.LoadAssetAtPath<TargetData>(path);
            if (data == null) return;

            var serialized = new SerializedObject(data);
            serialized.FindProperty("_walkSpeed").floatValue = walkSpeed;
            serialized.FindProperty("_fleeSpeed").floatValue = fleeSpeed;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateTargetPair(
            string typeName,
            PrimitiveType primitiveType,
            Mesh customMesh,
            Vector3 targetScale,
            Vector3 miniScale,
            int interactableLayer,
            int grabbableLayer)
        {
            var targetDataPath = $"Assets/_Project/Data/Targets/Target_{typeName}.asset";
            var targetData = AssetDatabase.LoadAssetAtPath<TargetData>(targetDataPath);

            // 1. Target Prefab (Hunt Phase)
            var targetGo = GameObject.CreatePrimitive(primitiveType);
            targetGo.name = $"Target_{typeName}";
            targetGo.transform.localScale = targetScale;

            if (customMesh != null)
            {
                var mf = targetGo.GetComponent<MeshFilter>();
                if (mf != null) mf.sharedMesh = customMesh;
            }

            if (interactableLayer != -1)
            {
                targetGo.layer = interactableLayer;
            }

            var agent = targetGo.AddComponent<NavMeshAgent>();
            agent.speed = targetData != null ? targetData.WalkSpeed : 3f;
            agent.angularSpeed = 720f;
            agent.acceleration = 28f;
            agent.autoBraking = false;
            agent.stoppingDistance = 0.5f;

            var audioSource = targetGo.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 25f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            var shrinkable = targetGo.AddComponent<Shrinkable>();
            var shrinkSerialized = new SerializedObject(shrinkable);
            shrinkSerialized.FindProperty("_audioSource").objectReferenceValue = audioSource;
            shrinkSerialized.ApplyModifiedPropertiesWithoutUndo();

            var targetComponent = targetGo.AddComponent<Target>();
            if (targetData != null)
            {
                targetComponent.Data = targetData;
            }

            var targetAi = targetGo.AddComponent<TargetAI>();
            var aiSerialized = new SerializedObject(targetAi);
            aiSerialized.FindProperty("_agent").objectReferenceValue = agent;
            aiSerialized.FindProperty("_target").objectReferenceValue = targetComponent;
            aiSerialized.FindProperty("_audioSource").objectReferenceValue = audioSource;
            aiSerialized.ApplyModifiedPropertiesWithoutUndo();

            var targetPrefabPath = $"Assets/_Project/Prefabs/Targets/Target_{typeName}.prefab";
            var targetPrefab = PrefabUtility.SaveAsPrefabAsset(targetGo, targetPrefabPath);
            Object.DestroyImmediate(targetGo);

            // 2. Mini Prefab (Sorting Phase)
            var miniGo = GameObject.CreatePrimitive(primitiveType);
            miniGo.name = $"Mini_{typeName}";
            miniGo.transform.localScale = miniScale;

            if (customMesh != null)
            {
                var mf = miniGo.GetComponent<MeshFilter>();
                if (mf != null) mf.sharedMesh = customMesh;
            }

            if (grabbableLayer != -1)
            {
                miniGo.layer = grabbableLayer;
            }

            var rb = miniGo.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.linearDamping = 1f;
            rb.angularDamping = 1f;

            var sortable = miniGo.AddComponent<SortableItem>();
            if (targetData != null)
            {
                sortable.Data = targetData;
            }

            var miniPrefabPath = $"Assets/_Project/Prefabs/Targets/Mini_{typeName}.prefab";
            var miniPrefab = PrefabUtility.SaveAsPrefabAsset(miniGo, miniPrefabPath);
            Object.DestroyImmediate(miniGo);

            // 3. Link Prefabs to TargetData
            if (targetData != null)
            {
                var serializedData = new SerializedObject(targetData);
                serializedData.FindProperty("_prefab").objectReferenceValue = targetPrefab;
                serializedData.FindProperty("_miniPrefab").objectReferenceValue = miniPrefab;
                serializedData.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
                var folderName = System.IO.Path.GetFileName(path);
                if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
                {
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }
    }
}
#endif
