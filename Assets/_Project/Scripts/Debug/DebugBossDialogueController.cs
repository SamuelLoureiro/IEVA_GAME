using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Humor;
using VARG.Mission;

namespace VARG.DebugTools
{
    public class DebugBossDialogueController : MonoBehaviour
    {
        [SerializeField] private bool _showGui = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private TargetData _chicken;
        private TargetData _cow;
        private TargetData _pig;
        private TargetData _human;

        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            LoadTargets();
        }

        private void LoadTargets()
        {
#if UNITY_EDITOR
            _chicken = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Chicken.asset");
            _cow = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Cow.asset");
            _pig = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Pig.asset");
            _human = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Human.asset");
#endif
        }

        private void OnGUI()
        {
            if (!_showGui) return;
            if (BossDialogueManager.Instance == null)
            {
                GUILayout.BeginArea(new Rect(10, 300, 300, 80), "Boss Dialogue Debug (B14)", GUI.skin.window);
                GUILayout.Label("BossDialogueManager.Instance is null.\nCheck Console or Resources/Systems/[Systems].");
                GUILayout.EndArea();
                return;
            }

            GUILayout.BeginArea(new Rect(10, 300, 300, 520), "Boss Dialogue Debug (B14)", GUI.skin.window);

            GUILayout.Label($"Last Line: {BossDialogueManager.Instance.LastSpokenLine ?? "(None)"}");
            GUILayout.Label($"Expression: {BossDialogueManager.Instance.LastSpokenExpression}");

            GUILayout.Space(6);
            GUILayout.Label("<b>-- Capture Events --</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Correct"))
            {
                GameEvents.TriggerCorrectCapture(_chicken);
            }
            if (GUILayout.Button("Out of Order"))
            {
                GameEvents.TriggerOutOfOrderCapture(_pig, _cow);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Not in List"))
            {
                GameEvents.TriggerNotInListCapture(_human);
            }
            if (GUILayout.Button("Beam Denied"))
            {
                GameEvents.TriggerBeamDenied();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("<b>-- Sorting Events --</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Right Cage"))
            {
                GameEvents.TriggerCorrectCage(_chicken);
            }
            if (GUILayout.Button("Wrong Cage"))
            {
                GameEvents.TriggerWrongCage(_cow);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Right Discard"))
            {
                GameEvents.TriggerCorrectDiscard(_pig);
            }
            if (GUILayout.Button("Wrong Discard"))
            {
                GameEvents.TriggerWrongDiscard(_chicken);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("<b>-- Lives (Anger Tiers) --</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Tier 1 (4)"))
            {
                GameEvents.TriggerLifeLost(4);
            }
            if (GUILayout.Button("Tier 2 (2)"))
            {
                GameEvents.TriggerLifeLost(2);
            }
            if (GUILayout.Button("Tier 3 (1)"))
            {
                GameEvents.TriggerLifeLost(1);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("<b>-- State & Timer --</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Time Low"))
            {
                GameEvents.TriggerTimeLow(TimerPhase.Capture);
            }
            if (GUILayout.Button("Idle (>20s)"))
            {
                GameEvents.TriggerIdle();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Victory"))
            {
                GameEvents.TriggerVictory();
            }
            if (GUILayout.Button("Defeat"))
            {
                GameEvents.TriggerDefeat(DefeatReason.NoLives);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}
