using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer
{
    public class shieldManager : MonoBehaviour
    {
        // Assigning the left and right shields as well as the parent playerManager. Also adding a float for the distance at which the shield dissappears.
        [Header("Player Settings"), SerializeField] private Transform leftShieldTransform;
        [SerializeField] private Transform rightShieldTransform;        
        [SerializeField] private playerManager player;
        [SerializeField] private float shieldDisableDist;

        // Getting the left and right shields colliders as well as initiating the angle the shields are at and setting health to max.
        [Header("Live Data"), SerializeField] private Collider2D leftCollider;
        [SerializeField] private Collider2D rightCollider;
        [SerializeField] private float angle = 0f;
        [SerializeField] private int health = 4;

        // Setting up a couple transforms and a float into which put the distance between them.
        // This is for disabling and enabling the shield with distance.
        [SerializeField] private Transform thisPlayer;
        [SerializeField] private Transform teamMate;
        [SerializeField] private float playerDist;

        // Setting some angles and a couple booleans used in the script.
        private Vector3 localEulerAngles = new Vector3(0, 0, 0);
        private bool isInitialized;
        private bool hitReady;

        // A function that is called near or at the start to set the two transforms based on tags.
        private void setTransforms()
        {
            thisPlayer = this.transform.parent.transform;
            if(this.transform.parent.tag == "Pelaaja-1")
            {
                teamMate = GameObject.FindWithTag("Pelaaja-3").transform;
            }
            else if(this.transform.parent.tag == "Pelaaja-2")
            {
                teamMate = GameObject.FindWithTag("Pelaaja-4").transform;
            }
            else if(this.transform.parent.tag == "Pelaaja-3")
            {
                teamMate = GameObject.FindWithTag("Pelaaja-1").transform;
            }
            else if(this.transform.parent.tag == "Pelaaja-4")
            {
                teamMate = GameObject.FindWithTag("Pelaaja-2").transform;
            }
        }

        // A function that is called by the collision slaves that either reduces the player HP or stuns them if they are at 0.
        public void collisionWithBall(Collision2D collision)
        {
            if (health == 0)
            {
                player.playerStop(1);
            } else if (hitReady)
            {
                health -= 1;
                angle += -15f;
                hitReady = false;
                player.squishPlayer(health);
                StartCoroutine(hitWait());
            }
        }

        // Initializing the shields, including creating some collision slave scripts on the shield halves.
        void Start()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                leftCollider = leftShieldTransform.GetComponent<Collider2D>();
                rightCollider = rightShieldTransform.GetComponent<Collider2D>();
                createSlave(leftCollider.gameObject, this, 9);
                createSlave(rightCollider.gameObject, this, 9);
                hitReady = true;
                setTransforms();
            }
        }
        // A coroutine to make use of the 'WaitForSeconds' thing to make sure that hits which collide with both shield halves don't suddenly do double damange.
        IEnumerator hitWait()
        {
            yield return new WaitForSeconds(0.1f);

            hitReady = true;
        }

        private void Update()
        {
            // This update makes sure that the angle changes properly, not too much nor too little.
            leftShieldTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f + (angle));
            rightShieldTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f + (angle*-1f));

            // Getting the distance between the player and disabling/enabling shield as appropriate.
            playerDist = Vector2.Distance(thisPlayer.position, teamMate.position);

            if(playerDist>shieldDisableDist)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
            } 
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }

        // A function that is called to create collision slaves on the shield halves.
        private static void createSlave(GameObject parent, shieldManager master, int layer)
        {
            var slave = parent.gameObject.AddComponent<shieldSlave>();
            slave.master = master;
            slave.layer = layer;
        }
    }
}
