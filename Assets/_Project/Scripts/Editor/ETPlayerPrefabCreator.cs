#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VARG.Player;
using VARG.UI;

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

            var interactor = cameraGo.AddComponent<PlayerInteractor>();
            var interactorSerialized = new SerializedObject(interactor);
            interactorSerialized.FindProperty("_playerCamera").objectReferenceValue = cam;
            interactorSerialized.FindProperty("_range").floatValue = 15f;
            interactorSerialized.ApplyModifiedPropertiesWithoutUndo();

            // HUD Canvas with central crosshair
            var canvasGo = new GameObject("HUD_Crosshair");
            canvasGo.transform.SetParent(rootGo.transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var crosshairGo = new GameObject("Crosshair");
            crosshairGo.transform.SetParent(canvasGo.transform, false);
            var crosshairRect = crosshairGo.AddComponent<RectTransform>();
            crosshairRect.sizeDelta = new Vector2(8f, 8f);
            crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
            crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.anchoredPosition = Vector2.zero;

            var crosshairImg = crosshairGo.AddComponent<Image>();
            crosshairImg.color = new Color(0.2f, 1f, 0.4f, 0.75f);

            var crosshairUI = canvasGo.AddComponent<CrosshairUI>();
            var crosshairUiSerialized = new SerializedObject(crosshairUI);
            crosshairUiSerialized.FindProperty("_interactor").objectReferenceValue = interactor;
            crosshairUiSerialized.FindProperty("_crosshairImage").objectReferenceValue = crosshairImg;
            crosshairUiSerialized.FindProperty("_defaultColor").colorValue = new Color(0.2f, 1f, 0.4f, 0.75f);
            crosshairUiSerialized.FindProperty("_targetDetectedColor").colorValue = new Color(1f, 0.15f, 0.15f, 0.95f);
            crosshairUiSerialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(rootGo, path);
            Object.DestroyImmediate(rootGo);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VARG] ETPlayer prefab updated with PlayerInteractor and CrosshairUI at: {path}");
        }
    }
}
#endif
