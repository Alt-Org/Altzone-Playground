using Examples.Config.Scripts;
using Photon.Pun;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    /// <summary>
    /// Player base class for common player data.
    /// </summary>
    public class PlayerActor : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private GameObject[] shields;

        [Header("Live Data"), SerializeField] private int playerPos;
        [SerializeField] private int teamIndex;
        [SerializeField] private Collider2D[] colliders;

        public int PlayerPos => playerPos;
        public int TeamMatePos => getTeamMatePos(playerPos);
        public int TeamIndex => teamIndex;

        private void Awake()
        {
            var player = PhotonNetwork.LocalPlayer;
            PhotonBattle.getPlayerProperties(player, out playerPos, out teamIndex);
            Debug.Log($"Awake {player.NickName} pos={playerPos} team={teamIndex}");
            var oppositeTeam = teamIndex == 0 ? 1 : 0;
            shields[oppositeTeam].SetActive(false);
            colliders = GetComponentsInChildren<Collider2D>(includeInactive: false);

            enabled = false; // Wait until game starts
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable pos={playerPos} team={teamIndex}");
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable pos={playerPos} team={teamIndex}");
        }

        private static int getTeamMatePos(int playerPos)
        {
            switch (playerPos)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 0;
                case 3:
                    return 1;
                default:
                    throw new UnityException($"invalid player pos: {playerPos}");
            }
        }
    }
}