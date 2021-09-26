using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts
{
    /// <summary>
    /// Lowest pane in lobby while in room to show current players list that have joined this room.
    /// </summary>
    public class PanePlayerListing : MonoBehaviour, IInRoomCallbacks
    {
        private const string playerPositionKey = LobbyManager.playerPositionKey;
        private const int playerIsGuest = LobbyManager.playerIsGuest;

        [SerializeField] private GameObject contentRoot;
        [SerializeField] private Text textTemplate;

        [SerializeField] private List<Text> textLines;

        private void Start()
        {
            textLines = new List<Text>();
        }

        private void OnEnable()
        {
            if (PhotonNetwork.InRoom)
            {
                updateStatus();
            }
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void updateStatus()
        {
            if (!PhotonNetwork.InRoom)
            {
                deleteExtraLines(textLines, 0);
                return;
            }
            var players = PhotonNetwork.CurrentRoom.GetPlayersByNickName().ToList();
            Debug.Log($"updateStatus {PhotonNetwork.NetworkClientState} lines: {textLines.Count} players: {players.Count}");

            // Synchronize line count with player count.
            while (textLines.Count < players.Count)
            {
                textLines.Add(duplicate(textTemplate));
            }
            deleteExtraLines(textLines, players.Count);
            // Update button captions
            for (var i = 0; i < players.Count; ++i)
            {
                var player = players[i];
                var line = textLines[i];
                update(line, player);
            }
        }

        private Text duplicate(Text template)
        {
            var templateParent = template.gameObject;
            var instance = Instantiate(templateParent, contentRoot.transform);
            instance.SetActive(true);
            var text = instance.GetComponent<Text>();
            Debug.Log("duplicate");
            return text;
        }

        private void update(Text line, Player player)
        {
            var text = line.GetComponent<Text>();
            var name = player.IsLocal ? $"<b>{player.NickName}</b>" : player.NickName;
            var status = $" {playerPositionKey}={player.GetCustomProperty(playerPositionKey, playerIsGuest)}";
            if (player.IsMasterClient)
            {
                status += " [M]";
            }
            var playerText = $"{name} {status}";
            Debug.Log($"update '{text.text}' -> '{playerText}'");
            text.text = playerText;
        }

        private static void deleteExtraLines(List<Text> lines, int linesToKeep)
        {
            while (lines.Count > linesToKeep)
            {
                var lastLine = lines[lines.Count - 1];
                Debug.Log($"Destroy '{lastLine.GetComponent<Text>().text}'");
                if (!lines.Remove(lastLine))
                {
                    throw new UnityException("can not remove line");
                }
                Destroy(lastLine.gameObject);
            }
        }

        void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
        {
            updateStatus();
        }

        void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
        {
            updateStatus();
        }

        void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            // NOP
        }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            updateStatus();
        }

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
        {
            // NOP
        }
    }
}
