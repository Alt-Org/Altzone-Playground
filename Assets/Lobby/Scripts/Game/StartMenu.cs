using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UiProto.Scripts.Window;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lobby.Scripts.Game
{
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private LevelIdDef mainMenu;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.NickName}");
                if (PhotonNetwork.InRoom)
                {
                    PhotonLobby.leaveRoom();
                }
                SceneManager.LoadScene(mainMenu.unityName);
                enabled = false;
            }
        }
    }
}