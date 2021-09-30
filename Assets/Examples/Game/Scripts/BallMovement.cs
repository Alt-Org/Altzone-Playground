using DigitalRuby;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityConstants;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Simple <c>Rigidbody2D</c> ball movement across network clients using simplest lag compensation.
    /// </summary>
    /// <remarks>
    /// <c>Rigidbody2D</c> is kinematic on remote clients and we use <c>OnPhotonSerializeView</c> to transfer our position and velocity.
    /// </remarks>
    public class BallMovement : MonoBehaviourPunCallbacks, IPunObservable
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 2; // Switch active team
        private const float minStartDirection = 0.2f;

        public float speed;
        public float teleportDistance;
        public float moveDistance;
        public Collider2D upperTeam;
        public Collider2D lowerTeam;
        public Color upperColor;
        public Color lowerColor;
        public Color originalColor;
        public bool isSPawnMiniBall;
        public bool isSendActiveTeamEvent;
        public GameObject miniBallPrefab;
        public bool canMove;
        public bool isUpper;
        public bool isLower;

        [Header("Photon"), SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        [Header("Mini Ball"), SerializeField] private float nextSpawnTime;
        [SerializeField] private bool firstBall;
        [SerializeField] private float nextSpawnDelay = 0.5f;

        private Transform _transform;
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        private SpriteRenderer _sprite;
        private PhotonView _photonView;
        private Vector3 initialPosition;
        private PhotonEventDispatcher photonEventDispatcher;

        private float lerpSmoothingFactor => 4f;
        private float ballMoveSpeed => speed;

        private void Awake()
        {
            Debug.Log("Awake");
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();
            _sprite = GetComponent<SpriteRenderer>();
            originalColor = _sprite.color;
            _photonView = PhotonView.Get(this);
            initialPosition = _transform.position;
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, data =>
            {
                var teamIndex = (int) (byte) data.CustomData;
                this.Publish(new ActiveTeamEvent(teamIndex));
            });
            canMove = false;
            if (!PoolManager.ContainsPrefab(miniBallPrefab.name))
            {
                PoolManager.AddPrefab(miniBallPrefab.name, miniBallPrefab);
            }
        }

        private void OnDestroy()
        {
            PoolManager.RecycleActiveObjects();
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
            canMove = true;
            _rigidbody.isKinematic = !_photonView.IsMine;
            _collider.enabled = _photonView.IsMine;
            nextSpawnTime = float.MaxValue;
            Debug.Log($"startPlaying IsMine={_photonView.IsMine} isKinematic={_rigidbody.isKinematic} collider={_collider.enabled}");
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

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            // Set info about latest collision for mini ball spawner
            nextSpawnTime = Time.time + nextSpawnDelay;
            firstBall = true;

            var hitObject = other.gameObject;
            var hitLayer = hitObject.layer;
            if (hitLayer == Layers.Default)
            {
                // SKip default layer
                return;
            }
            var hitY = hitObject.transform.position.y;
            var positionY = Mathf.Approximately(hitY, 0f)
                ? _transform.position.y // Stupid HACK for walls because they are positioned to origo and collider is moved using its offset!
                : hitY;
            var data = new CollisionEvent(hitObject, hitLayer, Time.time, positionY);
            this.Publish(data);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            //Debug.Log($"OnTriggerEnter2D {other.gameObject.name}");
            if (other.Equals(upperTeam))
            {
                isUpper = true;
            }
            else if (other.Equals(lowerTeam))
            {
                isLower = true;
            }
            if (isSendActiveTeamEvent)
            {
                checkBallAndTeam();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            //Debug.Log($"OnTriggerExit2D {other.gameObject.name}");
            if (other.Equals(upperTeam))
            {
                isUpper = false;
            }
            else if (other.Equals(lowerTeam))
            {
                isLower = false;
            }
            if (isSendActiveTeamEvent)
            {
                checkBallAndTeam();
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
                networkPosition += _rigidbody.velocity * networkLag;
            }
        }

        private void Update()
        {
            if (!canMove)
            {
                return;
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
                return;
            }
            if (isSPawnMiniBall)
            {
                updateForMiniBall();
            }
        }

        private void updateForMiniBall()
        {
            // Manage ball spawning defence points here!
            if (Time.time > nextSpawnTime)
            {
                nextSpawnTime = Time.time + nextSpawnDelay;
                var miniBall = PoolManager.CreateFromCache(miniBallPrefab.name);
                miniBall.transform.position = _transform.position;
                var _renderer = miniBall.GetComponent<SpriteRenderer>();
                _renderer.color = firstBall ? Color.yellow : Color.white;
                if (firstBall)
                {
                    firstBall = false;
                }
                var timer = miniBall.GetOrAddComponent<TimedReturnToPool>();
                timer.TimeToLive = 5;
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

        private void checkBallAndTeam()
        {
            if (isUpper && !isLower)
            {
                if (_sprite.color != upperColor)
                {
                    photonEventDispatcher.RaiseEvent(photonEventCode, (byte) 0);
                }
                _sprite.color = upperColor;
            }
            else if (isLower && !isUpper)
            {
                if (_sprite.color != upperColor)
                {
                    photonEventDispatcher.RaiseEvent(photonEventCode, (byte) 1);
                }
                _sprite.color = lowerColor;
            }
            else
            {
                _sprite.color = originalColor;
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

        public class CollisionEvent
        {
            public readonly GameObject hitObject;
            public readonly int layer;
            public readonly float collisionTime;
            public readonly float positionY;

            public CollisionEvent(GameObject hitObject, int layer, float collisionTime, float positionY)
            {
                this.hitObject = hitObject;
                this.layer = layer;
                this.collisionTime = collisionTime;
                this.positionY = positionY;
            }

            public override string ToString()
            {
                return $"CollisionEvent: {hitObject.name}, layer: {layer}, time: {collisionTime}, y: {positionY}";
            }
        }

        public class ActiveTeamEvent
        {
            public readonly int teamIndex;

            public ActiveTeamEvent(int teamIndex)
            {
                this.teamIndex = teamIndex;
            }

            public override string ToString()
            {
                return $"{nameof(teamIndex)}: {teamIndex}";
            }
        }
    }
}