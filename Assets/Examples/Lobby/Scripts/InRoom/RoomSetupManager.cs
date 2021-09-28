using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Lobby.Scripts.InRoom
{
    /// <summary>
    /// Prepares players in a room for the game play.
    /// </summary>
    public class RoomSetupManager : MonoBehaviour, IInRoomCallbacks
    {
        private const string playerPositionKey = LobbyManager.playerPositionKey;
        private const string playerMainSkillKey = LobbyManager.playerMainSkillKey;
        private const int playerIsGuest = LobbyManager.playerIsGuest;

        [SerializeField] private Button buttonPlayerP0;
        [SerializeField] private Button buttonPlayerP1;
        [SerializeField] private Button buttonPlayerP2;
        [SerializeField] private Button buttonPlayerP3;
        [SerializeField] private Button buttonGuest;
        [SerializeField] private Button buttonSpectator;
        [SerializeField] private Button buttonStartPlay;
        [SerializeField] private int localPlayerPosition;
        [SerializeField] private bool isLocalPlayerPositionUnique;

        private bool interactablePlayerP0;
        private bool interactablePlayerP1;
        private bool interactablePlayerP2;
        private bool interactablePlayerP3;
        private bool interactableGuest;
        private bool interactableSpectator;
        private bool interactableStartPlay;

        private string captionPlayerP0;
        private string captionPlayerP1;
        private string captionPlayerP2;
        private string captionPlayerP3;
        private string captionGuest;
        private string captionSpectator;

        private void OnEnable()
        {
            buttonPlayerP0.interactable = false;
            buttonPlayerP1.interactable = false;
            buttonPlayerP2.interactable = false;
            buttonPlayerP3.interactable = false;
            buttonGuest.interactable = false;
            buttonSpectator.interactable = false;
            buttonStartPlay.interactable = false;
            if (!PhotonNetwork.InRoom)
            {
                return;
            }
            // Reset player custom properties for new game
            var player = PhotonNetwork.LocalPlayer;
            player.CustomProperties.Clear();
            // Guest by default
            player.SetCustomProperties(new Hashtable { { playerPositionKey, LobbyManager.playerIsGuest } });
            // Set random main skill
            var mainSKill = Random.Range(1, 7);
            player.SetCustomProperties(new Hashtable { { playerMainSkillKey, mainSKill } });
            updateStatus();
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
                return;
            }
            resetState();
            // We need local player to check against other players
            var localPLayer = PhotonNetwork.LocalPlayer;
            localPlayerPosition = localPLayer.GetCustomProperty(playerPositionKey, playerIsGuest);
            isLocalPlayerPositionUnique = true;

            // Check other players first is they have reserved some player positions etc. from the room already.
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (!player.Equals(localPLayer))
                {
                    checkOtherPlayer(player);
                }
            }
            checkLocalPlayer(localPLayer);

            setButton(buttonPlayerP0, interactablePlayerP0, captionPlayerP0);
            setButton(buttonPlayerP1, interactablePlayerP1, captionPlayerP1);
            setButton(buttonPlayerP2, interactablePlayerP2, captionPlayerP2);
            setButton(buttonPlayerP3, interactablePlayerP3, captionPlayerP3);
            setButton(buttonGuest, interactableGuest, captionGuest);
            setButton(buttonSpectator, interactableSpectator, captionSpectator);
            setButton(buttonStartPlay, interactableStartPlay, null);
        }

        private void checkOtherPlayer(Player player)
        {
            Debug.Log($"checkOtherPlayer {player.ToStringFull()}");
            if (!player.HasCustomProperty(playerPositionKey))
            {
                return;
            }
            var curValue = player.GetCustomProperty(playerPositionKey, -1);
            if (isLocalPlayerPositionUnique && curValue >= LobbyManager.playerPosition0 && curValue <= LobbyManager.playerPosition3)
            {
                if (curValue == localPlayerPosition)
                {
                    Debug.Log("detected conflict");
                    isLocalPlayerPositionUnique = false; // Conflict with player positions!
                }
            }
            switch (curValue)
            {
                case LobbyManager.playerPosition0:
                    interactablePlayerP0 = false;
                    captionPlayerP0 = player.NickName;
                    break;
                case LobbyManager.playerPosition1:
                    interactablePlayerP1 = false;
                    captionPlayerP1 = player.NickName;
                    break;
                case LobbyManager.playerPosition2:
                    interactablePlayerP2 = false;
                    captionPlayerP2 = player.NickName;
                    break;
                case LobbyManager.playerPosition3:
                    interactablePlayerP3 = false;
                    captionPlayerP3 = player.NickName;
                    break;
            }
        }

        private void checkLocalPlayer(Player player)
        {
            Debug.Log($"checkLocalPlayer {player.ToStringFull()} pos={localPlayerPosition} ok={isLocalPlayerPositionUnique}");
            var curValue = player.GetCustomProperty(playerPositionKey, playerIsGuest);
            // Master client can *only* start the game when in room as player!
            interactableStartPlay = player.IsMasterClient && curValue >= LobbyManager.playerPosition0 && curValue <= LobbyManager.playerPosition3;
            switch (curValue)
            {
                case LobbyManager.playerPosition0:
                    interactablePlayerP0 = false;
                    captionPlayerP0 = $"<color=blue>{player.NickName}</color>";
                    break;
                case LobbyManager.playerPosition1:
                    interactablePlayerP1 = false;
                    captionPlayerP1 = $"<color=blue>{player.NickName}</color>";
                    break;
                case LobbyManager.playerPosition2:
                    interactablePlayerP2 = false;
                    captionPlayerP2 = $"<color=blue>{player.NickName}</color>";
                    break;
                case LobbyManager.playerPosition3:
                    interactablePlayerP3 = false;
                    captionPlayerP3 = $"<color=blue>{player.NickName}</color>";
                    break;
                case LobbyManager.playerIsGuest:
                    interactableGuest = false;
                    captionGuest = $"<color=blue>{player.NickName}</color>";
                    break;
                case LobbyManager.playerIsSpectator:
                    interactableSpectator = false;
                    captionSpectator = $"<color=blue>{player.NickName}</color>";
                    break;
            }
        }

        private void resetState()
        {
            interactablePlayerP0 = true;
            interactablePlayerP1 = true;
            interactablePlayerP2 = true;
            interactablePlayerP3 = true;
            interactableGuest = true;
            interactableSpectator = true;
            interactableStartPlay = false;

            captionPlayerP0 = "Player 1";
            captionPlayerP1 = "Player 2";
            captionPlayerP2 = "Player 3";
            captionPlayerP3 = "Player 4";
            captionGuest = "Guest";
            captionSpectator = "Spectator";
        }

        private static void setButton(Selectable selectable, bool interactable, string caption)
        {
            selectable.interactable = interactable;
            if (!string.IsNullOrEmpty(caption))
            {
                selectable.GetComponentInChildren<Text>().text = interactable
                    ? caption
                    : $"<b>|{caption}|</b>";
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
            updateStatus();
        }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            updateStatus();
        }

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
        {
            updateStatus();
        }
    }
}