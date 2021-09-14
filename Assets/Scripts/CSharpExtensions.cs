using System.Reflection;

public static class CSharpExtensions
{
    public static string GetFullName(this MemberInfo method)
    {
        var className = method.ReflectedType?.FullName ?? "UNKNOWN";
        if (className.StartsWith("<"))
        {
            // For anonymous types we try its parent type.
            className = method.ReflectedType?.DeclaringType?.FullName ?? "UNKNOWN";
        }
        return $"{className}.{method.Name}";
    }
}