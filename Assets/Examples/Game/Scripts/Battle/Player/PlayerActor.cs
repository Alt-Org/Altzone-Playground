using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Scene;
using Photon.Pun;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    public interface IPlayerActor
    {
        int PlayerPos { get; }
        int TeamMatePos { get; }
        int TeamIndex { get; }
        int OppositeTeam { get; }
        void setGhosted();
    }

    /// <summary>
    /// Player base class for common player data.
    /// </summary>
    public class PlayerActor : MonoBehaviour, IPlayerActor
    {
        [Header("Settings"), SerializeField] private GameObject[] shields;

        [Header("Live Data"), SerializeField] private int playerPos;
        [SerializeField] private int teamIndex;
        [SerializeField] private Collider2D[] colliders;

        int IPlayerActor.PlayerPos => playerPos;
        int IPlayerActor.TeamMatePos => getTeamMatePos(playerPos);
        int IPlayerActor.TeamIndex => teamIndex;
        int IPlayerActor.OppositeTeam => teamIndex == 0 ? 1 : 0;

        private void Awake()
        {
            var player = PhotonView.Get(this).Owner;
            PhotonBattle.getPlayerProperties(player, out playerPos, out teamIndex);
            Debug.Log($"Awake {player.NickName} pos={playerPos} team={teamIndex}");
            shields[((IPlayerActor)this).OppositeTeam].SetActive(false);
            colliders = GetComponentsInChildren<Collider2D>(includeInactive: false);

            // Re-parent and set name
            var sceneConfig = SceneConfig.Get();
            transform.parent = sceneConfig.actorParent.transform;
            name = $"{(player.IsLocal ? "L" : "R")}{playerPos}:{teamIndex}:{player.NickName}";
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable pos={playerPos} team={teamIndex}");
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable pos={playerPos} team={teamIndex}");
        }

        void IPlayerActor.setGhosted()
        {
            Debug.Log($"setGhosted pos={playerPos} team={teamIndex}");
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