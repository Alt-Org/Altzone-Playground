using UnityEngine;

namespace Prg.Scripts.Window
{
    public class WindowIsMenuHub : MonoBehaviour
    {
        [Header("Settings"), SerializeField]  private bool _isMenuHub;

        public bool isMenuHub => _isMenuHub;
    }
}
