using Examples.Config.Scripts;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Game room loader to establish well known state if level is loaded directly from Editor.
    /// </summary>
    public class GameRoomLoader : MonoBehaviour
    {
        public GameObject gameManager;
        public GameObject ball;

        private bool isDebugSetPlayerPropsSet;
        private bool isDebugSetPlayerPropsWait;

        private void Awake()
        {
            Debug.Log($"Awake: {PhotonNetwork.NetworkClientState}");
            if (PhotonNetwork.InRoom)
            {
                // Nothing to do!
                enabled = false;
                return;
            }
            // Disable game objects until room is ready
            gameManager.SetActive(false);
            ball.SetActive(false);
        }

        private void Update()
        {
            if (isDebugSetPlayerPropsWait)
            {
                if (isDebugSetPlayerPropsSet)
                {
                    PhotonBattle.getPlayerProperties(PhotonNetwork.LocalPlayer, out var playerPos, out var teamIndex);
                    if (playerPos != -1)
                    {
                        // Player props should be good to go!
                        isDebugSetPlayerPropsWait = false;
                    }
                }
                else
                {
                    PhotonBattle.setDebugPlayerProps();
                    isDebugSetPlayerPropsSet = true;
                }
                return;
            }
            if (PhotonNetwork.InRoom)
            {
                var player = PhotonNetwork.LocalPlayer;
                Debug.Log($"Start: {PhotonNetwork.NetworkClientState} with {player.GetDebugLabel()}");
                enabled = false;
                // Enable game objects when room is ready to play
                gameManager.SetActive(true);
                ball.SetActive(true);
                return;
            }
            if (PhotonNetwork.InLobby)
            {
                Debug.LogWarning($"createRoom: {PhotonNetwork.NetworkClientState}");
                PhotonLobby.createRoom("testing");
                // Must wait until player props are set before we can continue!
                isDebugSetPlayerPropsWait = true;
                return;
            }
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.ConnectedToMasterServer)
            {
                Debug.LogWarning($"joinLobby: {PhotonNetwork.NetworkClientState}");
                PhotonLobby.joinLobby();
                return;
            }
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected)
            {
                Debug.LogWarning($"connect: {PhotonNetwork.NetworkClientState}");
                var playerData = RuntimeGameConfig.Get().playerDataCache;
                PhotonLobby.connect(playerData.PlayerName);
            }
        }
    }
}