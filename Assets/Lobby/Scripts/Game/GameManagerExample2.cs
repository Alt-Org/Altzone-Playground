using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using System;
using UnityEngine;

namespace Lobby.Scripts.Game
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
    public class GameManagerExample2 : MonoBehaviour
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 1;

        public GameObject gameManager1;
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
            gameObject.SetActive(false); // Wait until we are signalled to go by setting us active again
        }

        private void Start()
        {
            Debug.Log($"Start: {PhotonNetwork.NetworkClientState}");
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, (data) =>
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

        private void OnEnable()
        {
            Debug.Log($"OnEnable: {PhotonNetwork.NetworkClientState}");
            this.Subscribe<BallMovementV2.Event>(OnBallCollision);
            this.Publish(new Event(scores[0]));
            this.Publish(new Event(scores[1]));
            instantiateLocalPlayer();
        }

        private void OnDisable()
        {
            this.Unsubscribe<BallMovementV2.Event>(OnBallCollision);
        }

        private void OnBallCollision(BallMovementV2.Event data)
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
                var teamIndex = data.positionY > 0 ? 1 : 0; // Select upper or lower team from Y coord
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
            Debug.Log($"instantiateLocalPlayer: {playerPrefab.name}");
            // Collect parameters for local player instantiation.
            var manager = gameManager1.GetComponent<GameManagerExample1>();
            var instantiationPosition = manager.playerStartPos[0].position;
            var playerName = PhotonNetwork.LocalPlayer.NickName;
            var instance = _instantiateLocalPlayer(playerPrefab.name, instantiationPosition, playerName);
            // Parent under us!
            instance.transform.parent = transform;
        }

        private static GameObject _instantiateLocalPlayer(string prefabName, Vector3 instantiationPosition, string playerName)
        {
            var instance = PhotonNetwork.Instantiate(prefabName, instantiationPosition, Quaternion.identity, 0, null);
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