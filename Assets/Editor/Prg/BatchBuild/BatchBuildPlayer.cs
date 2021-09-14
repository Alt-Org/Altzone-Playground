using Game.Config;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor.Prg.BatchBuild
{
    internal static class BatchBuildPlayer
    {
        private static string LOG_PREFIX = nameof(BatchBuildPlayer);
        private static string batchBuildPlayerStatusFile = "m_BuildPlayerStatus";

        private static string _projectFolderPath =>
            Directory.GetParent(Application.dataPath)?.FullName ?? "."; // Unity Editor: <path to project folder>/Assets

        private static string[] _scenes => EditorBuildSettings.scenes
            .Where(x => x.enabled)
            .Select(x => x.path)
            .ToArray();

        private static readonly List<string> logMessages = new List<string>();

        internal static void build()
        {
            try
            {
                // batchBuildPlayerStatusFile is build execution status marker file for external control
                // - if it exists after build then most probably something went wrong!
                using (File.Create(batchBuildPlayerStatusFile))
                {
                    _build();
                }
                File.Delete(batchBuildPlayerStatusFile);
            }
            catch (Exception e)
            {
                var message = "BUILD FAILED: " + e.Message;
                Log(message);
                File.WriteAllText(batchBuildPlayerStatusFile, message);
            }
            if (logMessages.Count > 0)
            {
                // Show logged messages without call stack for convenience!
                UnityEngine.Debug.Log($"{LOG_PREFIX} LOG_MESSAGES:\r\n{string.Join("\r\n", logMessages)}");
            }
        }

        private static void _build()
        {
            var projectFolderPath = _projectFolderPath;
            var options = getBuildPlayerOptions(projectFolderPath, _scenes);
            writeOutValue(projectFolderPath, "m_BuildProjectFolderPath", projectFolderPath);
            writeOutValue(projectFolderPath, "m_BuildOutputName", options.locationPathName);
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result == BuildResult.Failed)
            {
                throw new UnityException("Build failed");
            }
            writeBuildSummary(report, options, projectFolderPath);
        }

        private static void writeBuildSummary(BuildReport report, BuildPlayerOptions options, string projectFolderPath)
        {
            var summary = report.summary;
            // Create small summary file for the build
            var totalSeconds = (int) summary.totalTime.TotalSeconds;
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds - minutes * 60;
            var message = new StringBuilder()
                .Append("Build statistics:").AppendLine()
                .Append($"product  {Application.productName}").AppendLine()
                .Append($"version  {Application.version}").AppendLine()
                .Append($"platform {summary.platform}").AppendLine()
                .Append($"size     {summary.totalSize / (1024.0 * 1024.0):F1} MiB").AppendLine()
                .Append($"time     {minutes}:{seconds:00}").AppendLine()
                .Append($"scenes   {options.scenes.Length}").AppendLine()
                .Append($"photon   {PhotonNetwork.PunVersion}").AppendLine()
                .Append($"appid    {getAppIdRealtime()}").AppendLine()
                .Append($"defines  {getScriptingDefineSymbols(options.targetGroup)}").AppendLine()
                .Append($"artefact {getBuildArtefactName(options)}").AppendLine()
                .Append($"flags    {getBuildOptionsFlags(options.options)}").AppendLine()
                .ToString();
            writeOutValue(projectFolderPath, "m_BuildStatistics", message);
            Log(message);
        }

        private static string getBuildOptionsFlags(BuildOptions options)
        {
            var flags = "";
            if ((options & BuildOptions.Development) != 0)
            {
                flags += $"Development({(int)BuildOptions.Development}) ";
            }
            if (flags.Length == 0)
            {
                flags = "None(0)";
            }
            return flags;
        }

        private static string getAppIdRealtime()
        {
            var appSettings = getGameConfigAppSettings();
            if (appSettings == null)
            {
                appSettings = getPhotonAppSettings();
                if (appSettings == null)
                {
                    appSettings = new AppSettings();
                }
            }
            var appIdRealtime = appSettings.AppIdRealtime;
            return string.IsNullOrWhiteSpace(appIdRealtime) ? "not configured" : appIdRealtime;
        }

        private static AppSettings getGameConfigAppSettings()
        {
            var gameConfig = (GameConfig) Resources.Load(nameof(GameConfig), typeof(GameConfig));
            return gameConfig != null && gameConfig.photonAppSettings != null ? gameConfig.photonAppSettings.appSettings : null;
        }

        private static AppSettings getPhotonAppSettings()
        {
            // Sometimes this does not work due to how Photon tries to keep things "simple" for end users
            // and UNITY internal behaviour has changed breaking logic :-(
            var photonServerSettings = (ServerSettings) Resources.Load(PhotonNetwork.ServerSettingsFileName, typeof(ServerSettings));
            return photonServerSettings != null ? photonServerSettings.AppSettings : null;
        }

        private static string getScriptingDefineSymbols(BuildTargetGroup targetGroup)
        {
            var builder = new StringBuilder()
                .Append($"{targetGroup}({(int) targetGroup}):");
            var tokens = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';');
            foreach (var token in tokens)
            {
                // Skip: PHOTON_UNITY_NETWORKING;PUN_2_0_OR_NEWER;PUN_2_OR_NEWER;PUN_2_19_OR_NEWER
                if (token.StartsWith("PHOTON_UNITY_") || token.StartsWith("PUN_2_"))
                {
                    continue;
                }
                builder.Append(' ').Append(token);
            }
            return builder.ToString();
        }

        private static string getBuildArtefactName(BuildPlayerOptions options)
        {
            switch (options.targetGroup)
            {
                case BuildTargetGroup.Android:
                    return Path.GetFileName(options.locationPathName);
                case BuildTargetGroup.WebGL:
                    return "index.html";
                case BuildTargetGroup.Standalone:
                    return Path.GetFileName(options.locationPathName);
                default:
                    return "";
            }
        }

        private static void writeOutValue(string outputFolder, string filename, string textValue)
        {
            File.WriteAllText(Path.Combine(outputFolder, filename), textValue);
        }

        private static BuildPlayerOptions getBuildPlayerOptions(string projectFolder, string[] scenes)
        {
            parseArgs(Environment.GetCommandLineArgs(), out var a);
            Log($"buildTarget={a.buildTarget}");
            Log($"buildNumber={a.buildNumber}");
            Log($"projectFolder={projectFolder}");
            var target = (BuildTarget) Enum.Parse(typeof(BuildTarget), a.buildTarget);
            var targetGroup = BuildTargetGroup.Unknown;
            switch (target)
            {
                case BuildTarget.Android:
                    targetGroup = BuildTargetGroup.Android;
                    configure_Android(a.localFolder, a.isAppBundle);
                    break;
                case BuildTarget.WebGL:
                    targetGroup = BuildTargetGroup.WebGL;
                    break;
                case BuildTarget.StandaloneWindows64:
                    targetGroup = BuildTargetGroup.Standalone;
                    break;
            }
            var filename = target != BuildTarget.WebGL ? $"{Application.productName}.{Application.version}" : "";
            Log($"filename={filename}");
            var locationPathName = getLocationPathName(projectFolder, target, filename, a.isAppBundle);
            Log($"locationPathName={locationPathName}");
            var buildOptions = BuildOptions.None;
            if (a.isDevelopment)
            {
                if (target == BuildTarget.Android && a.isAppBundle)
                {
                    // It seems that Google Play Bundle refuses Development Builds :-(
                    // Use the Internal App Sharing for pre-alpha builds that require Debugging mode:
                    // https://play.google.com/apps/publish/internalappsharing/
                }
                else
                {
                    buildOptions |= BuildOptions.Development;
                }
            }
#if false
            if (a.isBuildReport)
            {
                buildOptions |= BuildOptions.DetailedBuildReport;
            }
#endif
            Log($"BuildOptions={buildOptions}");
            var options = new BuildPlayerOptions()
            {
                locationPathName = locationPathName,
                options = buildOptions,
                scenes = scenes,
                target = target,
                targetGroup = targetGroup,
            };
            Log($"scenes={string.Join(", ", options.scenes)}");
            return options;
        }

        private static string getLocationPathName(string projectFolder, BuildTarget target, string filename, bool isAppBundle)
        {
            var outputFolder = $"Build{target}";
            filename = filename.Replace(".", "_").Replace(" ", "_");
            switch (target)
            {
                case BuildTarget.Android:
                    var extension = isAppBundle ? "aab" : "apk";
                    return Path.Combine(projectFolder, outputFolder, Path.ChangeExtension(filename, extension));
                case BuildTarget.WebGL:
                    return Path.Combine(projectFolder, outputFolder);
                case BuildTarget.StandaloneWindows64:
                    return Path.Combine(projectFolder, outputFolder, Path.ChangeExtension(filename, "exe"));
            }
            throw new UnityException("Unsupported build target: " + target);
        }

        private static void configure_Android(string localFolder, bool isAppBundle)
        {
            string getLocalPasswordFor(string filename)
            {
                var file = Path.Combine(localFolder, filename);
                if (File.Exists(file))
                {
                    return File.ReadAllLines(file)[0];
                }
                return "";
            }

            EditorUserBuildSettings.buildAppBundle = isAppBundle;
            Log($"buildAppBundle={EditorUserBuildSettings.buildAppBundle}");
            PlayerSettings.Android.useCustomKeystore = true;
            Log($"useCustomKeystore={PlayerSettings.Android.useCustomKeystore}");

            // Check and configure:
            // - Android.keystoreName + keystorePass
            // - Android.keyaliasName + keyaliasPass
            Log($"localFolder={localFolder}");
            var keystoreHandle = Application.productName.ToLower().Replace(" ", "_");
            Log($"keystoreHandle={keystoreHandle}");
            if (isAppBundle || string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName))
            {
                var keystoreName = Path.Combine(localFolder, $"{keystoreHandle}.keystore");
                if (!File.Exists(keystoreName))
                {
                    Log($"keystoreName={keystoreName}");
                    throw new UnityException("PlayerSettings.Android.keystoreName must be set (one way or another), can not sign without it");
                }
                PlayerSettings.Android.keystoreName = keystoreName;
            }
            if (isAppBundle || string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName))
            {
                PlayerSettings.Android.keyaliasName = Application.productName.ToLower();
            }
            Log($"keystoreName={PlayerSettings.Android.keystoreName}");
            Log($"keyaliasName={PlayerSettings.Android.keyaliasName}");

            PlayerSettings.keystorePass = getLocalPasswordFor("keystore_password");
            PlayerSettings.keyaliasPass = getLocalPasswordFor("alias_password");
        }

        private class Args
        {
            public string buildTarget;
            public string buildNumber;
            public string localFolder;
            public bool isAppBundle;
            public bool isRebuild;
            public bool isDevelopment;
            public bool isBuildReport; // UNITY 2020 feature
        }

        private static void parseArgs(string[] args, out Args a)
        {
            a = new Args()
            {
                buildTarget = "",
                buildNumber = "0",
                localFolder = "",
                isAppBundle = true, // By default this is true currently as this is not set by build scripts
                isRebuild = false,
                isDevelopment = true, // By default this is true and ypu must explicitly ask for production
                isBuildReport = false,
            };

            var iMax = args.Length - 1;
            for (var i = 0; i <= iMax; ++i)
            {
                var arg = args[i];
                if (arg.ToLower().Equals("-buildTarget".ToLower()) && i < iMax)
                {
                    i += 1;
                    a.buildTarget = args[i];
                }
                else if (arg.ToLower().Equals("-appBundle".ToLower()))
                {
                    a.isAppBundle = true;
                }
                else if (arg.ToLower().Equals("-apk".ToLower()))
                {
                    a.isAppBundle = false;
                }
                else if (arg.ToLower().Equals("-myBuildNumber".ToLower()) && i < iMax)
                {
                    i += 1;
                    a.buildNumber = args[i];
                }
                else if (arg.ToLower().Equals("-myLocalFolder".ToLower()) && i < iMax)
                {
                    i += 1;
                    a.localFolder = args[i];
                }
                else if (arg.ToLower().Equals("-myLogPrefix".ToLower()) && i < iMax)
                {
                    i += 1;
                    LOG_PREFIX = args[i];
                }
                else if (arg.ToLower().Equals("-rebuild".ToLower()))
                {
                    a.isRebuild = true;
                }
                else if (arg.ToLower().Equals("-production".ToLower()))
                {
                    a.isDevelopment = false; // Reverse logic!
                }
#if false
                else if (arg.ToLower().Equals("-buildReport".ToLower()))
                {
                    a.isBuildReport = true;
                }
#endif
            }
        }

        private static void Log(string message)
        {
            UnityEngine.Debug.Log($"{LOG_PREFIX} {message}");
            logMessages.Add(message);
        }
    }
}