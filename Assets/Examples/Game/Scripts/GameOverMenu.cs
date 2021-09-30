using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UiProto.Scripts.Window;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Utility to close this room properly before allowing to exit to main menu!
    /// </summary>
    public class GameOverMenu : MonoBehaviour
    {
        [SerializeField] private LevelIdDef mainMenu;
        [SerializeField] private float waitForGracefulExit;

        private bool isExiting;
        private float timeToExitForcefully;

        private void Update()
        {
            if (isExiting)
            {
                GotoMenu();
            }
            else if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.NickName}");
                GotoMenu();
            }
        }

        private void GotoMenu()
        {
            // This is not perfect but will do!
            if (PhotonNetwork.InRoom)
            {
                if (isExiting)
                {
                    if (Time.time > timeToExitForcefully)
                    {
                        PhotonNetwork.Disconnect();
                    }
                }
                else
                {
                    isExiting = true;
                    timeToExitForcefully = Time.time + waitForGracefulExit;
                    PhotonLobby.leaveRoom();
                    return;
                }
            }
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected || state == ClientState.ConnectedToMasterServer)
            {
                Debug.Log($"LoadScene {PhotonNetwork.NetworkClientState} {mainMenu.unityName}");
                SceneManager.LoadScene(mainMenu.unityName);
                enabled = false;
            }
        }
    }
}