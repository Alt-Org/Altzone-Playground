using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Util
{
    public class MenuBuildReport: MonoBehaviour
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

            var files = parseBuildReport(buildReport);
            Debug.Log($"Build contains {files.Count} files");
        }

        private static List<string> parseBuildReport(string buildReport)
        {
            const string markerLine = "-------------------------------------------------------------------------------";
            const string assetsLine = "Used Assets and files from the Resources folder, sorted by uncompressed size:";
            var result = new List<string>();
            var processing = false;
            foreach (var line in File.ReadAllLines(buildReport))
            {
                if (processing)
                {
                    if (line == markerLine)
                    {
                        break;
                    }
                    result.Add(line);
                }
                if (line == assetsLine)
                {
                    processing = true;
                }
            }
            return result;
        }
    }
}