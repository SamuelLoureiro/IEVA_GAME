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

            GUILayout.BeginArea(new Rect(540, 10, 220, 180), "Vidas Debug (B04)", GUI.skin.window);

            GUILayout.Label($"Vidas Restantes: {LivesManager.Instance.Lives}");

            if (GUILayout.Button("Perder 1 Vida (-1)"))
            {
                LivesManager.Instance.LoseLife();
            }

            if (GUILayout.Button("Perder Todas as Vidas (Zerar)"))
            {
                while (LivesManager.Instance.Lives > 0)
                {
                    LivesManager.Instance.LoseLife();
                }
            }

            if (GUILayout.Button("Resetar Vidas (5)"))
            {
                LivesManager.Instance.ResetLives(5);
            }

            GUILayout.EndArea();
        }
    }
}
