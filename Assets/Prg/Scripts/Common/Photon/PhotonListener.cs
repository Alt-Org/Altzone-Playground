using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Utility to log all Photon events.
    /// </summary>
    /// <remarks>
    /// Should have very low "script execution order" number in order to be able to log events before others can see them.
    /// </remarks>
    public class PhotonListener : MonoBehaviour,
        IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, IPunOwnershipCallbacks
    {
        private const int maxTimeDifferenceMs = 60 * 60 * 1000;
        private const int minTimeDifferenceMs = -60 * 60 * 1000;
        private static int lastServerTimestamp;

        private static readonly Dictionary<string, string> photonRoomPropNames;
        private static readonly Dictionary<string, string> photonPlayerPropNames;

        static PhotonListener()
        {
            // https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_realtime_1_1_game_property_key.html
            photonPlayerPropNames = new Dictionary<string, string>()
            {
                { ActorProperties.PlayerName.ToString(), nameof(ActorProperties.PlayerName) },
                { ActorProperties.IsInactive.ToString(), nameof(ActorProperties.IsInactive) },
                { ActorProperties.UserId.ToString(), nameof(ActorProperties.UserId) },
            };
            photonRoomPropNames = new Dictionary<string, string>()
            {
                { GamePropertyKey.MaxPlayers.ToString(), nameof(GamePropertyKey.MaxPlayers) },
                { GamePropertyKey.IsVisible.ToString(), nameof(GamePropertyKey.IsVisible) },
                { GamePropertyKey.IsOpen.ToString(), nameof(GamePropertyKey.IsOpen) },
                { GamePropertyKey.PlayerCount.ToString(), nameof(GamePropertyKey.PlayerCount) },
                { GamePropertyKey.Removed.ToString(), nameof(GamePropertyKey.Removed) },
                { GamePropertyKey.PropsListedInLobby.ToString(), nameof(GamePropertyKey.PropsListedInLobby) },
                { GamePropertyKey.CleanupCacheOnLeave.ToString(), nameof(GamePropertyKey.CleanupCacheOnLeave) },
                { GamePropertyKey.MasterClientId.ToString(), nameof(GamePropertyKey.MasterClientId) },
                { GamePropertyKey.ExpectedUsers.ToString(), nameof(GamePropertyKey.ExpectedUsers) },
                { GamePropertyKey.PlayerTtl.ToString(), nameof(GamePropertyKey.PlayerTtl) },
                { GamePropertyKey.EmptyRoomTtl.ToString(), nameof(GamePropertyKey.EmptyRoomTtl) },
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            Create();
        }

        [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD")]
        private static void Create()
        {
            UnityExtensions.CreateGameObjectAndComponent<PhotonListener>(nameof(PhotonListener), isDontDestroyOnLoad: true);
        }

        private void OnEnable()
        {
            if (PhotonNetwork.NetworkingClient != null)
            {
                PhotonNetwork.AddCallbackTarget(this);
            }
            else
            {
                this.executeOnNextFrame(() => PhotonNetwork.AddCallbackTarget(this));
            }
            SceneManager.sceneLoaded += sceneLoaded;
            SceneManager.sceneUnloaded += sceneUnloaded;
        }

        private void Start()
        {
            LogPhotonStatus();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= sceneLoaded;
            SceneManager.sceneUnloaded -= sceneUnloaded;
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
        }

        private static void LogPhotonStatus(string message = null)
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var methodName = method != null && method.ReflectedType != null ? method.Name : "";
            _logMessage($"{methodName} {message}");
        }

        private static void sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _logMessage($"sceneLoaded {scene.name} ({scene.buildIndex})");
        }

        private static void sceneUnloaded(Scene scene)
        {
            _logMessage($"sceneUnloaded {scene.name} ({scene.buildIndex})");
        }

        private static void _logMessage(string message)
        {
            var c = PhotonNetwork.IsConnectedAndReady ? "r" : PhotonNetwork.IsConnected ? "c" : "-";
            var deltaTime = PhotonNetwork.ServerTimestamp - lastServerTimestamp;
            if (deltaTime > maxTimeDifferenceMs || deltaTime < minTimeDifferenceMs)
            {
                lastServerTimestamp = PhotonNetwork.ServerTimestamp;
                deltaTime = 0;
                c += ">";
            }
            Debug.Log($"{c}> {PhotonNetwork.NetworkClientState} {deltaTime} {message}");
        }

        private static string HashtableToString(Hashtable props, Dictionary<string, string> keyMapping)
        {
            var builder = new StringBuilder("{");
            foreach (var prop in props)
            {
                var keyName = prop.Key.ToString();
                if (keyMapping.TryGetValue(keyName, out var photonName))
                {
                    keyName = photonName;
                }
                builder.AppendFormat("{0}={1}, ", keyName, prop.Value);
            }
            if (builder.Length > 1)
            {
                builder.Length -= 2;
                builder.Append('}');
            }
            return builder.ToString();
        }

        #region IConnectionCallbacks

        public void OnConnected()
        {
            LogPhotonStatus();
        }

        public void OnConnectedToMaster()
        {
            /*
             * PhotonNetwork.GameVersion must be set _after_ calling PhotonNetwork.ConnectUsingSettings()
             * https://doc.photonengine.com/en-us/pun/v2/getting-started/feature-overview#versioning_by_gameversion
             * https://forum.photonengine.com/discussion/16543/how-to-prevent-older-versions-of-the-client-from-connecting-to-photonnetwork
             */
            LogPhotonStatus(
                $"AppVersion={PhotonNetwork.AppVersion} GameVersion={PhotonNetwork.GameVersion} scene={SceneManager.GetActiveScene().name}");
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            LogPhotonStatus(cause.ToString());
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            LogPhotonStatus($"BestRegion={regionHandler.BestRegion}");
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            LogPhotonStatus($"#{data.Count}");
            foreach (var entry in data)
            {
                Debug.LogFormat("+ data:{0}={1}", entry.Key, entry.Value);
            }
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            LogPhotonStatus(debugMessage);
        }

        #endregion

        #region ILobbyCallbacks

        public void OnJoinedLobby()
        {
            LogPhotonStatus();
        }

        public void OnLeftLobby()
        {
            LogPhotonStatus();
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            LogPhotonStatus($"#{roomList.Count}");
            foreach (var roomInfo in roomList)
            {
                Debug.LogFormat("+ {0}", roomInfo.GetDebugLabel());
            }
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            LogPhotonStatus($"#{lobbyStatistics.Count}");
            foreach (var lobbyInfo in lobbyStatistics)
            {
                Debug.LogFormat("+ {0}", lobbyInfo.ToString());
            }
        }

        #endregion

        #region IMatchmakingCallbacks

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            LogPhotonStatus($"#{friendList.Count}");
            foreach (var friendInfo in friendList)
            {
                Debug.LogFormat("+ friend {0}", friendInfo.ToString());
            }
        }

        public void OnCreatedRoom()
        {
            LogPhotonStatus(PhotonNetwork.CurrentRoom.GetDebugLabel());
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            LogPhotonStatus($"{returnCode} {message}");
        }

        public void OnJoinedRoom()
        {
            LogPhotonStatus(PhotonNetwork.CurrentRoom.GetDebugLabel());
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            LogPhotonStatus($"{returnCode} {message}");
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            LogPhotonStatus($"{returnCode} {message}");
        }

        public void OnLeftRoom()
        {
            LogPhotonStatus();
        }

        #endregion

        #region IInRoomCallbacks

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            LogPhotonStatus(newPlayer.GetDebugLabel());
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            LogPhotonStatus(otherPlayer.GetDebugLabel());
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            LogPhotonStatus($"{PhotonNetwork.CurrentRoom.GetDebugLabel()} changed {HashtableToString(propertiesThatChanged, photonRoomPropNames)}");
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            LogPhotonStatus($"{targetPlayer.GetDebugLabel()} changed {HashtableToString(changedProps, photonPlayerPropNames)}");
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            LogPhotonStatus(newMasterClient.GetDebugLabel());
        }

        #endregion

        #region IPunOwnershipCallbacks

        public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            LogPhotonStatus($"from {targetView.Controller} to {requestingPlayer}");
        }

        public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            LogPhotonStatus($"from {previousOwner} to {targetView.Controller}");
        }

        void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
        {
            LogPhotonStatus($"from {targetView.Controller} to {senderOfFailedRequest}");
        }

        #endregion
    }
}