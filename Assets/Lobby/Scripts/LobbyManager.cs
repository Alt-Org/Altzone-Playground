using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using UiProto.Scripts.Window;
using UnityEngine;

namespace Lobby.Scripts
{
    /// <summary>
    /// Manages local player position and setup in a room.
    /// </summary>
    /// <remarks>
    /// Settings are saved in player custom properties.
    /// </remarks>
    public class LobbyManager : MonoBehaviour
    {
        public const string playerPositionKey = "pp";

        public const int playerPosition0 = 0;
        public const int playerPosition1 = 1;
        public const int playerPosition2 = 2;
        public const int playerPosition3 = 3;
        public const int playerIsGuest = 10;
        public const int playerIsSpectator = 11;
        public const int startPlaying = 123;

        [SerializeField] private LevelIdDef gameLevel;

        private void OnEnable()
        {
            this.Subscribe<Event>(onEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe<Event>(onEvent);
        }

        private void onEvent(Event data)
        {
            Debug.Log($"onEvent {data}");
            if (data.playerPosition == startPlaying)
            {
                startTheGameplay(gameLevel.unityName);
                return;
            }
            setPlayer(PhotonNetwork.LocalPlayer, data.playerPosition);
        }

        private static void startTheGameplay(string levelName)
        {
            Debug.Log($"startTheGameplay {levelName}");
            PhotonNetwork.LoadLevel(levelName);
        }

        private static void setPlayer(Player player, int playerPosition)
        {
            if (!player.HasCustomProperty(playerPositionKey))
            {
                Debug.Log($"setPlayer {playerPositionKey} : {playerPosition}");
                player.SetCustomProperties(new Hashtable { { playerPositionKey, playerPosition } });
                return;
            }
            var curValue = player.GetCustomProperty<int>(playerPositionKey);
            Debug.Log($"setPlayer {playerPositionKey} : {curValue} <- {playerPosition}");
            player.SafeSetCustomProperty(playerPositionKey, playerPosition, curValue);
        }

        public class Event
        {
            public readonly int playerPosition;
            public Event(int playerPosition)
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