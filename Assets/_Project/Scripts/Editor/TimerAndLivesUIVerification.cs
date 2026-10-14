#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Mission;
using VARG.UI;

namespace VARG.Editor
{
    public static class TimerAndLivesUIVerification
    {
        [MenuItem("VARG/Verify A11-A12 - Timer and Lives UI Logic")]
        public static void RunVerification()
        {
            var passedCount = 0;
            var totalCount = 5;

            Debug.Log("[Verification A11-A12] Starting Timer, Lives, and Popups verification...");

            // Test 1: Check scene structure in 03_CityHunt
            CityHuntGreyboxBuilder.SetupHuntHUDOnly();
            var timerUi = Object.FindFirstObjectByType<TimerUI>();
            var livesUi = Object.FindFirstObjectByType<LivesUI>();
            var huntUi = Object.FindFirstObjectByType<HuntInventoryUI>();

            if (timerUi != null && livesUi != null && huntUi != null)
            {
                passedCount++;
                Debug.Log("[Verification A11-A12] Test 1 PASSED: Found TimerUI, LivesUI, and HuntInventoryUI in HuntHUD Canvas.");
            }
            else
            {
                Debug.LogError($"[Verification A11-A12] Test 1 FAILED: timerUi: {timerUi != null}, livesUi: {livesUi != null}, huntUi: {huntUi != null}");
            }

            // Create temporary test environment in memory for logic tests
            var testRoot = new GameObject("Test_HUD_Root");

            try
            {
                // Setup Managers
                var timerManagerGo = new GameObject("Test_TimerManager");
                timerManagerGo.transform.SetParent(testRoot.transform);
                var timerManager = timerManagerGo.AddComponent<TimerManager>();

                var livesManagerGo = new GameObject("Test_LivesManager");
                livesManagerGo.transform.SetParent(testRoot.transform);
                var livesManager = livesManagerGo.AddComponent<LivesManager>();

                // Setup UI test objects
                var timerTextGo = new GameObject("Test_TimerText");
                timerTextGo.transform.SetParent(testRoot.transform);
                var timerTmp = timerTextGo.AddComponent<TextMeshProUGUI>();

                var debtTextGo = new GameObject("Test_DebtText");
                debtTextGo.transform.SetParent(testRoot.transform);
                var debtTmp = debtTextGo.AddComponent<TextMeshProUGUI>();

                var testTimerUi = testRoot.AddComponent<TimerUI>();
                var serializedTimer = new SerializedObject(testTimerUi);
                serializedTimer.FindProperty("_timerText").objectReferenceValue = timerTmp;
                serializedTimer.FindProperty("_debtText").objectReferenceValue = debtTmp;
                serializedTimer.FindProperty("_lowTimeThreshold").floatValue = 15f;
                serializedTimer.FindProperty("_autoStartCaptureTimer").boolValue = false;
                serializedTimer.ApplyModifiedPropertiesWithoutUndo();

                // Test 2: Timer formatting and low time (<15s)
                timerManager.StartTimer(TimerPhase.Capture, 90f);
                testTimerUi.RefreshAll();
                var textAt90 = timerTmp.text;

                var methodHandleTime = typeof(TimerUI).GetMethod("HandleTimeChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                methodHandleTime?.Invoke(testTimerUi, new object[] { TimerPhase.Capture, 14f });
                var textAt14 = timerTmp.text;
                var colorAt14 = timerTmp.color;

                if (textAt90 == "01:30" && textAt14 == "00:14" && colorAt14 != Color.white)
                {
                    passedCount++;
                    Debug.Log($"[Verification A11-A12] Test 2 PASSED: 90s='{textAt90}', 14s='{textAt14}' with low time warning color.");
                }
                else
                {
                    Debug.LogError($"[Verification A11-A12] Test 2 FAILED: 90s='{textAt90}', 14s='{textAt14}', color={colorAt14}");
                }

                // Test 3: Lives reduction and display
                livesManager.ResetLives(5);
                var livesTextGo = new GameObject("Test_LivesText");
                livesTextGo.transform.SetParent(testRoot.transform);
                var livesTmp = livesTextGo.AddComponent<TextMeshProUGUI>();

                var testLivesUi = testRoot.AddComponent<LivesUI>();
                var serializedLives = new SerializedObject(testLivesUi);
                serializedLives.FindProperty("_livesText").objectReferenceValue = livesTmp;
                serializedLives.ApplyModifiedPropertiesWithoutUndo();

                testLivesUi.RefreshLivesDisplay();
                var textAt5Lives = livesTmp.text;

                livesManager.LoseLife();
                testLivesUi.RefreshLivesDisplay();
                var textAt4Lives = livesTmp.text;

                if (textAt5Lives == "Lives: 5" && textAt4Lives == "Lives: 4")
                {
                    passedCount++;
                    Debug.Log($"[Verification A11-A12] Test 3 PASSED: Lives correctly updated from '{textAt5Lives}' to '{textAt4Lives}'.");
                }
                else
                {
                    Debug.LogError($"[Verification A11-A12] Test 3 FAILED: Initial='{textAt5Lives}', After loss='{textAt4Lives}'");
                }

                // Test 4: Sorting debt display on Hunt HUD
                timerManager.ApplyPenalty(TimerPhase.Sorting, 10f);
                testTimerUi.RefreshDebtDisplay();

                if (timerManager.SortingDebt == 10f && debtTmp.text.Contains("-10s") && debtTextGo.activeSelf)
                {
                    passedCount++;
                    Debug.Log($"[Verification A11-A12] Test 4 PASSED: Sorting debt correctly displayed on Hunt HUD ('{debtTmp.text}').");
                }
                else
                {
                    Debug.LogError($"[Verification A11-A12] Test 4 FAILED: Debt={timerManager.SortingDebt}, Text='{debtTmp.text}', Active={debtTextGo.activeSelf}");
                }

                // Test 5: A12 Lifecycle - Auto start timer and stop
                timerManager.StopAllTimers();
                var autoStartMethod = typeof(TimerUI).GetMethod("InitializeTimerIfNeeded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                serializedTimer.FindProperty("_autoStartCaptureTimer").boolValue = true;
                serializedTimer.FindProperty("_defaultCaptureDuration").floatValue = 75f;
                serializedTimer.ApplyModifiedPropertiesWithoutUndo();

                autoStartMethod?.Invoke(testTimerUi, null);
                var wasStarted = timerManager.IsRunning(TimerPhase.Capture);
                var remainingDuration = timerManager.GetRemaining(TimerPhase.Capture);

                timerManager.StopTimer(TimerPhase.Capture);
                var wasStopped = !timerManager.IsRunning(TimerPhase.Capture);

                if (wasStarted && remainingDuration == 75f && wasStopped)
                {
                    passedCount++;
                    Debug.Log($"[Verification A11-A12] Test 5 PASSED: Capture Timer auto-started at {remainingDuration}s and stopped cleanly.");
                }
                else
                {
                    Debug.LogError($"[Verification A11-A12] Test 5 FAILED: wasStarted={wasStarted}, duration={remainingDuration}, wasStopped={wasStopped}");
                }
            }
            finally
            {
                Object.DestroyImmediate(testRoot);
            }

            Debug.Log($"[Verification A11-A12] Summary: {passedCount}/{totalCount} tests PASSED.");
            if (passedCount == totalCount)
            {
                Debug.Log(">>> [A11-A12 VERIFICATION SUCCESS] All criteria satisfied! <<<");
            }
            else
            {
                Debug.LogError(">>> [A11-A12 VERIFICATION FAILURE] Some tests failed. <<<");
            }
        }
    }
}
#endif
