using Examples.Lobby.Scripts;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Manages player rotation when an other team player is near.
    /// </summary>
    /// <remarks>
    /// For simplicity we assume that all players are present in the room when playing starts.
    /// </remarks>
    public class PlayerRotation : MonoBehaviourPunCallbacks
    {
        private static readonly PlayerRotation[] knownPlayers = new PlayerRotation[4];

        [Header("Live Data"), SerializeField] protected PhotonView _photonView;
        [SerializeField] protected Transform _transform;
        [SerializeField] private int playerPos;
        [SerializeField] private int teamIndex;
        [SerializeField] private PlayerRotation teamMate;

        private void Awake()
        {
            _photonView = photonView;
            _transform = GetComponent<Transform>();
            var player = _photonView.Owner;
            playerPos = player.GetCustomProperty(LobbyManager.playerPositionKey, -1);
            if (playerPos == 1 || playerPos == 3)
            {
                teamIndex = 1;
            }
            else
            {
                teamIndex = 0;
            }
            knownPlayers[playerPos] = this;
            teamMate = knownPlayers.FirstOrDefault(x => x != null && x.teamIndex == teamIndex && x.playerPos != playerPos);
            if (teamMate != null && teamMate.teamMate == null)
            {
                teamMate.teamMate = this;
            }
            Debug.Log($"Awake IsMine={_photonView.IsMine} teamMate={teamMate}");
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log($"OnEnable teamIndex={teamIndex} playerPos={playerPos}");
        }

        public override void OnDisable()
        {
            Debug.Log($"OnDisable IsMine={_photonView.IsMine}");
            base.OnDisable();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            enabled = false; // game play is over when one player leaves room
        }

        public override string ToString()
        {
            return $"{nameof(playerPos)}: {playerPos}, {nameof(teamIndex)}: {teamIndex}";
        }
    }
}