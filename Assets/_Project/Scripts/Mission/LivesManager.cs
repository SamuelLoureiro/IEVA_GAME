using System;
using UnityEngine;
using VARG.Core;
using VARG.Data;

namespace VARG.Mission
{
    public class LivesManager : Singleton<LivesManager>
    {
        public const int DefaultStartingLives = 5;

        public int Lives { get; private set; } = DefaultStartingLives;

        public event Action<int> OnLivesChanged;

        public void ResetLives(int amount)
        {
            Lives = Mathf.Max(0, amount);
            OnLivesChanged?.Invoke(Lives);
        }

        public void LoseLife()
        {
            if (Lives <= 0) return;

            Lives--;
            OnLivesChanged?.Invoke(Lives);
            GameEvents.TriggerLifeLost(Lives);

            if (Lives <= 0)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Defeat(DefeatReason.NoLives);
                }
            }
        }
    }
}
