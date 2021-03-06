using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Conditional UnityEngine.Debug wrapper for development.
/// </summary>
public static class Debug
{
    // See: https://answers.unity.com/questions/126315/debuglog-in-build.html
    // StackFrame: https://stackoverflow.com/questions/21884142/difference-between-declaringtype-and-reflectedtype
    // Method: https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous

#if UNITY_EDITOR
// no warnings when in Editor
#elif FORCE_LOG || DEVELOPMENT_BUILD
#if DEVELOPMENT_BUILD
#warning NOTE: Compiling development build
#endif
#if FORCE_LOG
#warning NOTE: Compiling WITH debug logging
#endif
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RuntimeInitializeOnLoadMethod()
    {
        // Reset static fields even when Domain Reloading is disabled.
        _isClassNameColor = false;
        _classNameColor = null;
        _classNameColorFilter = null;
        CachedMethods.Clear();
        _logLineAllowedFilter = null;
    }

    private static string _classNameColorFilter;
    private static string _classNameColor;
    private static bool _isClassNameColor;

    // Cache methods if method lookup is expensive.
    private static readonly Dictionary<MethodBase, bool> CachedMethods = new Dictionary<MethodBase, bool>();

    /// <summary>
    /// Filters log lines based on method name or other method properties.
    /// </summary>
    private static Func<MethodBase, bool> _logLineAllowedFilter;

    /// <summary>
    /// Adds log line filter.
    /// </summary>
    [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
    public static void AddLogLineAllowedFilter(Func<MethodBase, bool> filter)
    {
        _logLineAllowedFilter += filter;
    }

    /// <summary>
    /// Sets color for class name field in debug log line.
    /// </summary>
    /// <remarks>
    /// See: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html
    /// and
    /// https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html#ColorNames
    /// </remarks>
    /// <param name="colorName">Unity color name</param>
    /// <param name="logLineContentFilter">log writer filter</param>
    [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
    public static void SetColorForClassName(string colorName, ref Func<string, string> logLineContentFilter)
    {
        string RemoveColorFromLogLine(string line)
        {
            // getPrefix will add color to be removed here:
            // [<color={classNameColor}>{className}</color>]
            return line.Replace(_classNameColorFilter, "[").Replace("</color>]", "]");
        }

        if (string.IsNullOrWhiteSpace(colorName))
        {
            _isClassNameColor = false;
            _classNameColor = null;
            _classNameColorFilter = null;
            logLineContentFilter -= RemoveColorFromLogLine;
        }
        else
        {
            _isClassNameColor = true;
            _classNameColor = colorName;
            _classNameColorFilter = $"[<color={_classNameColor}>";
            logLineContentFilter += RemoveColorFromLogLine;
        }
    }

    [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.Log(message);
        }
        else if (IsMethodAllowedForLog(method))
        {
            UnityEngine.Debug.Log($"{GETPrefix(method)}{message}");
        }
    }

    [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogFormat(string format, params object[] args)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }
        else if (IsMethodAllowedForLog(method))
        {
            UnityEngine.Debug.LogFormat($"{GETPrefix(method)}{format}", args);
        }
    }

    [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(string message, Object context = null)
    {
        UnityEngine.Debug.LogWarning(message, context);
    }

    [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(string message, Object context = null)
    {
        UnityEngine.Debug.LogError(message, context);
    }

    private static string GETPrefix(MemberInfo method)
    {
        var className = method.ReflectedType?.Name ?? nameof(Debug);
        if (className.StartsWith("<"))
        {
            // For anonymous types we try its parent type.
            className = method.ReflectedType?.DeclaringType?.Name ?? nameof(Debug);
        }
        // removeColorFromLogLine will remove this if logged to file
        return _isClassNameColor
            ? $"[<color={_classNameColor}>{className}</color>] "
            : $"[{className}] ";
    }

    private static bool IsMethodAllowedForLog(MethodBase method)
    {
        if (_logLineAllowedFilter != null)
        {
            if (CachedMethods.TryGetValue(method, out var isMethodAllowed))
            {
                return isMethodAllowed;
            }
            // Invocation list works like OR and it will use short-circuit evaluation.
            var invocationList = _logLineAllowedFilter.GetInvocationList();
            foreach (var callback in invocationList)
            {
                var result = callback.DynamicInvoke(method);
                if (result is bool isAllowed && isAllowed)
                {
                    CachedMethods.Add(method, true);
                    return true;
                }
            }
            // Nobody accepted so it is rejected.
            CachedMethods.Add(method, false);
            return false;
        }
        return true;
    }

    #region Console log colors

    public static string White(string text)
    {
        return $"<color=white>{text}</color>";
    }

    public static string Red(string text)
    {
        return $"<color=red>{text}</color>";
    }

    public static string Magenta(string text)
    {
        return $"<color=magenta>{text}</color>";
    }

    public static string Yellow(string text)
    {
        return $"<color=yellow>{text}</color>";
    }

    public static string Brown(string text)
    {
        return $"<color=brown>{text}</color>";
    }

    #endregion
}