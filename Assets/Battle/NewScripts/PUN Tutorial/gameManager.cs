using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

namespace Altzone.NewPlayer 
{
    public class gameManager : MonoBehaviourPunCallbacks
    {
        #region Private Fields

        int player = 0;

        #endregion

        #region Public Fields
        
        public static gameManager Instance;

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

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

        private Vector3 PlayerPosFinder()
        {
            if(GameObject.FindGameObjectWithTag("Pelaaja-1") == null)
            {
                var spawn = GameObject.FindGameObjectWithTag("PelaajaAlue-1");
                player = 1;
                return spawn.transform.position;
            }
            else if(GameObject.FindGameObjectWithTag("Pelaaja-2") == null)
            {
                var spawn = GameObject.FindGameObjectWithTag("PelaajaAlue-2");
                player = 2;
                return spawn.transform.position;
            }
            else if(GameObject.FindGameObjectWithTag("Pelaaja-3") == null)
            {
                var spawn = GameObject.FindGameObjectWithTag("PelaajaAlue-3");
                player = 1;
                return spawn.transform.position;
            }
            else if(GameObject.FindGameObjectWithTag("Pelaaja-4") == null)
            {
                var spawn = GameObject.FindGameObjectWithTag("PelaajaAlue-4");
                player = 2;
                return spawn.transform.position;
            } else {
                return Vector3.zero;
            }
        }

        private Quaternion PlayerRotat()
        {
            if(player == 1)
            {
                return Quaternion.Euler(0f,0f,180f);
            }
            else if (player == 2)
            {
                return Quaternion.Euler(0f,0f,0f);
            }
            else
            {
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

            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
            }
            else
            {
                if (playerManager.LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                    PhotonNetwork.Instantiate(this.playerPrefab.name, PlayerPosFinder(), PlayerRotat(), 0);
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