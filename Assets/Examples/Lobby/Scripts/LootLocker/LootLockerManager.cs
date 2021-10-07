using LootLocker;
using LootLocker.Requests;
using System;
using UnityEngine;

namespace Examples.Lobby.Scripts.LootLocker
{
    /// <summary>
    /// Test driver for LootLocker Game BAAS
    /// </summary>
    public class LootLockerManager : MonoBehaviour
    {
        public void VerifyID(string playerIdentifier, Action<bool> onComplete)
        {
            var config = LootLockerConfig.Get();
            if (!(config.platform == LootLockerConfig.platformType.Steam || config.platform == LootLockerConfig.platformType.PlayStationNetwork))
            {
                throw new UnityException($"DO NOT CALL VerifyID for {playerIdentifier} - it is reserved for STEAM or PLAYSTATION platforms ONLY!");
            }
            Debug.Log($"VerifyID for {playerIdentifier}");
            LootLockerSDKManager.VerifyID(playerIdentifier, response =>
            {
                Debug.Log($"VerifyID: {response.success}");
                if (response.success)
                {
                    Debug.Log($"{response.statusCode} {response.text.Replace("\n    ", " ")}");
                }
                else
                {
                    Debug.Log($"{response.statusCode} {response.Error.Replace("\n    ", " ")}");
                }
                onComplete?.Invoke(response.success);
            });
        }

        public void StartSession(string playerIdentifier, Action<bool> onComplete)
        {
            Debug.Log($"StartSession for {playerIdentifier}");
            LootLockerSDKManager.StartSession(playerIdentifier, (response) =>
            {
                Debug.Log($"StartSession for {playerIdentifier}: {response.success}");
                if (response.success)
                {
                    Debug.Log($"{response.statusCode} {response.text.Replace("\n    ", " ")}");
                }
                else
                {
                    Debug.Log($"{response.statusCode} {response.Error.Replace("\n    ", " ")}");
                }
            });
        }
    }
}