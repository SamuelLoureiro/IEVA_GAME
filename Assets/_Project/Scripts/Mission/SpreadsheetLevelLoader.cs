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
                "# ==============================================================================\n" +
                "# VARG - CUSTOM SPREADSHEET LEVEL CONFIGURATION\n" +
                "# Edit this file in Excel, LibreOffice, Google Sheets, or Notepad.\n" +
                "# Available Targets: Cow, Pig, Chicken, Human (or Vaca, Porco, Galinha, Humano)\n" +
                "# ==============================================================================\n" +
                "Parametro,Valor\n" +
                "SequenciaAlvos,Cow;Pig;Chicken;Cow\n" +
                "AlvoProibido,Human\n" +
                "TempoCaca,90\n" +
                "TempoTriagem,45\n" +
                "AlvosExtras,2\n" +
                "AlvosProibidos,2\n" +
                "VelocidadeFuga,1.0\n";

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

                    var parts = line.Split(delimiter);
                    if (parts.Length < 2) continue;

                    var key = NormalizeKey(parts[0]);
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
                    statusMessage = "Spreadsheet has no valid targets in 'SequenciaAlvos'. Expected format: Cow;Pig;Chicken";
                    return false;
                }

                levelData = data;
                statusMessage = wasNewlyCreated
                    ? $"CustomLevel.csv criado e carregado! ({data.TargetSequence.Count} alvos). Edite-o no Excel a qualquer momento."
                    : $"Planilha carregada com sucesso! ({data.TargetSequence.Count} alvos, Proibido: {data.ForbiddenTarget}, Caça: {data.CaptureTime:F0}s, Triagem: {data.SortingTime:F0}s)";
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
                if (norm.Contains("tempocaca") || norm.Contains("alvoproibido") || norm.Contains("parametro") || norm.Contains("tempotriagem"))
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

        private static void ParseTargetSequence(string[] parts, List<TargetType> list)
        {
            list.Clear();
            var tokens = new List<string>();
            var subDelimiters = new[] { ';', ',', '|', '\t', ' ' };

            for (var i = 1; i < parts.Length; i++)
            {
                var part = parts[i].Trim().Trim('"');
                if (string.IsNullOrEmpty(part)) continue;

                var subParts = part.Split(subDelimiters, StringSplitOptions.RemoveEmptyEntries);
                for (var j = 0; j < subParts.Length; j++)
                {
                    tokens.Add(subParts[j].Trim());
                }
            }

            foreach (var token in tokens)
            {
                if (TryParseTargetType(token, out var target))
                {
                    list.Add(target);
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
