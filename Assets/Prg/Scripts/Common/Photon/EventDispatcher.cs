using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    public class EventDispatcher : MonoBehaviour, IOnEventCallback
    {
        private readonly Action<EventData>[] listeners = new Action<EventData>[200];

        private readonly RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
        };

        public static EventDispatcher Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<EventDispatcher>();
                if (_Instance == null)
                {
                    UnityExtensions.CreateGameObjectAndComponent<EventDispatcher>(nameof(EventDispatcher), isDontDestroyOnLoad: false);
                }
            }
            return _Instance;
        }

        private static EventDispatcher _Instance;

        private void Awake()
        {
            if (_Instance == null)
            {
                _Instance = this;
            }
            // https://doc.photonengine.com/en-us/pun/v2/gameplay/optimization
            // Reuse EventData to decrease garbage collection but EventData will be overwritten for every event!
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance = true;
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            if (_Instance == this)
            {
                _Instance = null;
            }
        }

        public void registerEvent(byte eventCode, Action<EventData> callback)
        {
            if (eventCode > 199 || eventCode == 0)
            {
                throw new UnityException("invalid event code " + eventCode);
            }
            if (listeners[eventCode] == null)
            {
                listeners[eventCode] = callback;
            }
            else
            {
                listeners[eventCode] += callback;
            }
        }

        public void RaiseEvent(byte eventCode, object data)
        {
            PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }

        void IOnEventCallback.OnEvent(EventData photonEvent)
        {
            // https://doc.photonengine.com/en-us/pun/current/gameplay/rpcsandraiseevent#raiseevent
            var eventCode = photonEvent.Code;
            if (eventCode > 199 || eventCode == 0)
            {
                return; // internal events
            }
            listeners[eventCode]?.Invoke(photonEvent);
        }
    }
}