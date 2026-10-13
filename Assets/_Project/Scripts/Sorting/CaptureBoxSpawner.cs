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
                var anchor = transform.Find("SpawnAnchor");
                _spawnAnchor = anchor != null ? anchor : transform;
            }
        }

        private void Start()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnItemAdded += HandleItemAdded;
                InventoryManager.Instance.OnInventoryCleared += ClearSpawned;

                SpawnExistingItems();
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
                InventoryManager.Instance.OnInventoryCleared -= ClearSpawned;
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

        private void HandleItemAdded(CapturedItem item)
        {
            SpawnItem(item);
        }

        public GameObject SpawnItem(CapturedItem item)
        {
            if (item.Data == null) return null;

            var prefab = item.Data.MiniPrefab;
            if (prefab == null)
            {
                Debug.LogWarning($"[CaptureBoxSpawner] TargetData '{item.Data.name}' has no MiniPrefab assigned.");
                return null;
            }

            var spawnPos = GetRandomSpawnPosition();
            var randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            var instance = Instantiate(prefab, spawnPos, randomRot);
            instance.name = $"{prefab.name}_{_spawnedObjects.Count + 1}";

            var sortable = instance.GetComponent<SortableItem>();
            if (sortable == null)
            {
                sortable = instance.AddComponent<SortableItem>();
            }
            sortable.Initialize(item);

            if (instance.GetComponent<Grabbable>() == null)
            {
                instance.AddComponent<Grabbable>();
            }

            if (instance.GetComponent<HighlightOnHover>() == null)
            {
                instance.AddComponent<HighlightOnHover>();
            }

            var rb = instance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                var targetType = item.Data != null ? item.Data.Type : TargetType.Chicken;
                rb.mass = targetType switch
                {
                    TargetType.Cow => 2.5f,
                    TargetType.Pig => 1.4f,
                    TargetType.Human => 1.4f,
                    TargetType.Chicken => 0.6f,
                    _ => 1.0f
                };
            }

            var grabbableLayer = LayerMask.NameToLayer("Grabbable");
            if (grabbableLayer != -1)
            {
                instance.layer = grabbableLayer;
            }

            _spawnedObjects.Add(instance);
            return instance;
        }

        public void ClearSpawned()
        {
            for (var i = _spawnedObjects.Count - 1; i >= 0; i--)
            {
                if (_spawnedObjects[i] != null)
                {
                    Destroy(_spawnedObjects[i]);
                }
            }
            _spawnedObjects.Clear();
        }

        private Vector3 GetRandomSpawnPosition()
        {
            var center = _spawnAnchor != null ? _spawnAnchor.position : transform.position;
            var halfX = _spawnAreaSize.x * 0.5f;
            var halfZ = _spawnAreaSize.z * 0.5f;

            var xOffset = Random.Range(-halfX, halfX);
            var yOffset = Random.Range(0f, _spawnAreaSize.y);
            var zOffset = Random.Range(-halfZ, halfZ);

            return center + new Vector3(xOffset, yOffset, zOffset);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.35f);
            var center = _spawnAnchor != null ? _spawnAnchor.position : transform.position;
            Gizmos.DrawWireCube(center, _spawnAreaSize);
        }
    }
}
