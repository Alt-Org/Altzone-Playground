using DigitalRuby.Pooling;
using UnityEngine;

namespace DigitalRuby
{
    public static class PoolManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void beforeSceneLoad()
        {
            // We allow pooled objects to be seen and edited as usual!
            SpawningPool.DefaultHideFlags = HideFlags.None;
            Application.quitting += () => isApplicationQuitting = true;
        }

        private static bool isApplicationQuitting;
        private static int createCount;

        public static bool ContainsPrefab(string key)
        {
            return SpawningPool.ContainsPrefab(key);
        }

        public static void AddPrefab(string key, GameObject prefab)
        {
            if (SpawningPool.ContainsPrefab(key))
            {
                throw new UnityException("prefab pool key is in use already: " + key);
            }
            SpawningPool.AddPrefab(key, prefab);
        }

        public static GameObject CreateFromCache(string key)
        {
            var pooledObject = SpawningPool.CreateFromCache(key);
            if (pooledObject.name.Contains("(Clone)"))
            {
                pooledObject.name = pooledObject.name.Replace("(Clone)", $"({++createCount})");
            }
            return pooledObject;
        }

        public static bool ReturnToCache(GameObject pooledObject, string key)
        {
            return pooledObject.ReturnToCache(key);
        }

        public static void RecycleActiveObjects()
        {
            if (isApplicationQuitting)
            {
                return;
            }
            SpawningPool.RecycleActiveObjects();
        }
    }
}