using UnityEngine;

namespace DigitalRuby
{
    /// <summary>
    /// Component to return its <c>GameObject</c> to pool when timer expires.
    /// </summary>
    /// <remarks>
    /// Note that when <c>GameObject</c> is in pool it is inactive and this implementation relies on this!
    /// </remarks>
    public class TimedReturnToPool : MonoBehaviour
    {
        /// <summary>
        /// Timer count down initial value.
        /// </summary>
        [SerializeField] private float timeToLive;

        /// <summary>
        /// Timer expiration time.
        /// </summary>
        [SerializeField] private float expirationTime;

        /// <summary>
        /// Time to live setter to control the "lifetime" of this <c>GameObject</c> until it is returned to the pool.
        /// </summary>
        public float TimeToLive
        {
            get => timeToLive;
            set
            {
                timeToLive = value;
                expirationTime = Time.time + value;
            }
        }

        private void Update()
        {
            if (Time.time > expirationTime)
            {
                if (timeToLive > 0)
                {
                    timeToLive = 0;
                    PoolManager.ReturnToCache(gameObject);
                }
            }
        }
    }
}