using UnityEngine;
using VARG.Data;
using VARG.Mission;

namespace VARG.DebugTools
{
    public class DebugTimerController : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private bool _showGui = false;
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

            GUILayout.Label($"Capture: {captureRem:F1}s {(isCapRunning ? "[RUNNING]" : "[STOPPED]")}");
            GUILayout.Label($"Sorting: {sortingRem:F1}s {(isSortRunning ? "[RUNNING]" : "[STOPPED]")}");
            GUILayout.Label($"Sorting Debt: {debt:F1}s");

            if (GUILayout.Button("Start Capture (90s)"))
            {
                TimerManager.Instance.StartTimer(TimerPhase.Capture, 90f);
            }

            if (GUILayout.Button("Stop Capture (Simulate Ship Beam)"))
            {
                TimerManager.Instance.StopTimer(TimerPhase.Capture);
            }

            if (GUILayout.Button("Capture Penalty (-10s)"))
            {
                TimerManager.Instance.ApplyPenalty(TimerPhase.Capture, 10f);
            }

            if (GUILayout.Button("Sorting Penalty (-10s / Debt)"))
            {
                TimerManager.Instance.ApplyPenalty(TimerPhase.Sorting, 10f);
            }

            if (GUILayout.Button("Start Sorting (45s)"))
            {
                TimerManager.Instance.StartTimer(TimerPhase.Sorting, 45f);
            }

            if (GUILayout.Button("Stop Sorting (Simulate Conclusion)"))
            {
                TimerManager.Instance.StopTimer(TimerPhase.Sorting);
            }

            if (GUILayout.Button("Simulate Critical Time (Jump to 10s)"))
            {
                var activePhase = TimerManager.Instance.IsRunning(TimerPhase.Capture) ? TimerPhase.Capture : TimerPhase.Sorting;
                var current = TimerManager.Instance.GetRemaining(activePhase);
                if (current > 10f)
                {
                    TimerManager.Instance.ApplyPenalty(activePhase, current - 10f);
                }
            }

            if (GUILayout.Button("Simulate Time Out (Zero)"))
            {
                var activePhase = TimerManager.Instance.IsRunning(TimerPhase.Capture) ? TimerPhase.Capture : TimerPhase.Sorting;
                TimerManager.Instance.ApplyPenalty(activePhase, 9999f);
            }

            if (GUILayout.Button("Pause / Resume"))
            {
                TimerManager.Instance.Pause(!TimerManager.Instance.IsPaused);
            }

            if (GUILayout.Button("Reset Debt"))
            {
                TimerManager.Instance.ResetDebt();
            }

            GUILayout.EndArea();
        }
    }
}
