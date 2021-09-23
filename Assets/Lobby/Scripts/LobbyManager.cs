using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Lobby.Scripts
{
    public class LobbyManager : MonoBehaviour
    {
        public const int playerPosition0 = 0;
        public const int playerPosition1 = 1;
        public const int playerPosition2 = 2;
        public const int playerPosition3 = 3;
        public const int playerIsGuest = 10;
        public const int playerIsSpectator = 11;

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