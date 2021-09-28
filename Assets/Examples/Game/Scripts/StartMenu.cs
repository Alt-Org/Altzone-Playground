using Photon.Pun;
using UiProto.Scripts.Window;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Helper class to load menu or game over etc. level from game play.
    /// </summary>
    /// <remarks>
    /// Do not close room here because loading new level during room close leads to errors from Photon!
    /// </remarks>
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private LevelIdDef mainMenu;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.NickName}");
                GotoMenu();
                enabled = false;
            }
        }

        public void GotoMenu()
        {
            SceneManager.LoadScene(mainMenu.unityName);
        }
    }
}