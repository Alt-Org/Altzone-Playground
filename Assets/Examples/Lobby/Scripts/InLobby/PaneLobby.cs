using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Lobby.Scripts.InLobby
{
    public class PaneLobby : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text lobbyText;

        private void Start()
        {
            titleText.text = $"Welcome to {Application.productName} {PhotonLobby.gameVersion}";
        }

        private void Update()
        {
            if (!PhotonNetwork.InLobby)
            {
                return;
            }
            var playerCount = PhotonNetwork.CountOfPlayers;
            lobbyText.text = playerCount == 1
                ? "You are the only player here"
                : $"There is {playerCount} players online";
            ;
        }
    }
}