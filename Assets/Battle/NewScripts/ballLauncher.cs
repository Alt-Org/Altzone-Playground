using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer 
{
    public class ballLauncher : MonoBehaviour
    {
        // The role of this script is the following:
        // When colliding with a player head, to follow the player head.
        // After a 3 second timer, launch away in a direction determined by the orientation of two players.

        #region Serializable Private Fields

        // The controller this ball has.
        [Header("Ball Settings"), SerializeField] private ballController controller;

        // A pair of variables to store the player holding this, as well as that players team mate, as transforms for calculating direction to go.
        [Header("Live Things"), SerializeField] private Transform holdplayer;
        [SerializeField]private Transform teammate;
        // A timer for how long a player should hold.
        [SerializeField]private float carryTimer;
        // Direction that ultimately points away from the team mate.
        [SerializeField] private Vector2 dir;
        // Distance between team mates.
        [SerializeField] private float dist;
        // Original speed of the ball.
        [SerializeField] private float orgSpeed;
        // An internal boolean for making sure that the ball is launched only once on relase.

        #endregion

        #region Private Field

        // A simple bool meant to indicate if we are in the state to launch the ball.
        private bool launchState;

        #endregion

        #region Public Methods

        /// <summary>
        /// Method that's called by the ballSlave and handed what the ball had collided with, which is expected to be a player.
        /// </summary>
        public void collided(GameObject other) {
            // Making sure we only do something when we hit a player head.
            if(other.transform.CompareTag("PlayerHead"))
            {
                // Setting the parent player of the head we hit as the holding player.
                holdplayer = other.transform.parent;

                // Setting the team mate through tag comparison.
                if(holdplayer.CompareTag("Pelaaja-1"))
                {
                    teammate = GameObject.FindWithTag("Pelaaja-3").transform;
                }
                else if(holdplayer.CompareTag("Pelaaja-2"))
                {
                    teammate = GameObject.FindWithTag("Pelaaja-4").transform;
                }
                else if(holdplayer.CompareTag("Pelaaja-3"))
                {
                    teammate = GameObject.FindWithTag("Pelaaja-1").transform;
                }
                else if(holdplayer.CompareTag("Pelaaja-4"))
                {
                    teammate = GameObject.FindWithTag("Pelaaja-2").transform;
                }

                // Setting 'targetTime' on both players to 0 to allow them to move.
                var player1Manager = holdplayer.GetComponent<playerManager>();
                player1Manager.playerStop(0);
                var player2Manager = teammate.GetComponent<playerManager>();
                player2Manager.playerStop(0);

                // Set the carry timer to 3 seconds.
                carryTimer = 3f;

                // Setting the ball to be the child of the player for carrying purposes.
                this.transform.SetParent(holdplayer);

                // Zero out the players movement and set him on a specific spot.
                this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                this.GetComponent<Rigidbody2D>().angularVelocity = 0.0f;

                // Turning off the playerBallStoppers so players aren't stopped when carrying the ball.
                playerBallStopper[] stoppers;
                stoppers = FindObjectsOfType<playerBallStopper>();
                foreach (var stopper in stoppers)
                {
                    stopper.ballCaught = true;
                }

                // Turning off the shieldManagers so players shields aren't damaged when carrying the ball.
                shieldManager[] managers;
                managers = FindObjectsOfType<shieldManager>();
                foreach (var manager in managers)
                {
                    manager.ballCaught = true;
                }

                // Setting launchState boolean to true as we're about to launch the ball.
                launchState = true;
            }
        }

        #endregion

        #region Monobehaviour Callbacks


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            // Getting the org speed from the ball controller.
            orgSpeed = controller.ballMoveSpeed;
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity once every frame.
        /// </summary>
        void Update()
        {
            // Calculating direction...
            dir = (holdplayer.position - teammate.position).normalized;
            // ...and distance.
            dist = Vector2.Distance(holdplayer.position, teammate.position);

            // Counting down the carry timer.
            carryTimer -= Time.deltaTime;

            // Doing an if to ensure that the balls position changes accordingly if it is being carried.
            if (carryTimer > 0)
            {   
                // Checking if the holder and teammate swap which one is in front and doing teh appropriate changes-
                if(holdplayer.position.y > teammate.position.y){
                    // Switching holder and teammate around as we found the teammate to be closer to the center Y.
                    var temphold = holdplayer;
                    holdplayer = teammate;
                    teammate = temphold;

                    // Setting the ball to be the child of the holder for carrying purposes.
                    this.transform.SetParent(holdplayer);                    
                }

                // Setting ball position to be pointing away from the teammate at a set distance.
                this.transform.localPosition = dir.normalized * 1.55f;

                // Telling the controller that it's time to stop.
                controller.stop = true;
                controller.ballMoveSpeed = 0f;
            }
            else
            {
                // Telling the controller that we're not stopped anymore, and setting the speed.
                controller.stop = false;
                controller.ballMoveSpeed = orgSpeed;
                
                // if launchState is true, time to launch the ball.
                if(launchState)
                {
                    // Launchstate false since we're now launching.
                    launchState = false;
                    // Getting the balls rigidbody
                    var thisRigBod = GetComponent<Rigidbody2D>();
                    // If distance is above original speed, it now becomes original speed.
                    if (dist>orgSpeed) {dist = orgSpeed;}
                    // Using distance to determine speed.
                    thisRigBod.velocity = dir * dist;
                    // Setting this transform free of any parents.
                    this.transform.parent = null;
                    // Setting the controllers 'ballMoveSpeed' to be the distance so the ball doesn't speed back up.
                    controller.ballMoveSpeed = dist;

                    // Turning on the playerBallStoppers.
                    playerBallStopper[] stoppers;
                    stoppers = FindObjectsOfType<playerBallStopper>();
                    foreach (var stopper in stoppers)
                    {
                        stopper.ballCaught = false;
                    }

                    // Turning on the shieldManagers.
                    shieldManager[] managers;
                    managers = FindObjectsOfType<shieldManager>();
                    foreach (var manager in managers)
                    {
                        manager.ballCaught = false;
                    }
                }
            }
        }
        #endregion
    }
}
