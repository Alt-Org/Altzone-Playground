using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Test helper to create a room for single player offline mode testing.
    /// </summary>
    public class PhotonOfflineMode : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnEnable()
        {
            if (!PhotonNetwork.OfflineMode && PhotonNetwork.NetworkClientState == ClientState.PeerCreated)
            {
                Debug.Log($"ENABLE OfflineMode {PhotonNetwork.NetworkClientState} {PhotonNetwork.ServerTimestamp}");
                PhotonNetwork.OfflineMode = true;
                PhotonNetwork.JoinRandomRoom();
                return;
            }
            this.enabled = false;
        }

        private void OnDisable()
        {
            if (PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.OfflineMode = false;
                Debug.Log($"DISABLE OfflineMode {PhotonNetwork.NetworkClientState} {PhotonNetwork.ServerTimestamp}");
            }
        }
#endif
    }
}