using System;
using System.Collections.Generic;
using UnityEngine;
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
        public event Action<CapturedItem> OnItemRemoved;
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

        public bool Remove(CapturedItem item)
        {
            var removed = _items.Remove(item);
            if (removed)
            {
                OnItemRemoved?.Invoke(item);
            }
            return removed;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= _items.Count) return false;
            var item = _items[index];
            _items.RemoveAt(index);
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public bool Contains(TargetData data)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Data == data)
                {
                    return true;
                }
            }
            return false;
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
