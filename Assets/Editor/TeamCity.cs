using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Utility class to perform command line builds.
    /// </summary>
    /// <remarks>
    /// Should be compatible with CI systems.
    /// </remarks>
    internal static class TeamCity
    {
        private const string LOG_PREFIX = nameof(TeamCity);

        private static readonly List<string> logMessages = new List<string>();

        private static string[] _scenes => EditorBuildSettings.scenes
            .Where(x => x.enabled)
            .Select(x => x.path)
            .ToArray();

        [MenuItem("Window/ALT-Zone/Build/Check Android Build")]
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

            // We assume that local keystore and password folder is one level up from current working directory
            // - that should be UNITY project folder
            var keystore = Path.Combine("..", $"local_{getCurrentUser()}", "altzone.keystore");
            configure_Android(keystore);
            Log($"output filename: {getOutputFile(BuildTarget.Android)}");
        }

        [MenuItem("Window/ALT-Zone/Build/Create Build Script")]
        private static void create_Build_Test_Script()
        {
            const string name = "m_BuildScript.bat";
            const string script = @"@echo off
set UNITY=C:\Program Files\Unity\Hub\Editor\2019.4.28f1\Editor\Unity.exe

set BUILDTARGET=%1
if ""%BUILDTARGET%"" == ""Win64"" goto :valid_build
if ""%BUILDTARGET%"" == ""Android"" goto :valid_build
if ""%BUILDTARGET%"" == ""WebGL"" goto :valid_build
echo *
echo * Can not build: invalid build target '%BUILDTARGET%'
echo *
echo * Build target must be one of UNITY command line build target:
echo *
echo *	Win64
echo *	Android
echo *	WebGL
echo *
goto :eof

:valid_build

set PROJECTPATH=./
set METHOD=Editor.TeamCity.build
set LOGFILE=m_Build_%BUILDTARGET%.log
if ""%BUILDTARGET%"" == ""Android"" (
    set ANDROID_KEYSTORE=-keystore ..\local_%USERNAME%\altzone.keystore
)
rem try to simulate TeamCity invocation
set CUSTOM_OPTIONS=%ANDROID_KEYSTORE%
set UNITY_OPTIONS=-batchmode -projectPath %PROJECTPATH% -buildTarget %BUILDTARGET% -executeMethod %METHOD% %CUSTOM_OPTIONS% -quit -logFile ""%LOGFILE%""

echo Start build
echo ""%UNITY%"" %UNITY_OPTIONS%
""%UNITY%"" %UNITY_OPTIONS%
set RESULT=%ERRORLEVEL%
if ""%RESULT%"" == ""0"" (
    echo Build done, check log for results
    goto :eof
)
echo *
echo * Build FAILED with %RESULT%, check log for errors
echo *
";
            File.WriteAllText(name, script);
            UnityEngine.Debug.Log($"Build script '{name}' written");
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
                string outputDir;
                BuildTargetGroup targetGroup;
                switch (args.buildTarget)
                {
                    case BuildTarget.Android:
                        outputDir = Path.Combine("buildAndroid", getOutputFile(args.buildTarget));
                        targetGroup = BuildTargetGroup.Android;
                        configure_Android(args.keystoreName);
                        break;
                    case BuildTarget.WebGL:
                        outputDir = "buildWebGL";
                        targetGroup = BuildTargetGroup.WebGL;
                        break;
                    case BuildTarget.StandaloneWindows64:
                        outputDir = Path.Combine("buildWin64", getOutputFile(args.buildTarget));
                        targetGroup = BuildTargetGroup.Standalone;
                        break;
                    default:
                        throw new UnityException($"build target '{args.buildTarget}' not supported");
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
                var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
                Log($"build result: {buildReport.summary.result}");
                if (buildReport.summary.result != BuildResult.Succeeded)
                {
                    EditorApplication.Exit(1);
                }
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
            string extension;
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

        private static void configure_Android(string keystore)
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

            EditorUserBuildSettings.buildAppBundle = true; // For Google Play this must be always true!
            Log($"buildAppBundle={EditorUserBuildSettings.buildAppBundle}");
            PlayerSettings.Android.useCustomKeystore = true;
            Log($"useCustomKeystore={PlayerSettings.Android.useCustomKeystore}");
            PlayerSettings.Android.keyaliasName = Application.productName.ToLower();
            Log($"keyaliasName={PlayerSettings.Android.keyaliasName}");

            if (!File.Exists(PlayerSettings.Android.keystoreName))
            {
                throw new UnityException($"Keystore file '{keystore}' not found, can not sign without it");
            }

            // Password files must be in same folder where keystore is!
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

        /// <summary>
        /// CommandLine class to parse and hold UNITY standard command line parameters and some custom build parameters.
        /// </summary>
        private class CommandLine
        {
            // Standard UNITY command line parameters.
            public readonly string projectPath;
            public readonly BuildTarget buildTarget;

            // Custom build parameters.
            public readonly string keystoreName;
            public readonly bool isDevelopmentBuild;

            private CommandLine(string projectPath, BuildTarget buildTarget, string keystoreName, bool isDevelopmentBuild)
            {
                this.projectPath = projectPath;
                this.buildTarget = buildTarget;
                this.keystoreName = keystoreName;
                this.isDevelopmentBuild = isDevelopmentBuild;
            }

            public override string ToString()
            {
                return
                    $"{nameof(projectPath)}: {projectPath}, {nameof(buildTarget)}: {buildTarget}, {nameof(keystoreName)}: {keystoreName}, {nameof(isDevelopmentBuild)}: {isDevelopmentBuild}";
            }

            // Build target parameter mapping
            // See: https://docs.unity3d.com/Manual/CommandLineArguments.html
            // See: https://docs.unity3d.com/2019.4/Documentation/ScriptReference/BuildTarget.html
            private static readonly Dictionary<string, BuildTarget> knownBuildTargets = new Dictionary<string, BuildTarget>
            {
                { "Win64", BuildTarget.StandaloneWindows64 },
                { "Android", BuildTarget.Android },
                { "WebGL", BuildTarget.WebGL },
            };

            public static CommandLine Parse(string[] args)
            {
                var projectPath = "./";
                var buildTarget = BuildTarget.StandaloneWindows64;
                var keystore = "";
                var isDevelopmentBuild = false;
                for (var i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];
                    switch (arg)
                    {
                        case "-projectPath":
                            i += 1;
                            projectPath = args[i];
                            break;
                        case "-buildTarget":
                            i += 1;
                            if (!knownBuildTargets.TryGetValue(args[i], out buildTarget))
                            {
                                throw new ArgumentException($"BuildTarget '{args[i]}' is invalid or unsupported");
                            }
                            break;
                        case "-keystore":
                            i += 1;
                            keystore = args[i];
                            break;
                        case "-DevelopmentBuild":
                            isDevelopmentBuild = true;
                            break;
                    }
                }
                return new CommandLine(projectPath, buildTarget, keystore, isDevelopmentBuild);
            }
        }
    }
}