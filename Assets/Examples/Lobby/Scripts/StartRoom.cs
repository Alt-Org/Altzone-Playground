using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Examples.Lobby.Scripts
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
            if (PhotonWrapper.InRoom)
            {
                inLobby.SetActive(false);
                inRoom.SetActive(true);
                enabled = false;
            }
        }
    }
}