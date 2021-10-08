using LootLocker.Requests;
using System.Threading.Tasks;

namespace Examples.Lobby.Scripts.LootLocker
{
    /// <summary>
    /// Async wrapper to <c>LootLocker</c> SDK API.
    /// </summary>
    public static class LootLockerAsync
    {
        public static async Task<LootLockerSessionResponse> StartSession(string deviceId)
        {
            var taskCompletionSource = new TaskCompletionSource<LootLockerSessionResponse>();
            LootLockerSDKManager.StartSession(deviceId, response =>
            {
                taskCompletionSource.TrySetResult(response);
            });
            return await taskCompletionSource.Task;
        }

        public static async Task<PlayerNameResponse> SetPlayerName(string playerName)
        {
            var taskCompletionSource = new TaskCompletionSource<PlayerNameResponse>();
            LootLockerSDKManager.SetPlayerName(playerName, response =>
            {
                taskCompletionSource.TrySetResult(response);
            });
            return await taskCompletionSource.Task;
        }

        public static async Task<PlayerNameResponse> GetPlayerName()
        {
            var taskCompletionSource = new TaskCompletionSource<PlayerNameResponse>();
            LootLockerSDKManager.GetPlayerName(response =>
            {
                taskCompletionSource.TrySetResult(response);
            });
            return await taskCompletionSource.Task;
        }
    }
}