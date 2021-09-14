using System;
using System.Globalization;

namespace Editor.Prg
{
    public static class EditorCultureInfo
    {
        public static readonly CultureInfo forSorting = new CultureInfo("fi-FI");

        public static StringComparer sortComparer => StringComparer.Create(forSorting, ignoreCase: false);
    }
}
