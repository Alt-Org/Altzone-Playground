using Examples.Lobby.Scripts;
using Photon.Pun;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Simple player movement across network clients using mouse or touch.
    /// </summary>
    public class PlayerMovement : MonoBehaviourPunCallbacks
    {
        [Header("Live Data"), SerializeField] protected PhotonView _photonView;
        [SerializeField] protected Transform _transform;
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Vector3 inputTarget;
        [SerializeField] private bool isMoving;
        [SerializeField] private Vector3 validTarget;
        [SerializeField] private Rect playArea;

        private static float playerMoveSpeed => 5;

        private void Awake()
        {
            _photonView = photonView;
            _transform = GetComponent<Transform>();
            initialPosition = _transform.position;
            validTarget = initialPosition;
            Debug.Log($"Awake IsMine={_photonView.IsMine} initialPosition={initialPosition}");
        }

        public override void OnEnable()
        {
            Debug.Log($"OnEnable IsMine={_photonView.IsMine} initialPosition={initialPosition}");
            base.OnEnable();
            if (PhotonNetwork.InRoom)
            {
                startPlaying();
            }
        }

        private void Update()
        {
            if (!isMoving)
            {
                return;
            }
            var speed = playerMoveSpeed * Time.deltaTime;
            var newPosition = Vector3.MoveTowards(_transform.position, validTarget, speed);
            isMoving = newPosition != validTarget;
            _transform.position = newPosition;
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            startPlaying();
        }

        public void setPlayArea(Rect area)
        {
            Debug.Log($"setPlayArea {area}");
            playArea = area;
        }

        public void moveTo(Vector3 position)
        {
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
            var player = _photonView.Owner;
            var playerPos = player.GetCustomProperty(LobbyManager.playerPositionKey, -1);
            // Rotate
            if (playerPos == 1 || playerPos == 3)
            {
                _transform.rotation = Quaternion.Euler(0f, 0f, 180f); // Upside down
            }
        }

        [PunRPC]
        private void MoveTowardsRpc(Vector3 targetPosition)
        {
            validTarget = targetPosition;
            isMoving = true;
        }
    }
}