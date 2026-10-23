using UnityEngine;
using VARG.Data;
using VARG.Inventory;
using VARG.Mission;

namespace VARG.Core
{
    public class GameManager : Singleton<GameManager>
    {
        private const string BootSceneName = "00_Boot";
        private const string BriefingSceneName = "01_BossBriefing";
        private const string ShipFlightSceneName = "02_ShipFlight";
        private const string CityHuntSceneName = "03_CityHunt";
        private const string ShipSortingSceneName = "04_ShipSorting";
        private const string ResultsSceneName = "05_Results";

        [SerializeField] private CampaignConfig _campaignConfig;
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private ScoringConfig _scoringConfig;
        [SerializeField] private PenaltyConfig _penaltyConfig;

        public const int MaxLevel = 10;
        public int CurrentLevel { get; private set; } = 1;
        public LevelResult LastResult { get; private set; }
        public bool IsVictory { get; private set; }
        public DefeatReason? LastDefeatReason { get; private set; }

        private LevelConfig _customSpreadsheetLevelConfig;
        public bool IsCustomSpreadsheetLevel => _customSpreadsheetLevelConfig != null;

        public void SetCustomSpreadsheetLevel(LevelConfig customConfig) => _customSpreadsheetLevelConfig = customConfig;
        public void ClearCustomSpreadsheetLevel() => _customSpreadsheetLevelConfig = null;

        protected override void Awake()
        {
            base.Awake();
            if (_campaignConfig == null) _campaignConfig = Resources.Load<CampaignConfig>("Data/CampaignConfig");
            if (_gameConfig == null) _gameConfig = Resources.Load<GameConfig>("Data/GameConfig");
            if (_scoringConfig == null) _scoringConfig = Resources.Load<ScoringConfig>("Data/ScoringConfig");
            if (_penaltyConfig == null) _penaltyConfig = Resources.Load<PenaltyConfig>("Data/PenaltyConfig");
        }

        public LevelConfig GetCurrentLevelConfig()
        {
            if (_customSpreadsheetLevelConfig != null) return _customSpreadsheetLevelConfig;
            if (_campaignConfig == null || _campaignConfig.Levels == null || _campaignConfig.Levels.Length == 0) return null;
            var idx = Mathf.Clamp(CurrentLevel - 1, 0, _campaignConfig.Levels.Length - 1);
            return _campaignConfig.Levels[idx];
        }

        public void StartLevel()
        {
            IsVictory = false;
            LastDefeatReason = null;
            var config = GetCurrentLevelConfig();

            InventoryManager.Instance?.Clear();
            LivesManager.Instance?.ResetLives(_penaltyConfig != null ? _penaltyConfig.StartingLives : 5);
            TimerManager.Instance?.ResetTimers();

            if (MissionManager.Instance != null)
            {
                if (!IsCustomSpreadsheetLevel && config != null)
                {
                    MissionManager.Instance.GenerateMission(config);
                }
                else if (IsCustomSpreadsheetLevel)
                {
                    MissionManager.Instance.ResetProgress();
                }
            }

            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(BriefingSceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(BriefingSceneName);
        }

        public void GoToHunt()
        {
            var targetScene = (_gameConfig != null && _gameConfig.UseShipFlight) ? ShipFlightSceneName : CityHuntSceneName;
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(targetScene);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }

        public void GoToSorting()
        {
            TimerManager.Instance?.StopTimer(TimerPhase.Capture);
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(ShipSortingSceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(ShipSortingSceneName);
        }

        public void Victory()
        {
            TimerManager.Instance?.StopAllTimers();
            IsVictory = true;
            LastDefeatReason = null;
            CalculateAndStoreResult();
            GameEvents.TriggerVictory();
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(ResultsSceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(ResultsSceneName);
        }

        public void Defeat(DefeatReason reason)
        {
            TimerManager.Instance?.StopAllTimers();
            IsVictory = false;
            LastDefeatReason = reason;
            CalculateAndStoreResult();
            GameEvents.TriggerDefeat(reason);
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(ResultsSceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(ResultsSceneName);
        }

        public void NextLevel()
        {
            ClearCustomSpreadsheetLevel();
            if (CurrentLevel >= MaxLevel)
            {
                RestartCampaign();
            }
            else
            {
                CurrentLevel++;
                StartLevel();
            }
        }

        public void RestartCampaign()
        {
            ClearCustomSpreadsheetLevel();
            CurrentLevel = 1;
            StartLevel();
        }

        public void RetryLevel() => StartLevel();

        public void GoToMainMenu()
        {
            ClearCustomSpreadsheetLevel();
            CurrentLevel = 1;
            TimerManager.Instance?.StopAllTimers();
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(BootSceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(BootSceneName);
        }

        private void CalculateAndStoreResult()
        {
            var capTime = TimerManager.Instance != null ? TimerManager.Instance.GetRemaining(TimerPhase.Capture) : 0f;
            var sortTime = TimerManager.Instance != null ? TimerManager.Instance.GetRemaining(TimerPhase.Sorting) : 0f;
            var lives = LivesManager.Instance != null ? LivesManager.Instance.Lives : 0;

            var errors = 0;
            if (InventoryManager.Instance != null)
            {
                for (var i = 0; i < InventoryManager.Instance.Items.Count; i++)
                {
                    if (InventoryManager.Instance.Items[i].Result != CaptureResult.Correct) errors++;
                }
            }

            var pps = _scoringConfig != null ? _scoringConfig.PointsPerSecond : 10;
            var ppl = _scoringConfig != null ? _scoringConfig.PointsPerLife : 100;
            var score = Mathf.Max(0, Mathf.RoundToInt((capTime + sortTime) * pps) + lives * ppl);

            LastResult = new LevelResult(capTime, sortTime, lives, errors, score);
        }
    }
}
