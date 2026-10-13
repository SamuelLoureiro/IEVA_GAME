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
    public static class HuntInventoryUIVerification
    {
        [MenuItem("VARG/Verify A10 - Hunt Inventory UI Logic")]
        public static void RunVerification()
        {
            var passedCount = 0;
            var totalCount = 4;

            Debug.Log("[Verification A10] Starting HuntInventoryUI verification...");

            // Test 1: Check scene structure in 03_CityHunt
            CityHuntGreyboxBuilder.SetupHuntHUDOnly();
            var huntUi = Object.FindFirstObjectByType<HuntInventoryUI>();
            var tmp = huntUi != null ? huntUi.GetComponentInChildren<TextMeshProUGUI>() : null;

            if (huntUi != null && tmp != null)
            {
                passedCount++;
                Debug.Log("[Verification A10] Test 1 PASSED: Found HuntInventoryUI with TextMeshProUGUI in HuntHUD Canvas.");
            }
            else
            {
                Debug.LogError($"[Verification A10] Test 1 FAILED: huntUi: {huntUi != null}, tmp: {tmp != null}");
            }

            // Create temporary test environment in memory for logic tests
            var testRoot = new GameObject("Test_HuntUI_Root");

            try
            {
                // Setup MissionManager
                var missionGo = new GameObject("Test_MissionManager");
                missionGo.transform.SetParent(testRoot.transform);
                var missionManager = missionGo.AddComponent<MissionManager>();

                var levelConfig = ScriptableObject.CreateInstance<LevelConfig>();
                var serializedLevel = new SerializedObject(levelConfig);
                serializedLevel.FindProperty("_listSize").intValue = 3;
                serializedLevel.FindProperty("_extraSpawnsPerRequired").intValue = 0;
                serializedLevel.FindProperty("_forbiddenSpawnCount").intValue = 1;
                serializedLevel.ApplyModifiedPropertiesWithoutUndo();

                missionManager.GenerateMission(levelConfig);

                // Setup HuntInventoryUI in test GameObject
                var uiGo = new GameObject("Test_HuntUI");
                uiGo.transform.SetParent(testRoot.transform);
                var textGo = new GameObject("Test_Text");
                textGo.transform.SetParent(uiGo.transform);
                var testTmp = textGo.AddComponent<TextMeshProUGUI>();

                var testHuntUi = uiGo.AddComponent<HuntInventoryUI>();
                var serializedUi = new SerializedObject(testHuntUi);
                serializedUi.FindProperty("_progressText").objectReferenceValue = testTmp;
                serializedUi.FindProperty("_progressFormat").stringValue = "Targets: {0}/{1}";
                serializedUi.FindProperty("_completedFormat").stringValue = "Targets: {0}/{1} (Return to Ship!)";
                serializedUi.ApplyModifiedPropertiesWithoutUndo();

                // Test 2: Target progression format at 0/3 and 1/3
                testHuntUi.RefreshDisplay();
                var textAtStart = testTmp.text;

                var expectedTarget1 = missionManager.RequiredOrder[0];
                missionManager.ValidateCapture(expectedTarget1);
                testHuntUi.RefreshDisplay();
                var textAfterOne = testTmp.text;

                if (textAtStart == "Targets: 0/3" && textAfterOne == "Targets: 1/3")
                {
                    passedCount++;
                    Debug.Log($"[Verification A10] Test 2 PASSED: Initial format '{textAtStart}' and progress format '{textAfterOne}' match expected.");
                }
                else
                {
                    Debug.LogError($"[Verification A10] Test 2 FAILED: Start='{textAtStart}', AfterOne='{textAfterOne}'");
                }

                // Test 3: Complete mission format
                while (!missionManager.IsComplete)
                {
                    var expected = missionManager.RequiredOrder[missionManager.CurrentIndex];
                    missionManager.ValidateCapture(expected);
                }

                testHuntUi.RefreshDisplay();
                var textAtComplete = testTmp.text;

                if (textAtComplete == "Targets: 3/3 (Return to Ship!)")
                {
                    passedCount++;
                    Debug.Log($"[Verification A10] Test 3 PASSED: Completed format '{textAtComplete}' matches expected.");
                }
                else
                {
                    Debug.LogError($"[Verification A10] Test 3 FAILED: Got '{textAtComplete}', expected 'Targets: 3/3 (Return to Ship!)'");
                }

                // Test 4: Event-driven reactivity
                // Reset mission and test if event triggers update automatically without manual RefreshDisplay()
                missionManager.GenerateMission(levelConfig);
                testHuntUi.RefreshDisplay();
                var beforeEventText = testTmp.text;

                // ValidateCapture fires GameEvents.OnCorrectCapture which testHuntUi is subscribed to
                var nextExpected = missionManager.RequiredOrder[0];
                missionManager.ValidateCapture(nextExpected);
                var afterEventText = testTmp.text;

                if (beforeEventText == "Targets: 0/3" && afterEventText == "Targets: 1/3")
                {
                    passedCount++;
                    Debug.Log($"[Verification A10] Test 4 PASSED: Reactive update on GameEvents.OnCorrectCapture succeeded ('{afterEventText}').");
                }
                else
                {
                    Debug.LogError($"[Verification A10] Test 4 FAILED: Before='{beforeEventText}', After='{afterEventText}'");
                }
            }
            finally
            {
                Object.DestroyImmediate(testRoot);
            }

            Debug.Log($"[Verification A10] Summary: {passedCount}/{totalCount} tests PASSED.");
            if (passedCount == totalCount)
            {
                Debug.Log(">>> [A10 VERIFICATION SUCCESS] All criteria satisfied! <<<");
            }
            else
            {
                Debug.LogError(">>> [A10 VERIFICATION FAILURE] Some tests failed. <<<");
            }
        }
    }
}
#endif
