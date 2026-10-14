#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using VARG.Core;
using VARG.Data;
using VARG.Humor;
using VARG.Input;
using VARG.Inventory;
using VARG.Mission;

namespace VARG.Editor
{
    public static class InitialSetupEditor
    {
        [MenuItem("VARG/Setup Initial Project Assets")]
        public static void SetupProject()
        {
            SetupLayers();
            EnsureDirectories();
            CreateConfigs();
            CreateScenes();
            CreateSystemsPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VARG] Initial Project Assets, Scenes, Layers, and Systems Prefab created successfully!");
        }

        [MenuItem("VARG/Setup Layers and Tags")]
        public static void SetupLayers()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;

            var tagManager = new SerializedObject(assets[0]);
            var layersProp = tagManager.FindProperty("layers");
            if (layersProp == null) return;

            SetLayer(layersProp, 6, "Player");
            SetLayer(layersProp, 7, "Interactable");
            SetLayer(layersProp, 8, "Grabbable");
            SetLayer(layersProp, 9, "CageTrigger");

            tagManager.ApplyModifiedProperties();
            Debug.Log("[VARG] Configured Layers: Player (6), Interactable (7), Grabbable (8), CageTrigger (9)");
        }

        private static void SetLayer(SerializedProperty layersProp, int index, string layerName)
        {
            if (index >= layersProp.arraySize) return;
            var element = layersProp.GetArrayElementAtIndex(index);
            if (element != null && string.IsNullOrEmpty(element.stringValue))
            {
                element.stringValue = layerName;
            }
        }

        private static void EnsureDirectories()
        {
            EnsureDirectory("Assets/_Project/Data");
            EnsureDirectory("Assets/_Project/Data/Targets");
            EnsureDirectory("Assets/_Project/Data/Levels");
            EnsureDirectory("Assets/_Project/Scenes");
            EnsureDirectory("Assets/Resources/Systems");
        }

        private static void EnsureDirectory(string relativePath)
        {
            if (!AssetDatabase.IsValidFolder(relativePath))
            {
                var parent = Path.GetDirectoryName(relativePath)?.Replace("\\", "/");
                var folderName = Path.GetFileName(relativePath);
                if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
                {
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }

        private static void CreateScenes()
        {
            string[] sceneNames =
            {
                "00_Boot",
                "01_BossBriefing",
                "02_ShipFlight",
                "03_CityHunt",
                "04_ShipSorting",
                "05_Results"
            };

            var buildScenes = new EditorBuildSettingsScene[sceneNames.Length];

            for (var i = 0; i < sceneNames.Length; i++)
            {
                var path = $"Assets/_Project/Scenes/{sceneNames[i]}.unity";
                if (!File.Exists(path))
                {
                    var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                        UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                        UnityEditor.SceneManagement.NewSceneMode.Single);
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, path);
                }
                buildScenes[i] = new EditorBuildSettingsScene(path, true);
            }

            EditorBuildSettings.scenes = buildScenes;
        }

        private static void CreateConfigs()
        {
            // TargetData for the 4 targets
            CreateTargetData("Target_Chicken", TargetType.Chicken, "Chicken", 3f, 5f);
            CreateTargetData("Target_Cow", TargetType.Cow, "Cow", 2f, 4f);
            CreateTargetData("Target_Pig", TargetType.Pig, "Pig", 2.5f, 4.5f);
            CreateTargetData("Target_Human", TargetType.Human, "Human", 4f, 7f);
            // PenaltyConfig
            const string penaltyPath = "Assets/_Project/Data/PenaltyConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<PenaltyConfig>(penaltyPath) == null)
            {
                var penalty = ScriptableObject.CreateInstance<PenaltyConfig>();
                AssetDatabase.CreateAsset(penalty, penaltyPath);
            }

            // ScoringConfig
            const string scoringPath = "Assets/_Project/Data/ScoringConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<ScoringConfig>(scoringPath) == null)
            {
                var scoring = ScriptableObject.CreateInstance<ScoringConfig>();
                AssetDatabase.CreateAsset(scoring, scoringPath);
            }

            // GameConfig
            const string gameConfigPath = "Assets/_Project/Data/GameConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<GameConfig>(gameConfigPath) == null)
            {
                var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(gameConfig, gameConfigPath);
            }

            // Levels
            for (var i = 1; i <= 3; i++)
            {
                var levelPath = $"Assets/_Project/Data/Levels/LevelConfig_{i}.asset";
                if (AssetDatabase.LoadAssetAtPath<LevelConfig>(levelPath) == null)
                {
                    var level = ScriptableObject.CreateInstance<LevelConfig>();
                    AssetDatabase.CreateAsset(level, levelPath);
                }
            }

            CreateCampaignConfig();
            CreateBossLineSet();
        }

        private static void CreateTargetData(string assetName, TargetType type, string displayName, float walkSpeed, float fleeSpeed)
        {
            var path = $"Assets/_Project/Data/Targets/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<TargetData>(path) != null) return;

            var target = ScriptableObject.CreateInstance<TargetData>();
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty("_id").stringValue = assetName;
            serializedObject.FindProperty("_type").enumValueIndex = (int)type;
            serializedObject.FindProperty("_displayName").stringValue = displayName;
            serializedObject.FindProperty("_walkSpeed").floatValue = walkSpeed;
            serializedObject.FindProperty("_fleeSpeed").floatValue = fleeSpeed;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(target, path);
        }

        private static void CreateCampaignConfig()
        {
            const string campaignPath = "Assets/_Project/Data/CampaignConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<CampaignConfig>(campaignPath) != null) return;

            var campaign = ScriptableObject.CreateInstance<CampaignConfig>();
            AssetDatabase.CreateAsset(campaign, campaignPath);
        }

        private static void CreateSystemsPrefab()
        {
            const string prefabPath = "Assets/Resources/Systems/[Systems].prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

            var systemsGo = new GameObject("[Systems]");
            systemsGo.AddComponent<GameManager>();
            systemsGo.AddComponent<MissionManager>();
            systemsGo.AddComponent<InventoryManager>();
            systemsGo.AddComponent<TimerManager>();
            systemsGo.AddComponent<LivesManager>();
            systemsGo.AddComponent<BossDialogueManager>();
            systemsGo.AddComponent<SceneLoader>();
            systemsGo.AddComponent<InputManager>();

            PrefabUtility.SaveAsPrefabAsset(systemsGo, prefabPath);
            Object.DestroyImmediate(systemsGo);
            Debug.Log($"[VARG] [Systems] prefab created at {prefabPath}");
        }

        [MenuItem("VARG/Update [Systems] Prefab References")]
        public static void UpdateSystemsPrefab()
        {
            const string prefabPath = "Assets/Resources/Systems/[Systems].prefab";
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null)
            {
                Debug.LogWarning($"[VARG] Prefab not found at {prefabPath}");
                return;
            }

            var gameManager = root.GetComponent<GameManager>();
            if (gameManager != null)
            {
                var campaignConfig = AssetDatabase.LoadAssetAtPath<CampaignConfig>("Assets/_Project/Data/CampaignConfig.asset");
                var gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Project/Data/GameConfig.asset");
                var scoringConfig = AssetDatabase.LoadAssetAtPath<ScoringConfig>("Assets/_Project/Data/ScoringConfig.asset");
                var penaltyConfig = AssetDatabase.LoadAssetAtPath<PenaltyConfig>("Assets/_Project/Data/PenaltyConfig.asset");

                var serializedGm = new SerializedObject(gameManager);
                var campProp = serializedGm.FindProperty("_campaignConfig");
                var gameProp = serializedGm.FindProperty("_gameConfig");
                var scorProp = serializedGm.FindProperty("_scoringConfig");
                var penProp = serializedGm.FindProperty("_penaltyConfig");

                if (campProp != null) campProp.objectReferenceValue = campaignConfig;
                if (gameProp != null) gameProp.objectReferenceValue = gameConfig;
                if (scorProp != null) scorProp.objectReferenceValue = scoringConfig;
                if (penProp != null) penProp.objectReferenceValue = penaltyConfig;

                serializedGm.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[VARG] GameManager references on [Systems] prefab updated!");
            }

            var missionManager = root.GetComponent<MissionManager>();
            if (missionManager != null)
            {
                var chicken = AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Chicken.asset");
                var cow = AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Cow.asset");
                var pig = AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Pig.asset");
                var human = AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Human.asset");
                var penaltyConfig = AssetDatabase.LoadAssetAtPath<PenaltyConfig>("Assets/_Project/Data/PenaltyConfig.asset");

                var serializedMm = new SerializedObject(missionManager);
                var penProp = serializedMm.FindProperty("_penaltyConfig");
                var targetsProp = serializedMm.FindProperty("_availableTargets");

                if (penProp != null) penProp.objectReferenceValue = penaltyConfig;
                if (targetsProp != null)
                {
                    targetsProp.arraySize = 4;
                    targetsProp.GetArrayElementAtIndex(0).objectReferenceValue = chicken;
                    targetsProp.GetArrayElementAtIndex(1).objectReferenceValue = cow;
                    targetsProp.GetArrayElementAtIndex(2).objectReferenceValue = pig;
                    targetsProp.GetArrayElementAtIndex(3).objectReferenceValue = human;
                }

                serializedMm.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[VARG] MissionManager references on [Systems] prefab updated!");
            }

            var bossDialogueManager = root.GetComponent<BossDialogueManager>();
            if (bossDialogueManager != null)
            {
                var bossLineSet = AssetDatabase.LoadAssetAtPath<BossLineSet>("Assets/_Project/Data/BossLineSet.asset");
                var serializedBdm = new SerializedObject(bossDialogueManager);
                var linesProp = serializedBdm.FindProperty("_lines");
                if (linesProp != null) linesProp.objectReferenceValue = bossLineSet;
                serializedBdm.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[VARG] BossDialogueManager references on [Systems] prefab updated!");
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("VARG/Create or Update BossLineSet Asset")]
        public static void CreateBossLineSet()
        {
            const string path = "Assets/_Project/Data/BossLineSet.asset";
            var asset = AssetDatabase.LoadAssetAtPath<BossLineSet>(path);
            var isNew = asset == null;

            if (isNew)
            {
                asset = ScriptableObject.CreateInstance<BossLineSet>();
            }

            var ser = new SerializedObject(asset);

            SetArray(ser, "_correctCaptureLines", new[]
            {
                "Excelente. Mais um item pra cota e menos uma dor de cabeça pro meu relatório.",
                "Registrado no inventário! Quem diria que você sabia ler o briefing?!",
                "Correto! O setor de pesquisa agradece a cooperação dessa criatura.",
                "Muito bem, estagiário. Continue assim e talvez eu assine seu relatório de estágio."
            });

            SetArray(ser, "_outOfOrderCaptureLines", new[]
            {
                "Eu pedi {expected}, e você me traz {captured}?! Você sequer olhou a ordem?!",
                "Incompetência cósmica! A ordem exigia {expected}, não {captured}!",
                "Parabéns, desorganizou o protocolo inteiro! A prioridade era {expected}!",
                "{captured} agora não! Era a vez de {expected}! O tempo tá correndo!"
            });

            SetArray(ser, "_notInListCaptureLines", new[]
            {
                "{captured}?! ISSO NÃO ESTAVA NA LISTA! Você quer ser demitido?!",
                "Por que tem um {captured} a bordo?! Eu proibi expressamente esse tipo de espécime!",
                "Desastre diplomático! Largue esse {captured} antes que a federação descubra!",
                "Não autorizei a abdução de {captured}! Isso é crime intergaláctico de classe B!"
            });

            SetArray(ser, "_beamDeniedLines", new[]
            {
                "Acesso negado! O feixe só recolhe a nave quando a cota estiver completa!",
                "Voltando de mãos vazias?! A missão não acabou, estagiário! Vá atrás dos alvos!",
                "Não tente fugir! Termine a lista antes de pedir resgate!"
            });

            SetArray(ser, "_correctCageLines", new[]
            {
                "Gaiola certa. Menos um problema no setor de triagem.",
                "Espécime alocado com sucesso. O cheiro na nave tá diminuindo.",
                "Perfeito! Triagem correta sem violar os direitos galácticos."
            });

            SetArray(ser, "_wrongCageLines", new[]
            {
                "Você colocou {captured} na gaiola errada! A criatura quase foi mastigada!",
                "Errou a gaiola! Mais uma dessas e o custo do conserto sai do seu contracheque!",
                "Isso não é um habitat, é uma salada biológica! Gaiola errada!"
            });

            SetArray(ser, "_correctDiscardLines", new[]
            {
                "Descarte correto. Lixo espacial reciclado conforme as normas da ISO cósmica.",
                "Eliminado com sucesso. Menos peso na nave."
            });

            SetArray(ser, "_wrongDiscardLines", new[]
            {
                "Você jogou {captured} na lixeira?! ERA UM ALVO DA MISSÃO, ANIMAL!",
                "Descarte não autorizado! Pare de jogar a cota da empresa no triturador!",
                "Você acabou de desintegrar um item da pesquisa! Minha pressão tá em 300!"
            });

            SetArray(ser, "_lifeLostTier1Lines", new[]
            {
                "Cuidado, estagiário! Recursos da nave não nascem em árvore cósmica.",
                "Menos uma chance. Concentre-se antes que eu rebaixe você pra lavador de turbina!",
                "Um errinho básico... ainda estamos no lucro, mas não abuse."
            });

            SetArray(ser, "_lifeLostTier2Lines", new[]
            {
                "Mais um erro e eu te deixo na Terra junto com as vacas! Só restam {lives} chances!",
                "Minha pressão tá subindo! Se você perder mais uma vida, quem vai pro triturador é você!",
                "Alerta amarelo! Só temos {lives} tentativas restantes! Pare de brincar em serviço!"
            });

            SetArray(ser, "_lifeLostTier3Lines", new[]
            {
                "ÚLTIMA CHANCE! Se você errar mais uma vez, eu cancelo sua cidadania alienígena!",
                "UMA VIDA! UMA ÚNICA VIDA! Minha carreira de comandante está por um fio!",
                "ALERTA VERMELHO! Respire fundo e não faça nenhuma besteira, estagiário!"
            });

            SetArray(ser, "_timeLowLines", new[]
            {
                "O relógio tá correndo na fase de {phase}! Menos enrolação e mais trabalho!",
                "Quinze segundos?! Acelera esse passo antes que o turno termine!",
                "Hora extra não é remunerada nessa galáxia! Termine logo esse {phase}!"
            });

            SetArray(ser, "_victoryLines", new[]
            {
                "Missão cumprida! Hora de bater o ponto e fingir que trabalhamos duro.",
                "Cota batida! Você escapou de uma advertência formal por hoje.",
                "Excelente trabalho. O relatório trimestral vai ficar bonito pro alto conselho."
            });

            SetArray(ser, "_defeatLines", new[]
            {
                "Derrota inaceitável ({reason})! Seu relatório de desempenho foi direto pro RH!",
                "Fim da linha! O estagiário conseguiu fracassar a operação mais simples do quadrante!",
                "Missão falhou: {reason}. Vou fingir que nem te conheço no tribunal estelar."
            });

            SetArray(ser, "_idleLines", new[]
            {
                "Você ganha por hora agora? Mexa-se!",
                "A Terra não vai se abduzir sozinha, estagiário!",
                "Está contemplando o cosmos ou travou aí embaixo?!",
                "Alô? Tem alguém em casa ou o cérebro virou gelatina espacial?!"
            });

            ser.ApplyModifiedPropertiesWithoutUndo();

            if (isNew)
            {
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[VARG] BossLineSet asset created/updated at {path}");
        }

        private static void SetArray(SerializedObject so, string propertyName, string[] values)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) return;
            prop.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).stringValue = values[i];
            }
        }
    }
}
#endif
