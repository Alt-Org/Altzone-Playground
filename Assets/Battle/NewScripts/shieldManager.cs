using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer
{
    public class shieldManager : MonoBehaviour
    {
        // Assigning the left and right shields as well as the parent playerManager.
        [Header("Player Settings"), SerializeField] private Transform leftShieldTransform;
        [SerializeField] private Transform rightShieldTransform;        
        [SerializeField] private playerManager player;

        // Getting the left and right shields colliders as well as initiating the angle the shields are at and setting health to max.
        [Header("Live Data"), SerializeField] private Collider2D leftCollider;
        [SerializeField] private Collider2D rightCollider;
        [SerializeField] private float angle = 0f;
        [SerializeField] private int health = 4;

        // Setting some angles and a couple booleans used in the script.
        private Vector3 localEulerAngles = new Vector3(0, 0, 0);
        private bool isInitialized;
        private bool hitReady;

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
        // A coroutine to make use of the 'WaitForSeconds' thing to make sure that hits which collide with both shield halves don't suddenly do double damange.
        IEnumerator hitWait()
        {
            yield return new WaitForSeconds(0.1f);

            hitReady = true;
        }

        // This update makes sure that the angle changes properly, not too much nor too little.
        private void Update()
        {
            leftShieldTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f + (angle));
            rightShieldTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f + (angle*-1f));
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
