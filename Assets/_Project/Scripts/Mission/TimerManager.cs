using System;
using UnityEngine;
using VARG.Core;
using VARG.Data;

namespace VARG.Mission
{
    public class TimerManager : Singleton<TimerManager>
    {
        private const float TimeLowThreshold = 15f;

        private float _captureRemaining;
        private float _sortingRemaining;
        private bool _isCaptureRunning;
        private bool _isSortingRunning;
        private bool _captureTimeLowTriggered;
        private bool _sortingTimeLowTriggered;

        public float SortingDebt { get; private set; }

        public event Action<TimerPhase, float> OnTimeChanged;
        public event Action<TimerPhase, float> OnPenaltyApplied;
        public event Action<TimerPhase> OnTimeUp;

        private void Update()
        {
            var dt = Time.deltaTime;
            if (_isCaptureRunning) TickPhase(TimerPhase.Capture, ref _captureRemaining, ref _captureTimeLowTriggered, ref _isCaptureRunning, dt);
            if (_isSortingRunning) TickPhase(TimerPhase.Sorting, ref _sortingRemaining, ref _sortingTimeLowTriggered, ref _isSortingRunning, dt);
        }

        private void TickPhase(TimerPhase phase, ref float remaining, ref bool lowTriggered, ref bool isRunning, float dt)
        {
            remaining = Mathf.Max(0f, remaining - dt);
            OnTimeChanged?.Invoke(phase, remaining);

            if (!lowTriggered && remaining <= TimeLowThreshold && remaining > 0f)
            {
                lowTriggered = true;
                GameEvents.TriggerTimeLow(phase);
            }

            if (remaining <= 0f)
            {
                isRunning = false;
                OnTimeUp?.Invoke(phase);
                GameManager.Instance?.Defeat(phase == TimerPhase.Capture ? DefeatReason.CaptureTimeUp : DefeatReason.SortingTimeUp);
            }
        }

        public void StartTimer(TimerPhase phase, float duration)
        {
            if (phase == TimerPhase.Capture)
            {
                _isSortingRunning = false;
                _captureRemaining = Mathf.Max(0f, duration);
                _isCaptureRunning = true;
                _captureTimeLowTriggered = false;
                OnTimeChanged?.Invoke(TimerPhase.Capture, _captureRemaining);
            }
            else
            {
                _isCaptureRunning = false;
                _sortingRemaining = Mathf.Max(0f, duration - SortingDebt);
                _isSortingRunning = true;
                _sortingTimeLowTriggered = false;
                ResetDebt();
                OnTimeChanged?.Invoke(TimerPhase.Sorting, _sortingRemaining);
            }
        }

        public void StopTimer(TimerPhase phase)
        {
            if (phase == TimerPhase.Capture) _isCaptureRunning = false;
            else _isSortingRunning = false;
        }

        public void StopAllTimers() => _isCaptureRunning = _isSortingRunning = false;

        public void ResetTimers()
        {
            StopAllTimers();
            ResetDebt();
            _captureRemaining = _sortingRemaining = 0f;
            _captureTimeLowTriggered = _sortingTimeLowTriggered = false;
        }

        public void ApplyPenalty(TimerPhase phase, float seconds)
        {
            if (seconds <= 0f) return;

            if (phase == TimerPhase.Capture && _isCaptureRunning)
            {
                _captureRemaining = Mathf.Max(0f, _captureRemaining - seconds);
                OnPenaltyApplied?.Invoke(TimerPhase.Capture, seconds);
                OnTimeChanged?.Invoke(TimerPhase.Capture, _captureRemaining);
                if (_captureRemaining <= 0f) TickPhase(TimerPhase.Capture, ref _captureRemaining, ref _captureTimeLowTriggered, ref _isCaptureRunning, 0f);
            }
            else if (phase == TimerPhase.Sorting)
            {
                if (_isSortingRunning)
                {
                    _sortingRemaining = Mathf.Max(0f, _sortingRemaining - seconds);
                    OnPenaltyApplied?.Invoke(TimerPhase.Sorting, seconds);
                    OnTimeChanged?.Invoke(TimerPhase.Sorting, _sortingRemaining);
                    if (_sortingRemaining <= 0f) TickPhase(TimerPhase.Sorting, ref _sortingRemaining, ref _sortingTimeLowTriggered, ref _isSortingRunning, 0f);
                }
                else
                {
                    SortingDebt += seconds;
                    OnPenaltyApplied?.Invoke(TimerPhase.Sorting, seconds);
                }
            }
        }

        public float GetRemaining(TimerPhase phase) => phase == TimerPhase.Capture ? _captureRemaining : _sortingRemaining;
        public bool IsRunning(TimerPhase phase) => phase == TimerPhase.Capture ? _isCaptureRunning : _isSortingRunning;
        public void ResetDebt() => SortingDebt = 0f;
    }
}
