using Photon.Pun;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lobby.Scripts.Game
{
    public class BallMovementV1 : MonoBehaviourPunCallbacks
    {
        private const float minStartDirection = 0.2f;

        public float speed;
        public Collider2D upperTeam;
        public Collider2D lowerTeam;
        public Color upperColor;
        public Color lowerColor;
        public Color originalColor;
        public bool canMove;
        public bool isUpper;
        public bool isLower;

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
            Debug.Log($"OnTriggerEnter2D {other.gameObject.name}");
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
            Debug.Log($"OnTriggerExit2D {other.gameObject.name}");
            if (other.Equals(upperTeam))
            {
                isUpper = false;
            }
            else if (other.Equals(lowerTeam))
            {
                isLower = false;
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
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine)
            {
                return;
            }
            if (!canMove)
            {
                return;
            }
            keepConstantVelocity(Time.fixedDeltaTime);
        }

        private void restart()
        {
            if (!photonView.IsMine)
            {
                return;
            }
            if (!canMove)
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