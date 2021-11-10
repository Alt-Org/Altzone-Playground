using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer
{
    public class playerBallStopper : MonoBehaviour
    {

        #region Serializable Private Fields

        // playerPlaceHolder helps hold the player object for setting the playerManagers.
        [SerializeField] private GameObject playerPlaceHolder;
        [SerializeField] private playerManager player1;
        [SerializeField] private playerManager player2;

        #endregion

        #region Public Field

        // Creating a public bool that's set by ballLauncher whenever the ball is caught to basically halt this script while the ball is being held.
        public bool ballCaught = false;

        #endregion

        #region Method.
        
        /// <summary>
        /// Method that is called whenever an object within this is moving, and not just now entering or exiting.
        /// </summary>
        void OnTriggerStay2D(Collider2D other)
        {            
            if(other.gameObject.layer == 9 && ballCaught == false)
            {                  
                if (gameObject.tag == "PelaajaAlue-1")
                {
                    if (player1 == null) {
                        playerPlaceHolder = GameObject.FindGameObjectWithTag("Pelaaja-1");
                        player1 = playerPlaceHolder.GetComponent<playerManager>();
                    } else {                        
                        player1.playerStop(2);
                    }

                    if (player2 == null) {
                        playerPlaceHolder = GameObject.FindGameObjectWithTag("Pelaaja-3");
                        player2 = playerPlaceHolder.GetComponent<playerManager>();
                    } else {                        
                        player2.playerStop(2);
                    }
                }
                else if (gameObject.tag == "PelaajaAlue-2")
                {
                    if (player1 == null) {
                        playerPlaceHolder = GameObject.FindGameObjectWithTag("Pelaaja-2");
                        player1 = playerPlaceHolder.GetComponent<playerManager>();
                    } else {                        
                        player1.playerStop(2);
                    }

                    if (player2 == null) {
                        playerPlaceHolder = GameObject.FindGameObjectWithTag("Pelaaja-4");
                        player2 = playerPlaceHolder.GetComponent<playerManager>();
                    } else {                        
                        player2.playerStop(2);
                    }
                }
            }
        }

        #endregion
    }
}
