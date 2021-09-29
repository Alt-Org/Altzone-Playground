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
        [SerializeField] private Vector3 targetPosition;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _transform = GetComponent<Transform>();
            initialPosition = _transform.position;
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

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            startPlaying();
        }

        public void moveTo(Vector3 position)
        {
            if (position.Equals(targetPosition))
            {
                return;
            }
            targetPosition = position;
            var curPos = _transform.position;
            Debug.Log($"moveTo {targetPosition} <- {curPos} delta {curPos - targetPosition}");
        }

        private void startPlaying()
        {
            Debug.Log($"startPlaying IsMine={_photonView.IsMine} initialPosition={initialPosition} owner={_photonView.Owner}");
            _transform.position = initialPosition;
            var player = _photonView.Owner;
            var playerPos = player.GetCustomProperty(LobbyManager.playerPositionKey, -1);
            // Rotate
            if (playerPos == 1 || playerPos == 3)
            {
                _transform.rotation = Quaternion.Euler(0f, 0f, 180f); // Upside down
            }
        }
    }
}