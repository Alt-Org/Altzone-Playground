#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Util
{
    public class ListMyAssets : MonoBehaviour
    {
        private static readonly string[] excludedFolders =
        {
            "Assets/Photon",
            "Assets/Resources/.*Test"
        };

        [MenuItem("Window/ALT-Zone/Test selected Asset")]
        private static void _CheckSelectedAsset()
        {
            var activeObject = Selection.activeObject;
            if (activeObject == null)
            {
                Debug.Log("Nothing is selected");
                return;
            }
            if (!AssetDatabase.Contains(activeObject))
            {
                Debug.Log($"Selected object is not asset: {activeObject}");
                return;
            }
            var builder = new StringBuilder();
            var message = $"Selected {activeObject}";
            builder.Append(message).AppendLine();
            Debug.Log(message);
            var assetPath = AssetDatabase.GetAssetPath(activeObject);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            message = $"{guid}\t{assetPath}";
            builder.Append(message).AppendLine();
            Debug.Log(message);
            var dependencies = AssetDatabase.GetDependencies(assetPath, recursive: true);
            if (dependencies.Length == 1 && dependencies[0] == assetPath)
            {
                Debug.Log($"Selected object has no external dependencies");
                return;
            }
            message = $"dependencies {dependencies.Length - 1}";
            builder.Append(message).AppendLine();
            Debug.Log(message);
            for (var i = 0; i < dependencies.Length; ++i)
            {
                var dependency = dependencies[i];
                var depGuid = AssetDatabase.AssetPathToGUID(dependency);
                if (depGuid == guid)
                {
                    continue; // Skip myself
                }
                message = $"{i}\t{depGuid}\t{dependency}";
                if (i < 10 && dependencies.Length < 10)
                {
                    Debug.Log(message);
                }
                builder.Append(message).AppendLine();
            }
            var path = Path.GetFullPath("asset_dependencies.txt");
            File.WriteAllText(path, builder.ToString());
            UnityEngine.Debug.Log($"Dependencies saved in {path}");
        }

        [MenuItem("Window/ALT-Zone/List my Assets")]
        private static void _ListMyAssets()
        {
            Debug.Log("");
            var excluded = new List<Regex>();
            foreach (var excludedFolder in excludedFolders)
            {
                excluded.Add(new Regex(excludedFolder));
            }

            var stopWatch = Stopwatch.StartNew();
            var folders = AssetDatabase.GetSubFolders("Assets");
            var context = new Context();
            foreach (var folder in folders)
            {
                handleSubFolder(folder, context, excluded);
            }
            stopWatch.Stop();
            var path = Path.GetFullPath("asset_list.txt");
            File.WriteAllText(path, context.getRawText());
            File.WriteAllText("asset_list2.txt", context.getSortedText());
            UnityEngine.Debug.Log(
                $"Project contains {context.folderCount} folders and {context.fileCount} files (took {stopWatch.ElapsedMilliseconds} ms). " +
                $"Excluded {context.excludedColderCount} folders.");
            UnityEngine.Debug.Log($"Report saved in {path}");
        }

        private static void handleSubFolder(string parent, Context context, List<Regex> excluded)
        {
            if (excludedFolders.Contains(parent))
            {
                context.excludedColderCount += 1;
                return;
            }
            context.folderCount += 1;
            string[] guids = AssetDatabase.FindAssets(null, new[] { parent });
            context.add($"{parent}:{guids.Length}");
            foreach (var guid in guids)
            {
                context.fileCount += 1;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var isExclude = false;
                foreach (var regex in excluded)
                {
                    if (regex.IsMatch(path))
                    {
                        Debug.Log($"skip {path} ({regex})");
                        context.excludedColderCount += 1;
                        isExclude = true;
                        break;
                    }
                }
                if (!isExclude)
                {
                    context.add(new AssetInfo(guid, path));
                }
            }
        }

        private class Context
        {
            public int folderCount;
            public int excludedColderCount;
            public int fileCount;

            private readonly StringBuilder builder;
            private readonly List<AssetInfo> assets = new List<AssetInfo>();

            public Context()
            {
                builder = new StringBuilder();
            }

            public void add(string info)
            {
                builder.Append(info).AppendLine();
            }

            public void add(AssetInfo assetInfo)
            {
                if (assets.Contains(assetInfo))
                {
                    Debug.Log($"duplicate {assetInfo}");
                    return;
                }
                builder.Append($"{assetInfo.guid}\t{assetInfo.path}").AppendLine();
                assets.Add(assetInfo);
            }

            public string getRawText()
            {
                return builder.ToString();
            }

            public string getSortedText()
            {
                assets.Sort((a, b) => String.Compare(a.path, b.path, StringComparison.Ordinal));
                return string.Join("\r\n", assets);
            }
        }

        private class AssetInfo : IComparable
        {
            public readonly string guid;
            public readonly string path;
            private readonly string id;

            public AssetInfo(string guid, string path)
            {
                this.guid = guid;
                this.path = path;
                id = $"{guid}:{path}";
            }

            public int CompareTo(object obj)
            {
                if (obj is AssetInfo other)
                {
                    return String.Compare(id, other.id, StringComparison.Ordinal);
                }
                return GetHashCode().CompareTo(obj?.GetHashCode() ?? 0);
            }

            protected bool Equals(AssetInfo other)
            {
                return id == other.id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj.GetType() != this.GetType())
                {
                    return false;
                }
                return Equals((AssetInfo) obj);
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }

            public override string ToString()
            {
                return $"{guid}\t{path}";
            }
        }
    }
}
#endif