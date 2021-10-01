using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UiProto.Scripts.Window;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Utility to close this room properly before allowing to exit to main menu!
    /// </summary>
    /// <remarks>
    /// And show scores and close room.
    /// </remarks>
    public class GameOverMenu : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private LevelIdDef mainMenu;
        [SerializeField] private float waitForGracefulExit;

        [Header("Scores"), SerializeField] private Text team0;
        [SerializeField] private Text team1;
        [SerializeField] private TeamScore[] scores;

        private bool isExiting;
        private float timeToExitForcefully;

        private void Start()
        {
            scores = new[]
            {
                new TeamScore { teamIndex = 0 },
                new TeamScore { teamIndex = 1 },
            };
            if (!PhotonNetwork.InRoom)
            {
                return;
            }
            var room = PhotonNetwork.CurrentRoom;
            foreach (var score in scores)
            {
                var key = $"T{score.teamIndex}";
                var value = room.GetCustomProperty<int>(key, -1);
                if (value > 0)
                {
                    score.wallCollisionCount = value;
                }
            }
            team0.text = scores[0].wallCollisionCount > 0 ? $"Team {scores[0].teamIndex}\r\n{scores[0].wallCollisionCount}" : "No\r\nscore";
            team1.text = scores[1].wallCollisionCount > 0 ? $"Team {scores[1].teamIndex}\r\n{scores[1].wallCollisionCount}" : "No\r\nscore";
            PhotonLobby.leaveRoom();
        }

        private void Update()
        {
            if (isExiting)
            {
                GotoMenu();
            }
            else if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.NickName}");
                GotoMenu();
            }
        }

        private void GotoMenu()
        {
            // This is not perfect but will do!
            if (PhotonNetwork.InRoom)
            {
                if (isExiting)
                {
                    if (Time.time > timeToExitForcefully)
                    {
                        PhotonNetwork.Disconnect();
                    }
                }
                else
                {
                    isExiting = true;
                    timeToExitForcefully = Time.time + waitForGracefulExit;
                    PhotonLobby.leaveRoom();
                    return;
                }
            }
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected || state == ClientState.ConnectedToMasterServer)
            {
                Debug.Log($"LoadScene {PhotonNetwork.NetworkClientState} {mainMenu.unityName}");
                SceneManager.LoadScene(mainMenu.unityName);
                enabled = false;
            }
        }
    }
}