using UnityEngine;
using VARG.Mission;

namespace VARG.DebugTools
{
    public class DebugLivesController : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private bool _showGui = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnGUI()
        {
            if (!_showGui) return;
            if (LivesManager.Instance == null) return;

            GUILayout.BeginArea(new Rect(540, 10, 220, 180), "Debug Lives (B04)", GUI.skin.window);

            GUILayout.Label($"Remaining Lives: {LivesManager.Instance.Lives}");

            if (GUILayout.Button("Lose 1 Life (-1)"))
            {
                LivesManager.Instance.LoseLife();
            }

            if (GUILayout.Button("Lose All Lives (Zero)"))
            {
                while (LivesManager.Instance.Lives > 0)
                {
                    LivesManager.Instance.LoseLife();
                }
            }

            if (GUILayout.Button("Reset Lives (5)"))
            {
                LivesManager.Instance.ResetLives(5);
            }

            GUILayout.EndArea();
        }
    }
}
