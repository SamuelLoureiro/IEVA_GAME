using System;
using System.Collections.Generic;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Inventory;

namespace VARG.Mission
{
    public class MissionManager : Singleton<MissionManager>
    {
        private const float DefaultPenalty = 10f;

        [SerializeField] private TargetData[] _availableTargets;
        [SerializeField] private PenaltyConfig _penaltyConfig;

        private readonly List<TargetData> _requiredOrder = new();

        public IReadOnlyList<TargetData> RequiredOrder => _requiredOrder;
        public TargetType ForbiddenType { get; private set; }
        public int CurrentIndex { get; private set; }
        public int RequiredCount => _requiredOrder.Count;
        public bool IsComplete => CurrentIndex >= RequiredCount && RequiredCount > 0;

        protected override void Awake()
        {
            base.Awake();
            LoadDependenciesIfMissing();
        }

        private void LoadDependenciesIfMissing()
        {
            if (_penaltyConfig == null)
            {
                _penaltyConfig = Resources.Load<PenaltyConfig>("Data/PenaltyConfig");
            }
        }

        public void GenerateMission(LevelConfig level)
        {
            LoadDependenciesIfMissing();
            _requiredOrder.Clear();
            CurrentIndex = 0;

            var allTypes = (TargetType[])Enum.GetValues(typeof(TargetType));
            ForbiddenType = allTypes[UnityEngine.Random.Range(0, allTypes.Length)];

            var allowedTypes = new List<TargetType>();
            for (var i = 0; i < allTypes.Length; i++)
            {
                if (allTypes[i] != ForbiddenType) allowedTypes.Add(allTypes[i]);
            }

            var listSize = level != null ? level.ListSize : 3;
            var chosenTypes = new List<TargetType>(allowedTypes);

            while (chosenTypes.Count < listSize)
            {
                chosenTypes.Add(allowedTypes[UnityEngine.Random.Range(0, allowedTypes.Count)]);
            }

            for (var i = 0; i < chosenTypes.Count; i++)
            {
                var randomIndex = UnityEngine.Random.Range(i, chosenTypes.Count);
                (chosenTypes[i], chosenTypes[randomIndex]) = (chosenTypes[randomIndex], chosenTypes[i]);
            }

            for (var i = 0; i < chosenTypes.Count; i++)
            {
                var targetData = GetTargetDataForType(chosenTypes[i]);
                if (targetData != null) _requiredOrder.Add(targetData);
            }
        }

        public void LoadCustomMission(IReadOnlyList<TargetType> targetSequence, TargetType forbiddenType)
        {
            LoadDependenciesIfMissing();
            _requiredOrder.Clear();
            CurrentIndex = 0;
            ForbiddenType = forbiddenType;

            for (var i = 0; i < targetSequence.Count; i++)
            {
                var targetData = GetTargetDataForType(targetSequence[i]);
                if (targetData != null) _requiredOrder.Add(targetData);
            }
        }

        public TargetData GetTargetDataForType(TargetType type)
        {
            if (_availableTargets == null) return null;
            for (var i = 0; i < _availableTargets.Length; i++)
            {
                if (_availableTargets[i] != null && _availableTargets[i].Type == type) return _availableTargets[i];
            }
            return null;
        }

        public bool IsForbidden(TargetType targetType) => targetType == ForbiddenType;

        public int GetRequiredCountForType(TargetType type)
        {
            var count = 0;
            for (var i = 0; i < _requiredOrder.Count; i++)
            {
                if (_requiredOrder[i] != null && _requiredOrder[i].Type == type) count++;
            }
            return count;
        }

        public CaptureResult ValidateCapture(TargetData captured)
        {
            if (captured == null) return CaptureResult.NotInList;

            var outOfOrderPenalty = _penaltyConfig != null ? _penaltyConfig.OutOfOrderCapturePenalty : DefaultPenalty;
            var notInListDebt = _penaltyConfig != null ? _penaltyConfig.NotInListSortingDebt : DefaultPenalty;

            TargetData expected = (!IsComplete && CurrentIndex < _requiredOrder.Count) ? _requiredOrder[CurrentIndex] : null;

            if (captured.Type == ForbiddenType)
            {
                InventoryManager.Instance?.Add(captured, CaptureResult.NotInList);
                TimerManager.Instance?.ApplyPenalty(TimerPhase.Sorting, notInListDebt);
                LivesManager.Instance?.LoseLife();
                GameEvents.TriggerNotInListCapture(captured);
                return CaptureResult.NotInList;
            }

            if (!IsComplete && expected != null && captured == expected)
            {
                CurrentIndex++;
                InventoryManager.Instance?.Add(captured, CaptureResult.Correct);
                GameEvents.TriggerCorrectCapture(captured);
                if (IsComplete) GameEvents.TriggerMissionComplete();
                return CaptureResult.Correct;
            }

            InventoryManager.Instance?.Add(captured, CaptureResult.OutOfOrder);
            TimerManager.Instance?.ApplyPenalty(TimerPhase.Capture, outOfOrderPenalty);
            LivesManager.Instance?.LoseLife();
            GameEvents.TriggerOutOfOrderCapture(captured, expected);
            return CaptureResult.OutOfOrder;
        }

        public void SetMissionDebug(List<TargetData> order, TargetType forbidden)
        {
            _requiredOrder.Clear();
            _requiredOrder.AddRange(order);
            ForbiddenType = forbidden;
            CurrentIndex = 0;
        }
    }
}
