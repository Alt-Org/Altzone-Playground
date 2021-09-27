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
    /// When <c>Defence</c> state changes, a notification is sent.
    /// </remarks>
    public class GestaltRing : MonoBehaviour
    {
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

        public Defence Defence
        {
            get => curDefence;
            set
            {
                if (value <= Defence.None)
                {
                    throw new UnityException("invalid Defence state: " + value);
                }
                curDefence = value;
                this.Publish(new Event(curDefence));
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
            Debug.Log($"set Defence {data.Defence}");
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