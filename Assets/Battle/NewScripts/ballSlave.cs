using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer 
{
    public class ballSlave : MonoBehaviour
    {
        [Header("Ball Settings"), SerializeField] private GameObject thisthing;
        
        // The only thing this script does is detect if the ball has collided with this, and then sends a signal to ballLauncher.
        private void OnCollisionEnter2D(Collision2D other)
        {
            Transform asd = other.transform;
            if(asd.CompareTag("Ball"))
            {
                asd.GetComponent<ballLauncher>().collided(this.gameObject);
            }
        }
    }
}
