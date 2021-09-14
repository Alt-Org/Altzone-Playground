using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using System.Linq;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class PlayerMoves : MonoBehaviour, IPunObservable
    {
        private readonly Vector2 moveOffset = Vector2.zero; // This is for player positioning testing only and should be removed later!

        [Header("Settings"), SerializeField, Min(float.Epsilon)] private float playerMoveSpeed;
        [SerializeField, Range(0, 3)] private int playerPosIndex;

        [Header("Play Area"), SerializeField] private Vector2 areaMin;
        [SerializeField] private Vector2 areaMax;

        [Header("Live Data"), SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;
        [SerializeField] protected PhotonView photonView;
        [SerializeField] private Vector3 mousePosition;
        [SerializeField] private float playerPositionZ;
        [SerializeField] private Korostaja playerHighlighter;

        [Header("Photon")] [SerializeField] private Vector3 networkPosition;
        [SerializeField] private float networkLag;

        [Header("Shadow")] [SerializeField] private Transform shadowPosition;
        [SerializeField] private bool hasShadowPosition;

        private bool isMouseDown;
        private bool hasPhotonView;

        private float xMin => areaMin.x + moveOffset.x;
        private float xMax => areaMax.x - moveOffset.x;
        private float yMin => areaMin.y + moveOffset.y;
        private float yMax => areaMax.y - moveOffset.y;

        private bool isInitialized;

        private void Awake()
        {
            Debug.Log($"Awake playerPosIndex={playerPosIndex} enabled={enabled}");
            // Grab photonView immediately as we need it when we want to take ownership
            photonView = PhotonView.Get(this);
            hasPhotonView = photonView != null;
            if (hasPhotonView)
            {
                photonView.ObservedComponents.Add(this);
            }
        }

        private void OnEnable()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                _camera = Camera.main;
                _transform = GetComponent<Transform>();
                mousePosition = _transform.position;
                playerPositionZ = mousePosition.z;
                networkPosition = mousePosition;
                playerHighlighter = GetComponentsInChildren<Korostaja>(includeInactive: true).FirstOrDefault();
                this.Subscribe<InputManager.ClickDownEvent>(onClickDownEvent);
                this.Subscribe<InputManager.ClickUpEvent>(onClickUpEvent);
            }
            if (playerHighlighter != null)
            {
                playerHighlighter.Korotus(true);
            }
            if (hasPhotonView)
            {
                var player = PhotonNetwork.LocalPlayer;
                photonView.TransferOwnership(player);
            }
        }

        private void OnDisable()
        {
            if (playerHighlighter != null)
            {
                playerHighlighter.Korotus(false);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe always as photonView can be destroyed before us
            this.Unsubscribe<InputManager.ClickDownEvent>(onClickDownEvent);
            this.Unsubscribe<InputManager.ClickUpEvent>(onClickUpEvent);
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(mousePosition);
            }
            else
            {
                networkPosition = (Vector3) stream.ReceiveNext();
                networkLag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            }
        }

        private void FixedUpdate()
        {
            if (hasShadowPosition)
            {
                shadowPosition.position = _transform.position;
            }
            if (!hasPhotonView || photonView.IsMine)
            {
                if (isMouseDown)
                {
                    movePlayer();
                }
            }
            else
            {
                _transform.position = Vector3.MoveTowards(_transform.position, networkPosition, Time.fixedDeltaTime + networkLag);
            }
        }

        public void onPelipaikkaVarattu(int pelipaikka, int tila)
        {
            if (pelipaikka != playerPosIndex)
            {
                return; // not us
            }
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"onPelipaikkaVarattu pelipaikka={pelipaikka} tila={tila} {player.GetDebugLabel()}");
            var isEnabled = tila == 1;
            this.enabled = isEnabled;
        }

        private void movePlayer()
        {
            var playerPosition = _transform.position;
            var speed = playerMoveSpeed * Time.deltaTime;
            _transform.position = Vector3.MoveTowards(playerPosition, mousePosition, speed);
        }

        private void onClickDownEvent(InputManager.ClickDownEvent data)
        {
            isMouseDown = true;
            mousePosition = _camera.ScreenToWorldPoint(data.ScreenPosition);
            mousePosition.x = Mathf.Clamp(mousePosition.x, xMin, xMax);
            mousePosition.y = Mathf.Clamp(mousePosition.y, yMin, yMax);
            mousePosition.z = playerPositionZ;
        }

        private void onClickUpEvent(InputManager.ClickUpEvent data)
        {
            isMouseDown = false;
            mousePosition = _transform.position;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (!isInitialized || !enabled)
            {
                return;
            }
            UnityEngine.Debug.DrawLine(_transform.position, mousePosition, Color.magenta, 0.1f);
        }
    }
}