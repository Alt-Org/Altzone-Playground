using UiProto.Scripts.Window;
using UnityEngine;

namespace Lobby.Scripts
{
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private LevelIdDef mainMenu;

        private void Update()
        {
            if (Input.anyKey)
            {
                SceneLoader.LoadScene(mainMenu.unityName);
                enabled = false;
            }
        }
    }
}
