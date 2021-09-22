using Editor.Prg;
using UiProto.Scripts.Window;
using UnityEditor;
using UnityEngine;

namespace Editor.UiProto
{
    [CustomEditor(typeof(WindowNames))]
    public class WindowNamesEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                return;
            }
            var windowNames = (WindowNames) target;
            if (GUILayout.Button("Sort Window Names by 'Name'"))
            {
                serializedObject.Update();
                windowNames.names.Sort((a, b) => EditorCultureInfo.sortComparer.Compare(a.windowName, b.windowName));
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Sort Window Names by 'Id'"))
            {
                serializedObject.Update();
                windowNames.names.Sort((a, b) => a.windowId.CompareTo(b.windowId));
                serializedObject.ApplyModifiedProperties();
            }
            GUILayout.Space(20);
            DrawDefaultInspector();
        }
    }
}