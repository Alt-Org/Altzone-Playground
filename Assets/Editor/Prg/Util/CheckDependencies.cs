using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Editor.Prg.Util
{
    public static class CheckDependencies
    {
        [MenuItem("Window/ALT-Zone/Util/Check Dependencies")]
        private static void _CheckDependencies()
        {
            UnityEngine.Debug.Log("*");
            var activeObject = Selection.activeObject;
            if (activeObject == null)
            {
                UnityEngine.Debug.Log("Nothing is selected");
                return;
            }
            var selectedGuids = Selection.assetGUIDs;
            var validExtensions = new[] { ".cs", ".prefab" };
            foreach (var guid in selectedGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.Contains(activeObject))
                {
                    var extension = Path.HasExtension(path) ? Path.GetExtension(path).ToLower() : "";
                    if (validExtensions.Contains(extension))
                    {
                        continue;
                    }
                }
                UnityEngine.Debug.Log($"Selected object is not supported asset: {path}");
                return;
            }
            UnityEngine.Debug.Log($"Search dependencies for {selectedGuids.Length} assets (in scenes, prefabs, ScriptableObjects)");
            const string assetRoot = "Assets";
            var foundCount = new int[selectedGuids.Length];
            Array.Clear(foundCount, 0, foundCount.Length);

            var assetFilters = new[] { "t:Scene", "t:Prefab", "t:ScriptableObject" };
            var totalCount = 0;
            foreach (var assetFilter in assetFilters)
            {
                string[] foundAssets = AssetDatabase.FindAssets(assetFilter, new[] { assetRoot });
                var searchCount = checkForGuidInAssets(selectedGuids, ref foundCount, foundAssets);
                totalCount += searchCount;
                UnityEngine.Debug.Log($"search {assetFilter}:{foundAssets.Length} found={searchCount}");
            }
            UnityEngine.Debug.Log(">");
            var noDepCount = 0;
            for (var i = 0; i < selectedGuids.Length; ++i)
            {
                if (foundCount[i] == 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(selectedGuids[i]);
                    UnityEngine.Debug.Log($"{path} has NO dependencies in this search");
                    noDepCount += 1;
                }
            }
            if (totalCount > 0)
            {
                if (noDepCount > 0)
                {
                    UnityEngine.Debug.Log(">");
                }
                for (var i = 0; i < selectedGuids.Length; ++i)
                {
                    var path = AssetDatabase.GUIDToAssetPath(selectedGuids[i]);
                    var depCount = foundCount[i];
                    var message = depCount > 0
                        ? $"has <b>{depCount} dependencies</b>"
                        : $"does not have <i>any dependencies</i> and <b>can be safely deleted</b>";
                    UnityEngine.Debug.Log($"{path} {message}");
                }
            }
        }

        private static int checkForGuidInAssets(string[] selectedGuids, ref int[] foundCount, string[] assetGuids)
        {
            var count = 0;
            foreach (var assetGuid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                var assetContent = File.ReadAllText(path);
                for (var guidIndex = 0; guidIndex < selectedGuids.Length; ++guidIndex)
                {
                    var guid = selectedGuids[guidIndex];
                    if (assetContent.Contains(guid))
                    {
                        var source = AssetDatabase.GUIDToAssetPath(guid);
                        UnityEngine.Debug.Log($"{source} found in {path}");
                        foundCount[guidIndex] += 1;
                        count += 1;
                    }
                }
            }
            return count;
        }
    }
}