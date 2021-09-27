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
        public LayerMask collisionToHeadMask;
        public int collisionToHead;

        public LayerMask collisionToWallMask;
        public int collisionToWall;

        public int headCollisionCount;
        public int wallCollisionCount;

        private void Awake()
        {
            collisionToHead = collisionToHeadMask.value;
            collisionToWall = collisionToWallMask.value;
        }

        private void OnEnable()
        {
            this.Subscribe<BallMovementV2.Event>(OnBallCollision);
        }

        private void OnDisable()
        {
            this.Unsubscribe<BallMovementV2.Event>(OnBallCollision);
        }

        private void OnBallCollision(BallMovementV2.Event data)
        {
            // var hasLayer = layerMask == (layerMask | 1 << _layer); // unity3d check if layer mask contains a layer

            var colliderMask = 1 << data.colliderLayer;
            var hasLayer = collisionToHead == (collisionToHead | colliderMask);
            if (hasLayer)
            {
                headCollisionCount += 1;
                Debug.Log($"headCollisionCount={headCollisionCount} {data}");
                GestaltRing.Get().Defence = Defence.Next;
                return;
            }
            hasLayer = collisionToWall == (collisionToWall | colliderMask);
            if (hasLayer)
            {
                wallCollisionCount += 1;
                Debug.Log($"wallCollisionCount={wallCollisionCount} {data}");
            }
            Debug.Log($"collision {data}");
        }
    }
}