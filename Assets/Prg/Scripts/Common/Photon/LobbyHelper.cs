using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Unity;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Photon
{
    public class LobbyHelper
    {
        public class KeyNames
        {
            public readonly string gameModeKey;
            public readonly string spectatorKey;
            public readonly string spectatorCountKey;

            public KeyNames(string gameModeKey, string spectatorKey, string spectatorCountKey)
            {
                this.gameModeKey = gameModeKey;
                this.spectatorKey = spectatorKey;
                this.spectatorCountKey = spectatorCountKey;
            }
        }

        private const int maxRoomNameLength = 16;
        private const int maxSpectatorsInRoom = 2;

        private static bool isApplicationQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void afterSceneLoad()
        {
            Application.quitting += () => isApplicationQuitting = true;
        }

        [Obsolete("This class is obsolete. Use PhotonLobby instead and handle spectators separately.")]
        public static LobbyHelper Get(KeyNames keynames)
        {
            return new LobbyHelper(keynames);
        }

        public readonly KeyNames keynames;

        private LobbyHelper(KeyNames keynames)
        {
            this.keynames = keynames;
        }

        public void connect(string playerName, bool isAutomaticallySyncScene = true)
        {
            if (isApplicationQuitting)
            {
                return;
            }
            SequenceDiagram.receive(nameof(LobbyHelper), SD.CONNECT);
            if (PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.OfflineMode = false;
                Debug.Log($"Fix OfflineMode: {PhotonNetwork.OfflineMode}");
            }
            if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
            {
                var photonAppSettings = ResourceLoader.Get().LoadAsset<PhotonAppSettings>(nameof(PhotonAppSettings));
                var appSettings = photonAppSettings != null ? photonAppSettings.appSettings : null;
                var gameVersion = Application.version;
                connectUsingSettings(appSettings, gameVersion, playerName, isAutomaticallySyncScene);
                return;
            }
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }

        public void disconnect()
        {
            SequenceDiagram.receive(nameof(LobbyHelper), SD.DISCONNECT);
            PhotonNetwork.Disconnect();
        }

        public void joinLobby()
        {
            if (isApplicationQuitting)
            {
                return;
            }
            SequenceDiagram.receive(nameof(LobbyHelper), SD.JOIN_LOBBY);
            if (PhotonNetwork.OfflineMode)
            {
                throw new UnityException("PhotonNetwork.OfflineMode not allowed here");
            }
            if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
            {
                Debug.Log(
                    $"JoinLobby {PhotonNetwork.NetworkClientState} scene={SceneManager.GetActiveScene().name} GameVersion={PhotonNetwork.GameVersion}");
                PhotonNetwork.JoinLobby();
                return;
            }
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }

        public void leaveLobby()
        {
            SequenceDiagram.receive(nameof(LobbyHelper), SD.LEAVE_LOBBY);
            if (PhotonNetwork.InLobby)
            {
                Debug.Log("LeaveLobby");
                PhotonNetwork.LeaveLobby();
                return;
            }
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }

        public void createRoom(string roomName, RoomOptions roomOptions)
        {
            if (isApplicationQuitting)
            {
                return;
            }
            SequenceDiagram.receive(nameof(LobbyHelper), SD.CREATE_ROOM);
            if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = null; // Let Photon generate room name us to ensure that room creation succeeds
            }
            else if (roomName.Length > maxRoomNameLength)
            {
                roomName = roomName.Substring(0, maxRoomNameLength);
            }
            var options = roomOptions;
            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
        }

        public bool joinRoom(RoomInfo roomInfo)
        {
            if (isApplicationQuitting)
            {
                return false;
            }
            SequenceDiagram.receive(nameof(LobbyHelper), SD.JOIN_ROOM);
            var player = PhotonNetwork.LocalPlayer;
            var isSpectator = false;//player.IsSpectator();
            countLobbyParticipants(roomInfo, out var playerCount, out var maxPlayerCount, out var spectatorCount);
            if (isSpectator)
            {
                if (spectatorCount == maxSpectatorsInRoom)
                {
                    Debug.LogWarning("Spectator can not join room: spectator limit exceeded: " + spectatorCount);
                    return false;
                }
            }
            else if (playerCount == maxPlayerCount && maxPlayerCount > 0)
            {
                Debug.LogWarning($"Player can not join full room: {playerCount}/{maxPlayerCount}");
                return false;
            }
            Debug.Log($"JoinRoom {roomInfo.GetDebugLabel()} {player.GetDebugLabel()}");
            var isJoined = PhotonNetwork.JoinRoom(roomInfo.Name);
            if (!isJoined)
            {
                Debug.LogWarning("PhotonNetwork JoinRoom failed");
            }
            return isJoined;
        }

        public void joinOrCreateRoom(string roomName, Hashtable customRoomProperties, string[] lobbyPropertyNames,
            bool isAutomaticallySyncScene = false)
        {
            if (isApplicationQuitting)
            {
                return;
            }
            SequenceDiagram.receive(nameof(LobbyHelper), SD.JOIN_ROOM);
            Debug.Log($"joinOrCreateRoom {roomName}");

            var options = new RoomOptions
            {
                CustomRoomProperties = customRoomProperties,
                CustomRoomPropertiesForLobby = lobbyPropertyNames,
            };
            PhotonNetwork.AutomaticallySyncScene = isAutomaticallySyncScene;
            PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
        }

        public void closeAndHideRoom(bool keepOpen = false, bool keepVisible = false)
        {
            SequenceDiagram.receive(nameof(LobbyHelper), SD.CLOSE_ROOM);
            if (!PhotonNetwork.InRoom)
            {
                throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
            }
            if (!PhotonNetwork.IsMasterClient)
            {
                throw new UnityException("Player is not Master Client: " + PhotonNetwork.LocalPlayer.GetDebugLabel());
            }
            PhotonNetwork.CurrentRoom.IsOpen = keepOpen;
            PhotonNetwork.CurrentRoom.IsVisible = keepVisible;
        }

        public void leaveRoom()
        {
            SequenceDiagram.receive(nameof(LobbyHelper), SD.LEAVE_ROOM);
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                return;
            }
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }

        public void countRoomParticipants(out int playerCount, out int maxPlayerCount)
        {
            countRoomParticipants(out playerCount, out maxPlayerCount, out var spectatorCount);
        }

        public void countRoomParticipants(out int playerCount, out int maxPlayerCount, out int spectatorCount)
        {
            if (PhotonNetwork.InRoom)
            {
                var room = PhotonNetwork.CurrentRoom;
                spectatorCount = 0;//room.CountSpectators();
                playerCount = room.PlayerCount - spectatorCount;
                maxPlayerCount = room.IsOffline ? 0 : room.MaxPlayers > 0 ? room.MaxPlayers - maxSpectatorsInRoom : 0;
                return;
            }
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }

        private static void countLobbyParticipants(RoomInfo room, out int playerCount, out int maxPlayerCount, out int spectatorCount)
        {
            // This relies that room custom properties are updated correctly!
            spectatorCount = 0;//room.GetSpectatorCount();
            playerCount = room.PlayerCount - spectatorCount;
            maxPlayerCount = room.MaxPlayers > 0 ? room.MaxPlayers - maxSpectatorsInRoom : 0;
        }

        public void setNewNewMasterClient(Player newMasterClient)
        {
            if (!PhotonNetwork.InRoom)
            {
                throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
            }
            if (newMasterClient != null)
            {
                if (PhotonNetwork.SetMasterClient(newMasterClient))
                {
                    Debug.Log($"set new master client {newMasterClient.GetDebugLabel()}");
                    return;
                }
            }
            // Try to find new master client that is not spectator - not mandatory but maybe good thing to do for the future.
            // - sort players so that everybody tries to set the same player as this runs asynchronously on multiple clients.
            var players = PhotonNetwork.CurrentRoom.GetSortedPlayerList();
            foreach (var player in players)
            {
                if (!player.IsMasterClient)// && !player.IsSpectator())
                {
                    // This is new master client that is not spectator
                    if (PhotonNetwork.SetMasterClient(player))
                    {
                        Debug.Log($"set new master client {player.GetDebugLabel()}");
                        break;
                    }
                }
            }
        }

        public void countSpectatorsInRoom(string keyName)
        {
            if (!PhotonNetwork.InRoom)
            {
                throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
            }
            var room = PhotonNetwork.CurrentRoom;
            var spectatorCount = 0;//room.CountSpectators();
            var currentValue = 0;//room.GetSpectatorCount();
            if (spectatorCount != currentValue)
            {
                SetSpectatorCount(room, spectatorCount, currentValue);
            }
        }

        public void checkSpectatorInRoom(Player player)
        {
            if (!PhotonNetwork.InRoom)
            {
                throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
            }
            /*if (false)//player.IsSpectator())
            {
                var room = PhotonNetwork.CurrentRoom;
                var spectatorCount = 0;//room.CountSpectators();
                var currentValue = 0;//room.GetSpectatorCount();
                if (spectatorCount != currentValue)
                {
                    SetSpectatorCount(room, spectatorCount, currentValue);
                }
            }*/
        }

        public bool IsSpectator(Player player)
        {
            return player.HasCustomProperty(keynames.spectatorKey);
        }

        public static int CountSpectators(Room room)
        {
            var spectatorCount = 0;
            /*foreach (var player in room.Players.Values)
            {
                if (false)//player.IsSpectator())
                {
                    spectatorCount += 1;
                }
            }*/
            return spectatorCount;
        }

        public void SetSpectatorCount(Room room, int newValue, int currentValue)
        {
            room.SafeSetCustomProperty(keynames.spectatorCountKey, (byte) newValue, (byte) currentValue);
        }

        public int GetSpectatorCount(RoomInfo room)
        {
            return room.GetCustomProperty(keynames.spectatorCountKey, (byte) 0);
        }

        private static void connectUsingSettings(AppSettings appSettings, string gameVersion, string playerName, bool isAutomaticallySyncScene)
        {
            if (PhotonNetwork.OfflineMode)
            {
                throw new UnityException("PhotonNetwork.OfflineMode not allowed here");
            }
            Debug.Log(
                $"ConnectUsingSettings {PhotonNetwork.NetworkClientState} scene={SceneManager.GetActiveScene().name}" +
                $" {(appSettings != null ? appSettings.ToStringFull() : "")}");
            PhotonNetwork.AutomaticallySyncScene = isAutomaticallySyncScene;
            PhotonNetwork.NickName = playerName;
            PhotonNetwork.GameVersion = "";
            var started = appSettings != null
                ? PhotonNetwork.ConnectUsingSettings(appSettings)
                : PhotonNetwork.ConnectUsingSettings();
            if (started)
            {
                PhotonNetwork.GameVersion = gameVersion;
                Debug.Log($"Set GameVersion: {PhotonNetwork.GameVersion}");
            }
        }
    }
}