using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Player;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    /// <summary>
    /// Interface for external ball control, for example for <c>Game Manager</c> to use.
    /// </summary>
    public interface IBallControl
    {
        int currentTeamIndex { get; }
        void teleportBall(Vector2 position, int teamIndex); // We need to know onto which team side we are landing!
        void moveBall(Vector2 direction, float speed);
        void showBall();
        void hideBall();
    }

    /// <summary>
    /// Simple ball with <c>Rigidbody2D</c> that synchronizes its movement across network using <c>PhotonView</c> and <c>RPC</c>.
    /// </summary>
    public class BallActor : MonoBehaviour, IPunObservable, IBallControl
    {
        [Header("Settings"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Collider2D _collider;

        [SerializeField] private LayerMask collisionToHeadMask;
        [SerializeField] private int collisionToHead;
        [SerializeField] private LayerMask collisionToWallMask;
        [SerializeField] private int collisionToWall;
        [SerializeField] private LayerMask collisionToBrickMask;
        [SerializeField] private int collisionToBrick;

        [Header("Live Data"), SerializeField] private int _curTeamIndex;
        [SerializeField] private float targetSpeed;
        [SerializeField] private BallCollision ballCollision;

        [Header("Photon"), SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        private Rigidbody2D _rigidbody;

        private PhotonView _photonView;
        //--private Vector2 curVelocity;

        // Configurable settings
        private GameVariables variables;

        private void Awake()
        {
            Debug.Log("Awake");
            variables = RuntimeGameConfig.Get().variables;
            _rigidbody = GetComponent<Rigidbody2D>();
            _photonView = PhotonView.Get(this);
            _rigidbody.isKinematic = !_photonView.IsMine;
            _collider.enabled = _photonView.IsMine;

            collisionToHead = collisionToHeadMask.value;
            collisionToWall = collisionToWallMask.value;
            collisionToBrick = collisionToBrickMask.value;

            _curTeamIndex = -1;
            targetSpeed = 0;
            ballCollision = gameObject.AddComponent<BallCollision>();
            ballCollision.enabled = false;
            ((IBallCollisionSource)ballCollision).onCurrentTeamChanged = onCurrentTeamChanged;
            ((IBallCollisionSource)ballCollision).onCollision2D = onBallCollision;
        }

        private void onCurrentTeamChanged(int newTeamIndex)
        {
            var oldTemIndex = _curTeamIndex;
            _curTeamIndex = newTeamIndex;
            Debug.Log($"onCurrentTeamChanged ({oldTemIndex}) <- ({newTeamIndex})");
            this.Publish(new ActiveTeamEvent(oldTemIndex, newTeamIndex));
        }

        private void onBallCollision(Collision2D other)
        {
            var otherGameObject = other.gameObject;
            var colliderMask = 1 << otherGameObject.layer;
            if (collisionToBrick == (collisionToBrick | colliderMask))
            {
                return;
            }
            if (collisionToHead == (collisionToHead | colliderMask))
            {
                // Contract: player is one level up from head collider
                var playerActor = otherGameObject.GetComponentInParent<PlayerActor>() as IPlayerActor;
                playerActor.headCollision();
                return;
            }
            if (collisionToWall == (collisionToWall | colliderMask))
            {
                return;
            }
            Debug.Log($"onBallCollision UNHANDLED team={_curTeamIndex} other={other.gameObject.name}");
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            ((IBallControl)this).showBall();
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
            if (PhotonNetwork.InRoom)
            {
                ((IBallControl)this).hideBall();
            }
        }

        int IBallControl.currentTeamIndex => _curTeamIndex;

        void IBallControl.teleportBall(Vector2 position, int teamIndex)
        {
            onCurrentTeamChanged(teamIndex);
            _rigidbody.position = position;
            Debug.Log($"teleportBall position={_rigidbody.position}");
        }

        void IBallControl.moveBall(Vector2 direction, float speed)
        {
            targetSpeed = speed;
            _rigidbody.velocity = direction.normalized * speed;
            //--curVelocity = _rigidbody.velocity;
            Debug.Log($"moveBall position={_rigidbody.position} velocity={_rigidbody.velocity} speed={targetSpeed}");
        }

        void IBallControl.showBall()
        {
            _photonView.RPC(nameof(setBallVisibilityRpc), RpcTarget.All, true);
        }

        void IBallControl.hideBall()
        {
            _photonView.RPC(nameof(setBallVisibilityRpc), RpcTarget.All, false);
        }

        private void _showBall()
        {
            ballCollision.enabled = true;
            _sprite.enabled = true;
            _collider.enabled = true;
            Debug.Log($"showBall position={_rigidbody.position}");
        }

        private void _hideBall()
        {
            ballCollision.enabled = false;
            _sprite.enabled = false;
            _collider.enabled = false;
            targetSpeed = 0;
            _rigidbody.velocity = Vector2.zero;
            //--curVelocity = _rigidbody.velocity;
            Debug.Log($"hideBall position={_rigidbody.position}");
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
                networkPosition = (Vector2)stream.ReceiveNext();
                _rigidbody.velocity = (Vector2)stream.ReceiveNext();
                //--curVelocity = _rigidbody.velocity;

                networkLag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                networkPosition += _rigidbody.velocity * networkLag;
            }
        }

        private void Update()
        {
            if (!_photonView.IsMine)
            {
                var curPos = _rigidbody.position;
                var deltaX = Mathf.Abs(curPos.x - networkPosition.x);
                var deltaY = Mathf.Abs(curPos.y - networkPosition.y);
                if (deltaX > variables.ballTeleportDistance || deltaY > variables.ballTeleportDistance)
                {
                    _rigidbody.position = networkPosition;
                }
                else
                {
                    _rigidbody.position = Vector2.MoveTowards(curPos, networkPosition, Time.deltaTime);
                }
            }
        }

        private void FixedUpdate()
        {
            if (!_photonView.IsMine)
            {
                return;
            }
            if (targetSpeed > 0)
            {
                //--if (curVelocity != _rigidbody.velocity)
                //--{
                //--    Debug.Log($"__> curVelocity {curVelocity} != {_rigidbody.velocity} _rigidbody.velocity");
                //--    curVelocity = _rigidbody.velocity;
                //--}
                keepConstantVelocity(Time.fixedDeltaTime);
            }
        }

        private void keepConstantVelocity(float deltaTime)
        {
            var _velocity = _rigidbody.velocity;
            var targetVelocity = _velocity.normalized * targetSpeed;
            if (targetVelocity == Vector2.zero)
            {
                randomReset(_curTeamIndex);
                return;
            }
            if (targetVelocity != _rigidbody.velocity)
            {
                //--Debug.Log($"keepConstantVelocity position={_rigidbody.position} velocity={targetVelocity}");
                _rigidbody.velocity = Vector2.Lerp(_velocity, targetVelocity, deltaTime * variables.ballLerpSmoothingFactor);
                //--curVelocity = _rigidbody.velocity;
            }
        }

        private void randomReset(int forTeam)
        {
            transform.position = Vector3.zero;
            var direction = forTeam == 0 ? Vector2.up : Vector2.down;
            _rigidbody.velocity = direction * targetSpeed;
            //--curVelocity = _rigidbody.velocity;
            //--Debug.Log($"randomReset position={_rigidbody.position} velocity={_rigidbody.velocity} speed={targetSpeed}");
        }

        [PunRPC]
        private void setBallVisibilityRpc(bool isVisible)
        {
            if (isVisible)
            {
                _showBall();
            }
            else
            {
                _hideBall();
            }
        }

        public class ActiveTeamEvent
        {
            public readonly int oldTeamIndex;
            public readonly int newTeamIndex;

            public ActiveTeamEvent(int oldTeamIndex, int newTeamIndex)
            {
                this.oldTeamIndex = oldTeamIndex;
                this.newTeamIndex = newTeamIndex;
            }

            public override string ToString()
            {
                return $"old: {oldTeamIndex}, new: {newTeamIndex}";
            }
        }
    }
}