using Prg.Scripts.Common.Util;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Prg.Util
{
    public class LogWriterMenu : MonoBehaviour
    {
        [MenuItem("Window/ALT-Zone/Util/Editor Log/Show location")]
        private static void Show()
        {
            getPath();
        }

        [MenuItem("Window/ALT-Zone/Util/Editor Log/Open in text editor")]
        private static void Load()
        {
            var path = getPath();
            if (File.Exists(path))
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
            Debug.Log($"Editor log {(File.Exists(path) ? "is" : "NOT")} found in: {path}");
            return path;
        }
    }
}