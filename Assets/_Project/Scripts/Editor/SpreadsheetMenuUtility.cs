#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using VARG.Mission;

namespace VARG.Editor
{
    public static class SpreadsheetMenuUtility
    {
        [MenuItem("VARG/Scenes/Open 00_Boot (Main Menu)", priority = 0)]
        public static void OpenBootScene()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity");
        }

        [MenuItem("VARG/Scenes/Open 01_BossBriefing", priority = 1)]
        public static void OpenBriefingScene()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Project/Scenes/01_BossBriefing.unity");
        }

        [MenuItem("VARG/Spreadsheet/Open CustomLevel.csv", priority = 10)]
        public static void OpenCustomLevelCsv()
        {
            var path = SpreadsheetLevelLoader.GetDefaultFilePath();
            if (!File.Exists(path))
            {
                SpreadsheetLevelLoader.EnsureTemplateExists(out path);
                Debug.Log($"[SpreadsheetMenuUtility] Template created at: {path}");
            }

            EditorUtility.OpenWithDefaultApp(path);
        }

        [MenuItem("VARG/Spreadsheet/Reveal CustomLevel.csv in File Explorer", priority = 11)]
        public static void RevealCustomLevelCsv()
        {
            var path = SpreadsheetLevelLoader.GetDefaultFilePath();
            if (!File.Exists(path))
            {
                SpreadsheetLevelLoader.EnsureTemplateExists(out path);
            }

            EditorUtility.RevealInFinder(path);
        }

        [MenuItem("VARG/Spreadsheet/Generate Default Template", priority = 12)]
        public static void GenerateDefaultTemplate()
        {
            if (SpreadsheetLevelLoader.EnsureTemplateExists(out var path))
            {
                EditorUtility.DisplayDialog(
                    "VARG Spreadsheet",
                    $"Template created successfully at:\n{path}\n\nYou can now edit it in Excel.",
                    "OK"
                );
            }
            else
            {
                var overwrite = EditorUtility.DisplayDialog(
                    "VARG Spreadsheet",
                    $"File already exists at:\n{path}\n\nDo you want to overwrite it with the default template?",
                    "Overwrite",
                    "Cancel"
                );

                if (overwrite)
                {
                    File.Delete(path);
                    SpreadsheetLevelLoader.EnsureTemplateExists(out path);
                    EditorUtility.DisplayDialog("VARG Spreadsheet", "Template reset to default!", "OK");
                }
            }
        }
    }
}
#endif
