using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    public class PhotonDisconnect : MonoBehaviour
    {
        private void Update()
        {
            var state = PhotonNetwork.NetworkClientState;
            var isOK = state == ClientState.PeerCreated || state == ClientState.ConnectedToMasterServer || state == ClientState.Disconnected;
            if (isOK)
            {
                Debug.Log("Photon ready and idle: " + state);
                enabled = false;
                return;
            }
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                return;
            }
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }
        }
    }
}