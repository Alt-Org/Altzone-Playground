using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Editor.Prg.Util
{
    public static class EditorUtil
    {
        public static List<string> getSceneNames()
        {
            var levelNames = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes.Where(x => x.enabled))
            {
                // We use just the level name without path and extension, duplicate level names should not be used
                var tokens = scene.path.Split('/');
                var levelName = tokens[tokens.Length - 1].Split('.')[0];
                levelNames.Add(levelName);
            }
            return levelNames;
        }
    }
}