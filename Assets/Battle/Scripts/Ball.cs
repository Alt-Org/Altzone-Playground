using Game.Config;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Unity;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    [RequireComponent(typeof(SpriteRenderer),
        typeof(Rigidbody2D),
        typeof(CircleCollider2D))]
    public class Ball : MonoBehaviourPunCallbacks, IPunObservable
    {
        private const float minStartDirection = 0.2f;

        [Header("Settings"), SerializeField, TagSelector]
        private string ballAreaTagName;

        [SerializeField, Min(float.Epsilon)] private float ballMoveSpeed;
        [SerializeField, Min(float.Epsilon)] private float lerpSmoothingFactor;
        [SerializeField] private Transform _transform;

        [Header("Live Data"), SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Vector3 initialPosition;

        [Header("Photon")] [SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        [Header("Shadow")] [SerializeField] private Transform shadowPosition;
        [SerializeField] private bool hasShadowPosition;

        private bool isDebugBallNoRandom;
        private bool isJoined;

        private void Awake()
        {
            Debug.Log($"Awake photonView={photonView}");
            photonView.ObservedComponents.Add(this); // In base class!
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            initialPosition = _transform.position;
            UnityEngine.Debug.Assert(!GetComponent<SpriteRenderer>().enabled, "keep sprite hidden to avoid scene instantiation lag spike", gameObject);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            isDebugBallNoRandom = false;
            //GameConfig.withDebug((dc) => isDebugBallNoRandom = dc.isDebugBallNoRandom);

            if (PhotonNetwork.OfflineMode)
            {
                // Currently script execution order is so that Ball is activated after room is created and joined :-(
                isJoined = true;
                _rigidbody.isKinematic = false;
                _collider.enabled = true;
                GetComponent<SpriteRenderer>().enabled = true;
                restart();
            }
            else
            {
                // Wait until we join room to start moving around
                _rigidbody.isKinematic = true;
                _collider.enabled = false;
            }
        }

        private void setup()
        {
            _rigidbody.isKinematic = !photonView.IsMine;
            _collider.enabled = photonView.IsMine;
            if (photonView.IsMine)
            {
                restart();
            }
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"OnJoinedRoom isJoined={isJoined}");
            if (isJoined)
            {
                return; // OfflineMode safeguard
            }
            isJoined = true;
            GetComponent<SpriteRenderer>().enabled = true;
            setup();
     }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log($"OnMasterClientSwitched newMasterClient={newMasterClient.GetDebugLabel()}");
            setup();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _collider.enabled = false;
            _rigidbody.velocity = Vector2.zero;
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
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

        private void FixedUpdate()
        {
            if (hasShadowPosition)
            {
                shadowPosition.position = _transform.position;
            }
            if (!isJoined)
            {
                // No move until game is in room!
                return;
            }
            if (photonView.IsMine)
            {
                keepConstantVelocity(Time.fixedDeltaTime);
            }
            else
            {
                // Teleporting a Rigidbody from one position to another uses Rigidbody.position
                var position = _rigidbody.position;
                if (Mathf.Abs(position.x - networkPosition.x) > 2f || Mathf.Abs(position.y - networkPosition.y) > 2f)
                {
                    _rigidbody.position = networkPosition;
                }
                else
                {
                    _rigidbody.position = Vector2.MoveTowards(position, networkPosition, Time.fixedDeltaTime);
                }
            }
        }

        private void restart()
        {
            _transform.position = initialPosition;
            var randomDir = isDebugBallNoRandom ? Vector2.left : getStartDirection();
            _rigidbody.velocity = randomDir * ballMoveSpeed;
            Debug.Log($"restart {_transform.position} velocity={_rigidbody.velocity}");
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
    }
}