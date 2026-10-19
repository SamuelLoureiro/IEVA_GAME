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

        [Header("Boss Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _bossVoiceClip;

        public static event Action<string, BossExpression> OnBossSpokeStatic;
        public event Action<string, BossExpression> OnBossSpoke;

        public BossLineSet Lines => _lines;
        public string LastSpokenLine { get; private set; }
        public BossExpression LastSpokenExpression { get; private set; }

        private float _nextAllowedSpeechTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents() => OnBossSpokeStatic = null;

        protected override void Awake()
        {
            base.Awake();
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            GameEvents.OnCorrectCapture += d => SpeakRandom(_lines?.CorrectCaptureLines, BossExpression.Smug, d);
            GameEvents.OnOutOfOrderCapture += (c, e) => SpeakRandom(_lines?.OutOfOrderCaptureLines, BossExpression.Angry, c, e);
            GameEvents.OnNotInListCapture += d => SpeakRandom(_lines?.NotInListCaptureLines, BossExpression.Furious, d);
            GameEvents.OnBeamDenied += () => SpeakRandom(_lines?.BeamDeniedLines, BossExpression.Annoyed);
            GameEvents.OnCorrectCage += d => SpeakRandom(_lines?.CorrectCageLines, BossExpression.Smug, d);
            GameEvents.OnWrongCage += d => SpeakRandom(_lines?.WrongCageLines, BossExpression.Angry, d);
            GameEvents.OnCorrectDiscard += d => SpeakRandom(_lines?.CorrectDiscardLines, BossExpression.Neutral, d);
            GameEvents.OnWrongDiscard += d => SpeakRandom(_lines?.WrongDiscardLines, BossExpression.Furious, d);
            GameEvents.OnLifeLost += HandleLifeLost;
            GameEvents.OnTimeLow += p => SpeakRandom(_lines?.TimeLowLines, BossExpression.Angry);
            GameEvents.OnVictory += () => SpeakRandom(_lines?.VictoryLines, BossExpression.Smug, highPriority: true);
            GameEvents.OnDefeat += r => SpeakRandom(_lines?.DefeatLines, BossExpression.Furious, highPriority: true);
            GameEvents.OnIdle += () => SpeakRandom(_lines?.IdleLines, BossExpression.Annoyed);
        }

        public void Speak(string rawLine, BossExpression expression, bool highPriority = false)
        {
            if (string.IsNullOrEmpty(rawLine)) return;
            if (!highPriority && Time.time < _nextAllowedSpeechTime) return;

            LastSpokenLine = rawLine;
            LastSpokenExpression = expression;
            _nextAllowedSpeechTime = Time.time + _minCooldown;

            PlayVoice();

            OnBossSpoke?.Invoke(rawLine, expression);
            OnBossSpokeStatic?.Invoke(rawLine, expression);
        }

        public void PlayVoice()
        {
            if (_audioSource != null && _bossVoiceClip != null)
            {
                _audioSource.PlayOneShot(_bossVoiceClip, 0.85f);
            }
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
