using System.Reflection;
using UnityEngine;

namespace Jarin.Scripts
{
    public static class LocalPlayerSettings
    {
        public static string PlayerName
        {
            get => PlayerPrefs.GetString(_propName(MethodBase.GetCurrentMethod()));
            set => PlayerPrefs.SetString(_propName(MethodBase.GetCurrentMethod()), value);
        }

        private static string _propName(MemberInfo method)
        {
            return method.Name.Substring(4); // Strip get_ or set_ prefixes
        }
    }
}