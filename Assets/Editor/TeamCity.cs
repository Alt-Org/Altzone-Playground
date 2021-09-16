using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class TeamCity
    {
        private const string LOG_PREFIX = nameof(TeamCity);

        private static readonly List<string> logMessages = new List<string>();

        private static string[] _scenes => EditorBuildSettings.scenes
            .Where(x => x.enabled)
            .Select(x => x.path)
            .ToArray();

        [MenuItem("Window/ALT-Zone/Test/Check Android Build")]
        private static void check_Android_Build()
        {
            string getCurrentUser()
            {
                var variables = Environment.GetEnvironmentVariables();
                foreach (var key in variables.Keys)
                {
                    if (key.Equals("USERNAME"))
                    {
                        return variables[key].ToString();
                    }
                }
                throw new ArgumentException("Environment variable 'USERNAME' not found");
            }

            var keystore = Path.Combine("..", $"local_{getCurrentUser()}", "altzone.keystore");
            configure_Android(keystore, isAppBundle: true);
            Log($"output filename: {getOutputFile(BuildTarget.Android)}");
        }

        internal static void build()
        {
            try
            {
                dumpEnvironment();
                var args = CommandLine.Parse(Environment.GetCommandLineArgs());
                Log($"build with args: {args}");
                var buildOptions = BuildOptions.None;
                if (args.isDevelopmentBuild)
                {
                    buildOptions |= BuildOptions.Development;
                }
                var outputDir = "";
                var targetGroup = BuildTargetGroup.Unknown;
                switch (args.buildTarget)
                {
                    case BuildTarget.Android:
                        outputDir = Path.Combine("buildAndroid", getOutputFile(args.buildTarget));
                        targetGroup = BuildTargetGroup.Android;
                        configure_Android(args.keystore, isAppBundle: true);
                        break;
                    case BuildTarget.WebGL:
                        outputDir = "buildWebGL";
                        targetGroup = BuildTargetGroup.WebGL;
                        break;
                    case BuildTarget.StandaloneWindows64:
                        outputDir = Path.Combine("buildWindows64", getOutputFile(args.buildTarget));
                        targetGroup = BuildTargetGroup.Standalone;
                        break;
                }
                // Output (artifacts) should be inside project folder for CI systems to find them
                var buildPlayerOptions = new BuildPlayerOptions
                {
                    locationPathName = Path.Combine(args.projectPath, outputDir),
                    options = buildOptions,
                    scenes = _scenes,
                    target = args.buildTarget,
                    targetGroup = targetGroup,
                };

                Log($"build output: {buildPlayerOptions.locationPathName}");
                BuildPipeline.BuildPlayer(buildPlayerOptions);
            }
            finally
            {
                if (logMessages.Count > 0)
                {
                    // Show logged messages without call stack for convenience!
                    UnityEngine.Debug.Log($"{LOG_PREFIX} LOG_MESSAGES:\r\n{string.Join("\r\n", logMessages)}");
                }
            }
        }

        private static string getOutputFile(BuildTarget buildTarget)
        {
            if (buildTarget == BuildTarget.WebGL)
            {
                return "buildWebGL";
            }
            var extension = "";
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    extension = "aab";
                    break;
                case BuildTarget.StandaloneWindows64:
                    extension = "exe";
                    break;
                default:
                    throw new UnityException($"getOutputFile: build target '{buildTarget}' not supported");
            }
            var filename = sanitizePath($"{Application.productName}_{Application.version}_{PlayerSettings.Android.bundleVersionCode}.{extension}");
            return filename;
        }

        private static void configure_Android(string keystore, bool isAppBundle)
        {
            string getLocalPasswordFor(string folder, string filename)
            {
                var file = Path.Combine(folder, filename);
                if (File.Exists(file))
                {
                    return File.ReadAllLines(file)[0];
                }
                throw new UnityException($"getLocalPasswordFor: file '{file}' not found");
            }

            void logObfuscated(string name, string value)
            {
                var result = (value == null || value.Length < 9)
                    ? "******"
                    : value.Substring(0, 3) + "******" + value.Substring(value.Length - 3);
                Log($"{name}={result}");
            }

            // Enable application signing with a custom keystore!
            // - Android.keystoreName : as command line parameter
            // - keystorePass : read from keystore folder
            // - Android.keyaliasName : product name in lowercase
            // - keyaliasPass : read from keystore folder

            Log("configure_Android");
            PlayerSettings.Android.keystoreName = keystore;
            Log($"keystoreName={PlayerSettings.Android.keystoreName}");

            EditorUserBuildSettings.buildAppBundle = isAppBundle; // For Google Play this must be always true!
            Log($"buildAppBundle={EditorUserBuildSettings.buildAppBundle}");
            PlayerSettings.Android.useCustomKeystore = true;
            Log($"useCustomKeystore={PlayerSettings.Android.useCustomKeystore}");
            PlayerSettings.Android.keyaliasName = Application.productName.ToLower();
            Log($"keyaliasName={PlayerSettings.Android.keyaliasName}");

            if (!File.Exists(PlayerSettings.Android.keystoreName))
            {
                throw new UnityException($"Keystore file '{keystore}' not found, can not sign without it");
            }

            var passwordFolder = Path.GetDirectoryName(keystore);
            Log($"passwordFolder={passwordFolder}");
            PlayerSettings.keystorePass = getLocalPasswordFor(passwordFolder, "keystore_password");
            logObfuscated("keystorePass", PlayerSettings.keystorePass);
            PlayerSettings.keyaliasPass = getLocalPasswordFor(passwordFolder, "alias_password");
            logObfuscated("keyaliasPass", PlayerSettings.keyaliasPass);
        }

        private static string sanitizePath(string path)
        {
            // https://www.mtu.edu/umc/services/websites/writing/characters-avoid/
            var illegalCharacters = new[]
            {
                '#', '<', '$', '+',
                '%', '>', '!', '`',
                '&', '*', '\'', '|',
                '{', '?', '"', '=',
                '}', '/', ':',
                '\\', ' ', '@',
            };
            for (var i = 0; i < path.Length; ++i)
            {
                var c = path[i];
                if (illegalCharacters.Contains(c))
                {
                    path = path.Replace(c, '_');
                }
            }
            return path;
        }

        private static void dumpEnvironment()
        {
            var variables = Environment.GetEnvironmentVariables();
            var keys = variables.Keys.Cast<string>().ToList();
            keys.Sort();
            Log($"GetEnvironmentVariables: {variables.Count}");
            foreach (var key in keys)
            {
                var value = variables[key];
                Log($"{key}={value}");
            }
        }

        private static void Log(string message)
        {
            UnityEngine.Debug.Log($"{LOG_PREFIX} {message}");
            logMessages.Add(message);
        }

        private class CommandLine
        {
            public readonly string keystore;
            public readonly string projectPath;
            public readonly BuildTarget buildTarget;
            public readonly bool isDevelopmentBuild;

            private CommandLine(string keystore, string projectPath, BuildTarget buildTarget, bool isDevelopmentBuild)
            {
                this.keystore = keystore;
                this.projectPath = projectPath;
                this.buildTarget = buildTarget;
                this.isDevelopmentBuild = isDevelopmentBuild;
            }

            public override string ToString()
            {
                return
                    $"{nameof(keystore)}: {keystore}, {nameof(projectPath)}: {projectPath}, {nameof(buildTarget)}: {buildTarget}, {nameof(isDevelopmentBuild)}: {isDevelopmentBuild}";
            }

            public static CommandLine Parse(string[] args)
            {
                var keystore = "";
                var projectPath = "./";
                var _buildTarget = BuildTarget.StandaloneWindows64.ToString();
                var isDevelopmentBuild = false;
                var iMax = args.Length - 1;
                for (var i = 0; i <= iMax; ++i)
                {
                    var arg = args[i];
                    switch (arg)
                    {
                        case "-keystore":
                            i += 1;
                            keystore = args[i];
                            break;
                        case "-buildTarget":
                            i += 1;
                            _buildTarget = args[i];
                            break;
                        case "-DevelopmentBuild":
                            isDevelopmentBuild = true;
                            break;
                    }
                }
                // https://docs.unity3d.com/2019.4/Documentation/ScriptReference/BuildTarget.html
                if (!Enum.TryParse<BuildTarget>(_buildTarget, true, out var buildTarget))
                {
                    if (_buildTarget == "Win64")
                    {
                        buildTarget = BuildTarget.StandaloneWindows64; // Patch TeamCity predefined "Build target" selection
                    }
                    else
                    {
                        Log($"Invalid BuildTarget: {_buildTarget}");
                        throw new ArgumentException($"Invalid BuildTarget: {_buildTarget}");
                    }
                }
                return new CommandLine(keystore, projectPath, buildTarget, isDevelopmentBuild);
            }
        }
    }
}