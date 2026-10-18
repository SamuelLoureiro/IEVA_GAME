using UnityEngine;

namespace VARG.Humor
{
    [RequireComponent(typeof(AudioSource))]
    public class AlienVoice : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _voiceClip;
        [SerializeField] private float _minInterval = 0.05f;
        [SerializeField] private float _minPitch = 0.92f;
        [SerializeField] private float _maxPitch = 1.12f;

        private float _lastPlayTime;

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            if (_audioSource != null)
            {
                _audioSource.playOnAwake = false;
            }
        }

        public void PlayBeep()
        {
            if (Time.time - _lastPlayTime < _minInterval)
            {
                return;
            }

            _lastPlayTime = Time.time;

            if (_audioSource == null || _voiceClip == null)
            {
                return;
            }

            _audioSource.pitch = Random.Range(_minPitch, _maxPitch);
            _audioSource.PlayOneShot(_voiceClip);
        }
    }
}
