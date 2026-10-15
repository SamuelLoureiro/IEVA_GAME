using UnityEngine;
using VARG.Data;
using VARG.Inventory;

namespace VARG.DebugTools
{
    public class DebugInventorySeeder : MonoBehaviour
    {
        [Header("Targets to Seed")]
        [SerializeField] private TargetData _chicken;
        [SerializeField] private TargetData _cow;
        [SerializeField] private TargetData _pig;
        [SerializeField] private TargetData _human;

        [Header("Options")]
        [SerializeField] private bool _seedOnStart = false;
        [SerializeField] private bool _showGui = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void Configure(bool seedOnStart)
        {
            _seedOnStart = seedOnStart;
        }

        private void Start()
        {
            LoadTargetsIfMissing();

            if (_seedOnStart && (InventoryManager.Instance == null || InventoryManager.Instance.Count == 0))
            {
                SeedDebugInventory();
            }
        }

        private void LoadTargetsIfMissing()
        {
#if UNITY_EDITOR
            if (_chicken == null) _chicken = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Chicken.asset");
            if (_cow == null) _cow = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Cow.asset");
            if (_pig == null) _pig = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Pig.asset");
            if (_human == null) _human = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Human.asset");
#endif
            if (_chicken == null || _cow == null || _pig == null || _human == null)
            {
                var allTargets = Resources.FindObjectsOfTypeAll<TargetData>();
                foreach (var target in allTargets)
                {
                    if (target == null) continue;
                    if (_chicken == null && target.Type == TargetType.Chicken) _chicken = target;
                    if (_cow == null && target.Type == TargetType.Cow) _cow = target;
                    if (_pig == null && target.Type == TargetType.Pig) _pig = target;
                    if (_human == null && target.Type == TargetType.Human) _human = target;
                }
            }
        }

        public void SeedDebugInventory()
        {
            if (InventoryManager.Instance == null) return;
            LoadTargetsIfMissing();

            InventoryManager.Instance.Clear();

            // 1. Correct captures
            if (_cow != null) InventoryManager.Instance.Add(_cow, CaptureResult.Correct);
            if (_pig != null) InventoryManager.Instance.Add(_pig, CaptureResult.Correct);

            // 2. Out of order capture
            if (_chicken != null) InventoryManager.Instance.Add(_chicken, CaptureResult.OutOfOrder);

            // 3. Not in list capture (forbidden target)
            if (_human != null) InventoryManager.Instance.Add(_human, CaptureResult.NotInList);

            Debug.Log($"[VARG] Inventory seeded with {InventoryManager.Instance.Count} items.");
        }

        private void OnGUI()
        {
            if (!_showGui) return;
            if (InventoryManager.Instance == null) return;

            GUILayout.BeginArea(new Rect(10, 300, 240, 220), "Inventory Debug", GUI.skin.window);

            GUILayout.Label($"Total Items: {InventoryManager.Instance.Count}");

            for (var i = 0; i < InventoryManager.Instance.Items.Count; i++)
            {
                var item = InventoryManager.Instance.Items[i];
                var nameStr = item.Data != null ? item.Data.DisplayName : "Null";
                GUILayout.Label($"[{i + 1}] {nameStr} - {item.Result}");
            }

            if (GUILayout.Button("Seed Inventory (Mock)"))
            {
                SeedDebugInventory();
            }

            if (GUILayout.Button("Clear Inventory"))
            {
                InventoryManager.Instance.Clear();
            }

            GUILayout.EndArea();
        }
    }
}
