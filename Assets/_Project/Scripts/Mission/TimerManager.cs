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
        private bool _isPaused;

        private bool _captureTimeLowTriggered;
        private bool _sortingTimeLowTriggered;

        public float SortingDebt { get; private set; }
        public bool IsPaused => _isPaused;

        public event Action<TimerPhase, float> OnTimeChanged;
        public event Action<TimerPhase, float> OnPenaltyApplied;
        public event Action<TimerPhase> OnTimeUp;

        private void Update()
        {
            if (_isPaused) return;

            var deltaTime = Time.deltaTime;

            if (_isCaptureRunning)
            {
                TickPhase(TimerPhase.Capture, ref _captureRemaining, ref _captureTimeLowTriggered, ref _isCaptureRunning, deltaTime);
            }

            if (_isSortingRunning)
            {
                TickPhase(TimerPhase.Sorting, ref _sortingRemaining, ref _sortingTimeLowTriggered, ref _isSortingRunning, deltaTime);
            }
        }

        private void TickPhase(TimerPhase phase, ref float remaining, ref bool lowTriggered, ref bool isRunning, float deltaTime)
        {
            remaining -= deltaTime;
            if (remaining < 0f) remaining = 0f;

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

                var reason = phase == TimerPhase.Capture ? DefeatReason.CaptureTimeUp : DefeatReason.SortingTimeUp;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Defeat(reason);
                }
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
                // Transitioning to Sorting automatically pauses/stops the Capture timer
                _isCaptureRunning = false;
                var finalDuration = Mathf.Max(0f, duration - SortingDebt);
                _sortingRemaining = finalDuration;
                _isSortingRunning = true;
                _sortingTimeLowTriggered = false;
                ResetDebt();
                OnTimeChanged?.Invoke(TimerPhase.Sorting, _sortingRemaining);
            }
        }

        public void StopTimer(TimerPhase phase)
        {
            if (phase == TimerPhase.Capture)
            {
                _isCaptureRunning = false;
            }
            else
            {
                _isSortingRunning = false;
            }
        }

        public void StopAllTimers()
        {
            _isCaptureRunning = false;
            _isSortingRunning = false;
        }

        public void Pause(bool paused)
        {
            _isPaused = paused;
        }

        public void ApplyPenalty(TimerPhase phase, float seconds)
        {
            if (seconds <= 0f) return;

            if (phase == TimerPhase.Capture)
            {
                if (_isCaptureRunning)
                {
                    _captureRemaining = Mathf.Max(0f, _captureRemaining - seconds);
                    OnPenaltyApplied?.Invoke(TimerPhase.Capture, seconds);
                    OnTimeChanged?.Invoke(TimerPhase.Capture, _captureRemaining);

                    if (_captureRemaining <= 0f)
                    {
                        _isCaptureRunning = false;
                        OnTimeUp?.Invoke(TimerPhase.Capture);
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.Defeat(DefeatReason.CaptureTimeUp);
                        }
                    }
                }
            }
            else
            {
                if (_isSortingRunning)
                {
                    _sortingRemaining = Mathf.Max(0f, _sortingRemaining - seconds);
                    OnPenaltyApplied?.Invoke(TimerPhase.Sorting, seconds);
                    OnTimeChanged?.Invoke(TimerPhase.Sorting, _sortingRemaining);

                    if (_sortingRemaining <= 0f)
                    {
                        _isSortingRunning = false;
                        OnTimeUp?.Invoke(TimerPhase.Sorting);
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.Defeat(DefeatReason.SortingTimeUp);
                        }
                    }
                }
                else
                {
                    // Sorting phase is not running yet (e.g. during Capture phase), accumulates as debt
                    SortingDebt += seconds;
                    OnPenaltyApplied?.Invoke(TimerPhase.Sorting, seconds);
                }
            }
        }

        public float GetRemaining(TimerPhase phase)
        {
            return phase == TimerPhase.Capture ? _captureRemaining : _sortingRemaining;
        }

        public bool IsRunning(TimerPhase phase)
        {
            return phase == TimerPhase.Capture ? _isCaptureRunning : _isSortingRunning;
        }

        public void ResetDebt()
        {
            SortingDebt = 0f;
        }
    }
}
