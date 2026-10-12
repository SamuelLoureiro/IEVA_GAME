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

            var interactableLayer = LayerMask.NameToLayer("Interactable");
            var grabbableLayer = LayerMask.NameToLayer("Grabbable");

            var pyramidMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Project/Meshes/Pyramid.asset");

            CreateTargetPair("Chicken", PrimitiveType.Sphere, null, new Vector3(0.8f, 0.8f, 0.8f), new Vector3(0.35f, 0.35f, 0.35f), interactableLayer, grabbableLayer);
            CreateTargetPair("Cow", PrimitiveType.Cube, pyramidMesh, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(0.4f, 0.4f, 0.4f), interactableLayer, grabbableLayer);
            CreateTargetPair("Pig", PrimitiveType.Cube, null, new Vector3(1f, 1f, 1f), new Vector3(0.35f, 0.35f, 0.35f), interactableLayer, grabbableLayer);
            CreateTargetPair("Human", PrimitiveType.Cylinder, null, new Vector3(0.8f, 1.4f, 0.8f), new Vector3(0.25f, 0.45f, 0.25f), interactableLayer, grabbableLayer);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VARG] Created all 8 Target and Mini prefabs and successfully linked to TargetData assets!");
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
            agent.angularSpeed = 180f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;

            var targetComponent = targetGo.AddComponent<Target>();
            if (targetData != null)
            {
                targetComponent.Data = targetData;
            }

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
