#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VARG.Core;
using VARG.Data;
using VARG.UI;

namespace VARG.Editor
{
    public static class ResultsScreenVerification
    {
        [MenuItem("VARG/Verify A13 - Results Screen Logic")]
        public static void RunVerification()
        {
            var passedCount = 0;
            var totalCount = 4;

            Debug.Log("[Verification A13] Starting ResultsScreen verification...");

            // Test 1: Check scene structure in 05_Results
            ResultsSceneBuilder.BuildScene();
            var resultsScreen = Object.FindFirstObjectByType<ResultsScreen>();

            if (resultsScreen != null)
            {
                passedCount++;
                Debug.Log("[Verification A13] Test 1 PASSED: Found ResultsScreen in 05_Results scene.");
            }
            else
            {
                Debug.LogError("[Verification A13] Test 1 FAILED: ResultsScreen not found in 05_Results.");
            }

            // Create temporary test environment in memory for logic tests
            var testRoot = new GameObject("Test_Results_Root");

            try
            {
                var titleGo = new GameObject("Title");
                titleGo.transform.SetParent(testRoot.transform);
                var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();

                var scoreGo = new GameObject("Score");
                scoreGo.transform.SetParent(testRoot.transform);
                var scoreTmp = scoreGo.AddComponent<TextMeshProUGUI>();

                var nextBtnGo = new GameObject("NextBtn");
                nextBtnGo.transform.SetParent(testRoot.transform);
                var nextBtn = nextBtnGo.AddComponent<Button>();

                var retryBtnGo = new GameObject("RetryBtn");
                retryBtnGo.transform.SetParent(testRoot.transform);
                var retryBtn = retryBtnGo.AddComponent<Button>();

                var barGo = new GameObject("Bar");
                barGo.transform.SetParent(testRoot.transform);
                var barImg = barGo.AddComponent<Image>();
                barImg.type = Image.Type.Filled;

                var testScreen = testRoot.AddComponent<ResultsScreen>();
                var serialized = new SerializedObject(testScreen);
                serialized.FindProperty("_titleText").objectReferenceValue = titleTmp;
                serialized.FindProperty("_scoreText").objectReferenceValue = scoreTmp;
                serialized.FindProperty("_nextLevelButton").objectReferenceValue = nextBtn;
                serialized.FindProperty("_retryButton").objectReferenceValue = retryBtn;
                serialized.FindProperty("_livesBar").objectReferenceValue = barImg;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                // Test 2: Victory display (Sample default result)
                testScreen.DisplayResults();

                if (titleTmp.text.Contains("COMPLETE") && nextBtnGo.activeSelf && retryBtnGo.activeSelf)
                {
                    passedCount++;
                    Debug.Log($"[Verification A13] Test 2 PASSED: Victory display shows '{titleTmp.text}' with Next Level active.");
                }
                else
                {
                    Debug.LogError($"[Verification A13] Test 2 FAILED: Title='{titleTmp.text}', NextActive={nextBtnGo.activeSelf}");
                }

                // Test 3: Defeat display (Remaining lives = 0)
                var gameManagerGo = new GameObject("Test_GameManager");
                gameManagerGo.transform.SetParent(testRoot.transform);
                var gm = gameManagerGo.AddComponent<GameManager>();

                // Trigger defeat to populate LastResult with 0 lives
                gm.Defeat(DefeatReason.NoLives);
                testScreen.DisplayResults();

                if (titleTmp.text.Contains("FAILED") && !nextBtnGo.activeSelf && retryBtnGo.activeSelf)
                {
                    passedCount++;
                    Debug.Log($"[Verification A13] Test 3 PASSED: Defeat display shows '{titleTmp.text}' with Next Level disabled.");
                }
                else
                {
                    Debug.LogError($"[Verification A13] Test 3 FAILED: Title='{titleTmp.text}', NextActive={nextBtnGo.activeSelf}");
                }

                // Test 4: Chart bar calculation
                gm.Victory(); // Resets with positive result
                testScreen.DisplayResults();
                var fillAmount = barImg.fillAmount;

                if (fillAmount > 0f && fillAmount <= 1f)
                {
                    passedCount++;
                    Debug.Log($"[Verification A13] Test 4 PASSED: Chart bar calculated successfully (fillAmount={fillAmount:F2}).");
                }
                else
                {
                    Debug.LogError($"[Verification A13] Test 4 FAILED: fillAmount={fillAmount}");
                }
            }
            finally
            {
                Object.DestroyImmediate(testRoot);
            }

            Debug.Log($"[Verification A13] Summary: {passedCount}/{totalCount} tests PASSED.");
            if (passedCount == totalCount)
            {
                Debug.Log(">>> [A13 VERIFICATION SUCCESS] All criteria satisfied! <<<");
            }
            else
            {
                Debug.LogError(">>> [A13 VERIFICATION FAILURE] Some tests failed. <<<");
            }
        }
    }
}
#endif
