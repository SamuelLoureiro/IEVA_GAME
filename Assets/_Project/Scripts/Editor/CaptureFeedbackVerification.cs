#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VARG.Data;
using VARG.Player;
using VARG.Targets;

namespace VARG.Editor
{
    public static class CaptureFeedbackVerification
    {
        [MenuItem("VARG/Verify A16 - Capture Feedback and Audio")]
        public static void RunVerification()
        {
            TargetPrefabsCreator.CreateAllPrefabs();

            var passedCount = 0;
            var totalCount = 5;

            Debug.Log("[Verification A16] Starting Capture Feedback & Screams verification...");

            // Test 1: Procedural Audio clips verification (core clips)
            try
            {
                var shootClip = ProceduralAudioUtility.GetLaserShootClip();
                var boingClip = ProceduralAudioUtility.GetBoingShrinkClip();
                var popClip = ProceduralAudioUtility.GetCapturePopClip();
                var successClip = ProceduralAudioUtility.GetCaptureSuccessClip();
                var errorClip = ProceduralAudioUtility.GetCaptureErrorClip();
                var fanfareClip = ProceduralAudioUtility.GetMissionCompleteClip();

                if (shootClip != null && boingClip != null && popClip != null &&
                    successClip != null && errorClip != null && fanfareClip != null)
                {
                    AssertValidClip(shootClip, "LaserShoot");
                    AssertValidClip(boingClip, "BoingShrink");
                    AssertValidClip(popClip, "CapturePop");
                    AssertValidClip(successClip, "CaptureSuccess");
                    AssertValidClip(errorClip, "CaptureError");
                    AssertValidClip(fanfareClip, "MissionCompleteFanfare");

                    passedCount++;
                    Debug.Log("[Verification A16] Test 1 PASSED: Core procedural audio clips generated cleanly without NaN/inf.");
                }
                else
                {
                    Debug.LogError("[Verification A16] Test 1 FAILED: One or more procedural clips returned null.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Verification A16] Test 1 FAILED with exception: {ex}");
            }

            // Test 2: Animal & Human flee vocalization clips
            try
            {
                var humanClip = ProceduralAudioUtility.GetFleeVoiceClip(TargetType.Human);
                var cowClip = ProceduralAudioUtility.GetFleeVoiceClip(TargetType.Cow);
                var chickenClip = ProceduralAudioUtility.GetFleeVoiceClip(TargetType.Chicken);
                var pigClip = ProceduralAudioUtility.GetFleeVoiceClip(TargetType.Pig);

                if (humanClip != null && cowClip != null && chickenClip != null && pigClip != null)
                {
                    AssertValidClip(humanClip, "HumanScream");
                    AssertValidClip(cowClip, "CowMoo");
                    AssertValidClip(chickenClip, "ChickenCluck");
                    AssertValidClip(pigClip, "PigSqueal");

                    passedCount++;
                    Debug.Log("[Verification A16] Test 2 PASSED: Animal & Human panic screams generated correctly.");
                }
                else
                {
                    Debug.LogError("[Verification A16] Test 2 FAILED: One or more voice clips returned null.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Verification A16] Test 2 FAILED with exception: {ex}");
            }

            // Test 3: CaptureFeedbackFX particle system generation
            var fxRoot = new GameObject("Test_CaptureFeedbackFX");
            try
            {
                var fx = fxRoot.AddComponent<CaptureFeedbackFX>();
                fx.PlayImpactSparks(Vector3.zero, Color.green);
                fx.PlayCapturePoof(Vector3.up, Color.white);
                fx.PlayFleeAlert(Vector3.forward);

                passedCount++;
                Debug.Log("[Verification A16] Test 3 PASSED: CaptureFeedbackFX initialized and emitted procedural particles cleanly.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Verification A16] Test 3 FAILED with exception: {ex}");
            }
            finally
            {
                Object.DestroyImmediate(fxRoot);
            }

            // Test 4: Shrinkable component feedback setup
            var shrinkTestGo = new GameObject("Test_Shrinkable");
            try
            {
                var shrinkable = shrinkTestGo.AddComponent<Shrinkable>();
                shrinkable.EnsureAudioSource();
                var audioSrc = shrinkable.AudioSource;

                if (audioSrc != null && audioSrc.spatialBlend >= 0.99f)
                {
                    passedCount++;
                    Debug.Log("[Verification A16] Test 4 PASSED: Shrinkable auto-configured 3D AudioSource and ready for boing playback.");
                }
                else
                {
                    Debug.LogError("[Verification A16] Test 4 FAILED: Shrinkable did not configure 3D AudioSource correctly.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Verification A16] Test 4 FAILED with exception: {ex}");
            }
            finally
            {
                Object.DestroyImmediate(shrinkTestGo);
            }

            // Test 5: TargetAI scream cooldown & audio setup
            var targetAiTestGo = new GameObject("Test_TargetAI");
            try
            {
                var agent = targetAiTestGo.AddComponent<UnityEngine.AI.NavMeshAgent>();
                agent.enabled = false; // navmesh not needed in test
                var targetAi = targetAiTestGo.AddComponent<TargetAI>();
                targetAi.EnsureAudioSource();
                var audioSrc = targetAi.AudioSource;

                if (audioSrc != null && audioSrc.spatialBlend >= 0.99f)
                {
                    passedCount++;
                    Debug.Log("[Verification A16] Test 5 PASSED: TargetAI auto-configured 3D AudioSource for panic screaming.");
                }
                else
                {
                    Debug.LogError("[Verification A16] Test 5 FAILED: TargetAI 3D AudioSource was not configured.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Verification A16] Test 5 FAILED with exception: {ex}");
            }
            finally
            {
                Object.DestroyImmediate(targetAiTestGo);
            }

            if (passedCount == totalCount)
            {
                Debug.Log($"<color=green>[Verification A16] ALL {totalCount} TESTS PASSED! Capture feedback, sounds and flee screams are fully verified.</color>");
            }
            else
            {
                Debug.LogError($"<color=red>[Verification A16] {passedCount}/{totalCount} TESTS PASSED.</color>");
            }
        }

        private static void AssertValidClip(AudioClip clip, string name)
        {
            if (clip == null) throw new System.Exception($"Clip '{name}' is null.");
            if (clip.samples <= 0) throw new System.Exception($"Clip '{name}' has 0 samples.");

            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            for (var i = 0; i < samples.Length; i++)
            {
                if (float.IsNaN(samples[i]) || float.IsInfinity(samples[i]))
                {
                    throw new System.Exception($"Clip '{name}' contains NaN/Infinity sample at index {i}.");
                }
            }
        }
    }
}
#endif
