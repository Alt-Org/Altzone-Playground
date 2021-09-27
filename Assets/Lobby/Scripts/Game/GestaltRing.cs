using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lobby.Scripts.Game
{
    public enum Defence
    {
        None = 0,
        Desensitisation = 1,
        Deflection = 2,
        Introjection = 3,
        Projection = 4,
        Retroflection = 5,
        Egotism = 6,
        Confluence = 7,
        Next = 8,
    }

    /// <summary>
    /// <c>GestaltRing</c> class manages global Gestalt <c>Defence</c> state.
    /// </summary>
    /// <remarks>
    /// When <c>Defence</c> state changes, a notification is sent through network to all players.
    /// </remarks>
    public class GestaltRing : MonoBehaviour
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 0;

        public static GestaltRing Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<GestaltRing>();
            }
            return _Instance;
        }

        private static GestaltRing _Instance;

        private static readonly Defence[] nextDefence =
        {
            Defence.None,
            Defence.Deflection,
            Defence.Introjection,
            Defence.Projection,
            Defence.Retroflection,
            Defence.Egotism,
            Defence.Confluence,
            Defence.Desensitisation,
        };

        [SerializeField] private Defence curDefence;
        private PhotonEventDispatcher photonEventDispatcher;

        public Defence Defence
        {
            get => curDefence;
            set
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    throw new UnityException($"Only Master Client can change {nameof(GestaltRing)} {nameof(Defence)} state");
                }
                byte payload;
                if (value == Defence.Next)
                {
                    payload = (byte) nextDefence[(int) curDefence];
                }
                else
                {
                    payload = (byte) value;
                }
                Debug.Log($"set Defence {(byte)curDefence} <- {payload}");
                photonEventDispatcher.RaiseEvent(photonEventCode, payload);
            }
        }

        private void Awake()
        {
            if (_Instance == null)
            {
                _Instance = this;
            }
        }

        private void Start()
        {
            // Start with some random Defence so that we have a valid state.
            curDefence = nextDefence[Random.Range(1, (int) Defence.Confluence)];
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, (data) =>
            {
                var newDefence = (Defence) Enum.ToObject(typeof(Defence), data.CustomData);
                Debug.Log($"set Defence {curDefence} <- {newDefence}");
                curDefence = newDefence;
                this.Publish(new Event(curDefence));
            });
        }

        private void OnEnable()
        {
            this.Subscribe<Event>(OnDefenceChanged);
        }

        private void OnDisable()
        {
            this.Unsubscribe<Event>(OnDefenceChanged);
        }

        private void OnDestroy()
        {
            if (_Instance == this)
            {
                _Instance = null;
            }
        }

        private void OnDefenceChanged(Event data)
        {
            Debug.Log($"changed Defence {data.Defence}");
        }

        public class Event
        {
            public readonly Defence Defence;

            public Event(Defence defence)
            {
                Defence = defence;
            }
        }
    }
}