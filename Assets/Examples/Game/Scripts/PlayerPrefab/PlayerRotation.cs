using Examples.Config.Scripts;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;

namespace Examples.Game.Scripts.PlayerPrefab
{
    /// <summary>
    /// Manages player rotation when an other team player is near.<br />
    /// All calculation is done using local players and nothing is synchronized over network.
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
        [SerializeField] private int actorId;

        [Header("Teammate"), SerializeField] private PlayerRotation teamMate;
        [SerializeField] private Transform _otherTransform;

        [Header("Calculations"), SerializeField] private float sqrDistanceBetween;
        [SerializeField] private float zSide;
        [SerializeField] private float zAngle;
        [SerializeField] private Vector3 myPrevPosition;
        [SerializeField] private Vector3 otherPrevPosition;

        [Header("Rotation Constants"), SerializeField] private float minPlayerRotationAngle;
        [SerializeField] private float maxPlayerRotationAngle;

        // Configurable settings
        private GameVariables variables;

        private bool isUpperSide => teamIndex == 1;

        private void Awake()
        {
            variables = RuntimeGameConfig.Get().variables;
            _photonView = photonView;
            actorId = _photonView.OwnerActorNr;
            _transform = GetComponent<Transform>();
            var player = _photonView.Owner;
            GameManager.getPlayerProperties(player, out playerPos, out teamIndex);
            knownPlayers[playerPos] = this;
            teamMate = knownPlayers.FirstOrDefault(x => x != null && x.teamIndex == teamIndex && x.playerPos != playerPos);
            if (teamMate != null)
            {
                _otherTransform = teamMate._transform;
                if (teamMate.teamMate == null)
                {
                    teamMate.teamMate = this;
                    teamMate._otherTransform = _transform;
                    teamMate.enabled = true;
                }
            }
            else
            {
                enabled = false; // no can do
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

        private void Update()
        {
            rotatePlayer();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (otherPlayer.ActorNumber == teamMate.actorId)
            {
                Debug.Log($"OnPlayerLeftRoom STOP PLAYING otherPlayer={otherPlayer.GetDebugLabel()}");
                enabled = false; // our game play is over when teammate leaves room
            }
        }

        private void rotatePlayer()
        {
            // TODO: check that x,y has changed on me and/or other before doing more calculations!
            var myPosition = _transform.position;
            var otherPosition = _otherTransform.position;
            if (myPosition == myPrevPosition && otherPosition == otherPrevPosition)
            {
                return;
            }
            myPrevPosition = myPosition;
            otherPrevPosition = otherPosition;
            sqrDistanceBetween = Mathf.Abs((myPosition - otherPosition).sqrMagnitude);
            zSide = otherPosition.x < myPosition.x ? -1f : 1f;
            if (sqrDistanceBetween > variables.playerSqrMinRotationDistance)
            {
                if (sqrDistanceBetween < variables.playerSqrMaxRotationDistance)
                {
                    zAngle = distanceToAngle(sqrDistanceBetween);
                    if (isUpperSide)
                    {
                        zSide = -zSide;
                        zAngle += 180f;
                    }
                    // Flipping from side to side when rotation angle is big causes kind of "glitch" as shield changes side very quickly :-(
                    _transform.rotation = Quaternion.Euler(0f, 0f, zAngle * zSide);
                }
                else
                {
                    zAngle = minPlayerRotationAngle;
                    if (isUpperSide)
                    {
                        zAngle += 180f;
                    }
                    _transform.rotation = Quaternion.Euler(0f, 0f, zAngle);
                }
            }
            else
            {
                zAngle = maxPlayerRotationAngle;
                if (isUpperSide)
                {
                    zAngle += 180f;
                }
                _transform.rotation = Quaternion.Euler(0f, 0f, zAngle);
            }
        }

        private float distanceToAngle(float sqrDistance)
        {
            // Linear conversion formula - could be optimized a bit!
            return (sqrDistance - variables.playerSqrMinRotationDistance) /
                (variables.playerSqrMaxRotationDistance - variables.playerSqrMinRotationDistance) *
                Mathf.Abs(minPlayerRotationAngle - maxPlayerRotationAngle) +
                Mathf.Max(minPlayerRotationAngle, maxPlayerRotationAngle);
        }

        public override string ToString()
        {
            return $"{nameof(playerPos)}: {playerPos}, {nameof(teamIndex)}: {teamIndex}";
        }
    }
}