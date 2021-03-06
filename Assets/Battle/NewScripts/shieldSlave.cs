using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer
{
    public class shieldSlave : MonoBehaviour
    {   
        // The master object this tells collisions to
        [Header("Live Data")] public shieldManager master;
        // Layer which we are comparing against, 9 is the layer for ball but it's set by the shieldManager on creation.
        public int layer;

        /// <summary>
        /// If the ball collides with this, tells its master shieldManager that the shield's been hit by the ball.
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == layer)
            {
                master.collisionWithBall(collision);
            }
        }
    }
}