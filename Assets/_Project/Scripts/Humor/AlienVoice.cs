using UnityEngine;

namespace VARG.Humor
{
    public class AlienVoice : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _voiceClip;
        [SerializeField] private float _minInterval = 0.05f;

        private float _lastPlayTime;
        private static AudioClip _fallbackBeep;

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            _audioSource.playOnAwake = false;
        }

        public void PlayBeep()
        {
            if (Time.time - _lastPlayTime < _minInterval)
            {
                return;
            }

            _lastPlayTime = Time.time;

            if (_audioSource == null) return;

            _audioSource.pitch = 1.0f;

            if (_voiceClip != null)
            {
                _audioSource.PlayOneShot(_voiceClip);
            }
            else
            {
                _audioSource.PlayOneShot(GetFallbackBeep());
            }
        }

        private static AudioClip GetFallbackBeep()
        {
            if (_fallbackBeep != null) return _fallbackBeep;

            const int sampleRate = 22050;
            const float duration = 0.035f;
            var sampleCount = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / sampleRate;
                var envelope = 1f - (t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * 440f * t) * envelope * 0.35f;
            }

            _fallbackBeep = AudioClip.Create("AlienBeep_Fallback", sampleCount, 1, sampleRate, false);
            _fallbackBeep.SetData(samples, 0);
            return _fallbackBeep;
        }
    }
}
