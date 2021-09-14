using Prg.Scripts.Common.Util;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Prg.Util
{
    public class LogWriterMenu : MonoBehaviour
    {
        [MenuItem("Window/ALT-Zone/Show Editor Log")]
        private static void Show()
        {
            var path = getPath();
            var isFound = File.Exists(path);
            Debug.Log($"Editor log {(isFound ? "" : "NOT ")}found in: {path}");
        }

        [MenuItem("Window/ALT-Zone/Load Editor Log")]
        private static void Load()
        {
            var path = getPath();
            var isFound = File.Exists(path);
            Debug.Log($"Editor log {(isFound ? "" : "NOT ")}found in: {path}");
            if (isFound)
            {
                InternalEditorUtility.OpenFileAtLineExternal(path, 1);
            }
        }

        private static string getPath()
        {
            var path = Path.Combine(Application.persistentDataPath, LogWriter.GetLogName());
            if (Application.platform.ToString().ToLower().Contains("windows"))
            {
                path = path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString());
            }
            return path;
        }
    }
}