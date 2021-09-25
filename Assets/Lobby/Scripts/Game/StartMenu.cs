using Photon.Pun;
using UiProto.Scripts.Window;
using UnityEngine;

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
                SceneLoader.LoadScene(mainMenu.unityName);
                enabled = false;
            }
        }
    }
}