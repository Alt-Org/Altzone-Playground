using Examples.Lobby.Scripts;
using ExitGames.Client.Photon;
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
        private const int photonEventCodeBrick = PhotonEventDispatcher.eventCodeBase + 3;

        public Transform[] playerStartPos = new Transform[4];
        public Rect[] playerPlayArea = new Rect[4];
        public GameObject playerPrefab;
        public BrickManager brickManager;

        public LayerMask collisionToHeadMask;
        public int collisionToHead;

        public LayerMask collisionToWallMask;
        public int collisionToWall;

        public LayerMask collisionToBrickMask;
        public int collisionToBrick;

        public TeamScore[] scores;

        [SerializeField] private Camera _camera;

        private PhotonEventDispatcher photonEventDispatcher;

        public Camera Camera
        {
            get => _camera;
            set => _camera = value;
        }

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
            collisionToBrick = collisionToBrickMask.value;
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

                this.Publish(new TeamScoreEvent(score));
                // Dirty hack to keep score updating in somewhere global storage
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
                {
                    var room = PhotonNetwork.CurrentRoom;
                    var key = $"T{score.teamIndex}";
                    var value = score.wallCollisionCount;
                    room.SetCustomProperties(new Hashtable { { key, value } });
                }
            });
            photonEventDispatcher.registerEventListener(photonEventCodeBrick, data =>
            {
                var brickId = (short) data.CustomData;
                brickManager.deleteBrick(brickId);
            });
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log($"OnEnable: {PhotonNetwork.NetworkClientState}");
            this.Subscribe<BallMovement.CollisionEvent>(OnCollisionEvent);
            this.Publish(new TeamScoreEvent(scores[0]));
            this.Publish(new TeamScoreEvent(scores[1]));
            instantiateLocalPlayer();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            this.Unsubscribe();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
            if (PhotonNetwork.IsMasterClient)
            {
                var menu = GetComponent<MainMenu>();
                menu.GotoMainMenu();
            }
        }

        private void OnCollisionEvent(BallMovement.CollisionEvent data)
        {
            // var hasLayer = layerMask == (layerMask | 1 << _layer); // unity3d check if layer mask contains a layer

            if (!PhotonNetwork.IsMasterClient)
            {
                return; // Only mast client handles these and forwards to all players
            }
            var _headCollisionCount = 0;
            var _wallCollisionCount = 0;
            var colliderMask = 1 << data.layer;
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
            if (_headCollisionCount > 0 || _wallCollisionCount > 0)
            {
                headOrWallCollision(_headCollisionCount, _wallCollisionCount, data.positionY);
            }
            hasLayer = collisionToBrick == (collisionToBrick | colliderMask);
            if (hasLayer)
            {
                brickCollision(data.hitObject);
            }
        }

        private void brickCollision(GameObject brickObject)
        {
            var brickMarker = brickObject.GetComponent<BrickMarker>();
            Debug.Log($"brickCollision {brickMarker} layer {brickObject.layer}");
            var payload = (short) brickMarker.BrickId;
            photonEventDispatcher.RaiseEvent(photonEventCodeBrick, payload);
        }

        private void headOrWallCollision(int _headCollisionCount, int _wallCollisionCount, float positionY)
        {
            Debug.Log($"headOrWallCollision head {_headCollisionCount} wall {_wallCollisionCount} positionY {positionY}");
            if (_headCollisionCount > 0)
            {
                GestaltRing.Get().Defence = Defence.Next;
            }
            var collisionSide = positionY > 0 ? 1 : 0; // Select collision side from Y coord
            var teamIndex = collisionSide == 1 ? 0 : 1; // Scores are awarded to opposite side!
            var score = scores[teamIndex];
            score.headCollisionCount += _headCollisionCount;
            score.wallCollisionCount += _wallCollisionCount;
            // Synchronize to all game managers
            var payload = score.ToBytes();
            photonEventDispatcher.RaiseEvent(photonEventCode, payload);
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
            // Calculate player area
            if (playerPlayArea[playerPos].width == 0)
            {
                // For convenience player start positions are kept under corresponding play area.
                // - play area is marked by collider to get its bounds for player area calculation
                var playAreaTransform = playerStartPos[playerPos].parent;
                var center = playAreaTransform.position;
                var bounds = playAreaTransform.GetComponent<Collider2D>().bounds;
                playerPlayArea[playerPos] = calculateRectFrom(center, bounds);
            }
            // Add input system to move player around
            var playerMovement = instance.GetComponent<PlayerMovement>();
            playerMovement.setPlayArea(playerPlayArea[playerPos]);
            var playerInput = instance.AddComponent<PlayerInput>();
            playerInput.Camera = Camera;
            playerInput.PlayerMovement = playerMovement;
            if (!Application.isMobilePlatform)
            {
                var keyboardInput = instance.AddComponent<PlayerInputKeyboard>();
                keyboardInput.PlayerMovement = playerMovement;
            }
        }

        private static GameObject _instantiateLocalPlayer(string prefabName, Vector3 instantiationPosition, string playerName)
        {
            var instance = PhotonNetwork.Instantiate(prefabName, instantiationPosition, Quaternion.identity);
            return instance;
        }

        private static Rect calculateRectFrom(Vector3 center, Bounds bounds)
        {
            var extents = bounds.extents;
            var size = bounds.size;
            var x = center.x - extents.x;
            var y = center.y - extents.y;
            var width = size.x;
            var height = size.y;
            return new Rect(x, y, width, height);
        }

        public class TeamScoreEvent
        {
            public readonly TeamScore score;

            public TeamScoreEvent(TeamScore score)
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