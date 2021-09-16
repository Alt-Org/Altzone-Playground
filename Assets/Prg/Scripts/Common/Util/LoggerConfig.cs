using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Prg.Scripts.Common.Util
{
    /// <summary>
    /// Debug logger config.
    /// </summary>
    public class LoggerConfig : ScriptableObject
    {
        /// <summary>
        /// Class <c>RegExpFilter</c> contains regexp pattern and flag to include or exclude class name filter patterns from logging.
        /// </summary>
        private class RegExpFilter
        {
            public bool isLogged;
            public Regex regex;
        }

        private const RegexOptions regexOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant;

        [Header("Settings")] public bool isLogToFile;
        public string colorForClassName;

        [Header("Class Filter"), TextArea(5, 20)] public string loggerRules;

        public static void createLoggerConfig(LoggerConfig config)
        {
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
#if UNITY_EDITOR
            if (!Debug.isDebugEnabled)
            {
                UnityEngine.Debug.LogWarning("<b>NOTE!</b> Application logging is totally disabled");
            }
#endif
        }

        [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
        private static void createLogWriter()
        {
            string filterPhotonLogMessage(string message)
            {
                // This is mainly to remove "formatting" form Photon ToString and ToStringFull messages and make then one liners!
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

        private List<RegExpFilter> buildFilter()
        {
            // Note that line parsing relies on TextArea JSON serialization which I have not tested very well!
            // - lines can start and end with "'" if content has something that needs to be "protected" during JSON parsing
            // - JSON multiline separator is LF "\n"
            var list = new List<RegExpFilter>();
            var lines = loggerRules;
            if (lines.StartsWith("'") && lines.EndsWith("'"))
            {
                lines = lines.Substring(1, lines.Length - 2);
            }
            foreach (var token in lines.Split('\n'))
            {
                var line = token.Trim();
                if (line.StartsWith("#"))
                {
                    continue;
                }
                try
                {
                    var isLogged = true;
                    if (line.EndsWith("=1"))
                    {
                        isLogged = true;
                        line = line.Substring(0, line.Length - 2);
                    }
                    else if (line.EndsWith("=0"))
                    {
                        isLogged = false;
                        line = line.Substring(0, line.Length - 2);
                    }
                    else if (line.Contains("="))
                    {
                        UnityEngine.Debug.LogError($"invalid Regex pattern '{line}', do not use '=' here");
                        continue;
                    }
                    var filter = new RegExpFilter
                    {
                        regex = new Regex(line, regexOptions),
                        isLogged = isLogged,
                    };
                    list.Add(filter);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"invalid Regex pattern '{line}': {e.GetType().Name} {e.Message}");
                }
            }
            return list;
        }
    }
}