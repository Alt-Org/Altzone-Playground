using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Lobby.Scripts.InRoom
{
    /// <summary>
    /// Middle pane in lobby while in room to manage current player "state".
    /// </summary>
    public class PaneInRoom : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Button[] buttons;

        private ClientState curClientState;
        private void Start()
        {
            buttons[0].onClick.AddListener(setPlayerAsGuest);
            buttons[1].onClick.AddListener(setPlayerAsSpectator);
            buttons[2].onClick.AddListener(startPlaying);
        }

        private void setPlayerAsGuest()
        {
            Debug.Log($"setPlayerAsGuest {LobbyManager.playerIsGuest}");
            this.Publish(new LobbyManager.Event(LobbyManager.playerIsGuest));

        }

        private void setPlayerAsSpectator()
        {
            Debug.Log($"setPlayerAsSpectator {LobbyManager.playerIsSpectator}");
            this.Publish(new LobbyManager.Event(LobbyManager.playerIsSpectator));

        }

        private void startPlaying()
        {
            Debug.Log($"startPlaying {LobbyManager.startPlaying}");
            this.Publish(new LobbyManager.Event(LobbyManager.startPlaying));

        }

        /// <summary>
        /// Stupid way to poll network state changes on every frame!
        /// </summary>
        private void Update()
        {
            if (curClientState == PhotonNetwork.NetworkClientState)
            {
                return;
            }
            curClientState = PhotonNetwork.NetworkClientState;
            var room = PhotonNetwork.CurrentRoom;
            title.text = room?.Name ?? "<color=red><b>Not in room</b></color>";
        }
    }
}
