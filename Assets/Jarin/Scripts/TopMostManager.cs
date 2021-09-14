using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Jarin.Scripts
{
    public enum TopMostState
    {
        None,
        Initializing,
        Menu,
        Lobby,
        Game,
    }

    public class TopMostManager : MonoBehaviour
    {
        // See CustomPropKeyExtensions for original key names
        private const string gameModeKey = "GM";
        private const string spectatorKey = "<s>";
        private const string spectatorCountKey = "SC";

        [SerializeField] private GameObject loader;
        [SerializeField] private GameObject menu;
        [SerializeField] private GameObject lobby;
        [SerializeField] private GameObject game;
        [SerializeField] private TopMostState state;

        public TopMostState State => state;

        private LobbyHelper lobbyHelper;

        private void Awake()
        {
            lobbyHelper = LobbyHelper.Get(new LobbyHelper.KeyNames(gameModeKey, spectatorKey, spectatorCountKey));
            state = TopMostState.None;
            loadAndHideNavigationElement(ref menu);
            loadAndHideNavigationElement(ref lobby);
            loadAndHideNavigationElement(ref game);
        }

        private void Update()
        {
            var photonState = PhotonNetwork.NetworkClientState;
            switch (photonState)
            {
                case ClientState.Joined:
                    if (state != TopMostState.Game)
                    {
                        JoinedRoom();
                    }
                    break;
                case ClientState.JoinedLobby:
                    if (state != TopMostState.Lobby)
                    {
                        JoinedLobby();
                    }
                    break;
                case ClientState.ConnectedToMasterServer:
                    if (state != TopMostState.Menu)
                    {
                        ShowingMenus();
                    }
                    break;
                default:
                    if (photonState == ClientState.PeerCreated || photonState == ClientState.Disconnected)
                    {
                        ConnectToPhoton();
                    }
                    break;
            }
        }

        private void ConnectToPhoton()
        {
            Debug.LogFormat("ConnectToPhoton {0} {1} player {2}", PhotonNetwork.NetworkClientState, state, LocalPlayerSettings.PlayerName);
            lobbyHelper.connect(LocalPlayerSettings.PlayerName);
            state = TopMostState.Initializing;
        }

        private void ShowingMenus()
        {
            Debug.LogFormat("ShowingMenus {0} {1} player {2}", PhotonNetwork.NetworkClientState, state, LocalPlayerSettings.PlayerName);
            state = TopMostState.Menu;
            loader.SetActive(false);
            menu.SetActive(true);
            lobby.SetActive(false);
            game.SetActive(false);
        }

        private void JoinedLobby()
        {
            Debug.LogFormat("JoinedLobby {0} {1} player {2}", PhotonNetwork.NetworkClientState, state, LocalPlayerSettings.PlayerName);
            state = TopMostState.Lobby;
            loader.SetActive(false);
            menu.SetActive(false);
            lobby.SetActive(true);
            game.SetActive(false);
        }

        private void JoinedRoom()
        {
            Debug.LogFormat("JoinedRoom {0} {1}", PhotonNetwork.NetworkClientState, state);
            state = TopMostState.Game;
            loader.SetActive(false);
            menu.SetActive(false);
            lobby.SetActive(false);
            game.SetActive(true);
        }

        private static void loadAndHideNavigationElement(ref GameObject parent)
        {
            if (parent.scene.handle == 0)
            {
                parent = Instantiate(parent);
                parent.name = parent.name.Replace("(Clone)", "");
            }
            parent.SetActive(false);
        }
    }
}