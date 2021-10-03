using Photon.Pun;
using Prg.Scripts.Common.Unity;
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
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private UnitySceneName mainScene;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.NickName}");
                GotoMainMenu();
                enabled = false;
            }
        }

        public void GotoMainMenu()
        {
            SceneManager.LoadScene(mainScene.sceneName);
        }
    }
}