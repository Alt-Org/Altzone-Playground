using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using System;
using System.Linq;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class ReservePlayspot : MonoBehaviour
    {
        private const string playerStatusKeyName = "S";

        private enum PlayerStatus
        {
            Idle = 1,
            Acquiring = 2,
            Playing = 3,
            Releasing = 4,
        }

        [Header("Event Settings")] public UnityEventInt2 pelipaikkaVarattu;

        [Header("Live Data"), SerializeField] private PhotonWatchdog watchdog;
        [SerializeField] private int playerPosIndex;
        [SerializeField] private string playerRoomPosPropName;
        [SerializeField] private PlayerStatus status;

        private void Awake()
        {
            Debug.Log("Awake");
            watchdog = PhotonWatchdog.Get();
            status = PlayerStatus.Idle;
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            watchdog.AddListener(onNetworkEvent);
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
            watchdog.RemoveListener(onNetworkEvent);
        }

        public void OnReservePlayspot(Vector2 screenPosition)
        {
            var _camera = Camera.main;
            if (_camera == null)
            {
                return;
            }
            var worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            if (worldPosition.x < 0)
            {
                if (worldPosition.y < 0)
                {
                    playerPosIndex = 0;
                }
                else
                {
                    playerPosIndex = 3;
                }
            }
            else
            {
                if (worldPosition.y < 0)
                {
                    playerPosIndex = 2;
                }
                else
                {
                    playerPosIndex = 1;
                }
            }
            Debug.Log($"OnReservePlayspot @ screen {screenPosition} -> world {worldPosition} -> index {playerPosIndex}");
            reservePlayspot();
        }

        private void reservePlayspot()
        {
            if (!PhotonNetwork.InRoom)
            {
                throw new UnityException("Not in room");
            }
            var player = PhotonNetwork.LocalPlayer;
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log($"reservePlayspot #{playerPosIndex} for {player.GetDebugLabel()} in {room.GetDebugLabel()}");
            playerRoomPosPropName = getPlayerIndexPropName(playerPosIndex);
            var playerStatus = getPlayerStatus(player);
            if (playerStatus == PlayerStatus.Idle)
            {
                reservePlayerPosition(player, room);
            }
            else if (playerStatus == PlayerStatus.Playing)
            {
                releasePlayerPosition(player, room);
            }
        }

        private void reservePlayerPosition(Player player, Room room)
        {
            status = PlayerStatus.Acquiring;
            Debug.Log($"PLAYER SetCustomProperty {playerStatusKeyName} {(int)status} {status}");
            player.SetCustomProperty(playerStatusKeyName, (byte) status);
        }

        private void releasePlayerPosition(Player player, Room room)
        {
            status = PlayerStatus.Releasing;
            Debug.Log($"PLAYER SetCustomProperty {playerStatusKeyName} {(int)status} {status}");
            player.SetCustomProperty(playerStatusKeyName, (byte) status);
        }

        private void onNetworkEvent(PhotonWatchdog.Notify notify, PhotonWatchdog.Verb verb, Player affectedPlayer)
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }
            var isAccept = verb == PhotonWatchdog.Verb.OnRoomPropertiesUpdate || verb == PhotonWatchdog.Verb.OnPlayerPropertiesUpdate;
            if (!isAccept)
            {
                return; // not prop update
            }
            var player = PhotonNetwork.LocalPlayer;
            if (verb == PhotonWatchdog.Verb.OnPlayerPropertiesUpdate && !affectedPlayer.Equals(player))
            {
                return; // not us
            }
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log(room.GetDebugLabel());
            Debug.Log($"status {(int)status} {status} {playerRoomPosPropName} : {player.GetDebugLabel()}");
            if (verb == PhotonWatchdog.Verb.OnRoomPropertiesUpdate)
            {
                if (status == PlayerStatus.Acquiring)
                {
                    // Check if we got player position reserved for us
                    checkRoomPropertiesForStartPlay(player, room);
                    return;
                }
                if (status == PlayerStatus.Releasing)
                {
                    // Release our player position
                    checkRoomPropertiesForStopPlay(player, room);
                }
            }
            else if (verb == PhotonWatchdog.Verb.OnPlayerPropertiesUpdate)
            {
                status = getPlayerStatus(player);
                switch (status)
                {
                    case PlayerStatus.Idle:
                        pelipaikkaVarattu.Invoke(playerPosIndex, 0);
                        return;
                    case PlayerStatus.Acquiring:
                        setRoomPropertiesForStartPlay(player, room);
                        return;
                    case PlayerStatus.Playing:
                        pelipaikkaVarattu.Invoke(playerPosIndex, 1);
                        return;
                    case PlayerStatus.Releasing:
                        setRoomPropertiesForStopPlay(player, room);
                        return;
                    default:
                        throw new UnityException("invalid status: " + status);
                }
            }
        }

        private void setRoomPropertiesForStartPlay(Player player, Room room)
        {
            Debug.Log($"ROOM SafeSetCustomProperty {playerRoomPosPropName} : 0 <- {player.ActorNumber}");
            room.SafeSetCustomProperty(playerRoomPosPropName, (byte) player.ActorNumber, 0);
        }

        private void setRoomPropertiesForStopPlay(Player player, Room room)
        {
            Debug.Log($"ROOM SafeSetCustomProperty {playerRoomPosPropName} : {player.ActorNumber} <- 0");
            room.SafeSetCustomProperty(playerRoomPosPropName, 0, (byte) player.ActorNumber);
        }

        private void checkRoomPropertiesForStartPlay(Player player, Room room)
        {
            var actorNumber = player.ActorNumber.ToString();
            var props = room.CustomProperties;
            var keys = props.Keys.OrderBy((x) => x.ToString()).ToList();
            Debug.Log($"checkRoomPropertiesForStartPlay {playerRoomPosPropName} {actorNumber} keys {string.Join(", ", keys)}");
            foreach (var key in keys)
            {
                var keyName = key.ToString();
                if (keyName == playerRoomPosPropName)
                {
                    var keyValue = props[key].ToString();
                    if (keyValue == actorNumber)
                    {
                        // Player position is reserved for us
                        status = PlayerStatus.Playing;
                        Debug.Log($"PLAYER SetCustomProperty {playerStatusKeyName} {(int)status} {status}");
                        player.SetCustomProperty(playerStatusKeyName, (byte) status);
                    }
                    else if (keyValue == "0")
                    {
                        // Try again as room is free
                        setRoomPropertiesForStartPlay(player, room);
                    }
                    else
                    {
                        // Player position is reserved for somebody else
                        status = PlayerStatus.Idle;
                        Debug.Log($"PLAYER SetCustomProperty {playerStatusKeyName} {(int)status} {status}");
                        player.SetCustomProperty(playerStatusKeyName, (byte) status);
                    }
                    return;
                }
            }
        }

        private void checkRoomPropertiesForStopPlay(Player player, Room room)
        {
            var actorNumber = player.ActorNumber.ToString();
            var props = room.CustomProperties;
            var keys = props.Keys.OrderBy((x) => x.ToString()).ToList();
            Debug.Log($"checkRoomPropertiesForStopPlay {playerRoomPosPropName} {actorNumber} keys {string.Join(", ", keys)}");
            foreach (var key in keys)
            {
                var keyName = key.ToString();
                if (keyName == playerRoomPosPropName)
                {
                    var keyValue = props[key].ToString();
                    if (keyValue == "0")
                    {
                        // Player position has been released
                        status = PlayerStatus.Idle;
                        Debug.Log($"PLAYER SetCustomProperty {playerStatusKeyName} {(int)status} {status}");
                        player.SetCustomProperty(playerStatusKeyName, (byte) status);
                    }
                    else if (keyValue == actorNumber)
                    {
                        // Try again as room still is reserved for us
                        setRoomPropertiesForStopPlay(player, room);
                    }
                    else
                    {
                        // Player position is reserved for somebody else - that's OK for us
                        status = PlayerStatus.Idle;
                        Debug.Log($"PLAYER SetCustomProperty {playerStatusKeyName} {(int)status} {status}");
                        player.SetCustomProperty(playerStatusKeyName, (byte) status);
                    }
                    return;
                }
            }
        }

        private static PlayerStatus getPlayerStatus(Player player)
        {
            foreach (var entry in player.CustomProperties)
            {
                var keyName = entry.Key.ToString();
                if (keyName == playerStatusKeyName)
                {
                    var propValue = entry.Value.ToString();
                    if (!string.IsNullOrWhiteSpace(propValue) && Enum.TryParse(propValue, out PlayerStatus enumValue))
                    {
                        return enumValue;
                    }
                    break; // Invalid prop value
                }
            }
            return PlayerStatus.Idle;
        }

        private static string getPlayerIndexPropName(int index)
        {
            return $"P{index}";
        }
    }
}