using System.Collections.Generic;
using UnityEngine;
using VARG.Data;
using VARG.Inventory;
using VARG.Mission;

namespace VARG.Sorting
{
    public class CaptureBoxSpawner : MonoBehaviour
    {
        [SerializeField] private Transform _spawnAnchor;
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(2.0f, 0.2f, 1.8f);

        private readonly List<GameObject> _spawnedObjects = new();

        public IReadOnlyList<GameObject> SpawnedObjects => _spawnedObjects;

        private static readonly Vector3 DefaultSpawnCenter = new(0f, 0.35f, 1.2f);

        private void Start()
        {
            if (_spawnedObjects.Count == 0)
            {
                SpawnExistingItems();
            }
        }

        public void SpawnExistingItems()
        {
            ClearSpawned();
            if (InventoryManager.Instance == null) return;

            var items = InventoryManager.Instance.Items;
            if (items.Count == 0)
            {
                SeedFallbackItems();
                return;
            }

            for (var i = 0; i < items.Count; i++)
            {
                SpawnItem(items[i]);
            }
        }

        private void SeedFallbackItems()
        {
            if (MissionManager.Instance == null) return;

            var targets = MissionManager.Instance.RequiredOrder;
            if (targets == null || targets.Count == 0)
            {
                MissionManager.Instance.GenerateMission(null);
                targets = MissionManager.Instance.RequiredOrder;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    InventoryManager.Instance.Add(targets[i], CaptureResult.Correct);
                }
            }

            var seeded = InventoryManager.Instance.Items;
            for (var i = 0; i < seeded.Count; i++)
            {
                SpawnItem(seeded[i]);
            }
        }

        public GameObject SpawnItem(CapturedItem item)
        {
            if (item.Data == null || item.Data.MiniPrefab == null) return null;

            var anchor = _spawnAnchor != null ? _spawnAnchor.position : DefaultSpawnCenter;
            var randomPos = new Vector3(
                anchor.x + Random.Range(-_spawnAreaSize.x * 0.5f, _spawnAreaSize.x * 0.5f),
                Mathf.Max(anchor.y, 0.35f),
                anchor.z + Random.Range(-_spawnAreaSize.z * 0.5f, _spawnAreaSize.z * 0.5f)
            );

            var instance = Instantiate(item.Data.MiniPrefab, randomPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
            instance.transform.localScale *= 1.7f;

            var sortable = instance.GetComponent<SortableItem>() ?? instance.AddComponent<SortableItem>();
            sortable.Initialize(item);

            if (instance.GetComponent<Grabbable>() == null)
            {
                instance.AddComponent<Grabbable>();
            }

            var grabbableLayer = LayerMask.NameToLayer("Grabbable");
            if (grabbableLayer != -1)
            {
                SetLayerRecursively(instance, grabbableLayer);
            }

            if (SortingManager.Instance != null)
            {
                SortingManager.Instance.RegisterItem(sortable);
            }

            _spawnedObjects.Add(instance);
            return instance;
        }

        public void ClearSpawned()
        {
            if (SortingManager.Instance != null)
            {
                SortingManager.Instance.ClearItems();
            }

            for (var i = _spawnedObjects.Count - 1; i >= 0; i--)
            {
                if (_spawnedObjects[i] != null) Destroy(_spawnedObjects[i]);
            }
            _spawnedObjects.Clear();
        }

        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            var t = obj.transform;
            for (var i = 0; i < t.childCount; i++)
            {
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
            }
        }
    }
}
