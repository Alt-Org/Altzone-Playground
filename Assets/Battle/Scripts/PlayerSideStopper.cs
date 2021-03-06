using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class PlayerSideStopper : MonoBehaviour
    {
        [SerializeField] private GameObject playerplaceholder;
        [SerializeField] private OnlinePlayer player1;
        [SerializeField] private OnlinePlayer player2;

        
        void OnTriggerStay2D(Collider2D other)
        {            
            if(other.gameObject.layer == 9)
            {                  
                if (gameObject.tag == "PelaajaAlue-1")
                {
                    if (player1 == null) {
                        playerplaceholder = GameObject.FindGameObjectWithTag("Pelaaja-1");
                        player1 = playerplaceholder.GetComponent<OnlinePlayer>();
                    } else {                        
                        player1.playerStop(2);
                    }

                    if (player2 == null) {
                        playerplaceholder = GameObject.FindGameObjectWithTag("Pelaaja-3");
                        player2 = playerplaceholder.GetComponent<OnlinePlayer>();
                    } else {                        
                        player2.playerStop(2);
                    }
                }
                else if (gameObject.tag == "PelaajaAlue-2")
                {
                    if (player1 == null) {
                        playerplaceholder = GameObject.FindGameObjectWithTag("Pelaaja-2");
                        player1 = playerplaceholder.GetComponent<OnlinePlayer>();
                    } else {                        
                        player1.playerStop(2);
                    }

                    if (player2 == null) {
                        playerplaceholder = GameObject.FindGameObjectWithTag("Pelaaja-4");
                        player2 = playerplaceholder.GetComponent<OnlinePlayer>();
                    } else {                        
                        player2.playerStop(2);
                    }
                }
            }
        }
    }
}
