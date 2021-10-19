﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer
{
    public class ballController : MonoBehaviour
    {
        private const float minStartDirection = 0.2f;

        [Header("Settings"), Min(float.Epsilon)] public float ballMoveSpeed;
        [SerializeField, Min(float.Epsilon)] private float lerpSmoothingFactor;
        [SerializeField] private Transform _transform;

        [Header("Live Data"), SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Vector3 initialPosition;
        // This bool is used to stop the ball from getting reset constantly (causing a freeze) whenever the ball needs to be stopped.
        public bool stop = false;

        private bool isDebugBallNoRandom;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            initialPosition = _transform.position;
            //UnityEngine.Debug.Assert(!GetComponent<SpriteRenderer>().enabled, "keep sprite hidden to avoid scene instantiation lag spike", gameObject);
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
            // Setting the stoppers to function in case this needs a restart due to it being caught, which turned the stoppers off.
            playerBallStopper[] stoppers;
            stoppers = FindObjectsOfType<playerBallStopper>();
            foreach (var stopper in stoppers)
            {
                stopper.ballCaught = false;
            }
            // Same as above, but with the shieldManager
            shieldManager[] managers;
            managers = FindObjectsOfType<shieldManager>();
            foreach (var manager in managers)
            {
                manager.ballCaught = false;
            }

            _transform.parent = null;
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
            if (targetVelocity == Vector2.zero && stop == false)
            {
                restart();
                return;
            }
            _rigidbody.velocity = Vector2.Lerp(_velocity, targetVelocity, deltaTime * lerpSmoothingFactor);
        }
    }
}
