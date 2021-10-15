using Prg.Scripts.Common.Util;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Prg.Util
{
    public class LogWriterMenu : MonoBehaviour
    {
        [MenuItem("Window/ALT-Zone/Util/Editor Log/Add 'FORCE_LOG' define")]
        private static void AddDefine()
        {
            var knownTargets = new[] { BuildTarget.Android, BuildTarget.StandaloneWindows64, BuildTarget.WebGL };
            var count = AddScriptingDefineSymbolToAllBuildTargetGroups("FORCE_LOG", knownTargets);
            if (count == 0)
            {
                Debug.Log("FORCE_LOG seems to be defined already");
            }
            else if (count == knownTargets.Length)
            {
                Debug.Log("FORCE_LOG define has been added and project should recompile now");
            }
            else
            {
                Debug.Log("FORCE_LOG define has been added to some build targets");
            }
        }

        [MenuItem("Window/ALT-Zone/Util/Editor Log/Show location")]
        private static void Show()
        {
            getLogFilePath();
        }

        [MenuItem("Window/ALT-Zone/Util/Editor Log/Open in text editor")]
        private static void Load()
        {
            var path = getLogFilePath();
            if (File.Exists(path))
            {
                InternalEditorUtility.OpenFileAtLineExternal(path, 1);
            }
        }

        private static string getLogFilePath()
        {
            var path = Path.Combine(Application.persistentDataPath, LogWriter.GetLogName());
            if (Application.platform.ToString().ToLower().Contains("windows"))
            {
                path = path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString());
            }
            Debug.Log($"Editor log {(File.Exists(path) ? "is" : "NOT")} found in: {path}");
            return path;
        }

        /// <summary>
        /// Adds a given scripting define symbol to all build target groups
        /// You can see all scripting define symbols ( not the internal ones, only the one for this project), in the PlayerSettings inspector
        /// </summary>
        /// <param name="defineSymbol">Define symbol.</param>
        /// <param name="targets">Build targets to modify</param>
        private static int AddScriptingDefineSymbolToAllBuildTargetGroups(string defineSymbol, BuildTarget[] targets)
        {
            var count = 0;
            foreach (var target in targets)
            {
                var group = BuildPipeline.GetBuildTargetGroup(target);

                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();
                if (defineSymbols.Contains(defineSymbol))
                {
                    continue;
                }
                defineSymbols.Add(defineSymbol);
                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(@group, string.Join(";", defineSymbols.ToArray()));
                    count += 1;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"Could not set Photon {defineSymbol} defines for build target: {target} group: {@group}: {e}");
                }
            }
            return count;
        }
    }
}