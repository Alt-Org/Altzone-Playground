using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts
{
    public class PaneInRoom : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Button[] buttons;

        private void Start()
        {
            var room = PhotonNetwork.CurrentRoom;
            title.text = room?.Name ?? "Not in room";
            buttons[0].onClick.AddListener(() => this.Publish(new LobbyManager.Event(LobbyManager.playerIsGuest)));
            buttons[1].onClick.AddListener(() => this.Publish(new LobbyManager.Event(LobbyManager.playerIsSpectator)));
        }
   }
}
