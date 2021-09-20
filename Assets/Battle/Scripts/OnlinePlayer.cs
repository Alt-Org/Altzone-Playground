using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Unity.Input;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class OnlinePlayer : MonoBehaviourPunCallbacks, IPunObservable
    {
        [Header("Player Settings"), SerializeField, Min(float.Epsilon)] private float playerMoveSpeed;
        [SerializeField] private float playerTeleportDistance;
        [SerializeField] private float playerRotationZ;
        [SerializeField, Min(float.Epsilon)] private float keyboardMultiplier;

        [Header("Bot Settings"), SerializeField] private bool isBot;
        [SerializeField, Min(float.Epsilon)] private float botMoveSpeed;
        [SerializeField] private float minTeamPlayerDistance;

        [Header("Room Settings"), SerializeField] public string playerPosName;
        [SerializeField] private string otherPlayerPosName;

        [Header("Player Setup"), SerializeField] private GameObject peliHahmo;
        [SerializeField] private SpriteRenderer head;
        [SerializeField] private GameObject shield;

        [Header("Play Area Setup"), SerializeField] private Vector2 startPos;
        [SerializeField] private bool isPlayerClamped;
        [SerializeField] private Vector2 areaMin;
        [SerializeField] private Vector2 areaMax;

        [Header("Live Data"), SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;
        [SerializeField] private float positionZ;
        [SerializeField] private bool isMouseDown;
        [SerializeField] private Vector3 mousePosition;
        [SerializeField] private float networkLag;
        [SerializeField] private Vector2 input;
        [SerializeField] private ShieldTurns rotator;
        [SerializeField] private Ball ball;
        [SerializeField] private Transform ballTransform;
        [SerializeField] private OnlinePlayer teamPlayer;
        [SerializeField] private Transform teamPlayerTransform;
        public float targetTime = 0.0f;

        [Header("Photon"), SerializeField] private int playerActorNumber;
        [SerializeField] private int otherPlayerActorNumber;

        private bool isRemotePlayer;
        private float prevSpeed;

        public void playerStop(int source) {
            if(source == 1) 
            {
                targetTime = 1.0f;
            }
            else if((source == 2) && (targetTime <= 0.5f)) 
            {
                targetTime = 0.5f;
            }
            
        }

        public override void OnEnable()
        {
            prevSpeed = playerMoveSpeed;
            base.OnEnable();
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log(room.GetDebugLabel());
            _camera = Camera.main;
            _transform = GetComponent<Transform>();
            // Set position and rotation
            var temp = _transform.position;
            positionZ = temp.z;
            temp.x = startPos.x;
            temp.y = startPos.y;
            _transform.position = temp;
            _transform.rotation = Quaternion.Euler(0f, 0f, playerRotationZ);
            mousePosition = temp;
            if (photonView.IsMine)
            {
                if (!isBot)
                {
                    this.Subscribe<InputManager.ClickDownEvent>(onClickDownEvent);
                    this.Subscribe<InputManager.ClickUpEvent>(onClickUpEvent);
                }
            }
            else
            {
                isRemotePlayer = true;
                isPlayerClamped = false; // no need for remote to be clamped
            }
            PhotonNetwork.AddCallbackTarget(this);
            // Rename
            name = name.Replace("(Clone)", "");
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState} {name}");
            // Add and setup player rotation
            setupRotator();
            // Setup player relations
            var player = photonView.Owner;
            playerActorNumber = photonView.OwnerActorNr;
            otherPlayerActorNumber = -1;
            if (!isBot)
            {
                foreach (var somePlayer in room.GetPlayerList())
                {
                    if (!player.Equals(somePlayer))
                    {
                        checkPlayerWithOtherPlayer(player, somePlayer);
                    }
                }
            }
            else
            {
                foreach (var somePlayer in room.GetPlayerList())
                {
                    checkBotWithOtherPlayer(player, somePlayer);
                }
                checkMyOtherBots(player, room);
                // Get reference to ball
                UnityInstanceManager.TryGet("Ball", out ball);
                ballTransform = ball.transform;
            }
        }

        private void setupRotator()
        {
            if (rotator == null)
            {
                rotator = gameObject.GetOrAddComponent<ShieldTurns>();
                rotator.sqrMinPlayerRotationDistance = 1f * 2f;
                rotator.sqrMaxPlayerRotationDistance = 3f * 3f;
                rotator.sqrShieldDissappearDistance = 5.4f;
                rotator.shield = shield;
                rotator.playerRotationZ = playerRotationZ;
            }
            rotator._transform = peliHahmo.transform;
            rotator.enabled = false;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Debug.Log($"OnDisable {PhotonNetwork.NetworkClientState} {name}");
            if (photonView.IsMine)
            {
                if (!isBot)
                {
                    this.Unsubscribe<InputManager.ClickDownEvent>(onClickDownEvent);
                    this.Unsubscribe<InputManager.ClickUpEvent>(onClickUpEvent);
                }
            }
            PhotonNetwork.RemoveCallbackTarget(this);
            rotator.enabled = false;
        }

        private void Update()
        {
            
            targetTime -= Time.deltaTime;
            
            if (targetTime > 0.0f)
            {                
                head.color = Color.black;
                playerMoveSpeed = 0;
            } else {
                timerEnded();
            }

            if (photonView.IsMine)
            {
                readInput();
            }
            updatePosition();
        }

        private void timerEnded()
        {
            playerMoveSpeed = prevSpeed;
            head.color = new Color(0.0f, 0.5f, 0.0f, 1f);
        }

        #region Input events

        private void onClickDownEvent(InputManager.ClickDownEvent data)
        {
            isMouseDown = true;
            mousePosition = _camera.ScreenToWorldPoint(data.ScreenPosition);
            mousePosition.z = positionZ;
        }

        private void onClickUpEvent(InputManager.ClickUpEvent data)
        {
            isMouseDown = false;
            mousePosition = _transform.position;
        }

        private void readInput()
        {
            if (isBot)
            {
                calculateBotMovement();
                return;
            }
            if (isMouseDown)
            {
                return; // No keyboard/gamepad when mouse is down
            }
            // GetAxisRaw will only ever return 0, -1, or 1 (assuming a digital input such as a keyboard or joystick button)
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if (input.x != 0f)
            {
                mousePosition.x += input.x * keyboardMultiplier * Time.deltaTime;
            }
            if (input.y != 0f)
            {
                mousePosition.y += input.y * keyboardMultiplier * Time.deltaTime;
            }
        }

        #endregion

        #region Player and bot movement

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                input = _transform.position;
                stream.SendNext(input);
                stream.SendNext(targetTime);
            }
            else
            {
                // Network player, receive data
                input = (Vector2) stream.ReceiveNext();
                mousePosition.x = input.x;
                mousePosition.y = input.y;
                mousePosition.z = positionZ;
                targetTime = (float) stream.ReceiveNext();
                networkLag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            }
        }

        private void updatePosition()
        {
            var playerPosition = _transform.position;
            float speed;
            if (!isBot)
            {
                speed = playerMoveSpeed * Time.deltaTime;
            }
            else if (isRemotePlayer)
            {
                // Teleport shortcut
                if (Mathf.Abs(playerPosition.x - mousePosition.x) > playerTeleportDistance ||
                    Mathf.Abs(playerPosition.y - mousePosition.y) > playerTeleportDistance)
                {
                    _transform.position = mousePosition;
                    return;
                }
                speed = Time.deltaTime + networkLag;
            }
            else
            {
                speed = botMoveSpeed * Time.deltaTime;
            }
            if (isPlayerClamped)
            {
                mousePosition.x = Mathf.Clamp(mousePosition.x, areaMin.x, areaMax.x);
                mousePosition.y = Mathf.Clamp(mousePosition.y, areaMin.y, areaMax.y);
            }
            _transform.position = Vector3.MoveTowards(playerPosition, mousePosition, speed);
        }

        private static readonly Vector2 centerPosition = Vector2.zero;

        private void calculateBotMovement()
        {
            var myPosition = _transform.position;
            var ballPosition = ballTransform.position;
            // Ball must be vertically between center line and us
            var goForBall = (myPosition.y > centerPosition.y && myPosition.y > ballPosition.y)
                            || (myPosition.y < centerPosition.y && myPosition.y < ballPosition.y);
            if (!goForBall)
            {
                mousePosition.x = myPosition.x;
                mousePosition.y = myPosition.y;
                return;
            }
            if (otherPlayerActorNumber == -1)
            {
                mousePosition.x = ballPosition.x;
                mousePosition.y = myPosition.y;
                return;
            }
            // Check our position with ball and team player
            var otherPosition = teamPlayerTransform.position;
            var deltaBall = ballPosition.x - myPosition.x;
            var isBallLeft = deltaBall < 0f;
            var deltaOther = otherPosition.x - myPosition.x;
            var isOtherLeft = deltaOther < 0f;
            var canMoveX = true;
            if (isBallLeft)
            {
                if (isOtherLeft)
                {
                    var distanceToOther = Mathf.Abs(deltaOther);
                    canMoveX = distanceToOther > minTeamPlayerDistance;
                }
            }
            else // ball on right side
            {
                if (!isOtherLeft)
                {
                    var distanceToOther = Mathf.Abs(deltaOther);
                    canMoveX = distanceToOther > minTeamPlayerDistance;
                }
            }
            mousePosition.x = canMoveX ? ballPosition.x : myPosition.x;
            mousePosition.y = myPosition.y;
        }

        #endregion

        #region Gameplay situtational awareness

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            var player = photonView.Owner;
            if (player.IsLocal)
            {
                Debug.Log($"OnMasterClientSwitched newMasterClient={newMasterClient.GetDebugLabel()}");
            }
            if (PhotonNetwork.IsMasterClient)
            {
                var room = PhotonNetwork.CurrentRoom;
                checkForAllPlayers(room);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (otherPlayerActorNumber == -1)
            {
                var player = photonView.Owner;
                checkPlayerWithOtherPlayer(player, newPlayer);
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (otherPlayerActorNumber == -1)
            {
                var player = photonView.Owner;
                checkPlayerWithOtherPlayer(player, targetPlayer);
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (photonView.Owner.IsLocal)
            {
                Debug.Log($"OnPlayerLeftRoom otherPlayer={otherPlayer.GetDebugLabel()}");
            }
            if (otherPlayer.ActorNumber == otherPlayerActorNumber)
            {
                otherPlayerActorNumber = -1; // Disconnected
                if (isBot)
                {
                    isPlayerClamped = false;
                }
            }
            if (PhotonNetwork.IsMasterClient)
            {
                var room = PhotonNetwork.CurrentRoom;
                checkForAllPlayers(room);
            }
        }

        private void checkPlayerWithOtherPlayer(Player player, Player otherPlayer)
        {
            // Both players must have N=Pn, where n = 0..3
            // and values must be valid pairs for a team: P0/P2 and P1/P3§
            var myRoomPosPropName = player.GetCustomProperty<string>(CreateOnlinePlayer.playerPosKeyName);
            if (string.IsNullOrWhiteSpace(myRoomPosPropName))
            {
                return;
            }
            var otherPlayerRoomPosPropName = otherPlayer.GetCustomProperty<string>(CreateOnlinePlayer.playerPosKeyName);
            if (string.IsNullOrWhiteSpace(otherPlayerRoomPosPropName))
            {
                return;
            }
            if (otherPlayerRoomPosPropName != otherPlayerPosName)
            {
                return;
            }
            if (!UnityInstanceManager.TryGet<OnlinePlayer>(otherPlayerRoomPosPropName, out var other))
            {
                return;
            }
            // We must activate both participants at the same time when they are available
            // - when last team player joins and is ready to play
            connectWith(player, otherPlayer, other);
            Debug.Log(
                $"checkPlayerWithOtherPlayer MATCH {playerPosName}->{otherPlayerPosName} : player={player.GetDebugLabel()} otherPlayer={otherPlayer.GetDebugLabel()}");
        }

        private void checkBotWithOtherPlayer(Player player, Player otherPlayer)
        {
            var otherPlayerRoomPosPropName = otherPlayer.GetCustomProperty<string>(CreateOnlinePlayer.playerPosKeyName);
            if (string.IsNullOrWhiteSpace(otherPlayerRoomPosPropName))
            {
                return;
            }
            if (otherPlayerRoomPosPropName != otherPlayerPosName)
            {
                return;
            }
            if (!UnityInstanceManager.TryGet<OnlinePlayer>(otherPlayerRoomPosPropName, out var other))
            {
                return;
            }
            // We must activate both participants at the same time when they are available
            // - when last team player joins and is ready to play
            connectWith(player, otherPlayer, other);
            Debug.Log(
                $"checkBotWithOtherPlayer MATCH {playerPosName}->{otherPlayerPosName} : BOT otherPlayer={otherPlayer.GetDebugLabel()}");
        }

        private void checkMyOtherBots(Player player, Room room)
        {
            var roomPosKeyNames = CreateOnlinePlayer.roomPosKeyNames;
            var props = room.CustomProperties;
            foreach (var keyName in roomPosKeyNames)
            {
                var keyValue = props[keyName].ToString();
                if (keyValue != "0" && keyValue != playerPosName)
                {
                    if (UnityInstanceManager.TryGet<OnlinePlayer>(keyName, out var otherBot))
                    {
                        if (otherBot.playerPosName == otherPlayerPosName)
                        {
                            isPlayerClamped = true;
                            otherBot.isPlayerClamped = true;
                            connectWith(player, player, otherBot);
                            Debug.Log(
                                $"checkMyOtherBots MATCH {playerPosName}->{otherPlayerPosName} : BOT+BOT player={player.GetDebugLabel()}");
                        }
                    }
                }
            }
        }

        private void connectWith(Player player, Player otherPlayer, OnlinePlayer other)
        {
            rotator._otherTransform = other.peliHahmo.transform;
            rotator.enabled = true;
            other.rotator._otherTransform = peliHahmo.transform;
            other.rotator.enabled = true;

            otherPlayerActorNumber = otherPlayer.ActorNumber;
            other.otherPlayerActorNumber = player.ActorNumber;

            teamPlayer = other;
            teamPlayerTransform = other._transform;
            other.teamPlayer = this;
            other.teamPlayerTransform = _transform;
        }

        private void checkForAllPlayers(Room room)
        {
            var roomPosKeyNames = CreateOnlinePlayer.roomPosKeyNames;
            var props = room.CustomProperties;
            foreach (var keyName in roomPosKeyNames)
            {
                var keyValue = props[keyName].ToString();
                if (keyValue == "0" || !int.TryParse(keyValue, out var actorNUmber))
                {
                    continue;
                }
                var isPlayerPosValid = false;
                foreach (var player in room.GetPlayerList())
                {
                    if (player.ActorNumber == actorNUmber)
                    {
                        isPlayerPosValid = true;
                    }
                }
                if (!isPlayerPosValid)
                {
                    // Release player position
                    Debug.Log(room.GetDebugLabel());
                    Debug.Log($"ROOM SafeSetCustomProperty {keyName} : 0 <- {actorNUmber}");
                    room.SafeSetCustomProperty(keyName, 0, (byte) actorNUmber);
                }
            }
        }

        #endregion
    }
}