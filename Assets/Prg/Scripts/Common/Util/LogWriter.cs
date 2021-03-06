using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Prg.Scripts.Common.Util
{
    /// <summary>
    /// Simple file logger that catches all log messages from UNITY and writes them to a file.
    /// </summary>
    public class LogWriter : MonoBehaviour
    {
        public static Func<string, string> logLineContentFilter;

        private static readonly UTF8Encoding FileEncoding = new UTF8Encoding(false, false);
        private static LogWriter _instance;
        private static readonly object _lock = new object();

        [SerializeField] private string fileName;
        private StreamWriter _file;

        // Formatted log messages share this.
        private static readonly StringBuilder Builder = new StringBuilder(500);

        private void Awake()
        {
            if (_instance == null)
            {
                // Register us as the singleton!
                _instance = this;
                return;
            }
            Destroy(this);
            throw new UnityException($"LogWriter already created: {_instance}");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Close();
                _instance = null;
            }
        }

        private void OnEnable()
        {
            var baseName = GetLogName();
            try
            {
                var baseFileName = Path.Combine(Application.persistentDataPath, baseName);
                fileName = baseFileName;
                var retry = 1;
                for (;;)
                {
                    try
                    {
                        // Open for overwrite!
                        _file = new StreamWriter(fileName, false, FileEncoding) { AutoFlush = true };
                        break;
                    }
                    catch (IOException) // Sharing violation if more than one instance at the same time
                    {
                        if (++retry > 10) throw new UnityException("Unable to allocate log file");
                        var newSuffix = $"{retry:D2}_{logFileSuffix}";
                        fileName = baseFileName.Replace(logFileSuffix, newSuffix);
                    }
                }
                // Show effective log filename.
                if (Application.platform.ToString().ToLower().Contains("windows"))
                {
                    fileName = fileName.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString());
                }
                Debug.LogFormat("Logfile {0}", fileName);
                // Capture UNITY Console Logs in separate thread.
                Application.logMessageReceivedThreaded += UnityLogCallback;
            }
            catch (Exception x)
            {
                _file = null;
                UnityEngine.Debug.LogFormat("unable to create log file '{0}'", fileName);
                UnityEngine.Debug.LogException(x);
            }
        }

        private void OnDisable()
        {
            if (_instance != this)
            {
                return;
            }
            Application.logMessageReceivedThreaded -= UnityLogCallback;
        }

        public void OnApplicationQuit()
        {
            Close();
        }

        private void WriteLogInternal(string message)
        {
            if (_file != null)
            {
                _file.WriteLine(message);
                Flush();
            }
        }

        private void Close()
        {
            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }

        private void Flush()
        {
            if (_file != null)
            {
                _file.Flush();
            }
        }

        private static void WriteLog(string message)
        {
            _instance.WriteLogInternal(message);
        }

        private static string _prevLogString = "";
        private static int _prevLogLineCount = 0;

        /* Threaded callback for listening Unity logging */

        private static void UnityLogCallback(string logString, string stackTrace, LogType type)
        {
            lock (_lock)
            {
                if (logString == _prevLogString && type != LogType.Error)
                {
                    // Filter away messages that comes in every frame like:
                    // There are no audio listeners in the scene. Please ensure there is always one audio listener in the scene
                    // Warning	Mesh has more materials (2) than subsets (1)
                    _prevLogLineCount += 1;
                    return;
                }

                if (_prevLogLineCount > 0)
                {
                    WriteLog($"duplicate_lines {_prevLogLineCount}");
                    _prevLogLineCount = 0;
                }
                _prevLogString = logString;
                if (logLineContentFilter != null)
                {
                    // As we can modify the input parameter on the fly we must call each delegate separately with correct input.
                    var invocationList = logLineContentFilter.GetInvocationList();
                    foreach (var callback in invocationList)
                    {
                        logString = callback.DynamicInvoke(logString) as string;
                    }
                }
                // Reset builder
                Builder.Length = 0;

                // File log has timestamp (and optionally category) before message.
                Builder.AppendFormat("{0:HH:mm:ss.fff} ", DateTime.Now);
                if (type != LogType.Log)
                {
                    Builder.Append(type).Append(' ');
                }

                Builder.Append(logString);
                WriteLog(Builder.ToString());
                if (type == LogType.Error || type == LogType.Exception)
                {
                    // Show stack trace only for real errors.
                    if (stackTrace.Length > 5)
                    {
                        Builder.Length = 0;
                        Builder.AppendFormat("{0:HH:mm:ss.fff}\t{1}\t{2}", DateTime.Now, "STACK", stackTrace);
                        WriteLog(Builder.ToString());
                    }
                }
            }
        }

        private const string logFileSuffix = "game.log";

        public static string GetLogName()
        {
            if (!Application.platform.ToString().ToLower().EndsWith("editor"))
            {
                // Delete old files.
                var oldFiles = Directory.GetFiles(Application.persistentDataPath, $"*_{logFileSuffix}");
                var today = DateTime.Now.Day;
                foreach (var oldFile in oldFiles)
                {
                    if (File.GetCreationTime(oldFile).Day != today)
                    {
                        try
                        {
                            File.Delete(oldFile);
                        }
                        catch (IOException)
                        {
                            // NOP - we just swallow it
                        }
                    }
                }
            }
            var platform = Application.platform.ToString().ToLower();
            var prefix = platform.Contains("editor") ? "editor" : platform.Replace("player", "");
            var baseName = Application.productName.ToLower();
            return $"{prefix}_{baseName}_{logFileSuffix}";
        }
    }
}