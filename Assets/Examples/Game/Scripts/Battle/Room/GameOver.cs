using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Examples.Game.Scripts.Battle.Room
{
    /// <summary>
    /// Helper class to load game over screen.
    /// </summary>
    /// <remarks>
    /// Do not close room here because loading new level during room close leads to errors from Photon!
    /// </remarks>
    public class GameOver : MonoBehaviourPunCallbacks
    {
        [SerializeField] private UnitySceneName gameOverScene;

        private bool isLoading;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonWrapper.NetworkClientState}");
                gotoMainMenu();
                enabled = false;
            }
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
            if (PhotonNetwork.IsMasterClient)
            {
                gotoMainMenu();
                enabled = false;
            }
        }

        private void gotoMainMenu()
        {
            if (!isLoading)
            {
                isLoading = true;
                SceneManager.LoadScene(gameOverScene.sceneName);
            }
        }
    }
}