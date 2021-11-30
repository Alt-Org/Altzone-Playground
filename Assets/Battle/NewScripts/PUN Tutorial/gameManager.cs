using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Altzone.NewPlayer 
{
    public class gameManager : MonoBehaviourPunCallbacks
    {

        #region Serialized Private Fields

        // Spawn of player 1
        [SerializeField, Tooltip("Spawn position of Player 1")] private Transform spawn1;
        // Spawn of player 2
        [SerializeField, Tooltip("Spawn position of Player 2")] private Transform spawn2;
        // Spawn of player 3
        [SerializeField, Tooltip("Spawn position of Player 3")] private Transform spawn3;
        // Spawn of player 4
        [SerializeField, Tooltip("Spawn position of Player 4")] private Transform spawn4;

        
        [SerializeField, Tooltip("The prefab to use for representing player 1")]
        private GameObject playerPrefab1;
        [SerializeField, Tooltip("The prefab to use for representing player 2")]
        private GameObject playerPrefab2;
        [SerializeField, Tooltip("The prefab to use for representing player 3")]
        private GameObject playerPrefab3;
        [SerializeField, Tooltip("The prefab to use for representing player 4")]
        private GameObject playerPrefab4;

        #endregion

        #region Private Fields
        // This is set to 1 if the player needs to be flipped to face up.
        private int playerFlip = 0;

        // Four boolean custom properties that represent each player slot. True means there is a player in the slot.
        bool player1Slot;// = (bool)PhotonNetwork.CurrentRoom.CustomProperties["player1Slot"];
        bool player2Slot;// = (bool)PhotonNetwork.CurrentRoom.CustomProperties["player2Slot"];
        bool player3Slot;// = (bool)PhotonNetwork.CurrentRoom.CustomProperties["player3Slot"];
        bool player4Slot;// = (bool)PhotonNetwork.CurrentRoom.CustomProperties["player4Slot"];

        // A simple integer that's set to be 1-4 depending on which player we are initializing by the PlayerPrefabSet()
        private int curPlayer = 0;

        #endregion

        #region Public Fields
        
        public static gameManager Instance;

        #endregion

        #region Photon Callbacks

        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            }
        }


        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method that figures out which playerPrefab to use for a joining player.
        /// It does this by checking the rooms CustomProperties 'player#Slot' variables and seeing if they are true or falase.
        /// Then we return the correct prefab.
        /// </summary>
        private String PlayerPrefabSet()
        {
            if((bool)PhotonNetwork.CurrentRoom.CustomProperties["player1Slot"] == false)
            {
                Debug.Log("Player Prefab 1");
                player1Slot = true;
                Hashtable setValue = new Hashtable();
                setValue.Add("player1Slot", player1Slot);
                PhotonNetwork.CurrentRoom.SetCustomProperties(setValue);
                curPlayer = 1;
                return playerPrefab1.name;
            }
            else if((bool)PhotonNetwork.CurrentRoom.CustomProperties["player2Slot"] == false)
            {
                Debug.Log("Player Prefab 2");
                player2Slot = true;
                Hashtable setValue = new Hashtable();
                setValue.Add("player2Slot", player2Slot);
                PhotonNetwork.CurrentRoom.SetCustomProperties(setValue);
                curPlayer = 2;
                return playerPrefab2.name;
            }
            else if((bool)PhotonNetwork.CurrentRoom.CustomProperties["player3Slot"] == false)
            {
                Debug.Log("Player Prefab 3");
                player3Slot = true;
                Hashtable setValue = new Hashtable();
                setValue.Add("player3Slot", player3Slot);
                PhotonNetwork.CurrentRoom.SetCustomProperties(setValue);
                curPlayer = 3;
                return playerPrefab3.name;
            }
            else if((bool)PhotonNetwork.CurrentRoom.CustomProperties["player4Slot"] == false)
            {
                Debug.Log("Player Prefab 4");
                player4Slot = true;
                Hashtable setValue = new Hashtable();
                setValue.Add("player4Slot", player4Slot);
                PhotonNetwork.CurrentRoom.SetCustomProperties(setValue);
                curPlayer = 4;
                return playerPrefab4.name;
            } else {
                Debug.Log("Player Prefab 0");
                return playerPrefab1.name;
            }
        }

        /// <summary>
        /// Method that figures out which position the player should be spawned into.
        /// It does this by counting up and checking the 'Pelaaja-#' tags, until it finds one which returns null.
        /// Then we spawn the player in that position.
        /// We also set 'playerFlip' to flip the player to face up if he is player 1 or 3.
        /// </summary>
        private Vector3 PlayerPosFinder()
        {
            if(curPlayer == 1)
            {
                Debug.Log("Player position ");
                playerFlip = 1;
                return spawn1.position;
            }
            else if(curPlayer == 2)
            {
                Debug.Log("Player position 2");
                return spawn2.position;
            }
            else if(curPlayer == 3)
            {
                Debug.Log("Player position 3");
                playerFlip = 1;
                return spawn3.position;
            }
            else if(curPlayer == 4)
            {
                Debug.Log("Player position 4");
                return spawn4.position;
            } else {
                Debug.Log("Player position 0");
                return Vector3.zero;
            }
        }

        /// <summary>
        /// Method that sets the players rotation depending on a variable set by PlayerPosFinder()
        /// </summary>
        private Quaternion PlayerRotat()
        {
            if(playerFlip == 1)
            {
                Debug.Log("Player rotated");
                return Quaternion.Euler(0f,0f,180f);
            }
            else
            {
                Debug.Log("Player not rotated");
                return Quaternion.Euler(0f,0f,0f);
            }
        }  

        #endregion


        #region Public Methods

        /// <summary>
        /// Called through a button press on the in game UI.
        /// </summary>
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        #endregion

        #region Private Methods
        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            Debug.LogFormat("PhotonNetwork : Loading Level");
            PhotonNetwork.LoadLevel("Test Scene");
        }

        void Start()
        {
            Instance = this;

            if (playerPrefab1 == null||playerPrefab2 == null||playerPrefab3 == null||playerPrefab4 == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
            }
            else
            {
                if (playerManager.LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                    PhotonNetwork.Instantiate(PlayerPrefabSet(), PlayerPosFinder(), PlayerRotat(), 0);
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }
        }

        #endregion
    }
}