#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Mission;
using VARG.Player;
using VARG.Ship;

namespace VARG.Editor
{
    public static class ShipBeamZoneVerification
    {
        [MenuItem("VARG/Verify A09 - Ship Beam Zone Logic")]
        public static void RunVerification()
        {
            var passedCount = 0;
            var totalCount = 4;

            Debug.Log("[Verification A09] Starting ShipBeamZone verification...");

            // Test 1: Check scene structure in 03_CityHunt
            CityHuntGreyboxBuilder.SetupShipBeamZoneOnly();
            var beamZone = Object.FindFirstObjectByType<ShipBeamZone>();
            var triggerCollider = beamZone != null ? beamZone.GetComponent<Collider>() : null;

            if (beamZone != null && triggerCollider != null && triggerCollider.isTrigger)
            {
                passedCount++;
                Debug.Log("[Verification A09] Test 1 PASSED: Found ShipBeamZone with trigger collider in 03_CityHunt.");
            }
            else
            {
                Debug.LogError($"[Verification A09] Test 1 FAILED: beamZone: {beamZone != null}, trigger: {triggerCollider != null}");
            }

            // Create temporary test environment in memory for logic tests
            var testRoot = new GameObject("Test_ShipBeam_Root");

            try
            {
                // Prepare Managers
                var missionManagerGo = new GameObject("Test_MissionManager");
                missionManagerGo.transform.SetParent(testRoot.transform);
                var missionManager = missionManagerGo.AddComponent<MissionManager>();

                var timerManagerGo = new GameObject("Test_TimerManager");
                timerManagerGo.transform.SetParent(testRoot.transform);
                var timerManager = timerManagerGo.AddComponent<TimerManager>();

                // Prepare Beam Zone GameObject
                var beamGo = new GameObject("Test_ShipBeamZone");
                beamGo.transform.SetParent(testRoot.transform);
                var col = beamGo.AddComponent<CapsuleCollider>();
                col.isTrigger = true;
                var shipBeam = beamGo.AddComponent<ShipBeamZone>();

                // Prepare Player GameObject
                var playerGo = new GameObject("Test_Player");
                playerGo.transform.SetParent(testRoot.transform);
                playerGo.tag = "Player";
                var playerCc = playerGo.AddComponent<CharacterController>();
                var playerController = playerGo.AddComponent<ETPlayerController>();
                playerController.CanMove = true;

                // Test 2: Incomplete mission -> OnBeamDenied fired
                var levelConfig = ScriptableObject.CreateInstance<LevelConfig>();
                var serializedLevel = new SerializedObject(levelConfig);
                serializedLevel.FindProperty("_listSize").intValue = 2;
                serializedLevel.FindProperty("_extraSpawnsPerRequired").intValue = 0;
                serializedLevel.FindProperty("_forbiddenSpawnCount").intValue = 1;
                serializedLevel.ApplyModifiedPropertiesWithoutUndo();

                missionManager.GenerateMission(levelConfig);

                var deniedEventFired = false;
                GameEvents.OnBeamDenied += () => deniedEventFired = true;

                // Simulate entry into beam with incomplete order
                var method = typeof(ShipBeamZone).GetMethod("EvaluateBeamEntry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(shipBeam, new object[] { playerCc });

                if (deniedEventFired && playerController.CanMove && !shipBeam.IsTransitioning)
                {
                    passedCount++;
                    Debug.Log("[Verification A09] Test 2 PASSED: Incomplete mission correctly triggered OnBeamDenied without freezing player.");
                }
                else
                {
                    Debug.LogError($"[Verification A09] Test 2 FAILED: deniedEventFired={deniedEventFired}, CanMove={playerController.CanMove}, IsTransitioning={shipBeam.IsTransitioning}");
                }

                // Test 3: Complete mission -> Timer stopped & Player frozen
                timerManager.StartTimer(TimerPhase.Capture, 60f);

                // Validate capture to complete the mission
                while (!missionManager.IsComplete)
                {
                    var expected = missionManager.RequiredOrder[missionManager.CurrentIndex];
                    missionManager.ValidateCapture(expected);
                }

                method?.Invoke(shipBeam, new object[] { playerCc });

                var timerStopped = !timerManager.IsRunning(TimerPhase.Capture);
                var playerFrozen = !playerController.CanMove;
                var isTransitioning = shipBeam.IsTransitioning;

                if (timerStopped && playerFrozen && isTransitioning)
                {
                    passedCount++;
                    Debug.Log("[Verification A09] Test 3 PASSED: Complete mission stops capture timer, freezes player, and initiates transition.");
                }
                else
                {
                    Debug.LogError($"[Verification A09] Test 3 FAILED: timerStopped={timerStopped}, playerFrozen={playerFrozen}, isTransitioning={isTransitioning}");
                }

                // Test 4: Non-player collider ignored
                var nonPlayerGo = new GameObject("Test_NonPlayer");
                nonPlayerGo.transform.SetParent(testRoot.transform);
                var nonPlayerCol = nonPlayerGo.AddComponent<BoxCollider>();

                deniedEventFired = false;
                var isPlayerMethod = typeof(ShipBeamZone).GetMethod("IsPlayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var isPlayerResult = (bool)(isPlayerMethod?.Invoke(shipBeam, new object[] { nonPlayerCol }) ?? true);

                if (!isPlayerResult)
                {
                    passedCount++;
                    Debug.Log("[Verification A09] Test 4 PASSED: Non-player colliders are correctly ignored.");
                }
                else
                {
                    Debug.LogError("[Verification A09] Test 4 FAILED: Non-player collider was erroneously accepted.");
                }
            }
            finally
            {
                Object.DestroyImmediate(testRoot);
            }

            Debug.Log($"[Verification A09] Summary: {passedCount}/{totalCount} tests PASSED.");
            if (passedCount == totalCount)
            {
                Debug.Log(">>> [A09 VERIFICATION SUCCESS] All criteria satisfied! <<<");
            }
            else
            {
                Debug.LogError(">>> [A09 VERIFICATION FAILURE] Some tests failed. <<<");
            }
        }
    }
}
#endif
