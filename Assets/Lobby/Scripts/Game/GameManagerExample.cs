using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Lobby.Scripts.Game
{
    /// <summary>
    /// Example game manager that uses simple <c>LobbyManager</c> player custom properties protocol to communicate player positions for a game room.
    /// </summary>
    public class GameManagerExample : MonoBehaviour
    {
        private const string playerPositionKey = LobbyManager.playerPositionKey;
        private const string playerMainSkillKey = LobbyManager.playerMainSkillKey;

        public string player1;
        public string player2;
        public string player3;
        public string player4;
        public string spectators;
        public string others;

        private void Update()
        {
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
                PhotonLobby.Get().createRoom(null);
                return;
            }
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.ConnectedToMasterServer)
            {
                // Second join lobby
                PhotonLobby.Get().joinLobby();
                return;
            }
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected)
            {
                // First connect
                PhotonLobby.Get().connect($"Player{DateTime.Now.Second:00}");
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

        private void showPlayersInRooms()
        {
            var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            var countPlayers = 0;
            foreach (var player in players)
            {
                countPlayers += 1;
                var playerPos = player.GetCustomProperty(LobbyManager.playerPositionKey, -1);
                Debug.Log($"Player {player.NickName} found, pos {playerPos}");
                switch (playerPos)
                {
                    case LobbyManager.playerPosition0:
                        player1 += getPlayerInfo(player);
                        break;
                    case LobbyManager.playerPosition1:
                        player2 += getPlayerInfo(player);
                        break;
                    case LobbyManager.playerPosition2:
                        player3 += getPlayerInfo(player);
                        break;
                    case LobbyManager.playerPosition3:
                        player4 += getPlayerInfo(player);
                        break;
                    case LobbyManager.playerIsSpectator:
                        spectators += getPlayerInfo(player);
                        break;
                    default:
                        others += getPlayerInfo(player);
                        break;
                }
            }
            if (countPlayers == 0)
            {
                Debug.LogWarning($"Room {PhotonNetwork.CurrentRoom.Name} has no identified players");
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