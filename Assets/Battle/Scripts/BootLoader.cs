using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Util;
using Prg.Scripts.Config;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Altzone.Scripts
{
    public class BootLoader : MonoBehaviour
    {
        [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
        private static void checkDevelopmentStatus()
        {
            // This is just for debugging to get strings (numbers) formatted consistently
            // - everything that goes to UI should go through Localizer using player's locale preferences
            var ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void beforeSceneLoad()
        {
            checkDevelopmentStatus();

            var localDevConfig = ResourceLoader.Get().LoadAsset<LocalDevConfig>(nameof(LocalDevConfig));
            LocalDevConfig.Instance = Instantiate(localDevConfig);

            var folderConfig = ResourceLoader.Get().LoadAsset<FolderConfig>(nameof(FolderConfig));
            var resourceLoader = ResourceLoader.Get(folderConfig.primaryConfigFolder, localDevConfig.developmentConfigFolder);

            var loggerConfig = resourceLoader.LoadAsset<LoggerConfig>(nameof(LoggerConfig));
            LogConfigurator.createLoggerConfig(loggerConfig);

#if UNITY_EDITOR
            if (!Debug.isDebugEnabled)
            {
                UnityEngine.Debug.LogWarning("<b>NOTE!</b> Application logging is totally disabled");
            }
#endif
        }

        private static class LogConfigurator
        {
            public static void createLoggerConfig(LoggerConfig config)
            {
                // Log to file
                if (config.isLogToFile)
                {
                    createLogWriter();
                }
                // Log color
                var trimmed = string.IsNullOrEmpty(config.colorForClassName) ? "" : config.colorForClassName.Trim();
                if (trimmed.Length > 0)
                {
                    Debug.setColorForClassName(trimmed, ref LogWriter.logLineContentFilter);
                }
                // Install log filter as last thing here.
                var filterList = config.buildFilter();
                if (filterList.Count == 0)
                {
                    return;
                }
                Debug.logLineAllowedFilter += (method) =>
                {
                    // For anonymous types we try its parent type.
                    var isAnonymous = (method.ReflectedType?.Name.StartsWith("<"));
                    var type = isAnonymous.HasValue && isAnonymous.Value
                        ? method.ReflectedType?.DeclaringType
                        : method.ReflectedType;
                    var className = type?.FullName;
                    if (className == null)
                    {
                        return false;
                    }
#if UNITY_EDITOR && false
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        foreach (var regex in filterList)
                        {
                            if (regex.regex.IsMatch(className))
                            {
                                UnityEngine.Debug.Log($"MATCH {className} : {regex.regex} = {regex.isLogged}");
                                return regex.isLogged;
                            }
                        }
                        return false;
                    }
#endif
                    var match = filterList.FirstOrDefault(x => x.regex.IsMatch(className));
                    return match?.isLogged ?? false;
                };
            }

            [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
            private static void createLogWriter()
            {
                string filterPhotonLogMessage(string message)
                {
                    // This is mainly to remove "formatting" form Photon ToString and ToStringFull messages!
                    if (!string.IsNullOrEmpty(message))
                    {
                        if (message.Contains("\n") || message.Contains("\r") || message.Contains("\t"))
                        {
                            message = message.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
                        }
                    }
                    return message;
                }

                UnityExtensions.CreateGameObjectAndComponent<LogWriter>(nameof(LogWriter), isDontDestroyOnLoad: true);
                LogWriter.logLineContentFilter += filterPhotonLogMessage;
            }
        }
    }
}