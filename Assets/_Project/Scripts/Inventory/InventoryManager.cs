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

        public event Action<CapturedItem> OnItemAdded;
        public event Action<CapturedItem> OnItemRemoved;

        public void Add(CapturedItem item)
        {
            _items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void Remove(CapturedItem item)
        {
            if (_items.Remove(item))
            {
                OnItemRemoved?.Invoke(item);
            }
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
