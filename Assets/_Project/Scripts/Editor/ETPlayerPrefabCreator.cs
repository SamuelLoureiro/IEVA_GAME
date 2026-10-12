#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VARG.Player;

namespace VARG.Editor
{
    public static class ETPlayerPrefabCreator
    {
        [MenuItem("VARG/Create ETPlayer Prefab")]
        public static void CreatePrefab()
        {
            const string folder = "Assets/_Project/Prefabs/Player";
            const string path = "Assets/_Project/Prefabs/Player/ETPlayer.prefab";

            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Player");
            }

            var rootGo = new GameObject("ETPlayer");
            rootGo.tag = "Player";

            var playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                rootGo.layer = playerLayer;
            }

            var cc = rootGo.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            rootGo.AddComponent<ETPlayerController>();

            var cameraGo = new GameObject("MainCamera");
            cameraGo.transform.SetParent(rootGo.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            cameraGo.tag = "MainCamera";

            var cam = cameraGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.fieldOfView = 75f;

            cameraGo.AddComponent<AudioListener>();

            var cameraLook = cameraGo.AddComponent<ETCameraLook>();
            var lookSerialized = new SerializedObject(cameraLook);
            lookSerialized.FindProperty("_playerBody").objectReferenceValue = rootGo.transform;
            lookSerialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(rootGo, path);
            Object.DestroyImmediate(rootGo);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VARG] ETPlayer prefab successfully created at: {path}");
        }
    }
}
#endif
