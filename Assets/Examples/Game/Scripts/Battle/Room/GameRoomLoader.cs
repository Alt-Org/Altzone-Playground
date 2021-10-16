using Examples.Config.Scripts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using System;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Room
{
    /// <summary>
    /// Game room loader to establish well known state if level is loaded directly from Editor.
    /// </summary>
    public class GameRoomLoader : MonoBehaviourPunCallbacks
    {
        public GameObject[] objectsToManage;

        private void Awake()
        {
            if (PhotonNetwork.InRoom)
            {
                // Nothing to do!
                enabled = false;
                return;
            }
            Debug.Log($"Awake: {PhotonNetwork.NetworkClientState}");
            // Disable game objects until room is ready
            Array.ForEach(objectsToManage,x => x.SetActive(false));
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected)
            {
                Debug.Log($"connect: {PhotonNetwork.NetworkClientState}");
                var playerData = RuntimeGameConfig.Get().playerDataCache;
                PhotonLobby.connect(playerData.PlayerName);
                return;
            }
            throw new UnityException($"OnEnable: invalid connection state: {PhotonNetwork.NetworkClientState}");
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log($"joinLobby: {PhotonNetwork.NetworkClientState}");
            PhotonLobby.joinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log($"createRoom: {PhotonNetwork.NetworkClientState}");
            PhotonLobby.createRoom("testing");
        }

        public override void OnJoinedRoom()
        {
            PhotonBattle.setDebugPlayerProps(PhotonNetwork.LocalPlayer);
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"Start game for: {player.GetDebugLabel()}");
            enabled = false;
            // Enable game objects when room is ready to play
            Array.ForEach(objectsToManage,x => x.SetActive(true));
        }
    }
}