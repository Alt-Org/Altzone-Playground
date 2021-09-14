using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Prg.Scripts.Window
{
    /// <summary>
    /// Handles ESCAPE key press using default WindowManager functionality.
    /// </summary>
    public class EscapeHandler : MonoBehaviour
    {
        private static EscapeHandler _Instance;

        protected void Awake()
        {
            if (_Instance == null)
            {
                // Register us as the singleton!
                _Instance = this;
                gameObject.GetOrAddComponent<EscapeKeyPressed>();
                this.Subscribe<EscapeKeyPressed.Event>(OnEscapeKeyPressed);
                return;
            }
            throw new UnityException("Component added more than once: " + nameof(EscapeHandler));
        }

        protected void OnDestroy()
        {
            if (_Instance == this)
            {
                this.Unsubscribe<EscapeKeyPressed.Event>(OnEscapeKeyPressed);
                _Instance = null;
            }
        }

        private static void OnEscapeKeyPressed(EscapeKeyPressed.Event data)
        {
            WindowManager.SafeGoBack();
        }
    }
}