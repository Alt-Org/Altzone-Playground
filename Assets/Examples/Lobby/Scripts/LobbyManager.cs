using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using System.Collections;
using System.Linq;
using UiProto.Scripts.Window;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Examples.Lobby.Scripts
{
    /// <summary>
    /// Manages local player position and setup in a room and controls which level is loaded next.
    /// </summary>
    /// <remarks>
    /// Game settings are saved in player custom properties for each participating player.
    /// </remarks>
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        public const string playerPositionKey = "pp";
        public const string playerMainSkillKey = "mk";

        public const int playerPosition0 = 0;
        public const int playerPosition1 = 1;
        public const int playerPosition2 = 2;
        public const int playerPosition3 = 3;
        public const int playerIsGuest = 10;
        public const int playerIsSpectator = 11;
        public const int startPlaying = 123;

        [SerializeField] private LevelIdDef cancelLevel;
        [SerializeField] private LevelIdDef gameLevel;

        public override void OnEnable()
        {
            base.OnEnable();
            this.Subscribe<PlayerPosEvent>(OnPlayerPosEvent);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            this.Unsubscribe();
        }

        private void OnApplicationQuit()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonLobby.leaveRoom();
            }
            else if (PhotonNetwork.InLobby)
            {
                PhotonLobby.leaveLobby();
            }
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.NickName}");
                SceneManager.LoadScene(cancelLevel.unityName);
            }
        }

        private void OnPlayerPosEvent(PlayerPosEvent data)
        {
            Debug.Log($"onEvent {data}");
            if (data.playerPosition == startPlaying)
            {
                StartCoroutine(startTheGameplay(gameLevel.unityName));
                return;
            }
            setPlayer(PhotonNetwork.LocalPlayer, data.playerPosition);
        }

        private static IEnumerator startTheGameplay(string levelName)
        {
            Debug.Log($"startTheGameplay {levelName}");
            if (!PhotonNetwork.IsMasterClient)
            {
                throw new UnityException("only master client can start the game");
            }
            var masterPosition = PhotonNetwork.LocalPlayer.GetCustomProperty(playerPositionKey, -1);
            if (masterPosition < playerPosition0 || masterPosition > playerPosition3)
            {
                throw new UnityException("master client does not have valid player position: " + masterPosition);
            }
            // Snapshot player list before iteration because we can change it
            var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            foreach (var player in players)
            {
                var curValue = player.GetCustomProperty(playerPositionKey, -1);
                if (curValue >= playerPosition0 && curValue <= playerPosition3 || curValue == playerIsSpectator)
                {
                    continue;
                }
                Debug.Log($"KICK and CloseConnection for {player.GetDebugLabel()} {playerPositionKey}={curValue}");
                PhotonNetwork.CloseConnection(player);
                yield return null;
            }
            PhotonNetwork.LoadLevel(levelName);
        }

        private static void setPlayer(Player player, int playerPosition)
        {
            if (!player.HasCustomProperty(playerPositionKey))
            {
                Debug.Log($"setPlayer {playerPositionKey}={playerPosition}");
                player.SetCustomProperties(new Hashtable { { playerPositionKey, playerPosition } });
                return;
            }
            var curValue = player.GetCustomProperty<int>(playerPositionKey);
            Debug.Log($"setPlayer {playerPositionKey}=({curValue}<-){playerPosition}");
            player.SafeSetCustomProperty(playerPositionKey, playerPosition, curValue);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"OnDisconnected {cause}");
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
        }

        public override void OnLeftRoom() // IMatchmakingCallbacks
        {
            // Goto menu if we left (in)voluntarily any room
            // - typically master client kicked us off before starting a new game as we did not qualify to participate.
            Debug.Log($"OnLeftRoom {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
            Debug.Log($"LoadScene {cancelLevel.unityName}");
            SceneManager.LoadScene(cancelLevel.unityName);
        }

        public class PlayerPosEvent
        {
            public readonly int playerPosition;

            public PlayerPosEvent(int playerPosition)
            {
                this.playerPosition = playerPosition;
            }

            public override string ToString()
            {
                return $"{nameof(playerPosition)}: {playerPosition}";
            }
        }
    }
}