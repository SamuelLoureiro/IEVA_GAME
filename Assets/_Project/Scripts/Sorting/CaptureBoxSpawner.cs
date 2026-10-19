using System.Collections.Generic;
using UnityEngine;
using VARG.Data;
using VARG.Inventory;

namespace VARG.Sorting
{
    public class CaptureBoxSpawner : MonoBehaviour
    {
        [SerializeField] private Transform _spawnAnchor;
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(2.0f, 0.2f, 1.8f);

        private readonly List<GameObject> _spawnedObjects = new();

        public IReadOnlyList<GameObject> SpawnedObjects => _spawnedObjects;

        private void Awake()
        {
            if (_spawnAnchor == null)
            {
                var found = transform.Find("SpawnAnchor");
                if (found != null) _spawnAnchor = found;
            }
        }

        private void Start()
        {
            if (InventoryManager.Instance != null)
            {
                SpawnExistingItems();
            }
        }

        public void SpawnExistingItems()
        {
            ClearSpawned();
            if (InventoryManager.Instance == null) return;

            var items = InventoryManager.Instance.Items;
            for (var i = 0; i < items.Count; i++)
            {
                SpawnItem(items[i]);
            }
        }

        public GameObject SpawnItem(CapturedItem item)
        {
            if (item.Data == null || item.Data.MiniPrefab == null) return null;

            var anchor = _spawnAnchor != null ? _spawnAnchor.position : transform.position;
            var randomPos = new Vector3(
                anchor.x + Random.Range(-_spawnAreaSize.x * 0.5f, _spawnAreaSize.x * 0.5f),
                Mathf.Max(anchor.y, 0.35f),
                anchor.z + Random.Range(-_spawnAreaSize.z * 0.5f, _spawnAreaSize.z * 0.5f)
            );

            var instance = Instantiate(item.Data.MiniPrefab, randomPos, Quaternion.Euler(0, Random.Range(0, 360), 0));

            var sortable = instance.GetComponent<SortableItem>() ?? instance.AddComponent<SortableItem>();
            sortable.Initialize(item);

            if (instance.GetComponent<Grabbable>() == null)
            {
                instance.AddComponent<Grabbable>();
            }

            var grabbableLayer = LayerMask.NameToLayer("Grabbable");
            if (grabbableLayer != -1)
            {
                instance.layer = grabbableLayer;
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
    }
}
