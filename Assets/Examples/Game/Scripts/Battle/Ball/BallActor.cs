using Examples.Config.Scripts;
using Photon.Pun;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    /// <summary>
    /// Interface for external ball control, for example for <c>Game Manager</c> to use.
    /// </summary>
    public interface IBallControl
    {
        void teleportBall(Vector2 position);
        void moveBall(Vector2 direction, float speed);
        void showBall();
        void hideBall();
    }

    /// <summary>
    /// Simple ball with <c>Rigidbody2D</c> that synchronizes its movement across network using <c>PhotonView</c>.
    /// </summary>
    public class BallActor : MonoBehaviour, IPunObservable, IBallControl
    {
        [Header("Live Data"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private int curTeamIndex;
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

            curTeamIndex = -1;
            targetSpeed = 0;
            ballCollision = gameObject.AddComponent<BallCollision>();
            ballCollision.enabled = false;
            ((IBallCollisionSource)ballCollision).onCurrentTeamChanged = newTeamIndex =>
            {
                Debug.Log($"setCurrentTeam ({curTeamIndex}) <- ({newTeamIndex})");
                curTeamIndex = newTeamIndex;
            };
            ((IBallCollisionSource)ballCollision).onCollision2D = onBallCollision;

            enabled = false; // Wait until game starts
        }

        private void onBallCollision(Collision2D other)
        {
            Debug.Log($"onBallCollision team={curTeamIndex} other={other.gameObject.name}");
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            ballCollision.enabled = true;
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
            ballCollision.enabled = false;
            ((IBallControl)this).hideBall();
        }

        void IBallControl.teleportBall(Vector2 position)
        {
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
            if (!enabled)
            {
                throw new UnityException("ball must be enabled before show");
            }
            _sprite.enabled = true;
            _collider.enabled = true;
            Debug.Log($"showBall position={_rigidbody.position}");
        }

        void IBallControl.hideBall()
        {
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
                randomReset(curTeamIndex);
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
    }
}