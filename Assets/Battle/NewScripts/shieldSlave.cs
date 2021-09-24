using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer
{
    public class shieldSlave : MonoBehaviour
    {
        [Header("Live Data")] public shieldManager master;
        public int layer;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == layer)
            {
                master.collisionWithBall(collision);
            }
        }
    }
}