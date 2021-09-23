using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
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

        private void OnEnable()
        {
            this.Subscribe<Event>(onEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe<Event>(onEvent);
        }

        private static void onEvent(Event data)
        {
            Debug.Log($"onEvent {data}");
            if (data.playerPosition == startPlaying)
            {
                startTheGameplay();
                return;
            }
            setPlayer(PhotonNetwork.LocalPlayer, data.playerPosition);
        }

        private static void startTheGameplay()
        {
            Debug.Log("startTheGameplay");
        }

        private static void setPlayer(Player player, int playerPosition)
        {
            if (!player.HasCustomProperty(playerPositionKey))
            {
                player.SetCustomProperties(new Hashtable { { playerPositionKey, playerPosition } });
                return;
            }
            var curValue = player.GetCustomProperty<int>(playerPositionKey);
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