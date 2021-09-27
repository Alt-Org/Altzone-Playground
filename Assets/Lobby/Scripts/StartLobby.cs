using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using System;
using UnityEngine;

namespace Lobby.Scripts
{
    /// <summary>
    /// Helper class to enter Photon lobby.
    /// </summary>
    public class StartLobby : MonoBehaviour
    {
        [SerializeField] private GameObject inLobby;
        [SerializeField] private GameObject inRoom;

        private void Start()
        {
            inLobby.SetActive(false);
            inRoom.SetActive(false);
        }

        private void Update()
        {
            if (PhotonNetwork.InLobby)
            {
                inLobby.SetActive(true);
                enabled = false;
                return;
            }
            if (PhotonNetwork.InRoom)
            {
                PhotonLobby.leaveRoom();
                return;
            }
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.ConnectedToMasterServer)
            {
                PhotonLobby.joinLobby();
                return;
            }
            var isOK = state == ClientState.PeerCreated || state == ClientState.Disconnected;
            if (isOK)
            {
                PhotonLobby.connect($"Player{DateTime.Now.Second:00}");
            }
        }
    }
}
