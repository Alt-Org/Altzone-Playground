using DigitalRuby;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Lobby.Scripts.Game
{
    /// <summary>
    /// Example game manager that uses simple <c>LobbyManager</c> player custom properties protocol to communicate player positions for a game room.
    /// </summary>
    /// <remarks>
    /// Note that we disable us after start as there is no game functionality that goes on after loading this level!
    /// </remarks>
    public class GameManagerExample : MonoBehaviour
    {
        private const string playerPositionKey = LobbyManager.playerPositionKey;
        private const string playerMainSkillKey = LobbyManager.playerMainSkillKey;

        public string[] playerInfo = new string[6];

        public Camera _camera;
        public Transform[] playerStartPos = new Transform[4];
        public GameObject playerPrefab;

        private bool isDebugSetPlayerPropsSet;
        private bool isDebugSetPlayerPropsWait;

        private void Awake()
        {
            if (!PoolManager.ContainsPrefab(playerPrefab.name))
            {
                PoolManager.AddPrefab(playerPrefab.name, playerPrefab);
            }
        }

        private void OnDestroy()
        {
            PoolManager.RecycleActiveObjects();
        }

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
                showPlayersInRooms();
                enabled = false;
                return;
            }
            // If we enter here level has not been loaded using normal procedure and players/room setup might be totally wrong!
            Debug.LogWarning($"Update: {PhotonNetwork.NetworkClientState}");
            if (PhotonNetwork.InLobby)
            {
                // Third create a random room
                var dummy = new Hashtable();
                PhotonLobby.createRoom(null);
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

        private void makeRoomClosed()
        {
            if (PhotonNetwork.CurrentRoom.IsOpen)
            {
                Debug.Log($"Close room {PhotonNetwork.CurrentRoom.Name}");
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }

        private static void createPlayer(Vector3 position, string poolName, bool isUpperTeam)
        {
            Debug.Log($"createPlayer from {poolName} @ {position}");
            var player = PoolManager.CreateFromCache(poolName);
            var _transform = player.transform;
            _transform.position = position;
            if (isUpperTeam)
            {
                _transform.rotation = Quaternion.Euler(0f, 0f, 180f); // Upside down
            }
        }

        [Conditional("UNITY_EDITOR")]
        private static void setDebugPlayerProps()
        {
            var player = PhotonNetwork.LocalPlayer;
            player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { LobbyManager.playerPositionKey, 0 } });
            Debug.Log($"setDebugPlayerProps {player.GetDebugLabel()}");
        }

        private void showPlayersInRooms()
        {
            var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            var countPlayers = 0;
            var localPlayerIndex = -1;
            foreach (var player in players)
            {
                var playerPos = player.GetCustomProperty(LobbyManager.playerPositionKey, -1);
                Debug.Log($"Player {player.NickName} found, pos {playerPos}");
                switch (playerPos)
                {
                    // | ======= |
                    // |  3 |  1 | team 1 upper
                    // | ======= |
                    // |  0 |  2 | team 0 lower
                    // | ======= |
                    case LobbyManager.playerPosition0:
                        countPlayers += 1;
                        playerInfo[0] += getPlayerInfo(player);
                        createPlayer(playerStartPos[0].position, playerPrefab.name, isUpperTeam: false);
                        localPlayerIndex = player.Equals(PhotonNetwork.LocalPlayer) ? 0 : localPlayerIndex;
                        break;
                    case LobbyManager.playerPosition1:
                        countPlayers += 1;
                        playerInfo[1] += getPlayerInfo(player);
                        createPlayer(playerStartPos[1].position, playerPrefab.name, isUpperTeam: true);
                        localPlayerIndex = player.Equals(PhotonNetwork.LocalPlayer) ? 1 : localPlayerIndex;
                        break;
                    case LobbyManager.playerPosition2:
                        countPlayers += 1;
                        playerInfo[2] += getPlayerInfo(player);
                        createPlayer(playerStartPos[2].position, playerPrefab.name, isUpperTeam: false);
                        localPlayerIndex = player.Equals(PhotonNetwork.LocalPlayer) ? 2 : localPlayerIndex;
                        break;
                    case LobbyManager.playerPosition3:
                        countPlayers += 1;
                        playerInfo[3] += getPlayerInfo(player);
                        createPlayer(playerStartPos[3].position, playerPrefab.name, isUpperTeam: true);
                        localPlayerIndex = player.Equals(PhotonNetwork.LocalPlayer) ? 3 : localPlayerIndex;
                        break;
                    case LobbyManager.playerIsSpectator:
                        playerInfo[4] += getPlayerInfo(player);
                        break;
                    default:
                        playerInfo[5] += getPlayerInfo(player);
                        break;
                }
            }
            if (countPlayers == 0)
            {
                Debug.LogError($"Room {PhotonNetwork.CurrentRoom.Name} has no identified players");
            }
            if (localPlayerIndex == 1 || localPlayerIndex == 3)
            {
                var cameraTransform = _camera.transform;
                cameraTransform.rotation = Quaternion.Euler(0f, 0f, 180f); // Upside down
            }
        }

        private static readonly string[] skillNames = { "---", "Des", "Def", "Int", "Pro", "Ret", "Ego", "Con" };

        private static string getPlayerInfo(Player player)
        {
            var pos = player.GetCustomProperty(playerPositionKey, -1);
            var skill = Mathf.Clamp(player.GetCustomProperty(playerMainSkillKey, 0), 0, skillNames.Length - 1);
            var skillName = skillNames[skill];
            var info = $"{player.NickName} pos={pos} skill={skillName} ";
            return info;
        }
    }
}