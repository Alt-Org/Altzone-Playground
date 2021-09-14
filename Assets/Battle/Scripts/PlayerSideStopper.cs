using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class PlayerSideStopper : MonoBehaviour
    {
        
        public OnlinePlayer player;
        public GameObject playerobj;

        
        void OnTriggerStay2D(Collider2D other)
        {            
            if(other.gameObject.layer == 9)
            {
                if (!player) {                    
                    if (gameObject.tag == "PelaajaAlue-1")
                    {
                        playerobj = GameObject.FindGameObjectWithTag("Pelaaja-1");
                        if(playerobj)
                        {
                            player = playerobj.GetComponent<OnlinePlayer>();
                        }
                    }
                    else if (gameObject.tag == "PelaajaAlue-2")
                    {
                        playerobj = GameObject.FindGameObjectWithTag("Pelaaja-2");
                        if(playerobj)
                        {
                            player = playerobj.GetComponent<OnlinePlayer>();
                        }
                    }
                    else if (gameObject.tag == "PelaajaAlue-3")
                    {
                        playerobj = GameObject.FindGameObjectWithTag("Pelaaja-3");
                        if(playerobj)
                        {
                            player = playerobj.GetComponent<OnlinePlayer>();
                        }
                    }
                    else if (gameObject.tag == "PelaajaAlue-4")
                    {
                        playerobj = GameObject.FindGameObjectWithTag("Pelaaja-4");
                        if(playerobj)
                        {
                            player = playerobj.GetComponent<OnlinePlayer>();
                        }
                    }
                }
                
                if (player)
                {
                    player.playerStop(2);
                }
            }
        }
    }
}
