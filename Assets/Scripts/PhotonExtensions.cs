using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class PhotonExtensions
{
    #region To be reafactored elsewhere

    public enum CustomPropKey
    {
        // Room properties
        GameMode,
        WallType,
        SpectatorCount,
        RoomSetup,

        // Player properties
        PlayerSetup,
        IsSpectator,
    }

    private static readonly Dictionary<CustomPropKey, string> propKeyNames;

    static PhotonExtensions()
    {
        propKeyNames = new Dictionary<CustomPropKey, string>()
        {
            // Keep property names short to reduce payload when they are sent over network
            { CustomPropKey.GameMode, "GM" },
            { CustomPropKey.WallType, "WT" },
            { CustomPropKey.SpectatorCount, "SC" },
            { CustomPropKey.RoomSetup, "RS" },
            { CustomPropKey.PlayerSetup, "PS" },
            { CustomPropKey.IsSpectator, "<s>" },
        };
    }

    private static string KeyName(this CustomPropKey key)
    {
        if (propKeyNames.TryGetValue(key, out var keyName))
        {
            return keyName;
        }
        throw new UnityException("CustomPropKey not found: " + key);
    }

    public static string GetStatusText(this ClientState clientState)
    {
        // TODO: should be localizable
        return PhotonNetwork.InRoom
            ? "Room"
            : PhotonNetwork.InLobby
                ? "Lobby"
                : clientState == ClientState.ConnectedToMasterServer
                    ? "Master"
                    : (clientState == ClientState.PeerCreated ||
                       clientState == ClientState.Disconnected)
                        ? "Ready"
                        : "Wait";
    }

    public static string GetPlayerLabel(this Player player)
    {
        var label = player.NickName;
        var status = $" #{player.ActorNumber}";
        if (player.CustomProperties.TryGetValue(CustomPropKey.PlayerSetup.KeyName(), out var playerSetup))
        {
            status += $":{playerSetup}";
        }
        else
        {
            status += ":?";
        }
        if (player.IsMasterClient)
        {
            status += ",m";
        }
        if (player.IsSpectator())
        {
            status += ",s";
        }
        if (player.IsInactive)
        {
            status += ",out";
        }
        label += status;
        return label;
    }

    public static string GetRoomLabel(this RoomInfo room)
    {
        string formatRoomLabel(string gameMode, string roomName, int playerCount, int spectatorCount, int maxPlayers)
        {
            playerCount -= spectatorCount;
            var spectatorState = spectatorCount > 0 ? $"+{spectatorCount}" : "";
            var roomState = playerCount == maxPlayers ? "full" : "open";
            return $"{gameMode} {roomName} ({playerCount}/{maxPlayers}{spectatorState}) {roomState}";
        }

        return formatRoomLabel(room.GetGameMode(), room.Name, room.PlayerCount, room.GetSpectatorCount(), room.MaxPlayers);
    }

    public static string GetSortLabel(this RoomInfo room)
    {
        return $"{room.GetGameMode(),12}.{room.Name}"; // make first key part long enough to hold longest game mode name
    }

    public static int CountAllPlayers(this Room room)
    {
        var playerCount = 0;
        foreach (var player in room.Players.Values)
        {
            if (!player.IsSpectator())
            {
                playerCount += 1;
            }
        }
        return playerCount;
    }

    public static ICollection<Player> GetPlayerList(this Room room)
    {
        return room.Players.Values;
    }

    public static void SetSpectatorMode(this Player player, bool value)
    {
        player.SetCustomProperty(CustomPropKey.IsSpectator.KeyName(), value);
    }

    public static int GetSpectatorCount(this RoomInfo room)
    {
        return room.GetCustomProperty(CustomPropKey.SpectatorCount.KeyName(), (byte) 0);
    }

    public static int CountSpectators(this Room room)
    {
        var spectatorCount = 0;
        foreach (var player in room.Players.Values)
        {
            if (player.IsSpectator())
            {
                spectatorCount += 1;
            }
        }
        return spectatorCount;
    }

    public static bool IsSpectator(this Player player)
    {
        return player.HasCustomProperty(CustomPropKey.IsSpectator.KeyName());
    }

    public static string GetGameMode(this RoomInfo room)
    {
        if (room.CustomProperties.TryGetValue(CustomPropKey.GameMode.KeyName(), out var mode))
        {
            return mode.ToString();
        }
        if (room.RemovedFromList)
        {
            return "";
        }
        throw new UnityException("game mode not set in room info");
    }

    #endregion

    #region Player

    public static IOrderedEnumerable<Player> GetSortedPlayerList(this Room room)
    {
        return room.Players.Values.OrderBy((x) => x.ActorNumber);
    }

    #endregion

    #region Room

    public static bool GetUniquePlayerNameForRoom(this Room room, Player player, string playerName, string separator, out string uniquePlayerName)
    {
        if (!PhotonNetwork.InRoom)
        {
            throw new UnityException("Invalid connection state: " + PhotonNetwork.NetworkClientState);
        }
        if (room.PlayerCount > 0)
        {
            foreach (var otherPlayer in PhotonNetwork.PlayerList)
            {
                if (!otherPlayer.Equals(player) &&
                    string.Equals(otherPlayer.NickName, playerName, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Assign new name to current player.
                    uniquePlayerName = $"{playerName}{separator}{PhotonNetwork.LocalPlayer.ActorNumber}";
                    return false;
                }
            }
        }
        uniquePlayerName = playerName;
        return true;
    }

    #endregion

    #region CustomProperties

    public static bool HasCustomProperty(this Player player, string key)
    {
        return player.CustomProperties.ContainsKey(key);
    }

    public static bool HasCustomProperty(this RoomInfo room, string key)
    {
        return room.CustomProperties.ContainsKey(key);
    }

    public static void SetCustomProperty(this Player player, string key, bool value)
    {
        var props = value
            ? new Hashtable { { key, (byte) 1 } } // Add
            : new Hashtable { { key, null } }; // Remove
        player.SetCustomProperties(props);
    }

    public static void SetCustomProperty(this Player player, string key, byte value)
    {
        var props = new Hashtable { { key, value } };
        player.SetCustomProperties(props);
    }

    public static void SetCustomProperty(this Player player, string key, string value)
    {
        var props = new Hashtable { { key, value } };
        player.SetCustomProperties(props);
    }

    public static void RemoveCustomProperty(this Player player, string key)
    {
        if (player.CustomProperties.ContainsKey(key))
        {
            var props = new Hashtable { { key, null } };
            player.SetCustomProperties(props);
        }
    }

    public static void SafeSetCustomProperty(this Room room, string key, byte newValue, byte currentValue)
    {
        var props = new Hashtable { { key, newValue } };
        var expectedProps = new Hashtable { { key, currentValue } };
        room.SetCustomProperties(props, expectedProps);
    }

    public static void SafeSetCustomProperty(this Room room, string key, short newValue, short currentValue)
    {
        var props = new Hashtable { { key, newValue } };
        var expectedProps = new Hashtable { { key, currentValue } };
        room.SetCustomProperties(props, expectedProps);
    }

    public static void SafeSetCustomProperty(this Room room, string key, string newValue, string currentValue)
    {
        var props = new Hashtable { { key, newValue } };
        var expectedProps = new Hashtable { { key, currentValue } };
        room.SetCustomProperties(props, expectedProps);
    }

    public static T GetCustomProperty<T>(this Player player, string key, T defaultValue = default)
    {
        if (player.CustomProperties.TryGetValue(key, out var propValue))
        {
            if (propValue is T valueOfCorrectType)
            {
                return valueOfCorrectType;
            }
            throw new UnityException(
                $"GetCustomProperty value {propValue} ({propValue.GetType().FullName}) is not correct type: {typeof(T).FullName}");
        }
        return defaultValue;
    }

    public static T GetCustomProperty<T>(this RoomInfo room, string key, T defaultValue = default)
    {
        if (room.CustomProperties.TryGetValue(key, out var propValue))
        {
            if (propValue is T valueOfCorrectType)
            {
                return valueOfCorrectType;
            }
            throw new UnityException(
                $"GetCustomProperty value {propValue} ({propValue.GetType().FullName}) is not correct type: {typeof(T).FullName}");
        }
        return defaultValue;
    }

    #endregion

    #region Debugging

    public static string GetDebugLabel(this Player player, bool verbose = true)
    {
        if (player == null)
        {
            return "";
        }
        var status = $"{player.ActorNumber}";
        if (player.IsMasterClient)
        {
            status += ",m";
        }
        status += player.IsLocal ? ",l" : ",r";
        if (player.IsInactive)
        {
            status += ",out";
        }
        if (verbose)
        {
            status += $" {player.CustomProperties.AsSorted()}";
        }
        var playerName = verbose ? $"Player: {player.NickName}" : player.NickName;
        return $"{playerName} {status}";
    }

    public static string GetDebugLabel(this RoomInfo room) // Works for Room too!
    {
        // Replacement for room.ToString()
        return $"{room}{(room.RemovedFromList ? " removed." : "")} {room.CustomProperties.AsSorted()}";
    }

    private static string AsSorted(this Hashtable dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            return "{}";
        }
        var keys = dictionary.Keys.ToList();
        keys.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
        var builder = new StringBuilder("{");
        foreach (var key in keys)
        {
            var propValue = dictionary[key].ToString();
            if (propValue.Length > 12)
            {
                propValue = propValue.GetHashCode().ToString("X");
            }
            builder.Append(key).Append('=').Append(propValue).Append(", ");
        }
        builder.Length -= 2;
        builder.Append('}');
        return builder.ToString();
    }

    #endregion
}