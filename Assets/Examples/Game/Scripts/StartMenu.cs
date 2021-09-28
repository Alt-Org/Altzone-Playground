using Photon.Pun;
using UiProto.Scripts.Window;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Examples.Game.Scripts
{
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private LevelIdDef mainMenu;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.NickName}");
                SceneManager.LoadScene(mainMenu.unityName);
                enabled = false;
            }
        }
    }
}