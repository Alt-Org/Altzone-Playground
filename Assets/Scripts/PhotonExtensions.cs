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

    public static void SafeSetCustomProperty(this Player player, string key, byte newValue, byte currentValue)
    {
        var props = new Hashtable { { key, newValue } };
        var expectedProps = new Hashtable { { key, currentValue } };
        player.SetCustomProperties(props, expectedProps);
    }

    public static void SafeSetCustomProperty(this Player player, string key, string newValue, string currentValue)
    {
        var props = new Hashtable { { key, newValue } };
        var expectedProps = new Hashtable { { key, currentValue } };
        player.SetCustomProperties(props, expectedProps);
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