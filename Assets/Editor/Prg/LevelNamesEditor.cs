using Prg.Scripts.Window;
using System;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg
{
    [CustomEditor(typeof(LevelNames))]
    public class LevelNamesEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                return;
            }
            serializedObject.Update();
            var levelNames = (LevelNames)target;
            if (GUILayout.Button("Sort Level Names by 'Level Name'"))
            {
                levelNames.levels.Sort((a,b) => EditorCultureInfo.sortComparer.Compare(a.levelName, b.levelName));
            }
            if (GUILayout.Button("Sort Level Names by 'Scene Name'"))
            {
                levelNames.levels.Sort((a,b) => string.Compare(a.unityName, b.unityName, StringComparison.Ordinal));
            }
            if (GUILayout.Button("Sort Level Names by 'Id'"))
            {
                levelNames.levels.Sort((a,b) => a.levelId.CompareTo(b.levelId));
            }
            GUILayout.Space(20);
            DrawDefaultInspector();
        }
    }
}
