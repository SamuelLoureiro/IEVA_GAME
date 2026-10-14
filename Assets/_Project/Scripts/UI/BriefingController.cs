using TMPro;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Mission;

namespace VARG.UI
{
    public class BriefingController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private DialogueBox _dialogueBox;
        [SerializeField] private TargetShowcaseUI _showcaseUI;
        [SerializeField] private TextMeshProUGUI _levelTitleText;
        [SerializeField] private GameObject _bossAvatar;

        [Header("Settings")]
        [SerializeField] private float _secondsPerItem = 3.0f;
        [SerializeField] private float _minMemoryDuration = 8.0f;
        [SerializeField] private string _bossSpeakerName = "COMMANDER ZORP";

        private void Start()
        {
            InitializeBriefing();
        }

        public void Configure(DialogueBox dialogueBox, TargetShowcaseUI showcaseUI, TextMeshProUGUI levelTitleText, GameObject bossAvatar = null)
        {
            _dialogueBox = dialogueBox;
            _showcaseUI = showcaseUI;
            _levelTitleText = levelTitleText;
            _bossAvatar = bossAvatar;
        }

        private void InitializeBriefing()
        {
            var currentLevel = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1;
            var levelConfig = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevelConfig() : null;

            if (MissionManager.Instance != null && MissionManager.Instance.RequiredCount == 0)
            {
                MissionManager.Instance.GenerateMission(levelConfig);
            }

            if (_levelTitleText != null)
            {
                _levelTitleText.text = $"CAPTURE MISSION  |  LEVEL {currentLevel}";
            }

            if (_showcaseUI != null)
            {
                _showcaseUI.gameObject.SetActive(false);
            }

            var openingLine = GetBossOpeningLine(currentLevel);

            if (_dialogueBox != null)
            {
                _dialogueBox.Say(_bossSpeakerName, openingLine, OnBossFinishedSpeaking);
            }
            else
            {
                OnBossFinishedSpeaking();
            }
        }

        private string GetBossOpeningLine(int level)
        {
            if (level == 1)
            {
                return "Attention, rookie! Here are the targets you must capture on Earth and their exact order. Memorize the sequence before deploying the ship! Do NOT capture the forbidden target under any circumstances!";
            }

            return $"New mission authorized for Level {level}! The target quota has increased. Memorize the sequence carefully and don't waste my time!";
        }

        private void OnBossFinishedSpeaking()
        {
            StartCoroutine(TransitionBossToCornerRoutine());
        }

        private System.Collections.IEnumerator TransitionBossToCornerRoutine()
        {
            if (_bossAvatar != null)
            {
                var rect = _bossAvatar.GetComponent<RectTransform>();
                if (rect != null)
                {
                    var startPos = rect.anchoredPosition;
                    var targetPos = new Vector2(-620f, -160f);
                    var startScale = rect.localScale;
                    var targetScale = new Vector3(0.55f, 0.55f, 1f);
                    var duration = 0.55f;
                    var elapsed = 0f;

                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                        rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                        rect.localScale = Vector3.Lerp(startScale, targetScale, t);
                        yield return null;
                    }

                    rect.anchoredPosition = targetPos;
                    rect.localScale = targetScale;
                }
            }

            if (_dialogueBox != null)
            {
                var forbiddenStr = MissionManager.Instance != null ? MissionManager.Instance.ForbiddenType.ToString().ToUpper() : "HUMAN";
                var prompt = $"Here is your target sequence. Memorize the exact order! Watch out: {forbiddenStr} is FORBIDDEN!";
                _dialogueBox.Say(_bossSpeakerName, prompt, OnStage2Finished);
            }

            if (_showcaseUI != null)
            {
                var order = MissionManager.Instance != null ? MissionManager.Instance.RequiredOrder : null;
                _showcaseUI.DisplayShowcase(order);
            }
        }

        private void OnStage2Finished()
        {
            var levelConfig = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevelConfig() : null;
            var count = MissionManager.Instance != null && MissionManager.Instance.RequiredCount > 0
                ? MissionManager.Instance.RequiredCount
                : (levelConfig != null ? levelConfig.ListSize : 3);
            var memoryDuration = Mathf.Max(_minMemoryDuration, count * _secondsPerItem);

            if (_dialogueBox != null)
            {
                _dialogueBox.Say(_bossSpeakerName, "Memorize the sequence now! Deploy when ready, or time will run out!", null);
            }

            if (_showcaseUI != null)
            {
                _showcaseUI.StartCountdown(memoryDuration, ProceedToHunt);
            }
        }

        public void ProceedToHunt()
        {
            if (_showcaseUI != null)
            {
                _showcaseUI.HideShowcase();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToHunt();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load("03_CityHunt");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("03_CityHunt");
            }
        }
    }
}
