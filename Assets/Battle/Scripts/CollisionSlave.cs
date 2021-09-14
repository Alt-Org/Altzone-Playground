using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class CollisionSlave : MonoBehaviour
    {
        [Header("Live Data")] public OnlinePlayerShield master;
        public int layer;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == layer)
            {
                master.onCollisionEnter2D(collision);
            }
        }
    }
}