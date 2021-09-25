using Photon.Pun;
using System.Linq;
using UnityEngine;

namespace Lobby.Scripts.Game
{
    /// <summary>
    /// Example game manager that uses simple <c>LobbyManager</c> player custom properties protocol to communicate player positions for a game room.
    /// </summary>
    public class GameManagerExample : MonoBehaviour
    {
        public string player1;
        public string player2;
        public string player3;
        public string player4;
        public string spectators;
        public string others;

        private void Start()
        {
            if (!PhotonNetwork.InRoom)
            {
                Debug.LogWarning($"Not in room: {PhotonNetwork.NetworkClientState}");
                enabled = false;
                return;
            }
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.IsOpen)
            {
                Debug.Log($"Close room {PhotonNetwork.CurrentRoom.Name}");
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            var countPlayers = 0;
            foreach (var player in players)
            {
                countPlayers += 1;
                var playerPos = player.GetCustomProperty(LobbyManager.playerPositionKey, -1);
                Debug.Log($"{player.NickName} found in {playerPos}");
                switch (playerPos)
                {
                    case LobbyManager.playerPosition0:
                        player1 += $"{player.NickName} ";
                        break;
                    case LobbyManager.playerPosition1:
                        player2 += $"{player.NickName} ";
                        break;
                    case LobbyManager.playerPosition2:
                        player3 += $"{player.NickName} ";
                        break;
                    case LobbyManager.playerPosition3:
                        player4 += $"{player.NickName} ";
                        break;
                    case LobbyManager.playerIsSpectator:
                        spectators += $"{player.NickName} ";
                        break;
                    default:
                        others += $"{player.NickName} ";
                        break;
                }
            }
            if (countPlayers == 0)
            {
                Debug.LogWarning($"Room {PhotonNetwork.CurrentRoom.Name} has no identified players");
            }
        }
    }
}