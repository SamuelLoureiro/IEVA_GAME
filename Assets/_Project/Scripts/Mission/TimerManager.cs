using System;
using UnityEngine;
using VARG.Core;
using VARG.Data;

namespace VARG.Mission
{
    public class TimerManager : Singleton<TimerManager>
    {
        public float SortingDebt { get; private set; }

        public event Action<TimerPhase, float> OnTimeChanged;
        public event Action<TimerPhase, float> OnPenaltyApplied;
        public event Action<TimerPhase> OnTimeUp;

        public void StartTimer(TimerPhase phase, float duration)
        {
        }

        public void StopTimer(TimerPhase phase)
        {
        }

        public void Pause(bool paused)
        {
        }

        public void ApplyPenalty(TimerPhase phase, float seconds)
        {
        }

        public float GetRemaining(TimerPhase phase)
        {
            return 0f;
        }

        public void ResetDebt()
        {
            SortingDebt = 0f;
        }
    }
}
