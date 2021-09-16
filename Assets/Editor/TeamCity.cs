using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Editor
{
    internal static class TeamCity
    {
        private static string LOG_PREFIX = nameof(TeamCity);

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
                var buildOptions = BuildOptions.None;
                if (true)
                {
                    buildOptions |= BuildOptions.Development;
                }
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
                {
                    locationPathName = Path.Combine("buildOutput", "altzone.exe"),
                    options = buildOptions,
                    scenes = _scenes,
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                };

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
    }
}