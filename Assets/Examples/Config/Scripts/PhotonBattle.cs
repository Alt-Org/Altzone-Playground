using Examples.Model.Scripts.Model;
using ExitGames.Client.Photon;
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
            else if (playerPos == 0 || playerPos == 2)
            {
                teamIndex = 0;
            }
            else
            {
                teamIndex = -1;
            }
        }

        public static CharacterModel getPlayerCharacterModel(Player player)
        {
            var skillId = player.GetCustomProperty(playerMainSkillKey, -1);
            return Models.GetById<CharacterModel>(skillId);
        }

        [Conditional("UNITY_EDITOR")]
        public static void setDebugPlayerProps(Player player, int playerPos)
        {
            player.SetCustomProperties(new Hashtable
            {
                { playerPositionKey, playerPos },
                { playerMainSkillKey, (int)Defence.Deflection }
            });
            Debug.LogWarning($"setDebugPlayerProps {player.GetDebugLabel()}");
        }
    }
}