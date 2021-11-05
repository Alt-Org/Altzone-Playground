using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer
{
    public class ballController : MonoBehaviour
    {

        #region Private Serializable Fields 

        /// <summary>
        /// The speed at which the ball moves.
        /// </summary>
        [Header("Settings"), Min(float.Epsilon)] public float ballMoveSpeed;
        /// <summary>
        /// A smoothing factor for the balls movement, I don't actually quite know how this comes into play.
        /// </summary>
        [SerializeField, Min(float.Epsilon)] private float lerpSmoothingFactor;
        /// <summary>
        /// Live data begins here. Rigidbody of the ball.
        /// </summary>
        [Header("Live Data"), SerializeField] private Rigidbody2D _rigidbody;
        /// <summary>
        /// Transform of the ball.
        /// </summary>
        [SerializeField] private Transform _transform;
        /// <summary>
        /// Collider of the ball.
        /// </summary>
        [SerializeField] private Collider2D _collider;
        /// <summary>
        /// Initial position of the ball.
        /// </summary>
        [SerializeField] private Vector3 initialPosition;

        #endregion

        #region Private Fields

        /// <summary>
        /// A value that when applied to X and Y directions of the ball when it is sent moving, makes sure it is angled enough. (Not full vertical or horizontal)
        /// </summary>
        private const float minStartDirection = 0.2f;
        /// <summary>
        /// A value that when true, makes the ball go in one specific direction, for Debug use.
        /// </summary>
        private bool isDebugBallNoRandom;

        #endregion

        #region Public Fields

        /// <summary>
        /// This bool is used to stop the ball from getting reset constantly (causing a freeze) whenever the ball needs to be stopped.
        /// </summary>
        public bool stop = false;

        #endregion

        #region Monobehaviour Callbacks

        /// <summary>
        /// Called whenever the ball first awakes.
        /// </summary>
        private void Awake()
        {
            // Getting the transform, rigidbody, collider, and initial position live data when this awakes.
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            initialPosition = _transform.position;
            //UnityEngine.Debug.Assert(!GetComponent<SpriteRenderer>().enabled, "keep sprite hidden to avoid scene instantiation lag spike", gameObject);
        }

        /// <summary>
        /// Called whenever the ball is enabled.
        /// </summary>
        private void OnEnable()
        {
            // Setting the debug to false (changed whenever debugging).
            isDebugBallNoRandom = false;
            // rigidbody is not kinematic.
            _rigidbody.isKinematic = false;
            // Enabling the collider.
            _collider.enabled = true;
            // Enabling the Sprite renderer.
            GetComponent<SpriteRenderer>().enabled = true;
            // Running Restart to get the ball going.
            restart();
        }

        /// <summary>
        /// Called whenever the ball is disabled
        /// </summary>
        private void OnDisable()
        {
            // Disabling the collider.
            _collider.enabled = false;
            // Zeroing the velocity.
            _rigidbody.velocity = Vector2.zero;
        }

        /// <summary>
        /// Called on a fixed interval, bye default every 0.02 seconds or 50 times a second.
        /// </summary>
        private void FixedUpdate()
        {
            keepConstantVelocity(Time.fixedDeltaTime);
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Resets the ball to its default position and values and sends it going.
        /// </summary>
        private void restart()
        {
            // Setting the stoppers to function in case this needs a restart due to it being caught, which turned the stoppers off.
            playerBallStopper[] stoppers;
            stoppers = FindObjectsOfType<playerBallStopper>();
            foreach (var stopper in stoppers)
            {
                stopper.ballCaught = false;
            }
            // Settiong the shieldManagers to function in case this needs a restart due to it being caught, which turned the managers off.
            shieldManager[] managers;
            managers = FindObjectsOfType<shieldManager>();
            foreach (var manager in managers)
            {
                manager.ballCaught = false;
            }

            // Setting the parent to null as it needs to move free of any other object.
            _transform.parent = null;
            // Setting position to initial position, expected to be in the middle of the field.
            _transform.position = initialPosition;
            // Getting a random direction, except if isDebugBallNoRandom is true, then we just go left.
            var randomDir = isDebugBallNoRandom ? Vector2.left : getStartDirection();
            // Setting the rigidbodys velocity to be the random direction times ball move speed, so it goes at the speed we want.
            _rigidbody.velocity = randomDir * ballMoveSpeed;
            // Logging the position and velocity after restart.
            Debug.Log($"restart {_transform.position} velocity={_rigidbody.velocity}");
        }

        /// <summary>
        /// Randomizes a normalized direction that's constrained by minStartDirection to give it enough angle.
        /// </summary>
        private static Vector2 getStartDirection()
        {
            // infinite loop.
            for (;;)
            {
                // Getting a random normalized direction.
                var direction = Random.insideUnitCircle.normalized;
                // Checking if the new direction is angled enough as dictated by minStartDirection
                if (Mathf.Abs(direction.x) > minStartDirection && Mathf.Abs(direction.y) > minStartDirection)
                {   
                    // Returning the direction if the previous check is true.
                    return direction;
                }
            }
        }

        /// <summary>
        /// Makes sure that the ball keeps its velocity.
        /// </summary>
        private void keepConstantVelocity(float deltaTime)
        {
            // Getting the current velocity.
            var _velocity = _rigidbody.velocity;
            // Setting a target velocity we should hit.
            var targetVelocity = _velocity.normalized * ballMoveSpeed;
            // if the ball has stopped and it shouldn't be, restart.
            if (targetVelocity == Vector2.zero && stop == false)
            {
                restart();
                return;
            }
            // Setting the velocity with some math that makes it gradually speed up to the target speed.
            _rigidbody.velocity = Vector2.Lerp(_velocity, targetVelocity, deltaTime * lerpSmoothingFactor);
        }
        #endregion
    }
}
