using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Util
{
    public class MenuBuildReport : MonoBehaviour
    {
        private static readonly string[] excludedFolders =
        {
            "Assets/Photon",
        };

        [MenuItem("Window/ALT-Zone/Build/Check Build Report")]
        private static void WindowReport()
        {
            var buildTargetName = TeamCity.CommandLine.BuildTargetNameFrom(EditorUserBuildSettings.activeBuildTarget);
            Debug.Log($"Build target is {buildTargetName}");
            var buildReport = $"m_Build_{buildTargetName}.log";
            if (!File.Exists(buildReport))
            {
                Debug.LogWarning($"Build report {buildReport} not found");
                return;
            }

            var allFiles = parseBuildReport(buildReport, out var totalSize);
            Debug.Log($"Build contains {allFiles.Count} asset files, total size is {totalSize:### ### ###.0} kb");
            var assetFiles = new List<AssetLine>();
            int[] fileCount = { 0, 0, 0};
            float[] fileSize = { 0, 0, 0};
            float[] filePercent = { 0, 0, 0};
            foreach (var assetLine in allFiles)
            {
                if (assetLine.isAsset)
                {
                    assetFiles.Add(assetLine);
                    fileCount[0] += 1;
                    fileSize[0] += assetLine.fileSize;
                    filePercent[0] += assetLine.percentage;
                }
                else if (assetLine.isPackage)
                {
                    fileCount[1] += 1;
                    fileSize[1] += assetLine.fileSize;
                    filePercent[1] += assetLine.percentage;
                }
                else if (assetLine.isResource)
                {
                    fileCount[2] += 1;
                    fileSize[2] += assetLine.fileSize;
                    filePercent[2] += assetLine.percentage;
                }
                else
                {
                    Debug.LogError("Unknown asset line: " + assetLine.filePath);
                    return;
                }
            }
            Debug.Log($"Build contains {fileCount[0]} ASSET files, their size is {fileSize[0]:### ### ###.0} kb ({filePercent[0]:0.0}%)");
            Debug.Log($"Build contains {fileCount[1]} PACKAGE files, their size is {fileSize[1]:### ### ###.0} kb ({filePercent[1]:0.0}%)");
            Debug.Log($"Build contains {fileCount[2]} RESOURCE files, their size is {fileSize[2]:### ### ###.0} kb ({filePercent[2]:0.0}%)");
        }

        private static List<AssetLine> parseBuildReport(string buildReport, out float totalSize)
        {
            const string markerLine = "-------------------------------------------------------------------------------";
            const string assetsLine = "Used Assets and files from the Resources folder, sorted by uncompressed size:";
            var result = new List<AssetLine>();
            var processing = false;
            totalSize = 0;
            foreach (var line in File.ReadAllLines(buildReport))
            {
                if (processing)
                {
                    if (line == markerLine)
                    {
                        break;
                    }
                    var assetLine = new AssetLine(line);
                    totalSize += assetLine.fileSize;
                    result.Add(assetLine);
                }
                if (line == assetsLine)
                {
                    processing = true;
                }
            }
            return result;
        }

        private class AssetLine
        {
            private static readonly CultureInfo culture = CultureInfo.GetCultureInfo("en-US");
            private static readonly char[] separators1 = { '%' };
            private static readonly char[] separators2 = { ' ' };

            public readonly float fileSize;
            public readonly float percentage;
            public readonly string filePath;

            public bool isAsset => filePath.StartsWith("Assets/");
            public bool isPackage => filePath.StartsWith("Packages/");
            public bool isResource => filePath.StartsWith("Resources/");

            public AssetLine(string line)
            {
                var tokens = line.Split(separators1);
                if (tokens.Length != 2)
                {
                    filePath = line;
                    return;
                }
                filePath = tokens[1].Trim();
                tokens = tokens[0].Split(separators2, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 3)
                {
                    return;
                }
                fileSize = float.Parse(tokens[0], culture);
                percentage = float.Parse(tokens[2], culture);
            }
        }
    }
}