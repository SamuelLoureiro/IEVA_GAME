using System;

namespace VARG.Data
{
    [Serializable]
    public struct LevelResult
    {
        public float RemainingCaptureTime;
        public float RemainingSortingTime;
        public int RemainingLives;
        public int ErrorsCount;
        public int Score;

        public LevelResult(float remainingCaptureTime, float remainingSortingTime, int remainingLives, int errorsCount, int score)
        {
            RemainingCaptureTime = remainingCaptureTime;
            RemainingSortingTime = remainingSortingTime;
            RemainingLives = remainingLives;
            ErrorsCount = errorsCount;
            Score = score;
        }
    }
}
