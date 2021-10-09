using LootLocker.Requests;
using System.Threading.Tasks;

namespace Examples.Lobby.Scripts.LootLocker
{
    /// <summary>
    /// Async wrapper to <c>LootLocker</c> SDK API.
    /// </summary>
    /// <remarks>
    /// We use <c>TaskCompletionSource</c> to bind caller and callee "together".
    /// </remarks>
    public static class LootLockerAsync
    {
        public static Task<LootLockerSessionResponse> StartSession(string deviceId)
        {
            var taskCompletionSource = new TaskCompletionSource<LootLockerSessionResponse>();
            LootLockerSDKManager.StartSession(deviceId, response =>
            {
                taskCompletionSource.SetResult(response);
            });
            return taskCompletionSource.Task;
        }

        public static Task<PlayerNameResponse> SetPlayerName(string playerName)
        {
            var taskCompletionSource = new TaskCompletionSource<PlayerNameResponse>();
            LootLockerSDKManager.SetPlayerName(playerName, response =>
            {
                taskCompletionSource.SetResult(response);
            });
            return taskCompletionSource.Task;
        }

        public static Task<PlayerNameResponse> GetPlayerName()
        {
            var taskCompletionSource = new TaskCompletionSource<PlayerNameResponse>();
            LootLockerSDKManager.GetPlayerName(response =>
            {
                taskCompletionSource.SetResult(response);
            });
            return taskCompletionSource.Task;
        }
    }
}