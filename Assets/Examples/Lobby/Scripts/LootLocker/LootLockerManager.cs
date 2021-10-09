using Examples.Config.Scripts;
using LootLocker;
using LootLocker.Requests;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Examples.Lobby.Scripts.LootLocker
{
    [Serializable]
    public class PlayerHandle
    {
        [SerializeField] private string deviceId;
        [SerializeField] private string playerName;
        [SerializeField] private int player_id;

        public string DeviceId => deviceId;
        public int PlayerId => player_id;
        public string PlayerName => playerName;

        public PlayerHandle(string deviceId, string playerName, int playerId)
        {
            this.deviceId = deviceId;
            this.playerName = playerName;
            player_id = playerId;
        }
    }

    /// <summary>
    /// Test driver for LootLocker Game BAAS
    /// </summary>
    /// <remarks>
    /// We create a new session and try to synchronize player name between <c>LootLocker</c> and our <c>PlayerPrefs</c>.<br />
    /// We mostly ignore errors as next time player logins there is a chance to fix everything that need to be fixed.
    /// </remarks>
    public class LootLockerManager : MonoBehaviour
    {
        private const string do_not_save_it_here = "f1e477e40a312095f53887ebb3de4425b19e420a";

        [SerializeField] private bool isAsyncMode;
        [SerializeField] private PlayerHandle _playerHandle;
        [SerializeField] private bool isStartSessionReady;

        public PlayerHandle playerHandle => _playerHandle;
        public bool isValid => _playerHandle != null && isStartSessionReady;

        /// <summary>
        /// Asynchronous methods can be mind-boggling even though they make life a lot easier!
        /// </summary>
        private async void Awake()
        {
            Debug.Log("Awake start");
            LootLockerSDKManager.Init(do_not_save_it_here, "0.0.0.1", LootLockerConfig.platformType.Android, true);
            isStartSessionReady = false;
            if (isAsyncMode)
            {
                await asyncInit();
                isStartSessionReady = true;
            }
            else
            {
                callbackInit();
            }
            Debug.Log($"Awake exit {isStartSessionReady}");
        }

        private async Task asyncInit()
        {
            getPlayerPrefs(out var deviceId, out var playerName);
            Debug.Log($"StartSession for {playerName} {deviceId}");
            var sessionResp = await LootLockerAsync.StartSession(deviceId);
            if (!sessionResp.success)
            {
                if (sessionResp.text.Contains("Game not found"))
                {
                    Debug.LogError("INVALID game_key");
                }
                // Create dummy player using PlayerPrefs values
                _playerHandle = new PlayerHandle(deviceId, playerName, -1);
                return;
            }
            _playerHandle = new PlayerHandle(deviceId, playerName, sessionResp.player_id);

            if (!sessionResp.seen_before)
            {
                // This is new player
                Debug.Log($"SetPlayerName is NEW '{_playerHandle.PlayerName}'");
                var task = LootLockerAsync.SetPlayerName(_playerHandle.PlayerName); // Fire and forget
                return;
            }

            var getNameResp = await LootLockerAsync.GetPlayerName();
            if (!getNameResp.success || string.IsNullOrWhiteSpace(getNameResp.name))
            {
                // Failed to get or name is empty
                Debug.Log($"SetPlayerName '{_playerHandle.PlayerName}'");
                var task = LootLockerAsync.SetPlayerName(_playerHandle.PlayerName); // Fire and forget
                return;
            }
            if (_playerHandle.PlayerName != getNameResp.name)
            {
                // Update local name from LootLocker
                _playerHandle = new PlayerHandle(deviceId, _playerHandle.PlayerName, sessionResp.player_id);
                setPlayerPrefs(_playerHandle.PlayerName);
            }
        }

        private void callbackInit()
        {
            getPlayerPrefs(out var deviceId, out var playerName);
            Debug.Log($"StartSession for {playerName} {deviceId}");
            LootLockerSDKManager.StartSession(deviceId, (sessionResp) =>
            {
                if (!sessionResp.success)
                {
                    _playerHandle = new PlayerHandle(deviceId, playerName, 0);
                    isStartSessionReady = true;
                    return;
                }
                _playerHandle = new PlayerHandle(deviceId, playerName, sessionResp.player_id);
                if (!sessionResp.seen_before)
                {
                    Debug.Log($"SetPlayerName '{playerName}'");
                    LootLockerSDKManager.SetPlayerName(playerName, null); // Fire and forget
                    isStartSessionReady = true;
                    return;
                }
                LootLockerSDKManager.GetPlayerName(getNameResp =>
                {
                    if (!getNameResp.success)
                    {
                        isStartSessionReady = true;
                        return;
                    }
                    if (getNameResp.name == _playerHandle.PlayerName)
                    {
                        isStartSessionReady = true;
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(getNameResp.name))
                    {
                        _playerHandle = new PlayerHandle(deviceId, getNameResp.name, sessionResp.player_id);
                        setPlayerPrefs(_playerHandle.PlayerName);
                        isStartSessionReady = true;
                        return;
                    }
                    // Name was empty
                    Debug.Log($"SetPlayerName '{_playerHandle.PlayerName}'");
                    LootLockerSDKManager.SetPlayerName(_playerHandle.PlayerName, setNameResp =>
                    {
                        if (!setNameResp.success || string.IsNullOrWhiteSpace(setNameResp.name))
                        {
                            isStartSessionReady = true;
                            return;
                        }
                        _playerHandle = new PlayerHandle(deviceId, setNameResp.name, sessionResp.player_id);
                        setPlayerPrefs(_playerHandle.PlayerName);
                        isStartSessionReady = true;
                    });
                });
            });
        }

        public async Task SetPlayerName(string playerName)
        {
            var setNameResp = await LootLockerAsync.SetPlayerName(playerName);
            if (setNameResp.success)
            {
                Debug.Log($"Update player {_playerHandle.PlayerName} <- {setNameResp.name}");
                _playerHandle = new PlayerHandle(_playerHandle.DeviceId, setNameResp.name, _playerHandle.PlayerId);
            }
            setPlayerPrefs(_playerHandle.PlayerName);
        }

        private static void getPlayerPrefs(out string deviceId, out string playerName)
        {
            var playerData = RuntimeGameConfig.Get().playerDataCache;
            deviceId = playerData.PlayerHandle;
            playerName = playerData.PlayerName;
        }

        private static void setPlayerPrefs(string playerName)
        {
            var playerData = RuntimeGameConfig.Get().playerDataCache;
            if (playerData.PlayerName != playerName)
            {
                playerData.PlayerName = playerName;
                playerData.Save();
            }
        }
    }
}