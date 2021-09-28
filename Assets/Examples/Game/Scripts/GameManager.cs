using Examples.Lobby.Scripts;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using System;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Data holder class for team score.
    /// </summary>
    [Serializable]
    public class TeamScore
    {
        public int teamIndex;
        public int headCollisionCount;
        public int wallCollisionCount;

        public byte[] ToBytes()
        {
            return new[] { (byte) teamIndex, (byte) headCollisionCount, (byte) wallCollisionCount };
        }

        public static void FromBytes(object data, out int teamIndex, out int headCollisionCount, out int wallCollisionCount)
        {
            var payload = (byte[]) data;
            teamIndex = payload[0];
            headCollisionCount = payload[1];
            wallCollisionCount = payload[2];
        }

        public override string ToString()
        {
            return $"team: {teamIndex}, headCollision: {headCollisionCount}, wallCollision: {wallCollisionCount}";
        }
    }

    /// <summary>
    /// More game manager example functionality.
    /// </summary>
    public class GameManager : MonoBehaviourPunCallbacks
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 1;

        public Transform[] playerStartPos = new Transform[4];
        public GameObject playerPrefab;

        public LayerMask collisionToHeadMask;
        public int collisionToHead;

        public LayerMask collisionToWallMask;
        public int collisionToWall;

        public TeamScore[] scores;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            Debug.Log($"Awake: {PhotonNetwork.NetworkClientState}");
            scores = new[]
            {
                new TeamScore { teamIndex = 0 },
                new TeamScore { teamIndex = 1 },
            };
            collisionToHead = collisionToHeadMask.value;
            collisionToWall = collisionToWallMask.value;
            // We are disabled until room is ready to play!
            enabled = false;
        }

        private void Start()
        {
            Debug.Log($"Start: {PhotonNetwork.NetworkClientState}");
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, data =>
            {
                TeamScore.FromBytes(data.CustomData, out var _teamIndex, out var _headCollisionCount, out var _wallCollisionCount);
                var score = scores[_teamIndex];

                Debug.Log(
                    $"Synchronize head:{score.headCollisionCount}<-{_headCollisionCount} wall:{score.wallCollisionCount}<-{_wallCollisionCount}");
                score.headCollisionCount = _headCollisionCount;
                score.wallCollisionCount = _wallCollisionCount;

                this.Publish(new Event(score));
            });
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log($"OnEnable: {PhotonNetwork.NetworkClientState}");
            this.Subscribe<BallMovement.Event>(OnBallCollision);
            this.Publish(new Event(scores[0]));
            this.Publish(new Event(scores[1]));
            instantiateLocalPlayer();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            this.Unsubscribe<BallMovement.Event>(OnBallCollision);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
            if (PhotonNetwork.IsMasterClient)
            {
                var startMenu = GetComponent<StartMenu>();
                startMenu.GotoMenu();
            }
        }

        private void OnBallCollision(BallMovement.Event data)
        {
            // var hasLayer = layerMask == (layerMask | 1 << _layer); // unity3d check if layer mask contains a layer

            var _headCollisionCount = 0;
            var _wallCollisionCount = 0;
            var colliderMask = 1 << data.colliderLayer;
            var hasLayer = collisionToHead == (collisionToHead | colliderMask);
            if (hasLayer)
            {
                _headCollisionCount += 1;
            }
            hasLayer = collisionToWall == (collisionToWall | colliderMask);
            if (hasLayer)
            {
                _wallCollisionCount += 1;
            }
            if (_headCollisionCount == 0 && _wallCollisionCount == 0)
            {
                return; // Nothing we care about
            }
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"OnBallCollision head {_headCollisionCount} wall {_wallCollisionCount} data {data}");
                if (_headCollisionCount > 0)
                {
                    GestaltRing.Get().Defence = Defence.Next;
                }
                var collisionSide = data.positionY > 0 ? 1 : 0; // Select collision side from Y coord
                var teamIndex = collisionSide == 1 ? 0 : 1; // Scores are awarded to opposite side!
                var score = scores[teamIndex];
                score.headCollisionCount += _headCollisionCount;
                score.wallCollisionCount += _wallCollisionCount;
                // Synchronize to all game managers
                var payload = score.ToBytes();
                photonEventDispatcher.RaiseEvent(photonEventCode, payload);
            }
        }

        private void instantiateLocalPlayer()
        {
            // Collect parameters for local player instantiation.
            var player = PhotonNetwork.LocalPlayer;
            var playerPos = player.GetCustomProperty(LobbyManager.playerPositionKey, -1);
            if (playerPos < 0 || playerPos >= playerStartPos.Length)
            {
                throw new UnityException($"invalid player position '{playerPos}' for player {player.GetDebugLabel()}");
            }
            var instantiationPosition = playerStartPos[playerPos].position;
            var playerName = player.NickName;
            Debug.Log($"instantiateLocalPlayer i={playerPos} {playerName} : {playerPrefab.name} {instantiationPosition}");
            var instance = _instantiateLocalPlayer(playerPrefab.name, instantiationPosition, playerName);
            // Parent under us!
            var playerTransform = instance.transform;
            playerTransform.parent = transform;
        }

        private static GameObject _instantiateLocalPlayer(string prefabName, Vector3 instantiationPosition, string playerName)
        {
            var instance = PhotonNetwork.Instantiate(prefabName, instantiationPosition, Quaternion.identity);
            instance.name = instance.name.Replace("(Clone)", $"({playerName})");
            return instance;
        }

        public class Event
        {
            public readonly TeamScore score;

            public Event(TeamScore score)
            {
                this.score = score;
            }

            public override string ToString()
            {
                return $"{nameof(score)}: {score}";
            }
        }
    }
}