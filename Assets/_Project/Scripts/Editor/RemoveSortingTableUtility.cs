using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VARG.Sorting;

namespace VARG.Editor
{
    public static class RemoveSortingTableUtility
    {
        private const string PrefabPath = "Assets/_Project/Prefabs/Cages/CaptureBox.prefab";

        [MenuItem("VARG/Remove Table (Floor Spawner)", false, 1)]
        [MenuItem("Tools/VARG/Remove Table (Floor Spawner)", false, 1)]
        public static void Execute()
        {
            // 1. Clean up CaptureBox.prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                var toRemove = new List<GameObject>();

                foreach (Transform child in instance.transform)
                {
                    var n = child.name;
                    if (n.StartsWith("Stand") || n.StartsWith("Tray") || n.StartsWith("Border"))
                    {
                        toRemove.Add(child.gameObject);
                    }
                }

                for (var i = 0; i < toRemove.Count; i++)
                {
                    Object.DestroyImmediate(toRemove[i]);
                }

                var anchor = instance.transform.Find("SpawnAnchor");
                if (anchor == null)
                {
                    var anchorGo = new GameObject("SpawnAnchor");
                    anchorGo.transform.SetParent(instance.transform);
                    anchor = anchorGo.transform;
                }
                anchor.localPosition = new Vector3(0, 0.35f, 0);

                var spawner = instance.GetComponent<CaptureBoxSpawner>() ?? instance.AddComponent<CaptureBoxSpawner>();
                var so = new SerializedObject(spawner);
                var prop = so.FindProperty("_spawnAnchor");
                if (prop != null)
                {
                    prop.objectReferenceValue = anchor;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
                Object.DestroyImmediate(instance);
                AssetDatabase.SaveAssets();
            }

            // 2. Also clean up any table parts in the currently open scene (if 04_ShipSorting is loaded)
            var sceneSpawners = Object.FindObjectsByType<CaptureBoxSpawner>(FindObjectsSortMode.None);
            for (var s = 0; s < sceneSpawners.Length; s++)
            {
                var spawnerTransform = sceneSpawners[s].transform;
                var sceneToRemove = new List<GameObject>();
                foreach (Transform child in spawnerTransform)
                {
                    var n = child.name;
                    if (n.StartsWith("Stand") || n.StartsWith("Tray") || n.StartsWith("Border"))
                    {
                        sceneToRemove.Add(child.gameObject);
                    }
                }
                for (var i = 0; i < sceneToRemove.Count; i++)
                {
                    Undo.DestroyObjectImmediate(sceneToRemove[i]);
                }

                var sceneAnchor = spawnerTransform.Find("SpawnAnchor");
                if (sceneAnchor == null)
                {
                    var anchorGo = new GameObject("SpawnAnchor");
                    anchorGo.transform.SetParent(spawnerTransform);
                    sceneAnchor = anchorGo.transform;
                    Undo.RegisterCreatedObjectUndo(anchorGo, "Create SpawnAnchor");
                }
                sceneAnchor.localPosition = new Vector3(0, 0.35f, 0);
            }

            Debug.Log("<color=#00FF88><b>[RemoveSortingTableUtility] SUCESSO!</b> A mesa e os colisores foram removidos. Animais agora nascem limpos no chão da nave!</color>");
        }
    }
}
