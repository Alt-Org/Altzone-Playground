using Photon.Pun;
using Photon.Realtime;

namespace Prg.Scripts.Common.Photon
{
    public static class PhotonWrapper
    {
        public static string NetworkClientState => PhotonNetwork.NetworkClientState.ToString();

        public static bool InRoom => PhotonNetwork.InRoom;

        public static bool InLobby => PhotonNetwork.InLobby;

        /// <summary>
        /// Can join lobby now?
        /// </summary>
        public static bool IsPhotonReady => isPhotonReady();

        public static void LoadLevel(string levelUnityName)
        {
            PhotonNetwork.LoadLevel(levelUnityName);
        }
        private static bool isPhotonReady()
        {
            var state = PhotonNetwork.NetworkClientState;
            return state == ClientState.PeerCreated || state == ClientState.ConnectedToMasterServer || state == ClientState.Disconnected;
        }
    }
}