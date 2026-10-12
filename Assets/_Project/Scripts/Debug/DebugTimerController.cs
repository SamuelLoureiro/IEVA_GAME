using UnityEngine;
using VARG.Data;
using VARG.Mission;

namespace VARG.DebugTools
{
    public class DebugTimerController : MonoBehaviour
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
            if (TimerManager.Instance == null) return;

            GUILayout.BeginArea(new Rect(260, 10, 270, 420), "Timer Debug (B03)", GUI.skin.window);

            var captureRem = TimerManager.Instance.GetRemaining(TimerPhase.Capture);
            var sortingRem = TimerManager.Instance.GetRemaining(TimerPhase.Sorting);
            var debt = TimerManager.Instance.SortingDebt;
            var isCapRunning = TimerManager.Instance.IsRunning(TimerPhase.Capture);
            var isSortRunning = TimerManager.Instance.IsRunning(TimerPhase.Sorting);

            GUILayout.Label($"Captura: {captureRem:F1}s {(isCapRunning ? "[RODANDO]" : "[PARADO]")}");
            GUILayout.Label($"Triagem: {sortingRem:F1}s {(isSortRunning ? "[RODANDO]" : "[PARADO]")}");
            GUILayout.Label($"Dívida Triagem: {debt:F1}s");

            if (GUILayout.Button("Iniciar Captura (90s)"))
            {
                TimerManager.Instance.StartTimer(TimerPhase.Capture, 90f);
            }

            if (GUILayout.Button("Parar Captura (Simula Feixe da Nave)"))
            {
                TimerManager.Instance.StopTimer(TimerPhase.Capture);
            }

            if (GUILayout.Button("Penalidade Captura (-10s)"))
            {
                TimerManager.Instance.ApplyPenalty(TimerPhase.Capture, 10f);
            }

            if (GUILayout.Button("Penalidade Triagem (-10s / Dívida)"))
            {
                TimerManager.Instance.ApplyPenalty(TimerPhase.Sorting, 10f);
            }

            if (GUILayout.Button("Iniciar Triagem (45s)"))
            {
                TimerManager.Instance.StartTimer(TimerPhase.Sorting, 45f);
            }

            if (GUILayout.Button("Parar Triagem (Simula Conclusão)"))
            {
                TimerManager.Instance.StopTimer(TimerPhase.Sorting);
            }

            if (GUILayout.Button("Simular Tempo Crítico (Pular para 10s)"))
            {
                var activePhase = TimerManager.Instance.IsRunning(TimerPhase.Capture) ? TimerPhase.Capture : TimerPhase.Sorting;
                var current = TimerManager.Instance.GetRemaining(activePhase);
                if (current > 10f)
                {
                    TimerManager.Instance.ApplyPenalty(activePhase, current - 10f);
                }
            }

            if (GUILayout.Button("Simular Tempo Esgotado (Zerar)"))
            {
                var activePhase = TimerManager.Instance.IsRunning(TimerPhase.Capture) ? TimerPhase.Capture : TimerPhase.Sorting;
                TimerManager.Instance.ApplyPenalty(activePhase, 9999f);
            }

            if (GUILayout.Button("Pausar / Despausar"))
            {
                TimerManager.Instance.Pause(!TimerManager.Instance.IsPaused);
            }

            if (GUILayout.Button("Resetar Dívida"))
            {
                TimerManager.Instance.ResetDebt();
            }

            GUILayout.EndArea();
        }
    }
}
