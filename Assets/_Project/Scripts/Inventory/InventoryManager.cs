using System;
using System.Collections.Generic;
using VARG.Core;
using VARG.Data;

namespace VARG.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        private readonly List<CapturedItem> _items = new();

        public IReadOnlyList<CapturedItem> Items => _items;
        public int Count => _items.Count;

        public event Action<CapturedItem> OnItemAdded;
        public event Action OnInventoryCleared;

        public void Add(CapturedItem item)
        {
            _items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void Add(TargetData data, CaptureResult result)
        {
            Add(new CapturedItem(data, result));
        }

        public int CountByType(TargetType type)
        {
            var count = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Data != null && _items[i].Data.Type == type)
                {
                    count++;
                }
            }
            return count;
        }

        public void Clear()
        {
            _items.Clear();
            OnInventoryCleared?.Invoke();
        }
    }
}
