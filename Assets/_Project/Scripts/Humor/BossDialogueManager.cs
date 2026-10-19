using System;
using System.Collections.Generic;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Input;

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
        public float LastSpokenTime { get; private set; }

        private float _nextAllowedSpeechTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents()
        {
            OnBossSpokeStatic = null;
        }

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

        public void Speak(string rawLine, BossExpression expression, bool highPriority = false)
        {
            if (string.IsNullOrEmpty(rawLine)) return;

            if (!highPriority && Time.time < _nextAllowedSpeechTime)
            {
                return;
            }

            LastSpokenLine = rawLine;
            LastSpokenExpression = expression;
            LastSpokenTime = Time.time;
            _nextAllowedSpeechTime = Time.time + _minCooldown;

            OnBossSpoke?.Invoke(rawLine, expression);
            OnBossSpokeStatic?.Invoke(rawLine, expression);
        }

        public string PickLine(string categoryKey, string[] lines)
        {
            if (lines == null || lines.Length == 0) return string.Empty;
            return lines[UnityEngine.Random.Range(0, lines.Length)];
        }

        public void SetLineSet(BossLineSet lineSet)
        {
            _lines = lineSet;
        }

        private void HandleCorrectCapture(TargetData data)
        {
            if (_lines == null) return;
            var template = PickLine("CorrectCapture", _lines.CorrectCaptureLines);
            var line = FormatLine(template, captured: data);
            Speak(line, BossExpression.Smug);
        }

        private void HandleOutOfOrderCapture(TargetData captured, TargetData expected)
        {
            if (_lines == null) return;
            var template = PickLine("OutOfOrderCapture", _lines.OutOfOrderCaptureLines);
            var line = FormatLine(template, captured: captured, expected: expected);
            Speak(line, BossExpression.Angry);
        }

        private void HandleNotInListCapture(TargetData data)
        {
            if (_lines == null) return;
            var template = PickLine("NotInListCapture", _lines.NotInListCaptureLines);
            var line = FormatLine(template, captured: data);
            Speak(line, BossExpression.Furious);
        }

        private void HandleBeamDenied()
        {
            if (_lines == null) return;
            var template = PickLine("BeamDenied", _lines.BeamDeniedLines);
            var line = FormatLine(template);
            Speak(line, BossExpression.Annoyed);
        }

        private void HandleCorrectCage(TargetData data)
        {
            if (_lines == null) return;
            var template = PickLine("CorrectCage", _lines.CorrectCageLines);
            var line = FormatLine(template, captured: data);
            Speak(line, BossExpression.Smug);
        }

        private void HandleWrongCage(TargetData data)
        {
            if (_lines == null) return;
            var template = PickLine("WrongCage", _lines.WrongCageLines);
            var line = FormatLine(template, captured: data);
            Speak(line, BossExpression.Angry);
        }

        private void HandleCorrectDiscard(TargetData data)
        {
            if (_lines == null) return;
            var template = PickLine("CorrectDiscard", _lines.CorrectDiscardLines);
            var line = FormatLine(template, captured: data);
            Speak(line, BossExpression.Neutral);
        }

        private void HandleWrongDiscard(TargetData data)
        {
            if (_lines == null) return;
            var template = PickLine("WrongDiscard", _lines.WrongDiscardLines);
            var line = FormatLine(template, captured: data);
            Speak(line, BossExpression.Furious);
        }

        private void HandleLifeLost(int livesLeft)
        {
            if (_lines == null) return;

            string[] array;
            BossExpression expression;

            if (livesLeft <= 1)
            {
                array = _lines.LifeLostTier3Lines;
                expression = BossExpression.Furious;
            }
            else if (livesLeft <= 3)
            {
                array = _lines.LifeLostTier2Lines;
                expression = BossExpression.Angry;
            }
            else
            {
                array = _lines.LifeLostTier1Lines;
                expression = BossExpression.Annoyed;
            }

            var template = PickLine($"LifeLost_{livesLeft}", array);
            var line = FormatLine(template, lives: livesLeft);
            Speak(line, expression, highPriority: true);
        }

        private void HandleTimeLow(TimerPhase phase)
        {
            if (_lines == null) return;
            var template = PickLine("TimeLow", _lines.TimeLowLines);
            var phaseStr = phase == TimerPhase.Capture ? "Caça" : "Triagem";
            var line = FormatLine(template, phase: phaseStr);
            Speak(line, BossExpression.Angry);
        }

        private void HandleVictory()
        {
            if (_lines == null) return;
            var template = PickLine("Victory", _lines.VictoryLines);
            var line = FormatLine(template);
            Speak(line, BossExpression.Smug, highPriority: true);
        }

        private void HandleDefeat(DefeatReason reason)
        {
            if (_lines == null) return;
            var template = PickLine("Defeat", _lines.DefeatLines);
            var reasonStr = reason switch
            {
                DefeatReason.NoLives => "Sem vidas restantes",
                DefeatReason.CaptureTimeUp => "Tempo de caça esgotado",
                DefeatReason.SortingTimeUp => "Tempo de triagem esgotado",
                _ => "Missão fracassada"
            };
            var line = FormatLine(template, reason: reasonStr);
            Speak(line, BossExpression.Furious, highPriority: true);
        }

        private void HandleIdle()
        {
            if (_lines == null) return;
            var template = PickLine("Idle", _lines.IdleLines);
            var line = FormatLine(template);
            Speak(line, BossExpression.Annoyed);
        }

        private string FormatLine(string template, TargetData captured = null, TargetData expected = null, int? lives = null, string phase = null, string reason = null)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;
            var text = template;

            if (captured != null) text = text.Replace("{captured}", captured.DisplayName.ToUpper());
            if (expected != null) text = text.Replace("{expected}", expected.DisplayName.ToUpper());
            if (lives.HasValue) text = text.Replace("{lives}", lives.Value.ToString());
            if (!string.IsNullOrEmpty(phase)) text = text.Replace("{phase}", phase);
            if (!string.IsNullOrEmpty(reason)) text = text.Replace("{reason}", reason);

            return text;
        }
    }
}
