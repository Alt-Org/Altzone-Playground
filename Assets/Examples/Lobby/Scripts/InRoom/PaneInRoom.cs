using Photon.Pun;
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

        private void Start()
        {
            buttons[0].onClick.AddListener(setPlayerAsGuest);
            buttons[1].onClick.AddListener(setPlayerAsSpectator);
            buttons[2].onClick.AddListener(startPlaying);
        }

        private void setPlayerAsGuest()
        {
            Debug.Log($"setPlayerAsGuest {LobbyManager.playerIsGuest}");
            this.Publish(new LobbyManager.PlayerPosEvent(LobbyManager.playerIsGuest));
        }

        private void setPlayerAsSpectator()
        {
            Debug.Log($"setPlayerAsSpectator {LobbyManager.playerIsSpectator}");
            this.Publish(new LobbyManager.PlayerPosEvent(LobbyManager.playerIsSpectator));
        }

        private void startPlaying()
        {
            Debug.Log($"startPlaying {LobbyManager.startPlaying}");
            this.Publish(new LobbyManager.PlayerPosEvent(LobbyManager.startPlaying));
        }

        /// <summary>
        /// Stupid way to poll network state changes on every frame!
        /// </summary>
        private void Update()
        {
            title.text = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "<color=red><b>Not in room</b></color>";
        }
    }
}