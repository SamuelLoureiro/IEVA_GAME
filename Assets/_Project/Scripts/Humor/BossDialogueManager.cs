using System;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.UI;

namespace VARG.Humor
{
    public class BossDialogueManager : Singleton<BossDialogueManager>
    {
        [SerializeField] private BossLineSet _lines;
        [SerializeField] private float _minCooldown = 1.8f;

        [Header("Boss Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _bossVoiceClip;

        public BossLineSet Lines => _lines;
        public string LastSpokenLine { get; private set; }

        private float _nextAllowedSpeechTime;

        protected override void Awake()
        {
            base.Awake();
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;

            if (_lines == null) _lines = Resources.Load<BossLineSet>("Data/BossLineSet");
            if (_bossVoiceClip == null) _bossVoiceClip = Resources.Load<AudioClip>("Audio/Et");
        }

        private void OnEnable()
        {
            GameEvents.OnCorrectCapture += HandleCorrectCapture;
            GameEvents.OnOutOfOrderCapture += HandleOutOfOrderCapture;
            GameEvents.OnNotInListCapture += HandleNotInListCapture;

            GameEvents.OnCorrectCage += HandleCorrectCage;
            GameEvents.OnWrongCage += HandleWrongCage;
            GameEvents.OnCorrectDiscard += HandleCorrectDiscard;
            GameEvents.OnWrongDiscard += HandleWrongDiscard;

            GameEvents.OnVictory += HandleVictory;
            GameEvents.OnDefeat += HandleDefeat;
        }

        private void OnDisable()
        {
            GameEvents.OnCorrectCapture -= HandleCorrectCapture;
            GameEvents.OnOutOfOrderCapture -= HandleOutOfOrderCapture;
            GameEvents.OnNotInListCapture -= HandleNotInListCapture;

            GameEvents.OnCorrectCage -= HandleCorrectCage;
            GameEvents.OnWrongCage -= HandleWrongCage;
            GameEvents.OnCorrectDiscard -= HandleCorrectDiscard;
            GameEvents.OnWrongDiscard -= HandleWrongDiscard;

            GameEvents.OnVictory -= HandleVictory;
            GameEvents.OnDefeat -= HandleDefeat;
        }

        private void HandleCorrectCapture(TargetData d) => SpeakRandom(_lines?.CorrectCaptureLines, d);
        private void HandleOutOfOrderCapture(TargetData c, TargetData e) => SpeakRandom(_lines?.OutOfOrderCaptureLines, c);
        private void HandleNotInListCapture(TargetData d) => SpeakRandom(_lines?.NotInListCaptureLines, d);

        private void HandleCorrectCage(TargetData d) => SpeakRandom(_lines?.CorrectCageLines, d);
        private void HandleWrongCage(TargetData d) => SpeakRandom(_lines?.WrongCageLines, d);
        private void HandleCorrectDiscard(TargetData d) => SpeakRandom(_lines?.CorrectDiscardLines, d);
        private void HandleWrongDiscard(TargetData d) => SpeakRandom(_lines?.WrongDiscardLines, d);

        private void HandleVictory() => SpeakRandom(_lines?.VictoryLines);
        private void HandleDefeat(DefeatReason r) => SpeakRandom(_lines?.DefeatLines);

        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (Time.time < _nextAllowedSpeechTime) return;

            LastSpokenLine = text;
            _nextAllowedSpeechTime = Time.time + _minCooldown;

            PlayVoice();
            BossPopupUI.Show(text);
        }

        public void PlayVoice()
        {
            if (_audioSource != null && _bossVoiceClip != null)
            {
                _audioSource.PlayOneShot(_bossVoiceClip, 0.85f);
            }
        }

        private void SpeakRandom(string[] array, TargetData captured = null)
        {
            if (array == null || array.Length == 0) return;
            var text = array[UnityEngine.Random.Range(0, array.Length)];
            if (captured != null) text = text.Replace("{captured}", captured.DisplayName.ToUpper());
            Speak(text);
        }
    }
}
