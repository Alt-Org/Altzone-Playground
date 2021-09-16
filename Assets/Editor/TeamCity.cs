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
                        var localFolder = getLocalFolder(args.localUser);
                        configure_Android(passwordFolder: localFolder, keystoreFolder: localFolder, isAppBundle: true);
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
            var filename = sanitizePath($"{Application.productName}_{Application.version}");
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    return Path.ChangeExtension(filename, "aab");
                case BuildTarget.WebGL:
                    return "buildWebGL";
                case BuildTarget.StandaloneWindows64:
                    return Path.ChangeExtension(filename, "exe");
                default:
                    return "";
            }
        }

        private static string getLocalFolder(string user)
        {
            // For now local folder for passwords etc. should be one level up from current working folder and starting with "local_"
            return Path.Combine("..", $"local_{user}");
        }

        private static void configure_Android(string passwordFolder, string keystoreFolder, bool isAppBundle)
        {
            string getLocalPasswordFor(string filename)
            {
                var file = Path.Combine(passwordFolder, filename);
                if (File.Exists(file))
                {
                    return File.ReadAllLines(file)[0];
                }
                return "--- password file not found ---";
            }

            void logObfuscated(string name, string value)
            {
                string result;
                if (value == null || value.Length < 9)
                {
                    result = "******";
                }
                else
                {
                    result = value.Substring(0, 3) + "******" + value.Substring(value.Length - 3);
                }
                Log($"{name}={result}");
            }

            Log("configure_Android");
            EditorUserBuildSettings.buildAppBundle = isAppBundle;
            Log($"buildAppBundle={EditorUserBuildSettings.buildAppBundle}");
            PlayerSettings.Android.useCustomKeystore = true;
            Log($"useCustomKeystore={PlayerSettings.Android.useCustomKeystore}");

            // Check and configure:
            // - Android.keystoreName + keystorePass
            // - Android.keyaliasName + keyaliasPass
            var keystoreName = Application.productName.ToLower().Replace(" ", "_");
            if (isAppBundle || string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName))
            {
                var keystorePath = Path.Combine(keystoreFolder, $"{keystoreName}.keystore");
                PlayerSettings.Android.keystoreName = keystorePath;
            }
            if (isAppBundle || string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName))
            {
                PlayerSettings.Android.keyaliasName = Application.productName.ToLower();
            }
            Log($"keystoreName={PlayerSettings.Android.keystoreName}");
            Log($"keyaliasName={PlayerSettings.Android.keyaliasName}");

            Log($"passwordFolder={passwordFolder}");
            PlayerSettings.keystorePass = getLocalPasswordFor("keystore_password");
            logObfuscated("keystorePass", PlayerSettings.keystorePass);
            PlayerSettings.keyaliasPass = getLocalPasswordFor("alias_password");
            logObfuscated("keyaliasPass", PlayerSettings.keyaliasPass);

            if (!File.Exists(PlayerSettings.Android.keystoreName))
            {
                throw new UnityException("PlayerSettings.Android.keystoreName must be set (one way or another), can not sign without it");
            }
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
            public readonly string localUser;
            public readonly string projectPath;
            public readonly BuildTarget buildTarget;
            public readonly bool isDevelopmentBuild;

            private CommandLine(string localUser, string projectPath, BuildTarget buildTarget, bool isDevelopmentBuild)
            {
                this.localUser = localUser;
                this.projectPath = projectPath;
                this.buildTarget = buildTarget;
                this.isDevelopmentBuild = isDevelopmentBuild;
            }

            public override string ToString()
            {
                return $"{nameof(localUser)}: {localUser}, {nameof(projectPath)}: {projectPath}, {nameof(buildTarget)}: {buildTarget}, {nameof(isDevelopmentBuild)}: {isDevelopmentBuild}";
            }

            public static CommandLine Parse(string[] args)
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
                    foreach (var key in variables.Keys)
                    {
                        if (key.Equals("COMPUTERNAME"))
                        {
                            return variables[key].ToString();
                        }
                    }
                    return "noname";
                }

                var localUserOverride = "";
                var projectPath = "./";
                var _buildTarget = BuildTarget.StandaloneWindows64.ToString();
                var isDevelopmentBuild = false;
                var iMax = args.Length - 1;
                for (var i = 0; i <= iMax; ++i)
                {
                    var arg = args[i];
                    switch (arg)
                    {
                        case "-username":
                            i += 1;
                            localUserOverride = args[i];
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
                var localUser = !string.IsNullOrEmpty(localUserOverride) ? localUserOverride : getCurrentUser();
                return new CommandLine(localUser, projectPath, buildTarget, isDevelopmentBuild);
            }
        }
    }
}