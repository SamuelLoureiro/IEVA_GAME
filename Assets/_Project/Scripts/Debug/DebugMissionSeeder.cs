using System.Collections.Generic;
using UnityEngine;
using VARG.Data;
using VARG.Mission;

namespace VARG.DebugTools
{
    public class DebugMissionSeeder : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private bool _seedOnStart = false;
        [SerializeField] private bool _showGui = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private LevelConfig _levelConfig1;
        private CaptureResult? _lastResult;

        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            LoadLevelConfig();
        }

        private void Start()
        {
            if (_seedOnStart)
            {
                SeedDefaultMission();
            }
        }

        public void SeedDefaultMission()
        {
            if (MissionManager.Instance == null) return;
            LoadLevelConfig();
            MissionManager.Instance.GenerateMission(_levelConfig1);
            _lastResult = null;
        }

        private void LoadLevelConfig()
        {
#if UNITY_EDITOR
            _levelConfig1 = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/_Project/Data/Levels/LevelConfig_1.asset");
#endif
        }

        private void OnGUI()
        {
            if (!_showGui) return;
            if (MissionManager.Instance == null) return;

            GUILayout.BeginArea(new Rect(540, 200, 280, 420), "Missão Debug (B05)", GUI.skin.window);

            GUILayout.Label($"Tipo Proibido: {MissionManager.Instance.ForbiddenType}");
            GUILayout.Label($"Progresso: {MissionManager.Instance.CurrentIndex} / {MissionManager.Instance.RequiredCount}");
            GUILayout.Label($"Completa: {(MissionManager.Instance.IsComplete ? "[SIM - PRONTA]" : "[NÃO]")}");

            if (_lastResult.HasValue)
            {
                GUILayout.Label($"Último Resultado: {_lastResult.Value}");
            }

            GUILayout.Space(5);
            GUILayout.Label("Ordem Requerida:");
            for (var i = 0; i < MissionManager.Instance.RequiredOrder.Count; i++)
            {
                var target = MissionManager.Instance.RequiredOrder[i];
                var prefix = i == MissionManager.Instance.CurrentIndex ? "-> " : "   ";
                var status = i < MissionManager.Instance.CurrentIndex ? "[OK]" : "[PENDENTE]";
                GUILayout.Label($"{prefix}[{i + 1}] {target.DisplayName} {status}");
            }

            GUILayout.Space(5);

            if (GUILayout.Button("1. Gerar Missão (Nível 1)"))
            {
                LoadLevelConfig();
                MissionManager.Instance.GenerateMission(_levelConfig1);
                _lastResult = null;
            }

            if (GUILayout.Button("2. Capturar Correto (Correct)"))
            {
                if (!MissionManager.Instance.IsComplete && MissionManager.Instance.CurrentIndex < MissionManager.Instance.RequiredOrder.Count)
                {
                    var expected = MissionManager.Instance.RequiredOrder[MissionManager.Instance.CurrentIndex];
                    _lastResult = MissionManager.Instance.ValidateCapture(expected);
                }
            }

            if (GUILayout.Button("3. Capturar Fora de Ordem (OutOfOrder)"))
            {
                TargetData wrongAllowed = null;
                var allTypes = (TargetType[])System.Enum.GetValues(typeof(TargetType));
                for (var i = 0; i < allTypes.Length; i++)
                {
                    var type = allTypes[i];
                    if (type != MissionManager.Instance.ForbiddenType)
                    {
                        var candidate = MissionManager.Instance.GetTargetDataForType(type);
                        if (!MissionManager.Instance.IsComplete && candidate != MissionManager.Instance.RequiredOrder[MissionManager.Instance.CurrentIndex])
                        {
                            wrongAllowed = candidate;
                            break;
                        }
                    }
                }

                if (wrongAllowed != null)
                {
                    _lastResult = MissionManager.Instance.ValidateCapture(wrongAllowed);
                }
            }

            if (GUILayout.Button("4. Capturar Proibido (NotInList)"))
            {
                var forbiddenData = MissionManager.Instance.GetTargetDataForType(MissionManager.Instance.ForbiddenType);
                if (forbiddenData != null)
                {
                    _lastResult = MissionManager.Instance.ValidateCapture(forbiddenData);
                }
            }

            GUILayout.EndArea();
        }
    }
}
