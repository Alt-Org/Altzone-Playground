using Photon.Pun;
using UnityEngine;

namespace Lobby.Scripts.Game
{
    public class BallMovementV1 : MonoBehaviourPunCallbacks, IPunObservable
    {
        private const float minStartDirection = 0.2f;

        public float speed;
        public float teleportDistance;
        public float moveDistance;
        public Collider2D upperTeam;
        public Collider2D lowerTeam;
        public Color upperColor;
        public Color lowerColor;
        public Color originalColor;
        public bool canMove;
        public bool isUpper;
        public bool isLower;

        [Header("Photon")] [SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        private Transform _transform;
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _sprite;
        private PhotonView _photonView;
        private Vector3 initialPosition;

        private float lerpSmoothingFactor => 4f;
        private float ballMoveSpeed => speed;

        private void Awake()
        {
            Debug.Log("Awake");
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            originalColor = _sprite.color;
            _photonView = PhotonView.Get(this);
            initialPosition = _transform.position;
            canMove = false;
        }

        public override void OnEnable()
        {
            Debug.Log($"OnEnable IsMine={_photonView.IsMine}");
            base.OnEnable();
            restart();
            if (PhotonNetwork.InRoom)
            {
                startPlaying();
            }
        }

        private void startPlaying()
        {
            Debug.Log("*");
            Debug.Log($"startPlaying IsMine={_photonView.IsMine}");
            Debug.Log("*");
            canMove = true;
            _rigidbody.isKinematic = !photonView.IsMine;
        }

        public override void OnDisable()
        {
            Debug.Log("OnDisable");
            base.OnDisable();
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            startPlaying();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //Debug.Log($"OnTriggerEnter2D {other.gameObject.name}");
            if (other.Equals(upperTeam))
            {
                isUpper = true;
            }
            else if (other.Equals(lowerTeam))
            {
                isLower = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            //Debug.Log($"OnTriggerExit2D {other.gameObject.name}");
            if (other.Equals(upperTeam))
            {
                isUpper = false;
            }
            else if (other.Equals(lowerTeam))
            {
                isLower = false;
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // https://doc.photonengine.com/en-us/pun/v2/gameplay/lagcompensation
            // Better algo might be:
            // https://sharpcoderblog.com/blog/pun-2-lag-compensation
            // See also:
            // https://forum.unity.com/threads/photon-pun2-non-rigidbody-lag-compensation-and-jittery-cars.912779/
            if (stream.IsWriting)
            {
                stream.SendNext(_rigidbody.position);
                stream.SendNext(_rigidbody.velocity);
            }
            else
            {
                networkPosition = (Vector2) stream.ReceiveNext();
                _rigidbody.velocity = (Vector2) stream.ReceiveNext();

                networkLag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
                networkPosition += (_rigidbody.velocity * networkLag);
            }
        }

        private void Update()
        {
            if (isUpper && !isLower)
            {
                _sprite.color = upperColor;
            }
            else if (isLower && !isUpper)
            {
                _sprite.color = lowerColor;
            }
            else
            {
                _sprite.color = originalColor;
            }
            if (!photonView.IsMine)
            {
                var curPos = _rigidbody.position;
                var deltaX = Mathf.Abs(curPos.x - networkPosition.x);
                var deltaY = Mathf.Abs(curPos.y - networkPosition.y);
                if (deltaX > teleportDistance || deltaY > teleportDistance)
                {
                    _rigidbody.position = networkPosition;
                }
                else if (deltaX > moveDistance || deltaY > moveDistance)
                {
                    _rigidbody.position = Vector2.MoveTowards(curPos, networkPosition, Time.deltaTime);
                }
            }
        }

        private void FixedUpdate()
        {
            if (!canMove)
            {
                return;
            }
            keepConstantVelocity(Time.fixedDeltaTime);
        }

        private void restart()
        {
            if (!canMove)
            {
                return;
            }
            if (!photonView.IsMine)
            {
                return;
            }
            _transform.position = initialPosition;
            _rigidbody.velocity = getStartDirection() * ballMoveSpeed;
        }

        private void keepConstantVelocity(float deltaTime)
        {
            var _velocity = _rigidbody.velocity;
            var targetVelocity = _velocity.normalized * ballMoveSpeed;
            if (targetVelocity == Vector2.zero)
            {
                restart();
                return;
            }
            _rigidbody.velocity = Vector2.Lerp(_velocity, targetVelocity, deltaTime * lerpSmoothingFactor);
        }

        private static Vector2 getStartDirection()
        {
            for (;;)
            {
                var direction = Random.insideUnitCircle.normalized;
                if (Mathf.Abs(direction.x) > minStartDirection && Mathf.Abs(direction.y) > minStartDirection)
                {
                    return direction;
                }
            }
        }
    }
}