using Photon.Pun;
using UnityEngine;

namespace Lobby.Scripts
{
    /// <summary>
    /// Helper class to switch UI to room view from lobby view.
    /// </summary>
    public class StartRoom: MonoBehaviour
    {
        [SerializeField] private GameObject inLobby;
        [SerializeField] private GameObject inRoom;

        private void Update()
        {
            if (PhotonNetwork.InRoom)
            {
                inLobby.SetActive(false);
                inRoom.SetActive(true);
                enabled = false;
            }
        }
    }
}