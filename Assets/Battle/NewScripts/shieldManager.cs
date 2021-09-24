using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer
{
    public class shieldManager : MonoBehaviour
    {
        
        [Header("Player Settings"), SerializeField] private Transform leftShieldTransform;
        [SerializeField] private Transform rightShieldTransform;        
        [SerializeField] private playerMove player;

        
        [Header("Live Data"), SerializeField] private Collider2D leftCollider;
        [SerializeField] private Collider2D rightCollider;
        [SerializeField] private float angle = 0f;
        [SerializeField] private float health = 4f;

        private Vector3 localEulerAngles = new Vector3(0, 0, 0);
        private bool isInitialized;
        private bool hitReady;

        // Start is called before the first frame update
        void Start()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                leftCollider = leftShieldTransform.GetComponent<Collider2D>();
                rightCollider = rightShieldTransform.GetComponent<Collider2D>();
                createSlave(leftCollider.gameObject, this, 9);
                createSlave(rightCollider.gameObject, this, 9);
                health = 4f;
                hitReady = true;
            }
        }

        public void collisionWithBall(Collision2D collision)
        {
            if (health == 0)
            {
                return;
                //player.playerStop(1);
            } else if (hitReady)
            {
                health -= 1;
                angle += -15f;
                hitReady = false;
                StartCoroutine(hitWait());
            }
        }

        IEnumerator hitWait()
        {
            yield return new WaitForSeconds(0.1f);

            hitReady = true;
        }


        private void Update()
        {
            leftShieldTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f + (angle));
            rightShieldTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f + (angle*-1f));
        }

        private static void createSlave(GameObject parent, shieldManager master, int layer)
        {
            var slave = parent.gameObject.AddComponent<shieldSlave>();
            slave.master = master;
            slave.layer = layer;
        }
    }
}
