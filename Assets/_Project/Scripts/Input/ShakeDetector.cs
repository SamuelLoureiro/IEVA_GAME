using UnityEngine;

namespace VARG.Input
{
    public class ShakeDetector
    {
        private readonly float _thresholdG;
        private readonly int _requiredPeaks;
        private readonly float _windowSeconds;
        private readonly float _cooldownSeconds;

        private int _peakCount;
        private float _windowStartTime;
        private float _lastShakeTime = -10f;
        private bool _wasAboveThreshold;

        public ShakeDetector(
            float thresholdG = 1.75f,
            int requiredPeaks = 2,
            float windowSeconds = 0.6f,
            float cooldownSeconds = 1.0f)
        {
            _thresholdG = thresholdG;
            _requiredPeaks = requiredPeaks;
            _windowSeconds = windowSeconds;
            _cooldownSeconds = cooldownSeconds;
        }

        public bool ProcessSample(Vector3 accelerationInG, float currentTime)
        {
            if (currentTime - _lastShakeTime < _cooldownSeconds)
            {
                return false;
            }

            if (_peakCount > 0 && currentTime - _windowStartTime > _windowSeconds)
            {
                _peakCount = 0;
            }

            var isAboveThreshold = accelerationInG.magnitude >= _thresholdG;
            if (isAboveThreshold && !_wasAboveThreshold)
            {
                if (_peakCount == 0)
                {
                    _windowStartTime = currentTime;
                }

                _peakCount++;
                if (_peakCount >= _requiredPeaks)
                {
                    _peakCount = 0;
                    _lastShakeTime = currentTime;
                    _wasAboveThreshold = true;
                    return true;
                }
            }

            _wasAboveThreshold = isAboveThreshold;
            return false;
        }
    }
}
