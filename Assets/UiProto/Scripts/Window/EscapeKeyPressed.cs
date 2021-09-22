using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UiProto.Scripts.Window
{
    /// <summary>
    /// Tracks Escape key press so that is must pressed down and released exactly once (on the same level)!
    /// </summary>
    public class EscapeKeyPressed : MonoBehaviour
    {
        private static EscapeKeyPressed _Instance;

        private bool isEscapePressedDown;
        private bool isEscapePressedUp;
        private string activeScenePathDown;
        private string activeScenePathUp;

        protected void Awake()
        {
            if (_Instance == null)
            {
                // Register us as the singleton!
                _Instance = this;
                return;
            }
            throw new UnityException("Component added more than once: " + nameof(EscapeKeyPressed));
        }

        protected void OnDestroy()
        {
            if (_Instance == this)
            {
                _Instance = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                activeScenePathDown = SceneManager.GetActiveScene().path;
                //Debug.Log($"ESCAPE DOWN level={activeScenePathDown}");
                isEscapePressedDown = true;
                isEscapePressedUp = false;
                return;
            }
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                activeScenePathUp = SceneManager.GetActiveScene().path;
                //Debug.Log($"ESCAPE UP level={activeScenePathUp}");
                isEscapePressedUp = true;
                return;
            }
            if (isEscapePressedDown && isEscapePressedUp)
            {
                isEscapePressedDown = false;
                isEscapePressedUp = false;
                if (activeScenePathDown != activeScenePathUp)
                {
                    Debug.LogWarning($"ESCAPE SKIPPED down={activeScenePathDown} up={activeScenePathUp}");
                    return;
                }
                //Debug.Log($"ESCAPE SENT level={activeScenePathDown}");
                this.Publish(new Event());
            }
        }

        public class Event
        {
            // Event pattern for EscapeKeyPressed
        }
    }
}