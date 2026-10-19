using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Mission;

namespace VARG.Sorting
{
    public class SortingManager : Singleton<SortingManager>
    {
        [SerializeField] private float _fallbackSortingDuration = 45f;

        private readonly HashSet<SortableItem> _registeredItems = new();
        private readonly HashSet<SortableItem> _resolvedItems = new();
        private GrabController _grabController;
        private bool _isVictoryTriggered;

        public int TotalItems => _registeredItems.Count;
        public int RemainingItems => Mathf.Max(0, _registeredItems.Count - _resolvedItems.Count);
        public bool IsComplete => RemainingItems <= 0 && TotalItems > 0;

        public event Action<int, int> OnItemCountsChanged;
        protected override bool IsPersistent => false;

        private void Start()
        {
            _grabController = FindFirstObjectByType<GrabController>();
            ScanSceneItems();

            if (!TimerManager.HasInstance || !TimerManager.Instance.IsRunning(TimerPhase.Sorting))
            {
                var cfg = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevelConfig() : null;
                TimerManager.Instance?.StartTimer(TimerPhase.Sorting, cfg != null ? cfg.SortingTime : _fallbackSortingDuration);
            }
        }

        public void ScanSceneItems()
        {
            var items = FindObjectsByType<SortableItem>(FindObjectsSortMode.None);
            for (var i = 0; i < items.Length; i++) RegisterItem(items[i]);
        }

        public void ClearItems()
        {
            _registeredItems.Clear();
            _resolvedItems.Clear();
            _isVictoryTriggered = false;
            OnItemCountsChanged?.Invoke(0, 0);
        }

        public void UnregisterItem(SortableItem item)
        {
            if (item == null) return;
            _registeredItems.Remove(item);
            _resolvedItems.Remove(item);
            OnItemCountsChanged?.Invoke(RemainingItems, TotalItems);
        }

        public void RegisterItem(SortableItem item)
        {
            if (item == null || _registeredItems.Contains(item)) return;
            _registeredItems.Add(item);
            OnItemCountsChanged?.Invoke(RemainingItems, TotalItems);
        }

        public void NotifyItemResolved(SortableItem item)
        {
            if (item == null || _resolvedItems.Contains(item)) return;
            _resolvedItems.Add(item);
            OnItemCountsChanged?.Invoke(RemainingItems, TotalItems);

            if (RemainingItems <= 0 && !_isVictoryTriggered)
            {
                _isVictoryTriggered = true;
                StartCoroutine(VictoryRoutine());
            }
        }

        public int GetUnsortedCountForType(TargetType type)
        {
            var count = 0;
            foreach (var item in _registeredItems)
            {
                if (item != null && !item.IsSorted && item.Type == type) count++;
            }
            return count;
        }

        private IEnumerator VictoryRoutine()
        {
            yield return new WaitForSeconds(0.6f);
            while (_grabController != null && _grabController.IsHolding) yield return null;
            yield return new WaitForSeconds(0.3f);

            TimerManager.Instance?.StopTimer(TimerPhase.Sorting);
            GameEvents.TriggerVictory();
            GameManager.Instance?.Victory();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TimerManager.Instance?.StopTimer(TimerPhase.Sorting);
        }
    }
}
