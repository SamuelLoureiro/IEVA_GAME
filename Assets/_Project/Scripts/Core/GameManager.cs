using UnityEngine;
using VARG.Data;

namespace VARG.Core
{
    public class GameManager : Singleton<GameManager>
    {
        public int CurrentLevel { get; private set; } = 1;
        public LevelResult LastResult { get; private set; }

        public void StartLevel()
        {
        }

        public void GoToHunt()
        {
        }

        public void GoToSorting()
        {
        }

        public void Victory()
        {
            GameEvents.TriggerVictory();
        }

        public void Defeat(DefeatReason reason)
        {
            GameEvents.TriggerDefeat(reason);
        }

        public void NextLevel()
        {
            CurrentLevel++;
            StartLevel();
        }

        public void RetryLevel()
        {
            StartLevel();
        }
    }
}
