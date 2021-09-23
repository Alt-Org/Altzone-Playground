using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts
{
    /// <summary>
    /// Prepares players in a room for the game play.
    /// </summary>
    public class RoomSetupManager : MonoBehaviour, IInRoomCallbacks
    {
        [SerializeField] private Button buttonPlayerP0;
        [SerializeField] private Button buttonPlayerP1;
        [SerializeField] private Button buttonPlayerP2;
        [SerializeField] private Button buttonPlayerP3;
        [SerializeField] private Button buttonGuest;
        [SerializeField] private Button buttonSpectator;

        private bool interactablePlayerP0;
        private bool interactablePlayerP1;
        private bool interactablePlayerP2;
        private bool interactablePlayerP3;
        private bool interactableGuest;
        private bool interactableSpectator;

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
                return;
            }
            resetState();
            var localPLayer = PhotonNetwork.LocalPlayer;
            checkPlayer(localPLayer, isLocal: true);
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (!player.Equals(localPLayer))
                {
                    checkPlayer(player, isLocal: false);
                }
            }
            setButton(buttonPlayerP0, interactablePlayerP0, captionPlayerP0);
            setButton(buttonPlayerP1, interactablePlayerP1, captionPlayerP1);
            setButton(buttonPlayerP2, interactablePlayerP2, captionPlayerP2);
            setButton(buttonPlayerP3, interactablePlayerP3, captionPlayerP3);
            setButton(buttonGuest, interactableGuest, captionGuest);
            setButton(buttonSpectator, interactableSpectator, captionSpectator);
        }

        private void checkPlayer(Player player, bool isLocal)
        {
            Debug.Log($"checkPlayer {player.ToStringFull()}");
        }

        private void resetState()
        {
            interactablePlayerP0 = true;
            interactablePlayerP1 = true;
            interactablePlayerP2 = true;
            interactablePlayerP3 = true;
            interactableGuest = true;
            interactableSpectator = true;

            captionPlayerP0 = "Player 1";
            captionPlayerP1 = "Player 2";
            captionPlayerP2 = "Player 3";
            captionPlayerP3 = "Player 4";
            captionGuest = "Guest";
            captionSpectator = "Spectator";
        }

        private static void setButton(Button button, bool interactable, string caption)
        {
            button.interactable = interactable;
            button.GetComponentInChildren<Text>().text = interactable
                ? $"<b>{caption}</b>"
                : caption;
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