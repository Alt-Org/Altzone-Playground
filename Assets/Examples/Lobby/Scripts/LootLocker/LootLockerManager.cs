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
        [SerializeField] private string public_uid;
        [SerializeField] private string session_token;

        public string DeviceId => deviceId;
        public int PlayerId => player_id;

        public string PlayerName
        {
            get => playerName;
            set => playerName = value;
        }

        public PlayerHandle(string deviceId, string playerName, int playerId, string publicUid, string sessionToken)
        {
            this.deviceId = deviceId;
            this.playerName = playerName;
            player_id = playerId;
            public_uid = publicUid;
            session_token = sessionToken;
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
                // Create dummy player using PlayerPrefs values
                _playerHandle = new PlayerHandle(deviceId, playerName, 0, "", "");
                return;
            }
            _playerHandle = new PlayerHandle(deviceId, playerName, sessionResp.player_id, sessionResp.public_uid, sessionResp.session_token);

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
                _playerHandle.PlayerName = getNameResp.name;
                PlayerPrefs.SetString("lootLocker.playerName", _playerHandle.PlayerName);
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
                    _playerHandle = new PlayerHandle(deviceId, playerName, 0, "", "");
                    isStartSessionReady = true;
                    return;
                }
                _playerHandle = new PlayerHandle(deviceId, playerName, sessionResp.player_id, sessionResp.public_uid, sessionResp.session_token);
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
                        _playerHandle.PlayerName = getNameResp.name;
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
                        _playerHandle.PlayerName = setNameResp.name;
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
                _playerHandle.PlayerName = setNameResp.name;
            }
        }

        private static void getPlayerPrefs(out string deviceId, out string playerName)
        {
            deviceId = PlayerPrefs.GetString("lootLocker.deviceId", "");
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("lootLocker.deviceId", deviceId);
            }
            playerName = PlayerPrefs.GetString("lootLocker.playerName", "");
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = $"Player{1000 * (1 + DateTime.Now.Second % 10) + DateTime.Now.Millisecond:00}";
                PlayerPrefs.SetString("lootLocker.playerName", playerName);
            }
        }

        private bool isApplicationQuit;

        private void OnApplicationQuit()
        {
            isApplicationQuit = true;
        }

        private void OnDestroy()
        {
            if (!isApplicationQuit && isValid)
            {
                // Try to be polite and close session as we don't need it after this scene.
                LootLockerSDKManager.EndSession(_playerHandle.DeviceId, null);
            }
        }
    }
}