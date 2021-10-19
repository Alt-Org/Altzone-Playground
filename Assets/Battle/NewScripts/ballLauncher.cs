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
        private bool launchState;

        // On collision enter that is called when we collide with a player head.
        // For some reason, it is not the player head that it gets when colliding, but the player itself.
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

                // Setting launchState boolean to true as we're about to launch it.
                launchState = true;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            // Getting the org speed from the ball controller.
            orgSpeed = controller.ballMoveSpeed;
        }

        // Update is called once per frame
        void Update()
        {
            // Calculating direction...
            dir = (holdplayer.position - teammate.position).normalized;
            // ...and distance.
            dist = Vector2.Distance(holdplayer.position, teammate.position);

            // Counting down the carry timer.
            carryTimer -= Time.deltaTime;

            // Doing an if to ensure that the
            if (carryTimer > 0)
            {
                if (holdplayer.CompareTag("Pelaaja-1") || holdplayer.CompareTag("Pelaaja-3"))
                {
                    this.transform.localPosition = new Vector2(0, 1.55f);
                } 
                else
                {
                    this.transform.localPosition = new Vector2(0, -1.55f);
                }
                controller.stop = true;
                controller.ballMoveSpeed = 0f;
            }
            else
            {
                controller.stop = false;
                controller.ballMoveSpeed = orgSpeed;
                
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
    }
}
