#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using VARG.Data;
using VARG.Targets;

namespace VARG.Editor
{
    public static class TargetAIVerification
    {
        [MenuItem("VARG/Verify A07 - Target AI Logic")]
        public static void RunVerification()
        {
            var passedCount = 0;
            var totalCount = 4;

            // Test 1: Verify all 4 Target prefabs contain TargetAI and NavMeshAgent
            var types = new[] { "Chicken", "Cow", "Pig", "Human" };
            var allPrefabsValid = true;
            foreach (var type in types)
            {
                var path = $"Assets/_Project/Prefabs/Targets/Target_{type}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError($"[Verification] Prefab missing at {path}");
                    allPrefabsValid = false;
                    continue;
                }

                var ai = prefab.GetComponent<TargetAI>();
                var agent = prefab.GetComponent<NavMeshAgent>();
                var target = prefab.GetComponent<Target>();
                var shrinkable = prefab.GetComponent<Shrinkable>();

                if (ai == null || agent == null || target == null || shrinkable == null)
                {
                    Debug.LogError($"[Verification] Missing required components on {prefab.name}");
                    allPrefabsValid = false;
                }
            }

            if (allPrefabsValid)
            {
                passedCount++;
                Debug.Log("[Verification] Test 1 PASSED: All 4 Target prefabs have TargetAI, NavMeshAgent, Target, and Shrinkable.");
            }

            // Test 2: Verify TargetAI state transitions on a runtime instance
            var testGo = new GameObject("Test_TargetAI_Runner");
            var testAgent = testGo.AddComponent<NavMeshAgent>();
            var testTarget = testGo.AddComponent<Target>();
            var targetData = ScriptableObject.CreateInstance<TargetData>();
            var serializedData = new SerializedObject(targetData);
            serializedData.FindProperty("_walkSpeed").floatValue = 2.5f;
            serializedData.FindProperty("_fleeSpeed").floatValue = 5.5f;
            serializedData.ApplyModifiedPropertiesWithoutUndo();
            testTarget.Data = targetData;

            var testAi = testGo.AddComponent<TargetAI>();
            var testAiSerialized = new SerializedObject(testAi);
            testAiSerialized.FindProperty("_agent").objectReferenceValue = testAgent;
            testAiSerialized.FindProperty("_target").objectReferenceValue = testTarget;
            testAiSerialized.ApplyModifiedPropertiesWithoutUndo();

            // Simulate Awake & Start logic
            var initialSpeed = testAgent.speed;
            testAi.AlertDanger(Vector3.forward * 10f);
            var fleeStatePassed = testAi.CurrentState == TargetAIState.Flee && testAgent.speed >= 5.5f;

            if (fleeStatePassed)
            {
                passedCount++;
                Debug.Log($"[Verification] Test 2 PASSED: TargetAI correctly enters Flee state and increases speed to {testAgent.speed}.");
            }
            else
            {
                Debug.LogError($"[Verification] Test 2 FAILED: State is {testAi.CurrentState}, speed is {testAgent.speed}");
            }

            // Test 3: Verify SetCaptured disables AI and NavMeshAgent
            testAi.SetCaptured();
            var capturedStatePassed = testAi.CurrentState == TargetAIState.Captured && !testAgent.enabled;
            if (capturedStatePassed)
            {
                passedCount++;
                Debug.Log("[Verification] Test 3 PASSED: TargetAI enters Captured state and agent is disabled.");
            }
            else
            {
                Debug.LogError($"[Verification] Test 3 FAILED: State is {testAi.CurrentState}, agent enabled: {testAgent.enabled}");
            }

            // Test 4: Verify ResetState restores agent and state
            testAi.ResetState();
            var resetStatePassed = (testAi.CurrentState == TargetAIState.Idle || testAi.CurrentState == TargetAIState.Wander) && testAgent.enabled;
            if (resetStatePassed)
            {
                passedCount++;
                Debug.Log("[Verification] Test 4 PASSED: ResetState restores agent and state to Idle/Wander.");
            }
            else
            {
                Debug.LogError($"[Verification] Test 4 FAILED: State is {testAi.CurrentState}, agent enabled: {testAgent.enabled}");
            }

            Object.DestroyImmediate(testGo);
            Object.DestroyImmediate(targetData);

            if (passedCount == totalCount)
            {
                Debug.Log($"<color=green>[Verification] ALL {totalCount} TESTS PASSED for A07 TargetAI!</color>");
            }
            else
            {
                Debug.LogError($"[Verification] {passedCount}/{totalCount} tests passed.");
            }
        }
    }
}
#endif
