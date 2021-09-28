using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using System;
using UnityEngine;

namespace Lobby.Scripts.Game
{
    /// <summary>
    /// More game manager example functionality.
    /// </summary>
    public class GameManagerExample2 : MonoBehaviour
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 1;

        public LayerMask collisionToHeadMask;
        public int collisionToHead;

        public LayerMask collisionToWallMask;
        public int collisionToWall;

        public int headCollisionCount;
        public int wallCollisionCount;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            Debug.Log($"Awake: {PhotonNetwork.NetworkClientState}");
            collisionToHead = collisionToHeadMask.value;
            collisionToWall = collisionToWallMask.value;
            gameObject.SetActive(false); // Wait until we are signalled to go by setting us active again
        }

        private void Start()
        {
            Debug.Log($"Start: {PhotonNetwork.NetworkClientState}");
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, (data) =>
            {
                var payload = (byte[]) data.CustomData;

                Debug.Log($"Synchronize head:{headCollisionCount}<-{payload[0]} wall:{wallCollisionCount}<-{payload[1]}");
                headCollisionCount = payload[0];
                wallCollisionCount = payload[1];

                this.Publish(new Event(headCollisionCount, wallCollisionCount));
            });
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable: {PhotonNetwork.NetworkClientState}");
            this.Subscribe<BallMovementV2.Event>(OnBallCollision);
        }

        private void OnDisable()
        {
            this.Unsubscribe<BallMovementV2.Event>(OnBallCollision);
        }

        private void OnBallCollision(BallMovementV2.Event data)
        {
            // var hasLayer = layerMask == (layerMask | 1 << _layer); // unity3d check if layer mask contains a layer

            var _headCollisionCount = 0;
            var _wallCollisionCount = 0;
            var colliderMask = 1 << data.colliderLayer;
            var hasLayer = collisionToHead == (collisionToHead | colliderMask);
            if (hasLayer)
            {
                _headCollisionCount += 1;
            }
            hasLayer = collisionToWall == (collisionToWall | colliderMask);
            if (hasLayer)
            {
                _wallCollisionCount += 1;
            }
            if (_headCollisionCount == 0 && _wallCollisionCount == 0)
            {
                return; // Nothing we care about
            }
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"OnBallCollision {headCollisionCount}<-{_headCollisionCount} {wallCollisionCount}<-{_wallCollisionCount}");
                if (_headCollisionCount > 0)
                {
                    GestaltRing.Get().Defence = Defence.Next;
                }
                headCollisionCount += _headCollisionCount;
                wallCollisionCount += _wallCollisionCount;
                // Synchronize all game managers
                var payload = new[] { (byte) headCollisionCount, (byte) wallCollisionCount };
                photonEventDispatcher.RaiseEvent(photonEventCode, payload);
            }
        }

        public class Event
        {
            public readonly int headCollisionCount;
            public readonly int wallCollisionCount;

            public Event(int headCollisionCount, int wallCollisionCount)
            {
                this.headCollisionCount = headCollisionCount;
                this.wallCollisionCount = wallCollisionCount;
            }

            public override string ToString()
            {
                return $"{nameof(headCollisionCount)}: {headCollisionCount}, {nameof(wallCollisionCount)}: {wallCollisionCount}";
            }
        }
    }
}