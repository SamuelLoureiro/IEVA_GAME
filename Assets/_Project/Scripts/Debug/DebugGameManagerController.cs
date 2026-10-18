using UnityEngine;
using VARG.Core;
using VARG.Data;

namespace VARG.DebugTools
{
    public class DebugGameManagerController : MonoBehaviour
    {
        [Header("Debug Controls")]
        [SerializeField] private bool _showGui = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.digit1Key.wasPressedThisFrame) GameManager.Instance.StartLevel();
            if (keyboard.digit2Key.wasPressedThisFrame) GameManager.Instance.GoToHunt();
            if (keyboard.digit3Key.wasPressedThisFrame) GameManager.Instance.GoToSorting();
            if (keyboard.digit4Key.wasPressedThisFrame) GameManager.Instance.Victory();
            if (keyboard.digit5Key.wasPressedThisFrame) GameManager.Instance.Defeat(DefeatReason.NoLives);
            if (keyboard.digit6Key.wasPressedThisFrame) GameManager.Instance.Defeat(DefeatReason.CaptureTimeUp);
            if (keyboard.digit7Key.wasPressedThisFrame) GameManager.Instance.NextLevel();
            if (keyboard.digit8Key.wasPressedThisFrame) GameManager.Instance.RetryLevel();
        }

        private void OnGUI()
        {
            if (!_showGui) return;
            if (GameManager.Instance == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 220, 280), "GameManager Debug (Click or 1-8)", GUI.skin.window);

            GUILayout.Label($"Current Level: {GameManager.Instance.CurrentLevel}");
            var currentCfg = GameManager.Instance.GetCurrentLevelConfig();
            if (currentCfg != null)
            {
                GUILayout.Label($"Items: {currentCfg.ListSize} | Forb: {currentCfg.ForbiddenSpawnCount}");
                GUILayout.Label($"Hunt: {currentCfg.CaptureTime}s | Sort: {currentCfg.SortingTime}s");
            }

            if (GUILayout.Button("1. StartLevel (Briefing)"))
            {
                GameManager.Instance.StartLevel();
            }

            if (GUILayout.Button("2. GoToHunt (City / Ship)"))
            {
                GameManager.Instance.GoToHunt();
            }

            if (GUILayout.Button("3. GoToSorting (Sorting)"))
            {
                GameManager.Instance.GoToSorting();
            }

            if (GUILayout.Button("4. Victory (Results)"))
            {
                GameManager.Instance.Victory();
            }

            if (GUILayout.Button("5. Defeat NoLives"))
            {
                GameManager.Instance.Defeat(DefeatReason.NoLives);
            }

            if (GUILayout.Button("6. Defeat TimeUp"))
            {
                GameManager.Instance.Defeat(DefeatReason.CaptureTimeUp);
            }

            if (GUILayout.Button("7. NextLevel"))
            {
                GameManager.Instance.NextLevel();
            }

            if (GUILayout.Button("8. RetryLevel"))
            {
                GameManager.Instance.RetryLevel();
            }

            GUILayout.EndArea();
        }
    }
}
