using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Watch Photon network events on behalf of clients.
    /// </summary>
    /// <remarks>
    /// Script Execution order should be lower that default so that clients have time to initialize them before we send any notifications to them.
    /// </remarks>
    public class PhotonWatchdog : MonoBehaviour,
        IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, IPunOwnershipCallbacks
    {
        public enum Notify
        {
            Connection,
            StatusUpdate,
            Error,
        }

        public enum Verb
        {
            WaitForConnection,
            PeerCreated,
            OnConnected,
            OnConnectedToMaster,
            OnDisconnected,
            OnRegionListReceived,
            OnCustomAuthenticationResponse,
            OnCustomAuthenticationFailed,
            OnJoinedLobby,
            OnLeftLobby,
            OnRoomListUpdate,
            OnLobbyStatisticsUpdate,
            OnFriendListUpdate,
            OnCreatedRoom,
            OnCreateRoomFailed,
            OnJoinedRoom,
            OnJoinRoomFailed,
            OnJoinRandomFailed,
            OnLeftRoom,
            OnPlayerEnteredRoom,
            OnPlayerLeftRoom,
            OnRoomPropertiesUpdate,
            OnPlayerPropertiesUpdate,
            OnMasterClientSwitched,
            OnOwnershipRequest,
            OnOwnershipTransfered,
            OnOwnershipTransferFailed,
        }

        private class StateHolder
        {
            public readonly Notify notify;
            public readonly Verb verb;
            public readonly bool isStable;

            public StateHolder(Notify notify, Verb verb, ClientState? clientState = null)
            {
                this.notify = notify;
                this.verb = verb;
                var state = clientState ?? PhotonNetwork.NetworkClientState;
                switch (state)
                {
                    case ClientState.PeerCreated:
                    case ClientState.Authenticated:
                    case ClientState.JoinedLobby:
                    case ClientState.ConnectedToGameServer:
                    case ClientState.Joined:
                    case ClientState.Disconnected:
                    case ClientState.ConnectedToMasterServer:
                    case ClientState.ConnectedToNameServer:
                        isStable = true;
                        break;
                    default:
                        isStable = false;
                        break;
                }
            }

            public override string ToString()
            {
                return $"{(isStable ? "ready" : "wait")}:{notify}:{verb}";
            }
        }

        private static PhotonWatchdog _Instance;
        public static PhotonWatchdog Get()
        {
            if (_Instance == null)
            {
                _Instance = new GameObject(nameof(PhotonWatchdog)).AddComponent<PhotonWatchdog>();
            }
            return _Instance;
        }

        public ReadOnlyCollection<RoomInfo> currentRooms => getRoomListing();

        private Action<Notify, Verb, Player> onNetworkEvent;
        private StateHolder curState;
        private bool isApplicationQuitting;

        private List<RoomInfo> currentRoomList = new List<RoomInfo>(); // Cached list of current rooms

        private void Awake()
        {
            Debug.Log("Awake");
            curState = getState(PhotonNetwork.NetworkClientState);
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable #" + (onNetworkEvent?.GetInvocationList().Length ?? 0));
            PhotonNetwork.AddCallbackTarget(this);
            curState = getState(PhotonNetwork.NetworkClientState);
            if (curState.isStable)
            {
                invokeListeners(curState);
            }
            else
            {
                StartCoroutine(waitForStableState());
            }
        }

        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }

        private IEnumerator waitForStableState()
        {
            yield return null;
            var wait = new WaitForSeconds(1.0f);
            for (;;)
            {
                curState = getState(PhotonNetwork.NetworkClientState);
                if (curState.isStable)
                {
                    invokeListeners(curState);
                    break;
                }
                yield return wait;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            var size = onNetworkEvent?.GetInvocationList().Length ?? 0;
            if (size > 0)
            {
                Debug.Log("OnDisable #" + size);
            }
            PhotonNetwork.RemoveCallbackTarget(this);
            if (onNetworkEvent != null)
            {
                var length = onNetworkEvent.GetInvocationList().Length;
                if (length > 0)
                {
                    foreach (var @delegate in onNetworkEvent.GetInvocationList())
                    {
                        Debug.Log($"callback {@delegate.Method.GetFullName()}");
                    }
                    if (!isApplicationQuitting)
                    {
                        throw new UnityException("Failed to remove all listeners: " + length);
                    }
                }
            }
        }

        public void AddListener(Action<Notify, Verb, Player> listener)
        {
            if (onNetworkEvent != null)
            {
                if (onNetworkEvent.GetInvocationList().Contains(listener))
                {
                    throw new UnityException("AddListener twice not allowed: " + listener);
                }
            }
            onNetworkEvent += listener;
            Debug.Log($"AddListener #{(onNetworkEvent?.GetInvocationList().Length ?? 0)} {listener.Method.GetFullName()}");
            curState = getState(PhotonNetwork.NetworkClientState);
            Debug.Log($"send {curState}");
            listener.Invoke(curState.notify, curState.verb, null);
        }

        public void RemoveListener(Action<Notify, Verb, Player> listener)
        {
            if (onNetworkEvent != null)
            {
                if (!onNetworkEvent.GetInvocationList().Contains(listener))
                {
                    throw new UnityException("RemoveListener listener not found: " + listener);
                }
            }
            onNetworkEvent -= listener;
            Debug.Log($"RemoveListener #{onNetworkEvent?.GetInvocationList().Length ?? 0} {listener.Method.GetFullName()}");
        }

        private void invokeListeners(StateHolder newState, Player affectedPlayer = null)
        {
            if (isApplicationQuitting)
            {
                Debug.Log($"SKIP send {newState} Application is Quitting");
                return;
            }
            if (newState.notify == curState.notify && newState.verb == curState.verb)
            {
                Debug.Log($"send {newState}");
            }
            else
            {
                Debug.Log($"send {curState} <- {newState}");
                curState = newState;
            }
            onNetworkEvent?.Invoke(newState.notify, newState.verb, affectedPlayer);
        }

        public ReadOnlyCollection<RoomInfo> getRoomListing()
        {
            if (PhotonNetwork.InLobby)
            {
                return currentRoomList.AsReadOnly();
            }
            if (PhotonNetwork.NetworkClientState == ClientState.Joining)
            {
                // It seems that OnRoomListUpdate can happen between transitioning from lobby to room:
                // -> JoinedLobby -> Joining -> Joined
                currentRoomList.Clear();
                return currentRoomList.AsReadOnly();
            }
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }

        private void updateRoomListing(List<RoomInfo> roomList)
        {
            // We always remove and add entries to keep cached data up-to-date.
            foreach (var newRoomInfo in roomList)
            {
                var curRoomInfoIndex = currentRoomList.FindIndex(x => x.Equals(newRoomInfo));
                if (curRoomInfoIndex != -1)
                {
                    currentRoomList.RemoveAt(curRoomInfoIndex);
                    if (newRoomInfo.RemovedFromList)
                    {
                        continue; // No need to add as this will be disappear soon!
                    }
                }
                currentRoomList.Add(newRoomInfo);
            }
            if (currentRoomList.Any(x => x.RemovedFromList))
            {
                // Remove removed rooms from cache
                currentRoomList = currentRoomList.Where(x => !x.RemovedFromList).ToList();
            }
        }

        void IConnectionCallbacks.OnConnected()
        {
            invokeListeners(new StateHolder(Notify.Connection, Verb.OnConnected));
        }

        void IConnectionCallbacks.OnConnectedToMaster()
        {
            invokeListeners(new StateHolder(Notify.Connection, Verb.OnConnectedToMaster));
        }

        void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
        {
            invokeListeners(new StateHolder(Notify.Connection, Verb.OnDisconnected));
        }

        void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnRegionListReceived));
        }

        void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnCustomAuthenticationResponse));
        }

        void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
        {
            invokeListeners(new StateHolder(Notify.Error, Verb.OnCustomAuthenticationFailed));
        }

        void ILobbyCallbacks.OnJoinedLobby()
        {
            currentRoomList.Clear();
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnJoinedLobby));
        }

        void ILobbyCallbacks.OnLeftLobby()
        {
            currentRoomList.Clear();
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnLeftLobby));
        }

        void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
        {
            updateRoomListing(roomList);
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnRoomListUpdate));
        }

        void ILobbyCallbacks.OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnLobbyStatisticsUpdate));
        }

        void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnFriendListUpdate));
        }

        void IMatchmakingCallbacks.OnCreatedRoom()
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnCreatedRoom));
        }

        void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
        {
            invokeListeners(new StateHolder(Notify.Error, Verb.OnCreateRoomFailed));
        }

        void IMatchmakingCallbacks.OnJoinedRoom()
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnJoinedRoom));
        }

        void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
        {
            invokeListeners(new StateHolder(Notify.Error, Verb.OnJoinRoomFailed));
        }

        void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
        {
            invokeListeners(new StateHolder(Notify.Error, Verb.OnJoinRandomFailed));
        }

        void IMatchmakingCallbacks.OnLeftRoom()
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnLeftRoom));
        }

        void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnPlayerEnteredRoom), newPlayer);
        }

        void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnPlayerLeftRoom), otherPlayer);
        }

        void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnRoomPropertiesUpdate));
        }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnPlayerPropertiesUpdate), targetPlayer);
        }

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnMasterClientSwitched), newMasterClient);
        }

        void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnOwnershipRequest));
        }

        void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            invokeListeners(new StateHolder(Notify.StatusUpdate, Verb.OnOwnershipTransfered));
        }

        void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
        {
            invokeListeners(new StateHolder(Notify.Error, Verb.OnOwnershipTransferFailed));
        }

        private StateHolder getState(ClientState state)
        {
            switch (state)
            {
                case ClientState.PeerCreated:
                    return new StateHolder(Notify.Connection, Verb.PeerCreated, state);
                case ClientState.Authenticated:
                    return new StateHolder(Notify.Connection, Verb.OnConnected, state); // Not used
                case ClientState.JoinedLobby:
                    return new StateHolder(Notify.Connection, Verb.OnJoinedLobby, state);
                case ClientState.ConnectedToGameServer:
                    return new StateHolder(Notify.Connection, Verb.OnConnected, state);
                case ClientState.Joined:
                    return new StateHolder(Notify.Connection, Verb.OnJoinedRoom, state);
                case ClientState.Disconnected:
                    return new StateHolder(Notify.Connection, Verb.OnDisconnected, state);
                case ClientState.ConnectedToMasterServer:
                    return new StateHolder(Notify.Connection, Verb.OnConnectedToMaster, state);
                case ClientState.ConnectedToNameServer:
                    return new StateHolder(Notify.Connection, Verb.OnConnected, state);
                case ClientState.Authenticating:
                case ClientState.JoiningLobby:
                case ClientState.DisconnectingFromMasterServer:
                case ClientState.ConnectingToGameServer:
                case ClientState.Joining:
                case ClientState.Leaving:
                case ClientState.DisconnectingFromGameServer:
                case ClientState.ConnectingToMasterServer:
                case ClientState.Disconnecting:
                case ClientState.ConnectingToNameServer:
                case ClientState.DisconnectingFromNameServer:
                case ClientState.ConnectWithFallbackProtocol:
                    return new StateHolder(Notify.Connection, Verb.WaitForConnection, state);
                default:
                    throw new UnityException("unknown network state: " + state);
            }
        }
    }
}