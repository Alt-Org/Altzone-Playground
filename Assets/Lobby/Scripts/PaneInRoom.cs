﻿using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts
{
    /// <summary>
    /// Shows current player "state" in a room.
    /// </summary>
    public class PaneInRoom : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Button[] buttons;

        private ClientState curClientState;
        private void Start()
        {
            var room = PhotonNetwork.CurrentRoom;
            title.text = room?.Name ?? "Not in room";
            buttons[0].onClick.AddListener(() => this.Publish(new LobbyManager.Event(LobbyManager.playerIsGuest)));
            buttons[1].onClick.AddListener(() => this.Publish(new LobbyManager.Event(LobbyManager.playerIsSpectator)));
        }

        private void Update()
        {
            if (curClientState == PhotonNetwork.NetworkClientState)
            {
                return;
            }
            curClientState = PhotonNetwork.NetworkClientState;
            var room = PhotonNetwork.CurrentRoom;
            title.text = room?.Name ?? "Not in room";
        }
    }
}
