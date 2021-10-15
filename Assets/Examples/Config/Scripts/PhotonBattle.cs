using Examples.Model.Scripts.Model;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Diagnostics;

namespace Examples.Config.Scripts
{
    public static class PhotonBattle
    {
        public const string playerPositionKey = "pp";
        public const string playerMainSkillKey = "mk";

        public static void getPlayerProperties(Player player, out int playerPos, out int teamIndex)
        {
            playerPos = player.GetCustomProperty(playerPositionKey, -1);
            if (playerPos == 1 || playerPos == 3)
            {
                teamIndex = 1;
            }
            else
            {
                teamIndex = 0;
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void setDebugPlayerProps()
        {
            var player = PhotonNetwork.LocalPlayer;
            player.SetCustomProperties(new Hashtable
            {
                { playerPositionKey, 0 },
                { playerMainSkillKey, (int)Defence.Deflection }
            });
            Debug.LogWarning($"setDebugPlayerProps {player.GetDebugLabel()}");
        }
    }
}