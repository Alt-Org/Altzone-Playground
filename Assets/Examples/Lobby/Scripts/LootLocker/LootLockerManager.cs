using LootLocker.Requests;
using UnityEngine;

namespace Examples.Lobby.Scripts.LootLocker
{
    public class LootLockerManager : MonoBehaviour
    {
        public string playerIdentifier;

        private void Start()
        {
            Debug.Log($"StartSession for {playerIdentifier}");
            LootLockerSDKManager.StartSession(playerIdentifier, (response) =>
            {
                Debug.Log($"StartSession: {response.success}");
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