using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Util
{
    /// <summary>
    /// Example class to show how to access <c>SerializedObject</c> from code.
    /// </summary>
    public class MenuShowVersionInfo : MonoBehaviour
    {
        [MenuItem("Window/ALT-Zone/Show Version Info")]
        private static void ShowVersionInfo()
        {
            // Find out what kind of object you have:
            // var asset0 = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset");
            // Debug.Log("asset=" + asset0);

            // https://docs.unity3d.com/ScriptReference/AssetDatabase.LoadAssetAtPath.html
            // https://docs.unity3d.com/ScriptReference/SerializedProperty.html
            // https://docs.unity3d.com/ScriptReference/SerializedPropertyType.html

            var asset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/ProjectSettings.asset");
            Debug.Log("asset=" + asset);

            // Untyped generic access!
            var serializedObject = new SerializedObject(asset);

            var serializedProp = serializedObject.FindProperty("productName");
            Debug.Log($"{serializedProp.displayName}={serializedProp.stringValue} [{serializedProp.propertyType}]");
            serializedProp = serializedObject.FindProperty("bundleVersion");
            Debug.Log($"{serializedProp.displayName}={serializedProp.stringValue} [{serializedProp.propertyType}]");
            serializedProp = serializedObject.FindProperty("AndroidBundleVersionCode");
            Debug.Log($"{serializedProp.displayName}={serializedProp.intValue} [{serializedProp.propertyType}]");
        }
    }
}