using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Lobby.Scripts.InRoom
{
    /// <summary>
    /// Lowest pane in lobby while in room to show current players list that have joined this room.
    /// </summary>
    public class PanePlayerListing : MonoBehaviour, IInRoomCallbacks
    {
        private const string playerPositionKey = LobbyManager.playerPositionKey;
        private const string playerMainSkillKey = LobbyManager.playerMainSkillKey;
        private const int playerIsGuest = LobbyManager.playerIsGuest;

        [SerializeField] private Transform contentRoot;
        [SerializeField] private Text textTemplate;

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
            deleteExtraLines(contentRoot);
        }

        private void updateStatus()
        {
            // Use PaneRoomListing.updateStatus() style to manage dynamic text lines - IMHO is has better implementation!
            if (!PhotonNetwork.InRoom)
            {
                deleteExtraLines(contentRoot);
                return;
            }
            var players = PhotonNetwork.CurrentRoom.GetPlayersByNickName().ToList();
            Debug.Log($"updateStatus {PhotonNetwork.NetworkClientState} lines: {contentRoot.childCount} players: {players.Count}");

            // Synchronize line count with player count.
            while (contentRoot.childCount < players.Count)
            {
                addTextLine(contentRoot, textTemplate);
            }
            // Update text lines
            for (var i = 0; i < players.Count; ++i)
            {
                var player = players[i];
                var lineObject = contentRoot.GetChild(i).gameObject;
                lineObject.SetActive(true);
                var line = lineObject.GetComponent<Text>();
                update(line, player);
            }
            // Hide extra lines
            if (contentRoot.childCount > players.Count)
            {
                for (var i = players.Count; i < contentRoot.childCount; ++i)
                {
                    var lineObject = contentRoot.GetChild(i).gameObject;
                    if (lineObject.activeSelf)
                    {
                        lineObject.SetActive(false);
                    }
                }
            }
        }

        private void addTextLine(Transform parent, Text template)
        {
            var templateParent = template.gameObject;
            var instance = Instantiate(templateParent, parent);
            instance.SetActive(true);
        }

        private static readonly string[] skillNames = { "---", "Des", "Def", "Int", "Pro", "Ret", "Ego", "Con" };

        private static void update(Text line, Player player)
        {
            var text = line.GetComponent<Text>();
            var nickName = player.IsLocal ? $"<b>{player.NickName}</b>" : player.NickName;
            var pos = player.GetCustomProperty(playerPositionKey, playerIsGuest);
            var skill = Mathf.Clamp(player.GetCustomProperty(playerMainSkillKey, 0), 0, skillNames.Length - 1);
            var skillName = skillNames[skill];
            var status = $" pos={pos} skill={skillName}";
            if (player.IsMasterClient)
            {
                status += " [M]";
            }
            var playerText = $"{nickName} {status}";
            Debug.Log($"update '{text.text}' -> '{playerText}'");
            text.text = playerText;
        }

        private static void deleteExtraLines(Transform parent)
        {
            var childCount = parent.childCount;
            for (var i = childCount - 1; i >= 0; --i)
            {
                var child = parent.GetChild(i).gameObject;
                Destroy(child);
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