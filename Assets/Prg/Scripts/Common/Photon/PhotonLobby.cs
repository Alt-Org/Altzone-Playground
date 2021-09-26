using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Unity;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Convenience class to handle basic <c>PhotonNetwork</c> operations in consistent way.
    /// </summary>
    public class PhotonLobby
    {
        private const int maxRoomNameLength = 16;

        private static bool isApplicationQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void afterSceneLoad()
        {
            Application.quitting += () => isApplicationQuitting = true;
        }

        /// <summary>
        /// To override default <c>PhotonNetwork.GameVersion</c> set to <c>Application.version</c>.
        /// </summary>
        public static Func<string> gameVersion = () => _gameVersion;

        private static string _gameVersion => Application.version;

        public static PhotonLobby Get()
        {
            return new PhotonLobby();
        }

        private PhotonLobby()
        {
        }

        public void connect(string playerName, bool isAutomaticallySyncScene = true)
        {
            if (isApplicationQuitting)
            {
                return;
            }
            SequenceDiagram.receive(nameof(PhotonLobby), SD.CONNECT);
            if (PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.OfflineMode = false;
                Debug.Log($"Fix OfflineMode: {PhotonNetwork.OfflineMode}");
            }
            if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
            {
                var photonAppSettings = ResourceLoader.Get().LoadAsset<PhotonAppSettings>(nameof(PhotonAppSettings));
                var appSettings = photonAppSettings != null ? photonAppSettings.appSettings : null;
                connectUsingSettings(appSettings, playerName, isAutomaticallySyncScene);
                return;
            }
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }

        public void disconnect()
        {
            SequenceDiagram.receive(nameof(PhotonLobby), SD.DISCONNECT);
            PhotonNetwork.Disconnect();
        }

        public void joinLobby()
        {
            if (isApplicationQuitting)
            {
                return;
            }
            SequenceDiagram.receive(nameof(PhotonLobby), SD.JOIN_LOBBY);
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
            SequenceDiagram.receive(nameof(PhotonLobby), SD.LEAVE_LOBBY);
            if (PhotonNetwork.InLobby)
            {
                Debug.Log("LeaveLobby");
                PhotonNetwork.LeaveLobby();
                return;
            }
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }

        public void createRoom(string roomName, RoomOptions roomOptions = null)
        {
            if (isApplicationQuitting)
            {
                return;
            }
            SequenceDiagram.receive(nameof(PhotonLobby), SD.CREATE_ROOM);
            if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = null; // Let Photon generate room name us to ensure that room creation succeeds
            }
            else if (roomName.Length > maxRoomNameLength)
            {
                roomName = roomName.Substring(0, maxRoomNameLength);
            }
            var options = roomOptions ?? new RoomOptions
            {
                IsOpen = true,
                IsVisible = true,
            };
            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
        }

        public bool joinRoom(RoomInfo roomInfo)
        {
            if (isApplicationQuitting)
            {
                return false;
            }
            SequenceDiagram.receive(nameof(PhotonLobby), SD.JOIN_ROOM);
            Debug.Log($"JoinRoom {roomInfo.GetDebugLabel()} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
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
            SequenceDiagram.receive(nameof(PhotonLobby), SD.JOIN_ROOM);
            Debug.Log($"joinOrCreateRoom {roomName}");
            if (string.IsNullOrWhiteSpace(roomName))
            {
                throw new UnityException("roomName can not be null or empty");
            }
            var options = new RoomOptions
            {
                CustomRoomProperties = customRoomProperties,
                CustomRoomPropertiesForLobby = lobbyPropertyNames,
            };
            PhotonNetwork.AutomaticallySyncScene = isAutomaticallySyncScene;
            PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
        }

        public void closeRoom(bool keepVisible = false)
        {
            SequenceDiagram.receive(nameof(PhotonLobby), SD.CLOSE_ROOM);
            if (!PhotonNetwork.InRoom)
            {
                throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
            }
            if (!PhotonNetwork.IsMasterClient)
            {
                throw new UnityException("Player is not Master Client: " + PhotonNetwork.LocalPlayer.GetDebugLabel());
            }
            var room = PhotonNetwork.CurrentRoom;
            if (!room.IsOpen)
            {
                throw new UnityException("Room is closed already: " + room.GetDebugLabel());
            }
            room.IsOpen = false;
            room.IsVisible = keepVisible;
        }

        public void leaveRoom()
        {
            SequenceDiagram.receive(nameof(PhotonLobby), SD.LEAVE_ROOM);
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                return;
            }
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }

        private static void connectUsingSettings(AppSettings appSettings, string playerName, bool isAutomaticallySyncScene)
        {
            if (PhotonNetwork.OfflineMode)
            {
                throw new UnityException("PhotonNetwork.OfflineMode not allowed here");
            }
            // See PhotonNetwork.SendRate (which is 30 times per sec)
            // https://documentation.help/Photon-Unity-Networking-2/class_photon_1_1_pun_1_1_photon_network.html#a7b4c9628657402e59fe292502511dcf4
            // - original 10 times per second is way too slow to keep moving objects synchronized properly without glitches!
            PhotonNetwork.SerializationRate = 30;
            // https://doc.photonengine.com/en-us/pun/v2/gameplay/optimization
            // Reuse EventData to decrease garbage collection but EventData will be overwritten for every event!
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance = true;
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
                PhotonNetwork.GameVersion = gameVersion();
                Debug.Log($"Set GameVersion: {PhotonNetwork.GameVersion}");
            }
        }
    }
}