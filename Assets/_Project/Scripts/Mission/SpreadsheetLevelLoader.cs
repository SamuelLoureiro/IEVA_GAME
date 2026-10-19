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
                "Parameter,Value,Description\n" +
                "TargetSequence,\"Cow, Pig, Chicken, Cow\",\"Targets to capture in exact order. Any combination of: Cow, Pig, Chicken, Human\"\n" +
                "ForbiddenTarget,Human,\"Forbidden species (penalizes if captured). Can be Cow, Pig, Chicken, or Human (must not be in TargetSequence)\"\n" +
                "CaptureTime,90,\"Time limit for the city hunt phase in seconds\"\n" +
                "SortingTime,45,\"Time limit for the ship sorting phase in seconds\"\n" +
                "ExtraSpawns,2,\"Additional distractor targets roaming the city\"\n" +
                "ForbiddenSpawns,2,\"Number of forbidden targets spawned in the city\"\n" +
                "FleeSpeedMultiplier,1.0,\"Speed multiplier for escaping targets (e.g. 1.0)\"\n";

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
                Debug.LogWarning($"[SpreadsheetLevelLoader] Failed to create template file: {ex.Message}");
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
                var delimiter = DetectDelimiter(lines);

                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
                    {
                        continue;
                    }

                    var parts = SplitCsvLine(line, delimiter);
                    if (parts.Count < 2) continue;

                    var key = NormalizeKey(parts[0]);
                    if (key == "parameter") continue;

                    var value = parts[1].Trim().Trim('"');

                    switch (key)
                    {
                        case "sequenciaalvos":
                        case "targetsequence":
                        case "targets":
                        case "alvos":
                        case "sequence":
                            ParseTargetSequence(parts, data.TargetSequence);
                            foundSequence = data.TargetSequence.Count > 0;
                            break;

                        case "alvoproibido":
                        case "forbiddentarget":
                        case "forbidden":
                        case "proibido":
                            if (TryParseTargetType(value, out var forbidden))
                            {
                                data.ForbiddenTarget = forbidden;
                            }
                            break;

                        case "tempocaca":
                        case "capturetime":
                        case "timehunt":
                            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var cTime))
                            {
                                data.CaptureTime = Mathf.Max(10f, cTime);
                            }
                            break;

                        case "tempotriagem":
                        case "sortingtime":
                        case "timesort":
                            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var sTime))
                            {
                                data.SortingTime = Mathf.Max(10f, sTime);
                            }
                            break;

                        case "alvosextras":
                        case "extraspawns":
                        case "extras":
                            if (int.TryParse(value, out var extras))
                            {
                                data.ExtraSpawns = Mathf.Max(0, extras);
                            }
                            break;

                        case "alvosproibidos":
                        case "forbiddenspawns":
                            if (int.TryParse(value, out var fSpawns))
                            {
                                data.ForbiddenSpawns = Mathf.Max(0, fSpawns);
                            }
                            break;

                        case "velocidadefuga":
                        case "fleespeedmultiplier":
                        case "fleespeed":
                            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fSpeed))
                            {
                                data.FleeSpeedMultiplier = Mathf.Clamp(fSpeed, 0.2f, 3.0f);
                            }
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
                    statusMessage = $"ForbiddenTarget '{data.ForbiddenTarget}' cannot be in TargetSequence! Change ForbiddenTarget to another species (Cow, Pig, Chicken, or Human).";
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

        private static string NormalizeKey(string key)
        {
            return key.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
        }

        private static char DetectDelimiter(string[] lines)
        {
            var commaCount = 0;
            var semiCount = 0;
            var tabCount = 0;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//")) continue;

                var norm = NormalizeKey(line);
                if (norm.Contains("parameter") || norm.Contains("targetsequence") || norm.Contains("capturetime") || norm.Contains("forbiddentarget") ||
                    norm.Contains("sortingtime") || norm.Contains("parametro") || norm.Contains("tempocaca") || norm.Contains("alvoproibido"))
                {
                    if (line.Contains(';')) semiCount++;
                    else if (line.Contains(',')) commaCount++;
                    else if (line.Contains('\t')) tabCount++;
                }
            }

            if (semiCount > commaCount && semiCount > tabCount) return ';';
            if (tabCount > commaCount && tabCount > semiCount) return '\t';
            return ',';
        }

        private static List<string> SplitCsvLine(string line, char delimiter)
        {
            var result = new List<string>();
            var sb = new System.Text.StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(sb.ToString().Trim().Trim('"'));
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            result.Add(sb.ToString().Trim().Trim('"'));
            return result;
        }

        private static void ParseTargetSequence(IList<string> parts, List<TargetType> list)
        {
            list.Clear();
            if (parts.Count < 2) return;

            var val = parts[1].Trim().Trim('"');
            var subDelimiters = new[] { ',', ';', '|' };
            if (val.IndexOfAny(subDelimiters) >= 0)
            {
                var subParts = val.Split(subDelimiters, StringSplitOptions.RemoveEmptyEntries);
                foreach (var token in subParts)
                {
                    if (TryParseTargetType(token.Trim(), out var target))
                    {
                        list.Add(target);
                    }
                }
                return;
            }

            for (var i = 1; i < parts.Count; i++)
            {
                var part = parts[i].Trim().Trim('"');
                if (TryParseTargetType(part, out var target))
                {
                    list.Add(target);
                }
                else
                {
                    break;
                }
            }
        }

        public static bool TryParseTargetType(string name, out TargetType type)
        {
            var cleaned = name.Trim().ToLowerInvariant();
            switch (cleaned)
            {
                case "cow":
                case "vaca":
                    type = TargetType.Cow;
                    return true;
                case "pig":
                case "porco":
                    type = TargetType.Pig;
                    return true;
                case "chicken":
                case "galinha":
                    type = TargetType.Chicken;
                    return true;
                case "human":
                case "humano":
                    type = TargetType.Human;
                    return true;
                default:
                    return Enum.TryParse(name.Trim(), true, out type);
            }
        }
    }
}
