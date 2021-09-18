using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Utility class to perform command line builds.
    /// </summary>
    /// <remarks>
    /// Should be compatible with CI systems.<br />
    /// For example TeamCity, Jenkins and CircleCI are some well known CI/CD systems.
    /// </remarks>
    internal static class TeamCity
    {
        private const string LOG_PREFIX = nameof(TeamCity);

        private const string OUTPUT_ANDROID = "buildAndroid";
        private const string OUTPUT_WEBGL = "buildWebGL";
        private const string OUTPUT_WIN64 = "buildWin64";

        private static readonly List<string> logMessages = new List<string>();

        private static string outputBaseFilename => sanitizePath($"{Application.productName}_{Application.version}_{PlayerSettings.Android.bundleVersionCode}");

        private static string[] _scenes => EditorBuildSettings.scenes
            .Where(x => x.enabled)
            .Select(x => x.path)
            .ToArray();

        [MenuItem("Window/ALT-Zone/Build/Check Android Build")]
        private static void check_Android_Build()
        {
            // We assume that local keystore and password folder is one level up from current working directory
            // - that should be UNITY project folder
            var keystore = Path.Combine("..", $"local_{getCurrentUser()}", "altzone.keystore");
            configure_Android(keystore);
            Log($"output filename: {getOutputFile(BuildTarget.Android)}");
        }

        [MenuItem("Window/ALT-Zone/Build/Android Build Post Processing")]
        private static void fix_Android_Build()
        {
            const string scriptName = "m_BuildScript_PostProcess.bat";
            var symbolsName = $"{outputBaseFilename}-{Application.version}-v{PlayerSettings.Android.bundleVersionCode}.symbols";
            var script = MyCmdLineScripts.AndroidPostProcessScript.Replace("<<altzone_symbols_name>>", symbolsName);
            File.WriteAllText(scriptName, script);
            UnityEngine.Debug.Log($"PostProcess script '{scriptName}' written");
        }

        [MenuItem("Window/ALT-Zone/Build/WebGL Build Post Processing")]
        private static void fix_WebGL_Build()
        {
            void patchIndexHtml(string htmlFile, string curTitle, string newTitle)
            {
                var htmlContent = File.ReadAllText(htmlFile);
                var oldTitleText = $"<div class=\"title\">{curTitle}</div>";
                var newTitleText = $"<div class=\"title\">{newTitle}</div>";
                var newHtmlContent = htmlContent.Replace(oldTitleText, newTitleText);
                if (newHtmlContent == htmlContent)
                {
                    Log($"COULD NOT update file {htmlFile}, old title should be '{oldTitleText}'");
                    return;
                }
                Log($"update file {htmlFile}");
                Log($"old html title '{oldTitleText}'");
                Log($"new html title '{newTitleText}'");
                File.WriteAllText(htmlFile, newHtmlContent);
            }

            var indexHtml = Path.Combine(OUTPUT_WEBGL, "index.html");
            var curName = Application.productName;
            var newName = $"{Application.productName} {Application.version} {PlayerSettings.Android.bundleVersionCode}";
            patchIndexHtml(indexHtml, curName, newName);

            const string scriptName = "m_BuildScript_PostProcess.bat";
            File.WriteAllText(scriptName, MyCmdLineScripts.WebGLPostProcessScript);
            UnityEngine.Debug.Log($"PostProcess script '{scriptName}' written");
        }

        [MenuItem("Window/ALT-Zone/Build/Create Build Script")]
        private static void create_Build_Script()
        {
            const string scriptName = "m_BuildScript.bat";
            File.WriteAllText(scriptName, MyCmdLineScripts.BuildScript);
            UnityEngine.Debug.Log($"Build script '{scriptName}' written");
            var buildTargetName = CommandLine.BuildTargetNameFrom(EditorUserBuildSettings.activeBuildTarget);
            var driverName = $"{Path.GetFileNameWithoutExtension(scriptName)}_{buildTargetName}.bat";
            var driverScript = $"{scriptName} {buildTargetName} && pause";
            File.WriteAllText(driverName, driverScript);
            UnityEngine.Debug.Log($"Build script driver '{driverName}' written");
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
                        outputDir = Path.Combine(OUTPUT_ANDROID, getOutputFile(args.buildTarget));
                        targetGroup = BuildTargetGroup.Android;
                        configure_Android(args.keystoreName);
                        break;
                    case BuildTarget.WebGL:
                        outputDir = OUTPUT_WEBGL;
                        targetGroup = BuildTargetGroup.WebGL;
                        break;
                    case BuildTarget.StandaloneWindows64:
                        outputDir = Path.Combine(OUTPUT_WIN64, getOutputFile(args.buildTarget));
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
                if (Directory.Exists(buildPlayerOptions.locationPathName))
                {
                    Directory.Delete(buildPlayerOptions.locationPathName, recursive: true);
                }
                var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
                var summary = buildReport.summary;
                Log($"build result: {summary.result}");
                if (summary.result != BuildResult.Succeeded)
                {
                    EditorApplication.Exit(1);
                }
                // Post processing after successful build
                if (summary.platform == BuildTarget.Android)
                {
                    fix_Android_Build();
                }
                else if (summary.platform == BuildTarget.WebGL)
                {
                    fix_WebGL_Build();
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
            var filename = $"{outputBaseFilename}.{extension}";
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

            // EditorUserBuildSettings
            EditorUserBuildSettings.buildAppBundle = true; // For Google Play this must be always true!
            Log($"buildAppBundle={EditorUserBuildSettings.buildAppBundle}");
            EditorUserBuildSettings.androidCreateSymbolsZip = true;
            Log($"androidCreateSymbolsZip={EditorUserBuildSettings.androidCreateSymbolsZip}");
            EditorUserBuildSettings.androidReleaseMinification = AndroidMinification.Proguard;
            Log($"androidReleaseMinification={EditorUserBuildSettings.androidReleaseMinification}");

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

        private static string getCurrentUser()
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

        private static void dumpEnvironment()
        {
            var variables = Environment.GetEnvironmentVariables();
            var keys = variables.Keys.Cast<string>().ToList();
            keys.Sort();
            var builder = new StringBuilder($"GetEnvironmentVariables: {variables.Count}");
            foreach (var key in keys)
            {
                var value = variables[key];
                builder.AppendLine().Append($"{key}={value}");
            }
            Log(builder.ToString());
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

            public static string BuildTargetNameFrom(BuildTarget buildTarget)
            {
                var pair = knownBuildTargets.FirstOrDefault(x => x.Value == buildTarget);
                return !string.IsNullOrEmpty(pair.Key) ? pair.Key : "Unknown";
            }

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

        /// <summary>
        /// Collection of command line scripts our build "system".
        /// </summary>
        private static class MyCmdLineScripts
        {
            public static string BuildScript => _BuildScript;
            public static string AndroidPostProcessScript => _AndroidPostProcessScript;
            public static string WebGLPostProcessScript => _WebGLPostProcessScript;

            private const string _BuildScript = @"@echo off
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
if not ""%RESULT%"" == ""0"" (
    echo *
    echo * Build FAILED with %RESULT%, check log for errors
    echo *
    goto :eof
)
if not exist m_BuildScript_PostProcess.bat (
    echo Build done, check log for results
    goto :eof
)
echo Build done, start post processing
echo *
call m_BuildScript_PostProcess.bat
echo *
echo Post processing done
";

            private const string _AndroidPostProcessScript = @"@echo off
set BUILD_DIR=BuildAndroid
set DROPBOX_DIR=C:\Users\%USERNAME%\Dropbox\tekstit\altgame\BuildAndroid
set ZIP=C:\Program Files\7-Zip\7z.exe

echo BUILD_DIR=%BUILD_DIR%
echo DROPBOX_DIR=%DROPBOX_DIR%
echo ZIP=%ZIP%

if not exist ""%BUILD_DIR%"" (
    goto :eof
)

if not exist ""%ZIP%"" (
    echo ZIP not found
    goto :dropbox
)
:zip_symbols
set SYMBOLS_STORED=%BUILD_DIR%\<<altzone_symbols_name>>.zip
set SYMBOLS_DEFLATED=%BUILD_DIR%\<<altzone_symbols_name>>.deflated.zip
if not exist ""%SYMBOLS_STORED%"" (
    echo No symbols.zip file found
    goto :dropbox
)

set TEMP_SYMBOLS=%BUILD_DIR%\temp_symbols
echo UNZIP symbols to %TEMP_SYMBOLS%
if exist ""%TEMP_SYMBOLS%"" rmdir /S /Q ""%TEMP_SYMBOLS%""
""%ZIP%"" x -y -o""%TEMP_SYMBOLS%"" ""%SYMBOLS_STORED%""
set RESULT=%ERRORLEVEL%
echo UNZIP result %RESULT%
if not ""%RESULT%"" == ""0"" (
    echo UNZIP symbols failed
    exit /B 1
)

echo ZIP deflate symbols
if exist %SYMBOLS_DEFLATED% del /Q %SYMBOLS_DEFLATED%
""%ZIP%"" a -y -bd ""%SYMBOLS_DEFLATED%"" "".\%TEMP_SYMBOLS%\*""
set RESULT=%ERRORLEVEL%
echo ZIP result %RESULT%
if not ""%RESULT%"" == ""0"" (
    echo ZIP deflate symbols failed
    exit /B 1
)
echo clean up temp dir
if exist ""%SYMBOLS_STORED%"" del /Q ""%SYMBOLS_STORED%""
if exist ""%TEMP_SYMBOLS%"" rmdir /S /Q ""%TEMP_SYMBOLS%""
goto :dropbox

:dropbox
if not exist ""%DROPBOX_DIR%"" (
    goto :eof
)
if ""%LOGFILE%""  == """" (
    set LOGFILE=%0.log
)
robocopy ""%BUILD_DIR%"" ""%DROPBOX_DIR%"" /S /E /V /NP /R:0 /W:0 /LOG+:%LOGFILE%
set RESULT=%ERRORLEVEL%
echo ROBOCOPY result %RESULT%
goto :eof
";

            private const string _WebGLPostProcessScript = @"@echo off
set BUILD_DIR=BuildWebGL
set DROPBOX_DIR=C:\Users\%USERNAME%\Dropbox\tekstit\altgame\BuildWebGL
echo BUILD_DIR=%BUILD_DIR%
echo DROPBOX_DIR=%DROPBOX_DIR%
if not exist %DROPBOX_DIR% (
    goto :eof
)
if ""%LOGFILE%""  == """" (
    set LOGFILE=%0.log
)
robocopy %BUILD_DIR% %DROPBOX_DIR% /S /E /V /NP /R:0 /W:0 /LOG+:%LOGFILE%
goto :eof
";
        }
    }
}