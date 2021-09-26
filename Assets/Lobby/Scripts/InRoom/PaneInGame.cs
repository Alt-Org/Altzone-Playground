using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InRoom
{
    /// <summary>
    /// Top most pane in lobby while in room to manage player position in the game.
    /// </summary>
    public class PaneInGame : MonoBehaviour
    {
        [SerializeField] private Button[] buttons;

        private static readonly int[] positionMap =
        {
            LobbyManager.playerPosition0, LobbyManager.playerPosition1, LobbyManager.playerPosition2, LobbyManager.playerPosition3,
        };

        private void Start()
        {
            for (var i = 0; i < buttons.Length; ++i)
            {
                var capturedPositionIndex = i;
                buttons[i].onClick.AddListener(() => setPlayerPosition(capturedPositionIndex));
            }
        }

        private void setPlayerPosition(int positionIndex)
        {
            Debug.Log($"setPlayerPosition {positionIndex}");
            if (positionIndex < 0 || positionIndex > 3)
            {
                throw new UnityException("invalid positionIndex: " + positionIndex);
            }
            this.Publish(new LobbyManager.Event(positionMap[positionIndex]));
        }
    }
}