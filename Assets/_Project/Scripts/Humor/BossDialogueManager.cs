using System;
using UnityEngine;
using VARG.Core;
using VARG.Data;

namespace VARG.Humor
{
    public class BossDialogueManager : Singleton<BossDialogueManager>
    {
        [SerializeField] private BossLineSet _lines;
        [SerializeField] private float _minCooldown = 3.5f;

        public static event Action<string, BossExpression> OnBossSpokeStatic;
        public event Action<string, BossExpression> OnBossSpoke;

        public BossLineSet Lines => _lines;
        public string LastSpokenLine { get; private set; }
        public BossExpression LastSpokenExpression { get; private set; }

        private float _nextAllowedSpeechTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents() => OnBossSpokeStatic = null;

        private void OnEnable()
        {
            GameEvents.OnCorrectCapture += HandleCorrectCapture;
            GameEvents.OnOutOfOrderCapture += HandleOutOfOrderCapture;
            GameEvents.OnNotInListCapture += HandleNotInListCapture;
            GameEvents.OnBeamDenied += HandleBeamDenied;
            GameEvents.OnCorrectCage += HandleCorrectCage;
            GameEvents.OnWrongCage += HandleWrongCage;
            GameEvents.OnCorrectDiscard += HandleCorrectDiscard;
            GameEvents.OnWrongDiscard += HandleWrongDiscard;
            GameEvents.OnLifeLost += HandleLifeLost;
            GameEvents.OnTimeLow += HandleTimeLow;
            GameEvents.OnVictory += HandleVictory;
            GameEvents.OnDefeat += HandleDefeat;
            GameEvents.OnIdle += HandleIdle;
        }

        private void OnDisable()
        {
            GameEvents.OnCorrectCapture -= HandleCorrectCapture;
            GameEvents.OnOutOfOrderCapture -= HandleOutOfOrderCapture;
            GameEvents.OnNotInListCapture -= HandleNotInListCapture;
            GameEvents.OnBeamDenied -= HandleBeamDenied;
            GameEvents.OnCorrectCage -= HandleCorrectCage;
            GameEvents.OnWrongCage -= HandleWrongCage;
            GameEvents.OnCorrectDiscard -= HandleCorrectDiscard;
            GameEvents.OnWrongDiscard -= HandleWrongDiscard;
            GameEvents.OnLifeLost -= HandleLifeLost;
            GameEvents.OnTimeLow -= HandleTimeLow;
            GameEvents.OnVictory -= HandleVictory;
            GameEvents.OnDefeat -= HandleDefeat;
            GameEvents.OnIdle -= HandleIdle;
        }

        private void HandleCorrectCapture(TargetData d) => SpeakRandom(_lines?.CorrectCaptureLines, BossExpression.Smug, d);
        private void HandleOutOfOrderCapture(TargetData c, TargetData e) => SpeakRandom(_lines?.OutOfOrderCaptureLines, BossExpression.Angry, c, e);
        private void HandleNotInListCapture(TargetData d) => SpeakRandom(_lines?.NotInListCaptureLines, BossExpression.Furious, d);
        private void HandleBeamDenied() => SpeakRandom(_lines?.BeamDeniedLines, BossExpression.Annoyed);
        private void HandleCorrectCage(TargetData d) => SpeakRandom(_lines?.CorrectCageLines, BossExpression.Smug, d);
        private void HandleWrongCage(TargetData d) => SpeakRandom(_lines?.WrongCageLines, BossExpression.Angry, d);
        private void HandleCorrectDiscard(TargetData d) => SpeakRandom(_lines?.CorrectDiscardLines, BossExpression.Neutral, d);
        private void HandleWrongDiscard(TargetData d) => SpeakRandom(_lines?.WrongDiscardLines, BossExpression.Furious, d);
        private void HandleTimeLow(TimerPhase p) => SpeakRandom(_lines?.TimeLowLines, BossExpression.Angry);
        private void HandleVictory() => SpeakRandom(_lines?.VictoryLines, BossExpression.Smug, highPriority: true);
        private void HandleDefeat(DefeatReason r) => SpeakRandom(_lines?.DefeatLines, BossExpression.Furious, highPriority: true);
        private void HandleIdle() => SpeakRandom(_lines?.IdleLines, BossExpression.Annoyed);

        public void Speak(string rawLine, BossExpression expression, bool highPriority = false)
        {
            if (string.IsNullOrEmpty(rawLine)) return;
            if (!highPriority && Time.time < _nextAllowedSpeechTime) return;

            LastSpokenLine = rawLine;
            LastSpokenExpression = expression;
            _nextAllowedSpeechTime = Time.time + _minCooldown;

            OnBossSpoke?.Invoke(rawLine, expression);
            OnBossSpokeStatic?.Invoke(rawLine, expression);
        }

        public string PickLine(string categoryKey, string[] lines)
        {
            if (lines == null || lines.Length == 0) return string.Empty;
            return lines[UnityEngine.Random.Range(0, lines.Length)];
        }

        public void SetLineSet(BossLineSet lineSet) => _lines = lineSet;

        private void SpeakRandom(string[] array, BossExpression expr, TargetData captured = null, TargetData expected = null, bool highPriority = false)
        {
            if (array == null || array.Length == 0) return;
            var template = array[UnityEngine.Random.Range(0, array.Length)];
            if (captured != null) template = template.Replace("{captured}", captured.DisplayName.ToUpper());
            if (expected != null) template = template.Replace("{expected}", expected.DisplayName.ToUpper());
            Speak(template, expr, highPriority);
        }

        private void HandleLifeLost(int livesLeft)
        {
            if (_lines == null) return;
            var array = livesLeft <= 1 ? _lines.LifeLostTier3Lines : (livesLeft <= 3 ? _lines.LifeLostTier2Lines : _lines.LifeLostTier1Lines);
            var expr = livesLeft <= 1 ? BossExpression.Furious : (livesLeft <= 3 ? BossExpression.Angry : BossExpression.Annoyed);
            SpeakRandom(array, expr, highPriority: true);
        }
    }
}
