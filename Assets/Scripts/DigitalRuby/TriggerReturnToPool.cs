using UnityEngine;

namespace DigitalRuby
{
    /// <summary>
    /// Component to return its <c>GameObject</c> to pool when given trigger collider collides with us.
    /// </summary>
    /// <remarks>
    /// Note that when <c>GameObject</c> is in pool it is inactive and this implementation relies on this!
    /// </remarks>
    public class TriggerReturnToPool : MonoBehaviour
    {
        public LayerMask collisionMask;
        private int collisionLayer;

        private void Awake()
        {
            collisionLayer = collisionMask.value;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // var hasLayer = layerMask == (layerMask | 1 << _layer); // unity3d check if layer mask contains a layer

            var hitObject = other.gameObject;
            var hitLayer = hitObject.layer;
            var colliderMask = 1 << hitLayer;
            var hasLayer = collisionLayer == (collisionLayer | colliderMask);
            if (hasLayer)
            {
                PoolManager.ReturnToCache(gameObject);
            }
        }
    }
}
