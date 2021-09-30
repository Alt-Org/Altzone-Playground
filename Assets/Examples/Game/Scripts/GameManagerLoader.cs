using Examples.Lobby.Scripts;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Game manager loader to establish well known state before starting actual game manager.
    /// </summary>
    /// <remarks>
    /// Creates a random room and default player properties if there is no room when level is loaded - for simple one player testing.
    /// </remarks>
    public class GameManagerLoader : MonoBehaviour
    {
        public Camera _camera;
        public bool isSetupCamera;
        public GameManager gameManager;

        private bool isDebugSetPlayerPropsSet;
        private bool isDebugSetPlayerPropsWait;

        private void Update()
        {
            if (isDebugSetPlayerPropsWait)
            {
                if (isDebugSetPlayerPropsSet)
                {
                    if (PhotonNetwork.LocalPlayer.GetCustomProperty(LobbyManager.playerPositionKey, -1) != -1)
                    {
                        // Player props should be good to go!
                        isDebugSetPlayerPropsWait = false;
                    }
                }
                else
                {
                    setDebugPlayerProps();
                    isDebugSetPlayerPropsSet = true;
                }
                return;
            }
            if (PhotonNetwork.InRoom)
            {
                // Fourth (or first, normally) is to close room and show our players as an example
                if (PhotonNetwork.IsMasterClient)
                {
                    makeRoomClosed();
                }
                if (isSetupCamera)
                {
                    setupCamera(_camera);
                }
                enabled = false;
                gameManager.Camera = _camera;
                gameManager.enabled = true;
                return;
            }
            // If we enter here level has not been loaded using normal procedure and players/room setup might be totally wrong!
            Debug.LogWarning($"Update: {PhotonNetwork.NetworkClientState}");
            if (PhotonNetwork.InLobby)
            {
                // Third create a random room
                var dummy = new Hashtable();
                PhotonLobby.createRoom("testing");
                return;
            }
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.ConnectedToMasterServer)
            {
                // Second join lobby
                PhotonLobby.joinLobby();
                return;
            }
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected)
            {
                // First connect
                PhotonLobby.connect($"Player{DateTime.Now.Second:00}");
                isDebugSetPlayerPropsWait = true;
            }
        }

        private static void makeRoomClosed()
        {
            if (PhotonNetwork.CurrentRoom.IsOpen)
            {
                Debug.Log($"Close room {PhotonNetwork.CurrentRoom.Name}");
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }

        private static void setupCamera(Camera camera)
        {
            var playerPos = PhotonNetwork.LocalPlayer.GetCustomProperty(LobbyManager.playerPositionKey, -1);
            if (playerPos == 1 || playerPos == 3)
            {
                var cameraTransform = camera.transform;
                cameraTransform.rotation = Quaternion.Euler(0f, 0f, 180f); // Upside down
            }
        }

        [Conditional("UNITY_EDITOR")]
        private static void setDebugPlayerProps()
        {
            var player = PhotonNetwork.LocalPlayer;
            player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { LobbyManager.playerPositionKey, 0 } });
            Debug.Log($"setDebugPlayerProps {player.GetDebugLabel()}");
        }
    }
}