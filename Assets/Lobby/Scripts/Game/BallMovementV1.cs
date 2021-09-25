using Photon.Pun;
using UnityEngine;

namespace Lobby.Scripts.Game
{
    public class BallMovementV1 : MonoBehaviour
    {
        private const float minStartDirection = 0.2f;

        public float speed;

        private Transform _transform;
        private Rigidbody2D _rigidbody;
        private PhotonView photonView;
        private Vector3 initialPosition;

        private float lerpSmoothingFactor => 4f;
        private float ballMoveSpeed => speed;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody2D>();
            photonView = PhotonView.Get(this);
            initialPosition = _transform.position;
        }

        private void OnEnable()
        {
            restart();
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine)
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