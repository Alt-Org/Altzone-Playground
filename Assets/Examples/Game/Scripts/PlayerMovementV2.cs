using Examples.Lobby.Scripts;
using Photon.Pun;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Simple player movement across network clients using mouse or touch.
    /// </summary>
    public class PlayerMovementV2 : MonoBehaviourPunCallbacks
    {
        [Header("Live Data"), SerializeField]  protected PhotonView _photonView;
        [SerializeField] protected Transform _transform;
        [SerializeField] private Vector3 initialPosition;

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

        private void startPlaying()
        {
            Debug.Log($"startPlaying IsMine={_photonView.IsMine} initialPosition={initialPosition}");
            _transform.position = initialPosition;
            var player = PhotonNetwork.LocalPlayer;
            var localPlayerIndex = player.GetCustomProperty(LobbyManager.playerPositionKey, -1);
            // Rotate
            if (localPlayerIndex == 1 || localPlayerIndex == 3)
            {
                _transform.rotation = Quaternion.Euler(0f, 0f, 180f); // Upside down
            }
        }
    }
}