using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer
{
    public class ballController : MonoBehaviour
    {
        private const float minStartDirection = 0.2f;

        [Header("Settings"), SerializeField, Min(float.Epsilon)] private float ballMoveSpeed;
        [SerializeField, Min(float.Epsilon)] private float lerpSmoothingFactor;
        [SerializeField] private Transform _transform;

        [Header("Live Data"), SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Vector3 initialPosition;

        private bool isDebugBallNoRandom;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            initialPosition = _transform.position;
            UnityEngine.Debug.Assert(!GetComponent<SpriteRenderer>().enabled, "keep sprite hidden to avoid scene instantiation lag spike", gameObject);
        }

        private void OnEnable()
        {
            isDebugBallNoRandom = false;
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
            GetComponent<SpriteRenderer>().enabled = true;
            restart();
        }

        private void OnDisable()
        {
            _collider.enabled = false;
            _rigidbody.velocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            keepConstantVelocity(Time.fixedDeltaTime);
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
