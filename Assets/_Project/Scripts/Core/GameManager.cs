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

        private LevelConfig _runtimeLevelConfig;
        private int _cachedRuntimeLevel = -1;
        private LevelConfig _customSpreadsheetLevelConfig;

        public bool IsCustomSpreadsheetLevel => _customSpreadsheetLevelConfig != null;

        public void SetCustomSpreadsheetLevel(LevelConfig customConfig)
        {
            if (_customSpreadsheetLevelConfig != null && _customSpreadsheetLevelConfig != customConfig)
            {
                Destroy(_customSpreadsheetLevelConfig);
            }
            _customSpreadsheetLevelConfig = customConfig;
        }

        public void ClearCustomSpreadsheetLevel()
        {
            if (_customSpreadsheetLevelConfig != null)
            {
                Destroy(_customSpreadsheetLevelConfig);
                _customSpreadsheetLevelConfig = null;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            LoadDefaultConfigsIfMissing();
        }

        protected override void OnDestroy()
        {
            if (_runtimeLevelConfig != null)
            {
                Destroy(_runtimeLevelConfig);
                _runtimeLevelConfig = null;
            }

            if (_customSpreadsheetLevelConfig != null)
            {
                Destroy(_customSpreadsheetLevelConfig);
                _customSpreadsheetLevelConfig = null;
            }

            base.OnDestroy();
        }

        private void LoadDefaultConfigsIfMissing()
        {
            if (_campaignConfig == null)
            {
                _campaignConfig = Resources.Load<CampaignConfig>("Data/CampaignConfig");
            }

            if (_gameConfig == null)
            {
                _gameConfig = Resources.Load<GameConfig>("Data/GameConfig");
            }

            if (_scoringConfig == null)
            {
                _scoringConfig = Resources.Load<ScoringConfig>("Data/ScoringConfig");
            }

            if (_penaltyConfig == null)
            {
                _penaltyConfig = Resources.Load<PenaltyConfig>("Data/PenaltyConfig");
            }
        }

        public LevelConfig GetCurrentLevelConfig()
        {
            if (_customSpreadsheetLevelConfig != null)
            {
                return _customSpreadsheetLevelConfig;
            }

            if (_campaignConfig == null || _campaignConfig.Levels == null || _campaignConfig.Levels.Length == 0)
            {
                return null;
            }

            if (CurrentLevel <= _campaignConfig.Levels.Length)
            {
                return _campaignConfig.Levels[CurrentLevel - 1];
            }

            if (_runtimeLevelConfig != null && _cachedRuntimeLevel == CurrentLevel)
            {
                return _runtimeLevelConfig;
            }

            var lastConfig = _campaignConfig.Levels[_campaignConfig.Levels.Length - 1];
            var clampedLevel = Mathf.Min(CurrentLevel, MaxLevel);
            var extraLevels = clampedLevel - _campaignConfig.Levels.Length;
            var forbiddenSpawns = Mathf.Min(lastConfig.ForbiddenSpawnCount + (extraLevels / 2), 5);

            if (_runtimeLevelConfig == null)
            {
                _runtimeLevelConfig = ScriptableObject.CreateInstance<LevelConfig>();
            }

            _runtimeLevelConfig.Setup(
                lastConfig.ListSize + extraLevels,
                lastConfig.CaptureTime + (extraLevels * 10f),
                lastConfig.SortingTime + (extraLevels * 10f),
                lastConfig.ExtraSpawnsPerRequired,
                forbiddenSpawns,
                lastConfig.FleeSpeedMultiplier
            );
            _cachedRuntimeLevel = CurrentLevel;

            return _runtimeLevelConfig;
        }

        public void StartLevel()
        {
            IsVictory = false;
            LastDefeatReason = null;
            var levelConfig = GetCurrentLevelConfig();

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Clear();
            }

            if (LivesManager.Instance != null)
            {
                var startingLives = _penaltyConfig != null ? _penaltyConfig.StartingLives : 5;
                LivesManager.Instance.ResetLives(startingLives);
            }

            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.ResetTimers();
            }

            if (MissionManager.Instance != null && levelConfig != null && !IsCustomSpreadsheetLevel)
            {
                MissionManager.Instance.GenerateMission(levelConfig);
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load(BriefingSceneName);
            }
        }

        public void GoToHunt()
        {
            var useShipFlight = _gameConfig != null && _gameConfig.UseShipFlight;
            var targetScene = useShipFlight ? ShipFlightSceneName : CityHuntSceneName;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load(targetScene);
            }
        }

        public void GoToSorting()
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.StopTimer(TimerPhase.Capture);
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load(ShipSortingSceneName);
            }
        }

        public void Victory()
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.StopAllTimers();
            }

            IsVictory = true;
            LastDefeatReason = null;
            CalculateAndStoreResult();
            GameEvents.TriggerVictory();

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load(ResultsSceneName);
            }
        }

        public void Defeat(DefeatReason reason)
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.StopAllTimers();
            }

            IsVictory = false;
            LastDefeatReason = reason;
            CalculateAndStoreResult();
            GameEvents.TriggerDefeat(reason);

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load(ResultsSceneName);
            }
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

        public void RetryLevel()
        {
            StartLevel();
        }

        public void GoToMainMenu()
        {
            ClearCustomSpreadsheetLevel();
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.StopAllTimers();
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load(BootSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(BootSceneName);
            }
        }

        private void CalculateAndStoreResult()
        {
            var remainingCapture = 0f;
            var remainingSorting = 0f;

            if (TimerManager.Instance != null)
            {
                remainingCapture = TimerManager.Instance.GetRemaining(TimerPhase.Capture);
                remainingSorting = TimerManager.Instance.GetRemaining(TimerPhase.Sorting);
            }

            var remainingLives = LivesManager.Instance != null ? LivesManager.Instance.Lives : 0;
            var errorsCount = 0;

            if (InventoryManager.Instance != null)
            {
                foreach (var item in InventoryManager.Instance.Items)
                {
                    if (item.Result != CaptureResult.Correct)
                    {
                        errorsCount++;
                    }
                }
            }

            var pointsPerSecond = _scoringConfig != null ? _scoringConfig.PointsPerSecond : 10;
            var pointsPerLife = _scoringConfig != null ? _scoringConfig.PointsPerLife : 100;

            var timeScore = Mathf.RoundToInt((remainingCapture + remainingSorting) * pointsPerSecond);
            var livesScore = remainingLives * pointsPerLife;
            var totalScore = Mathf.Max(0, timeScore + livesScore);

            LastResult = new LevelResult(
                remainingCapture,
                remainingSorting,
                remainingLives,
                errorsCount,
                totalScore
            );
        }
    }
}
