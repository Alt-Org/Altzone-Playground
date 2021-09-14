using System;
using System.Collections.Generic;
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
        /// Class <c>RegExpFilter</c> contains regexp pattern and flag for logger to include or exclude class name filter patterns.
        /// </summary>
        public class RegExpFilter
        {
            public bool isLogged;
            public Regex regex;
        }

        private const RegexOptions regexOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant;

        [Header("Settings")] public bool isLogToFile;
        public string colorForClassName;

        [Header("Class Filter"), TextArea(5, 20)] public string loggerRules;

        public List<RegExpFilter> buildFilter()
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