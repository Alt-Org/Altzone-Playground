using Examples.Game.Scripts.Battle.Scene;
using System;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    /// <summary>
    /// Interface to provide data about ball collisions.
    /// </summary>
    public interface IBallCollisionSource
    {
        /// <summary>
        /// Sets active team index or -1 if either team is decisively active.
        /// </summary>
        Action<int> onCurrentTeamChanged { get; set; }

        /// <summary>
        /// Externalized collision handling.
        /// </summary>
        Action<Collision2D> onCollision2D { get; set; }
    }

    /// <summary>
    /// Collision manager for the ball.
    /// </summary>
    public class BallCollision : MonoBehaviour, IBallCollisionSource
    {
        [Header("Live Data"), SerializeField] private bool isUpper;
        [SerializeField] private bool isLower;

        private Collider2D upperTeam;
        private Collider2D lowerTeam;

        Action<int> IBallCollisionSource.onCurrentTeamChanged { get; set; }

        Action<Collision2D> IBallCollisionSource.onCollision2D { get; set; }

        private void Awake()
        {
            var sceneConfig = FindObjectOfType<SceneConfig>();
            upperTeam = sceneConfig.upperTeamCollider;
            lowerTeam = sceneConfig.lowerTeamCollider;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            ((IBallCollisionSource)this).onCollision2D?.Invoke(other);
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
                ((IBallCollisionSource)this).onCurrentTeamChanged?.Invoke(1);
            }
            else if (isLower && !isUpper)
            {
                // Activate lower team
                ((IBallCollisionSource)this).onCurrentTeamChanged?.Invoke(0);
            }
            else
            {
                // between teams
                ((IBallCollisionSource)this).onCurrentTeamChanged?.Invoke(-1);
            }
        }
    }
}