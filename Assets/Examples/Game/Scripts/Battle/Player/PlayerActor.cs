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
        float CurrentSpeed { get; }
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
        float IPlayerActor.CurrentSpeed => _Speed;

        private float _Speed;

        private void Awake()
        {
            var player = PhotonView.Get(this).Owner;
            PhotonBattle.getPlayerProperties(player, out playerPos, out teamIndex);
            var model = PhotonBattle.getPlayerCharacterModel(player);
            _Speed = model.Speed;
            Debug.Log($"Awake {player.NickName} pos={playerPos} team={teamIndex}");
            shields[((IPlayerActor)this).OppositeTeam].SetActive(false);
            colliders = GetComponentsInChildren<Collider2D>(includeInactive: false);

            // Re-parent and set name
            var sceneConfig = SceneConfig.Get();
            transform.parent = sceneConfig.actorParent.transform;
            name = $"{(player.IsLocal ? "L" : "R")}{playerPos}:{teamIndex}:{player.NickName}";

            setupPlayer(player);
        }

        private void setupPlayer(Photon.Realtime.Player player)
        {
            // Setup input system to move player around - PlayerMovement is required on both ends for RPC!
            var playerMovement = gameObject.AddComponent<PlayerMovement>();
            if (player.IsLocal)
            {
                setupLocalPlayer(playerMovement);
            }
            else
            {
                setupRemotePlayer();
            }
        }

        private void setupLocalPlayer(PlayerMovement playerMovement)
        {
            var sceneConfig = SceneConfig.Get();

            var playArea = sceneConfig.getPlayArea(playerPos);
            ((IRestrictedPlayer)playerMovement).setPlayArea(playArea);

            var playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.Camera = sceneConfig._camera;
            playerInput.PlayerMovement = playerMovement;
            if (!Application.isMobilePlatform)
            {
                var keyboardInput = gameObject.AddComponent<PlayerInputKeyboard>();
                keyboardInput.PlayerMovement = playerMovement;
            }
        }

        private void setupRemotePlayer()
        {
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