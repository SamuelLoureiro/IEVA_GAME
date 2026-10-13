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

        private int _totalItems;
        private int _remainingItems;
        private bool _isVictoryTriggered;

        public int TotalItems => _totalItems;
        public int RemainingItems => _remainingItems;
        public bool IsComplete => _remainingItems <= 0 && _totalItems > 0;

        public event Action<int, int> OnItemCountsChanged;

        private void Start()
        {
            ScanSceneItems();
            StartSortingTimer();
        }

        private void StartSortingTimer()
        {
            if (TimerManager.Instance == null) return;

            var config = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevelConfig() : null;
            var duration = config != null ? config.SortingTime : _fallbackSortingDuration;
            TimerManager.Instance.StartTimer(TimerPhase.Sorting, duration);
        }

        public void ScanSceneItems()
        {
            var items = FindObjectsByType<SortableItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                RegisterItem(item);
            }
        }

        public void RegisterItem(SortableItem item)
        {
            if (item == null || _registeredItems.Contains(item)) return;

            _registeredItems.Add(item);
            _totalItems = _registeredItems.Count;
            _remainingItems = _totalItems - _resolvedItems.Count;
            OnItemCountsChanged?.Invoke(_remainingItems, _totalItems);
        }

        public void NotifyItemResolved(SortableItem item)
        {
            if (item == null || _resolvedItems.Contains(item)) return;

            _resolvedItems.Add(item);
            _remainingItems = Mathf.Max(0, _totalItems - _resolvedItems.Count);
            OnItemCountsChanged?.Invoke(_remainingItems, _totalItems);

            if (_remainingItems <= 0 && !_isVictoryTriggered)
            {
                _isVictoryTriggered = true;
                StartCoroutine(VictorySequenceRoutine());
            }
        }

        private IEnumerator VictorySequenceRoutine()
        {
            yield return new WaitForSeconds(0.6f);

            var grab = FindFirstObjectByType<GrabController>();
            while (grab != null && grab.IsHolding)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);

            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.StopTimer(TimerPhase.Sorting);
            }

            GameEvents.TriggerVictory();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Victory();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (TimerManager.Instance != null && TimerManager.Instance.IsRunning(TimerPhase.Sorting))
            {
                TimerManager.Instance.StopTimer(TimerPhase.Sorting);
            }
        }
    }
}
