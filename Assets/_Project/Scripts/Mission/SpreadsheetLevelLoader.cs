using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using VARG.Data;

namespace VARG.Mission
{
    public static class SpreadsheetLevelLoader
    {
        public const string DefaultFileName = "CustomLevel.csv";

        public static string GetDefaultFilePath()
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), DefaultFileName);
            if (File.Exists(rootPath)) return rootPath;

            var streamingPath = Path.Combine(Application.streamingAssetsPath, DefaultFileName);
            if (File.Exists(streamingPath)) return streamingPath;

            var persistentPath = Path.Combine(Application.persistentDataPath, DefaultFileName);
            if (File.Exists(persistentPath)) return persistentPath;

            return rootPath;
        }

        public static bool EnsureTemplateExists(out string filePath)
        {
            filePath = GetDefaultFilePath();
            if (File.Exists(filePath)) return false;

            var content =
                "Parameter,Value,DefaultValue,Description\n" +
                "TargetSequence,\"Cow, Pig, Chicken, Cow\",\"Cow, Pig, Chicken, Cow\",\"Targets to capture in exact order. Any combination of: Cow, Pig, Chicken, Human\"\n" +
                "ForbiddenTarget,Human,Human,\"Forbidden species (penalizes if captured). Can be Cow, Pig, Chicken, or Human (must not be in TargetSequence)\"\n" +
                "CaptureTime,90,90,\"Time limit for the city hunt phase in seconds\"\n" +
                "SortingTime,45,45,\"Time limit for the ship sorting phase in seconds\"\n" +
                "ExtraSpawns,2,2,\"Additional distractor targets roaming the city\"\n" +
                "ForbiddenSpawns,2,2,\"Number of forbidden targets spawned in the city\"\n" +
                "FleeSpeedMultiplier,1.0,1.0,\"Speed multiplier for escaping targets (e.g. 1.0)\"\n";

            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(filePath, content);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create CSV template: {ex.Message}");
                return false;
            }
        }

        public static bool TryLoadCustomLevel(out SpreadsheetLevelData levelData, out string statusMessage)
        {
            levelData = new SpreadsheetLevelData();
            var path = GetDefaultFilePath();
            var wasNewlyCreated = false;

            if (!File.Exists(path))
            {
                EnsureTemplateExists(out path);
                wasNewlyCreated = true;
            }

            try
            {
                var lines = File.ReadAllLines(path);
                var data = new SpreadsheetLevelData();
                var foundSequence = false;

                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//")) continue;

                    var delimiter = line.Contains(';') ? ';' : ',';
                    var parts = SplitRow(line, delimiter);
                    if (parts.Count < 2) continue;

                    var key = NormalizeKey(parts[0]);
                    if (key == "parameter" || key == "key") continue;

                    var value = parts[1].Trim().Trim('"');

                    switch (key)
                    {
                        case "targetsequence":
                            ParseTargetSequence(value, data.TargetSequence);
                            foundSequence = data.TargetSequence.Count > 0;
                            break;

                        case "forbiddentarget":
                            if (TryParseTargetType(value, out var forbidden)) data.ForbiddenTarget = forbidden;
                            break;

                        case "capturetime":
                            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var cTime))
                                data.CaptureTime = Mathf.Max(10f, cTime);
                            break;

                        case "sortingtime":
                            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var sTime))
                                data.SortingTime = Mathf.Max(10f, sTime);
                            break;

                        case "extraspawns":
                            if (int.TryParse(value, out var extras)) data.ExtraSpawns = Mathf.Max(0, extras);
                            break;

                        case "forbiddenspawns":
                            if (int.TryParse(value, out var fSpawns)) data.ForbiddenSpawns = Mathf.Max(0, fSpawns);
                            break;

                        case "fleespeedmultiplier":
                            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fSpeed))
                                data.FleeSpeedMultiplier = Mathf.Clamp(fSpeed, 0.2f, 3.0f);
                            break;
                    }
                }

                if (!foundSequence || data.TargetSequence.Count == 0)
                {
                    statusMessage = "Spreadsheet has no valid targets in 'TargetSequence'. Expected format: Cow, Pig, Chicken, Human";
                    return false;
                }

                if (data.TargetSequence.Contains(data.ForbiddenTarget))
                {
                    statusMessage = $"ForbiddenTarget '{data.ForbiddenTarget}' cannot be in TargetSequence! Change ForbiddenTarget to another species.";
                    return false;
                }

                levelData = data;
                statusMessage = wasNewlyCreated
                    ? $"CustomLevel.csv created and loaded! ({data.TargetSequence.Count} targets)."
                    : $"Spreadsheet loaded successfully! ({data.TargetSequence.Count} targets, Forbidden: {data.ForbiddenTarget}, Hunt: {data.CaptureTime:F0}s, Sort: {data.SortingTime:F0}s)";
                return true;
            }
            catch (Exception ex)
            {
                statusMessage = $"Error reading CSV: {ex.Message}";
                return false;
            }
        }

        private static string NormalizeKey(string key) => key.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");

        private static List<string> SplitRow(string line, char delimiter)
        {
            var parts = new List<string>();
            var inQuotes = false;
            var start = 0;

            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == '"') inQuotes = !inQuotes;
                else if (line[i] == delimiter && !inQuotes)
                {
                    parts.Add(line.Substring(start, i - start).Trim().Trim('"'));
                    start = i + 1;
                }
            }

            parts.Add(line.Substring(start).Trim().Trim('"'));
            return parts;
        }

        private static void ParseTargetSequence(string valueString, List<TargetType> list)
        {
            list.Clear();
            if (string.IsNullOrWhiteSpace(valueString)) return;

            var tokens = valueString.Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < tokens.Length; i++)
            {
                if (TryParseTargetType(tokens[i].Trim(), out var target)) list.Add(target);
            }
        }

        public static bool TryParseTargetType(string name, out TargetType type)
        {
            var cleaned = name.Trim().ToLowerInvariant();
            switch (cleaned)
            {
                case "cow": case "vache": type = TargetType.Cow; return true;
                case "pig": case "cochon": type = TargetType.Pig; return true;
                case "chicken": case "poule": type = TargetType.Chicken; return true;
                case "human": case "humain": type = TargetType.Human; return true;
                default: return Enum.TryParse(name.Trim(), true, out type);
            }
        }
    }
}
