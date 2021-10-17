using Examples.Config.Scripts;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
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
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 5;

        [SerializeField] private int secondsRemaining;
        [SerializeField] private bool isBallFound;
        [SerializeField] private float ballFoundTime;
        [SerializeField] private float roomCountdownTime;

        private PhotonEventDispatcher photonEventDispatcher;

        // Configurable settings
        private GameVariables variables;

        private void Awake()
        {
            variables = RuntimeGameConfig.Get().variables;
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, data => { handleRoomTimerProgress(data.CustomData); });
        }

        private void Start()
        {
            Debug.Log($"Start: {PhotonNetwork.NetworkClientState} time={Time.time:0.00}");
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable: {PhotonNetwork.NetworkClientState} time={Time.time:0.00}");
            // Timer start running from here!
            secondsRemaining = variables.roomStartDelay;
            ballFoundTime = Time.time;
            sendRoomTimerProgress();
            roomCountdownTime = Time.time + 1.0f;
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable: {PhotonNetwork.NetworkClientState} time={Time.time:0.00}");
            this.Unsubscribe();
        }

        private void sendRoomTimerProgress()
        {
            // Synchronize to all game managers
            var payload = secondsRemaining;
            photonEventDispatcher.RaiseEvent(photonEventCode, payload);
        }

        private void handleRoomTimerProgress(object payload)
        {
            secondsRemaining = (int)payload;
            this.Publish(new CountdownEvent(variables.roomStartDelay, secondsRemaining));
            Debug.Log($"secondsRemaining={secondsRemaining}");
            if (secondsRemaining <= 0)
            {
                startRoom();
            }
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            if (Time.time > roomCountdownTime)
            {
                secondsRemaining -= 1;
                sendRoomTimerProgress();
                roomCountdownTime = Time.time + 1.0f;
            }
        }

        private void startRoom()
        {
            Debug.Log("*");
            Debug.Log($"* startRoom={Time.time:0.00}");
            Debug.Log("*");
       }

        public class CountdownEvent
        {
            public readonly int maxCountdownValue;
            public readonly int curCountdownValue;

            public CountdownEvent(int maxCountdownValue, int curCountdownValue)
            {
                this.maxCountdownValue = maxCountdownValue;
                this.curCountdownValue = curCountdownValue;
            }

            public override string ToString()
            {
                return $"max: {maxCountdownValue}, cur: {curCountdownValue}";
            }
        }
    }
}