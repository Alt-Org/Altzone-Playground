using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer 
{
    public class ballSlave : MonoBehaviour
    {
        [Header("Ball Settings"), SerializeField] private GameObject thisthing;
        
        /// <summary>
        /// This script is added to the players head, and if it detects the ball, it sends the ballLauncher a signal that the player has caught it.
        /// </summary>
        private void OnCollisionEnter2D(Collision2D othercollider)
        {
            Transform other = othercollider.transform;
            if(other.CompareTag("Ball"))
            {
                other.GetComponent<ballLauncher>().collided(this.gameObject);
            }
        }
    }
}
