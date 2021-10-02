using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Battle.Scripts.Photon
{
    /// <summary>
    /// Helper class to create a room for continuous online mode testing
    /// so that players can join and leave as they wish.
    /// </summary>
    public class PhotonRandomRoom : MonoBehaviour
    {
        // See CustomPropKeyExtensions for original key names
        private const string gameModeKey = "GM";
        private const string spectatorKey = "<s>";
        private const string spectatorCountKey = "SC";

        [Header("Settings"), SerializeField] private string roomName;
        [SerializeField] private bool isOfflineMode;

        [Header("Event Settings")] public UnityEvent roomEstablished;

        [Header("Live Data"), SerializeField] private PhotonWatchdog watchdog;
        [SerializeField] private bool isNetworkStable;

        private LobbyHelper lobbyHelper;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = $"DEMO{DateTime.Now.Month:00}{DateTime.Now.Day:00}";
            }
            PhotonNetwork.OfflineMode = isOfflineMode;
            Debug.Log($"Awake isOfflineMode={PhotonNetwork.OfflineMode} roomName={roomName}");
            watchdog = PhotonWatchdog.Get();
            lobbyHelper = LobbyHelper.Get(new LobbyHelper.KeyNames(gameModeKey, spectatorKey, spectatorCountKey));
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            checkInitialNetworkState();
            watchdog.AddListener(onNetworkEvent);
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
            watchdog.RemoveListener(onNetworkEvent);
        }

        private void checkInitialNetworkState()
        {
            var state = PhotonNetwork.NetworkClientState;
            isNetworkStable =
                state == ClientState.PeerCreated ||
                state == ClientState.Disconnected ||
                state == ClientState.ConnectedToMasterServer;
            if (!isNetworkStable)
            {
                // If we are in lobby or room!
                if (state == ClientState.JoinedLobby)
                {
                    lobbyHelper.leaveLobby(); // We have to leave lobby in order to force refresh room listing
                }
                else if (state == ClientState.Joined)
                {
                    lobbyHelper.leaveRoom(); // Should not come here but in practice we can at least during testing
                }
            }
        }

        private bool isNetworkStateStable()
        {
            var state = PhotonNetwork.NetworkClientState;
            var canConnect = state == ClientState.PeerCreated || state == ClientState.Disconnected;
            var canJoinLobby = state == ClientState.ConnectedToMasterServer;
            return canConnect || canJoinLobby;
        }

        private void onNetworkEvent(PhotonWatchdog.Notify notify, PhotonWatchdog.Verb verb, Player affectedPlayer)
        {
            Debug.Log($"recv {notify} {verb} {affectedPlayer} stable={isNetworkStable}");
            if (!isNetworkStable)
            {
                isNetworkStable = isNetworkStateStable();
                if (!isNetworkStable)
                {
                    return; // Wait until Photon is ready to connect.
                }
            }
            var state = PhotonNetwork.NetworkClientState;
            var canConnect = state == ClientState.PeerCreated || state == ClientState.Disconnected;
            if (canConnect)
            {
                lobbyHelper.connect($"Player{DateTime.Now.Second:00}");
                return;
            }
            var canCreateRoom = state == ClientState.ConnectedToMasterServer;
            if (canCreateRoom && verb != PhotonWatchdog.Verb.OnCreateRoomFailed)
            {
                var roomProperties = new Hashtable
                {
                    { lobbyHelper.keynames.gameModeKey, "0" },
                };
                var lobbyPropertyNames = new[]
                {
                    lobbyHelper.keynames.gameModeKey,
                };
                lobbyHelper.joinOrCreateRoom(roomName, roomProperties, lobbyPropertyNames);
                return;
            }
            if (verb == PhotonWatchdog.Verb.OnCreatedRoom)
            {
                // OK
                Debug.Log($"room created {PhotonNetwork.CurrentRoom.GetDebugLabel()}");
                var room = PhotonNetwork.CurrentRoom;
                var props = new Hashtable
                {
                    { "P0", (byte) 0 },
                    { "P1", (byte) 0 },
                    { "P2", (byte) 0 },
                    { "P3", (byte) 0 },
                };
                room.SetCustomProperties(props);
                return;
            }
            if (verb == PhotonWatchdog.Verb.OnCreateRoomFailed)
            {
                // Should not happen
                Debug.LogWarning("failed to create random room");
                return;
            }
            if (!PhotonNetwork.InRoom)
            {
                return;
            }
            // Room logic follows
            if (verb == PhotonWatchdog.Verb.OnJoinedRoom)
            {
                var room = PhotonNetwork.CurrentRoom;
                var player = PhotonNetwork.LocalPlayer;
                if (!room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "", out var uniquePlayerName))
                {
                    // Make player name unique within this room if it was not!
                    PhotonNetwork.NickName = uniquePlayerName;
                }
            }
            // Online and offline events have different order, try to synchronize this here
            if (verb == PhotonWatchdog.Verb.OnJoinedRoom || verb == PhotonWatchdog.Verb.OnRoomPropertiesUpdate)
            {
                var room = PhotonNetwork.CurrentRoom;
                var gameMode = room.GetCustomProperty<string>(lobbyHelper.keynames.gameModeKey);
                if (gameMode == "0")
                {
                    room.SafeSetCustomProperty(lobbyHelper.keynames.gameModeKey, "1", gameMode);
                }
                else if (gameMode == "1")
                {
                    Debug.Log($"Room is READY isOfflineMode={PhotonNetwork.OfflineMode} roomName={roomName}");
                    roomEstablished.Invoke();
                    enabled = false; // Our job is done now
                }
            }
        }
    }
}