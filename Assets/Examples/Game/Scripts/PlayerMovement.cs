﻿using Examples.Game.Scripts.Config;
using Examples.Lobby.Scripts;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Simple player movement across network clients using mouse or touch.
    /// </summary>
    /// <remarks>
    /// Listens input events that control movement to given position and team activation events that control if player can move.
    /// </remarks>
    public class PlayerMovement : MonoBehaviourPunCallbacks
    {
        [Header("Live Data"), SerializeField] protected PhotonView _photonView;
        [SerializeField] protected Transform _transform;
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Vector3 inputTarget;
        [SerializeField] private bool isMoving;
        [SerializeField] private bool canMove;
        [SerializeField] private Vector3 validTarget;
        [SerializeField] private int playerPos;
        [SerializeField] private int teamIndex;
        [SerializeField] private Rect playArea;
        [SerializeField] private PlayerColor playerColor;

        // Configurable settings
        private GameVariables variables;

        private void Awake()
        {
            variables = RuntimeGameConfig.Get().variables;
            _photonView = photonView;
            _transform = GetComponent<Transform>();
            initialPosition = _transform.position;
            validTarget = initialPosition;
            playerColor = GetComponent<PlayerColor>();
            // Set playerName for Editor
            var playerName = _photonView.Owner.NickName;
            name = name.Replace("(Clone)", $"({playerName})");
            // Re-parent so that all game actors are in one place. These are the ball and all players.
            var actorParent = FindObjectOfType<BallMovement>().transform.parent;
            _transform.parent = actorParent;
            Debug.Log($"Awake {playerName} IsMine={_photonView.IsMine} initialPosition={initialPosition}");
        }

        public override void OnEnable()
        {
            Debug.Log($"OnEnable IsMine={_photonView.IsMine} initialPosition={initialPosition}");
            base.OnEnable();
            this.Subscribe<BallMovement.ActiveTeamEvent>(OnActiveTeam);
            if (PhotonNetwork.InRoom)
            {
                startPlaying();
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            this.Unsubscribe<BallMovement.ActiveTeamEvent>(OnActiveTeam);
        }

        private void OnActiveTeam(BallMovement.ActiveTeamEvent data)
        {
            canMove = data.teamIndex == teamIndex;
            if (canMove)
            {
                playerColor.setNormalColor();
            }
            else
            {
                playerColor.setDisabledColor();
                if (isMoving)
                {
                    stopMoving();
                }
            }
        }

        private void Update()
        {
            if (!isMoving)
            {
                return;
            }
            if (!canMove)
            {
                return;
            }
            var speed = variables.playerMoveSpeed * Time.deltaTime;
            var newPosition = Vector3.MoveTowards(_transform.position, validTarget, speed);
            isMoving = newPosition != validTarget;
            _transform.position = newPosition;
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            startPlaying();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            enabled = false; // game play is over when one player leaves room
        }

        public void setPlayArea(Rect area)
        {
            Debug.Log($"setPlayArea {area}");
            playArea = area;
        }

        public void moveTo(Vector3 position)
        {
            if (!canMove)
            {
                return;
            }
            if (position.Equals(inputTarget))
            {
                return;
            }
            inputTarget = position;
            position.x = Mathf.Clamp(inputTarget.x, playArea.xMin, playArea.xMax);
            position.y = Mathf.Clamp(inputTarget.y, playArea.yMin, playArea.yMax);
            // Send position to all players
            _photonView.RPC(nameof(MoveTowardsRpc), RpcTarget.All, position);
        }

        private void startPlaying()
        {
            Debug.Log($"startPlaying IsMine={_photonView.IsMine} initialPosition={initialPosition} owner={_photonView.Owner}");
            _transform.position = initialPosition;
            validTarget = initialPosition;
            canMove = true; // assume we can move on start but are stationary
            isMoving = false;
            var player = _photonView.Owner;
            playerPos = player.GetCustomProperty(LobbyManager.playerPositionKey, -1);
            // Rotate
            if (playerPos == 1 || playerPos == 3)
            {
                _transform.rotation = Quaternion.Euler(0f, 0f, 180f); // Upside down
                teamIndex = 1;
            }
            else
            {
                teamIndex = 0;
            }
            if (_photonView.Owner.IsLocal)
            {
                playerColor.setHighLightColor(Color.yellow);
            }
        }

        private void stopMoving()
        {
            validTarget = _transform.position;
            isMoving = false;
        }

        [PunRPC]
        private void MoveTowardsRpc(Vector3 targetPosition)
        {
            validTarget = targetPosition;
            isMoving = true;
        }
    }
}