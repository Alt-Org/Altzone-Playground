using Examples.Game.Scripts.Battle.Scene;
using System;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    /// <summary>
    /// Collision manager for the ball.
    /// </summary>
    public class BallCollision : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private bool isUpper;
        [SerializeField] private bool isLower;

        private Collider2D upperTeam;
        private Collider2D lowerTeam;

        /// <summary>
        /// Sets active team index or -1 if either is decisively active.
        /// </summary>
        public Action<int> setCurrentTeam;

        /// <summary>
        /// Externalized collision handling.
        /// </summary>
        public Action<Collision2D> onCollision2D;

        private void Awake()
        {
            var sceneConfig = FindObjectOfType<SceneConfig>();
            upperTeam = sceneConfig.upperTeam;
            lowerTeam = sceneConfig.lowerTeam;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            onCollision2D?.Invoke(other);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (other.Equals(upperTeam))
            {
                isUpper = true;
                checkBallAndTeam();
            }
            else if (other.Equals(lowerTeam))
            {
                isLower = true;
                checkBallAndTeam();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.Equals(upperTeam))
            {
                isUpper = false;
                checkBallAndTeam();
            }
            else if (other.Equals(lowerTeam))
            {
                isLower = false;
                checkBallAndTeam();
            }
        }

        private void checkBallAndTeam()
        {
            if (isUpper && !isLower)
            {
                // activate upper team
                setCurrentTeam?.Invoke(1);
            }
            else if (isLower && !isUpper)
            {
                // Activate lower team
                setCurrentTeam?.Invoke(0);
            }
            else
            {
                // between teams
                setCurrentTeam?.Invoke(-1);
            }
        }
    }
}