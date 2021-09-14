using System.Collections.Generic;
using UnityEngine;

namespace Prg.Scripts.Window
{
    //[CreateAssetMenu(menuName = "ALT-Zone/WindowConfig")]
    public class WindowConfig : ScriptableObject
    {
        public List<WindowInstance> windows;
    }
}