using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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

    public static bool isDebugEnabled =>
#if FORCE_LOG || DEVELOPMENT_BUILD
        true;
#else
        false;
#endif

    private static string classNameColorFilter;
    private static string classNameColor;
    private static bool isClassNameColor;

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
    public static void setColorForClassName(string colorName, ref Func<string, string> logLineContentFilter)
    {
        string removeColorFromLogLine(string line)
        {
            // getPrefix will add color to be removed here:
            // [<color={classNameColor}>{className}</color>]
            return line.Replace(classNameColorFilter, "[").Replace("</color>]", "]");
        }

        if (string.IsNullOrWhiteSpace(colorName))
        {
            isClassNameColor = false;
            classNameColor = null;
            classNameColorFilter = null;
            logLineContentFilter -= removeColorFromLogLine;
        }
        else
        {
            isClassNameColor = true;
            classNameColor = colorName;
            classNameColorFilter = $"[<color={classNameColor}>";
            logLineContentFilter += removeColorFromLogLine;
        }
    }

    // Cache methods if method lookup is expensive.
    private static readonly Dictionary<MethodBase, bool> cachedMethods = new Dictionary<MethodBase, bool>();

    /// <summary>
    /// Filters log lines based on method who initiated logging.
    /// </summary>
    public static Func<MethodBase, bool> logLineAllowedFilter;

    [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.Log(message);
        }
        else if (isMethodAllowedForLog(method))
        {
            UnityEngine.Debug.Log($"{getPrefix(method)}{message}");
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
        else if (isMethodAllowedForLog(method))
        {
            UnityEngine.Debug.LogFormat($"{getPrefix(method)}{format}", args);
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

    private static string getPrefix(MemberInfo method)
    {
        var className = method.ReflectedType?.Name ?? nameof(Debug);
        if (className.StartsWith("<"))
        {
            // For anonymous types we try its parent type.
            className = method.ReflectedType?.DeclaringType?.Name ?? nameof(Debug);
        }
        // removeColorFromLogLine will remove this if logged to file
        return isClassNameColor
            ? $"[<color={classNameColor}>{className}</color>] "
            : $"[{className}] ";
    }

    private static bool isMethodAllowedForLog(MethodBase method)
    {
        if (logLineAllowedFilter != null)
        {
            if (cachedMethods.TryGetValue(method, out var isMethodAllowed))
            {
                return isMethodAllowed;
            }
            // Invocation list works like OR and it will use short-circuit evaluation.
            var invocationList = logLineAllowedFilter.GetInvocationList();
            foreach (var callback in invocationList)
            {
                var result = callback.DynamicInvoke(method);
                if (result is bool isAllowed && isAllowed)
                {
                    cachedMethods.Add(method, true);
                    return true;
                }
            }
            // Nobody accepted so it is rejected.
            cachedMethods.Add(method, false);
            return false;
        }
        return true;
    }

    #region Console log colors

    public static string white(string text)
    {
        return $"<color=white>{text}</color>";
    }

    public static string red(string text)
    {
        return $"<color=red>{text}</color>";
    }

    public static string magenta(string text)
    {
        return $"<color=magenta>{text}</color>";
    }

    public static string yellow(string text)
    {
        return $"<color=yellow>{text}</color>";
    }

    public static string brown(string text)
    {
        return $"<color=brown>{text}</color>";
    }

    #endregion
}