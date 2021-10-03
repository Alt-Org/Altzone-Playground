using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace UiProto.Scripts
{
    public class PhotonDisconnect : MonoBehaviour
    {
        private void Update()
        {
            if (PhotonWrapper.IsPhotonReady)
            {
                Debug.Log("Photon is ready: " + PhotonWrapper.NetworkClientState);
                enabled = false;
                return;
            }
            if (PhotonWrapper.InRoom)
            {
                PhotonLobby.leaveRoom();
                return;
            }
            if (PhotonWrapper.InLobby)
            {
                PhotonLobby.leaveLobby();
            }
        }
    }
}