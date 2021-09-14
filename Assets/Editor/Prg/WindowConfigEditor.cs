using Prg.Scripts.Window;
using System;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg
{
    [CustomEditor(typeof(WindowConfig))]
    public class WindowConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                return;
            }
            var windowConfig = (WindowConfig) target;
            if (GUILayout.Button("Sort Window Config by 'Window Name'"))
            {
                serializedObject.Update();
                windowConfig.windows.Sort((a, b) => EditorCultureInfo.sortComparer.Compare(a.window.windowName, b.window.windowName));
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Sort Window Config by 'Prefab Name'"))
            {
                serializedObject.Update();
                windowConfig.windows.Sort((a, b) => string.Compare(a.windowTemplateName, b.windowTemplateName, StringComparison.Ordinal));
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Sort Window Config by 'Id'"))
            {
                serializedObject.Update();
                windowConfig.windows.Sort((a, b) => a.window.windowId.CompareTo(b.window.windowId));
                serializedObject.ApplyModifiedProperties();
            }
            GUILayout.Space(20);
            DrawDefaultInspector();
        }
    }
}