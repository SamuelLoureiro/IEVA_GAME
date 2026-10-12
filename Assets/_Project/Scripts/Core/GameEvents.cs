using System;
using UnityEngine;
using VARG.Data;

namespace VARG.Core
{
    public static class GameEvents
    {
        public static event Action<TargetData> OnCorrectCapture;
        public static event Action<TargetData, TargetData> OnOutOfOrderCapture;
        public static event Action<TargetData> OnNotInListCapture;
        public static event Action OnMissionComplete;
        public static event Action OnBeamDenied;

        public static event Action<TargetData> OnCorrectCage;
        public static event Action<TargetData> OnWrongCage;
        public static event Action<TargetData> OnCorrectDiscard;
        public static event Action<TargetData> OnWrongDiscard;

        public static event Action<int> OnLifeLost;
        public static event Action<TimerPhase> OnTimeLow;

        public static event Action OnVictory;
        public static event Action<DefeatReason> OnDefeat;
        public static event Action OnIdle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents()
        {
            OnCorrectCapture = null;
            OnOutOfOrderCapture = null;
            OnNotInListCapture = null;
            OnMissionComplete = null;
            OnBeamDenied = null;

            OnCorrectCage = null;
            OnWrongCage = null;
            OnCorrectDiscard = null;
            OnWrongDiscard = null;

            OnLifeLost = null;
            OnTimeLow = null;

            OnVictory = null;
            OnDefeat = null;
            OnIdle = null;
        }

        public static void TriggerCorrectCapture(TargetData data) => OnCorrectCapture?.Invoke(data);
        public static void TriggerOutOfOrderCapture(TargetData captured, TargetData expected) => OnOutOfOrderCapture?.Invoke(captured, expected);
        public static void TriggerNotInListCapture(TargetData data) => OnNotInListCapture?.Invoke(data);
        public static void TriggerMissionComplete() => OnMissionComplete?.Invoke();
        public static void TriggerBeamDenied() => OnBeamDenied?.Invoke();

        public static void TriggerCorrectCage(TargetData data) => OnCorrectCage?.Invoke(data);
        public static void TriggerWrongCage(TargetData data) => OnWrongCage?.Invoke(data);
        public static void TriggerCorrectDiscard(TargetData data) => OnCorrectDiscard?.Invoke(data);
        public static void TriggerWrongDiscard(TargetData data) => OnWrongDiscard?.Invoke(data);

        public static void TriggerLifeLost(int livesLeft) => OnLifeLost?.Invoke(livesLeft);
        public static void TriggerTimeLow(TimerPhase phase) => OnTimeLow?.Invoke(phase);

        public static void TriggerVictory() => OnVictory?.Invoke();
        public static void TriggerDefeat(DefeatReason reason) => OnDefeat?.Invoke(reason);
        public static void TriggerIdle() => OnIdle?.Invoke();
    }
}
