using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Nelinpeli
{
    public class StoneScriptAdder : MonoBehaviour
    {
        
        [SerializeField] private GameObject p1hcounter;
        [SerializeField] private GameObject p2hcounter;
        [SerializeField] private GameObject wallhpcounter;
        [SerializeField] private bool topside;
        [SerializeField] private GameObject walldiamond;
        [SerializeField] private GameObject wallsquare;

        [Header("Live Data"), SerializeField] private Collider2D childCollider;
        public float backhit;
        [SerializeField] private GameObject[] players;
        [SerializeField] private GameObject player1;
        [SerializeField] private GameObject player2;
        private string[] HPCounter = new string[]{"", "*", "**", "***", "****"};
        public int player1hit = 4;
        public int player2hit = 4;
        public int wallhp = 4;
        

        // Start is called before the first frame update
        void Start()
        {
            backhit = 0;
            foreach (Transform child in transform){
                createRock(child.gameObject, this, 9);
            }

            SlaveInit(walldiamond, 3);
            SlaveInit(wallsquare, 3);
            
        }

        private void Update() {
            MakeSlaves();
        }

        private static void createRock(GameObject parent, StoneScriptAdder master, int layer)
        {
            var slave = parent.gameObject.AddComponent<StoneColliderControl>();
            slave.master = master;
            slave.layer = layer;
        }

        public void HPCount(int player)
        {
            if(player == 1)
            {
                player1hit -= 1;
                p1hcounter.GetComponent<Text>().text = HPCounter[player1hit];
            }
            else if(player == 2)
            {
                player2hit -= 1;
                p2hcounter.GetComponent<Text>().text = HPCounter[player2hit];
            }
            else
            {
                wallhp -= 1;
                wallhpcounter.GetComponent<Text>().text = HPCounter[wallhp];
            }
        }

        private void MakeSlaves()
        {
            if (topside)
            {
                players = GameObject.FindGameObjectsWithTag("TopSide");

                if ((player1 == null) && (players[0] != null)) {
                    player1 = players[0];
                    SlaveInit(player1, 1);
                }

                if (player2 == null && (players[1] != null)) {
                    player2 = players[1];
                    SlaveInit(player2, 2);
                }

            }
            else
            {
                players = GameObject.FindGameObjectsWithTag("BotSide");

                if (player1 == null && (players[0] != null)) {
                    player1 = players[0];
                    SlaveInit(player1, 1);
                }

                if (player2 == null && (players[1] != null)) {
                    player2 = players[1];
                    SlaveInit(player2, 2);
                }
            }
        }

        private void SlaveInit(GameObject slave, int player)
        {            
            var script = slave.gameObject.AddComponent<ScoreSlave>();
            script.master = this;
            script.layer = 9;
            script.player = player;
        }
    }
}
