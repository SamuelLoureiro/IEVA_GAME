using System;
using UnityEngine;
using VARG.Core;

namespace VARG.Mission
{
    public class LivesManager : Singleton<LivesManager>
    {
        public int Lives { get; private set; } = 5;

        public event Action<int> OnLivesChanged;

        public void ResetLives(int amount)
        {
            Lives = amount;
            OnLivesChanged?.Invoke(Lives);
        }

        public void LoseLife()
        {
            if (Lives <= 0) return;

            Lives--;
            OnLivesChanged?.Invoke(Lives);
            GameEvents.TriggerLifeLost(Lives);
        }
    }
}
