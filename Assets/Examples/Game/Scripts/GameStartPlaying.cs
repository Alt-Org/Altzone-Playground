using Examples.Config.Scripts;
using Photon.Pun;
using System;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Script to control when game (room) can start playing.
    /// </summary>
    /// <remarks>
    /// In practice we just wait until we found the ball and disable it for given time (<c>roomStartDelay</c>).
    /// </remarks>
    public class GameStartPlaying : MonoBehaviour
    {
        [SerializeField] private bool isBallFound;
        [SerializeField] private float ballFoundTime;
        [SerializeField] private float roomStartTime;
        [SerializeField] private BallMovement ballMovement;

        // Configurable settings
        private GameVariables variables;

        private void Awake()
        {
            variables = RuntimeGameConfig.Get().variables;
        }

        private void Start()
        {
            Debug.Log($"Start: {PhotonNetwork.NetworkClientState} time={Time.time:0.00}");
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable: {PhotonNetwork.NetworkClientState} time={Time.time:0.00}");
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable: {PhotonNetwork.NetworkClientState} time={Time.time:0.00}");
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            if (!isBallFound)
            {
                ballMovement = FindObjectOfType<BallMovement>();
                isBallFound = ballMovement != null;
                if (isBallFound)
                {
                    // Timer start running from here!
                    ballFoundTime = Time.time;
                    roomStartTime = ballFoundTime + variables.roomStartDelay;
                    Debug.Log($"ballFoundTime={ballFoundTime:0.00} roomStartTime={roomStartTime:0.00}");
                    ballMovement.enabled = false;
                }
                return;
            }
            if (Time.time > roomStartTime)
            {
                startRoom();
                enabled = false;
            }
        }

        private void startRoom()
        {
            Debug.Log($"startRoom={Time.time:0.00}");
            ballMovement.enabled = true;
        }
    }
}