using DigitalRuby.Pooling;
using UnityEngine;

namespace DigitalRuby
{
    public class PoolManager : MonoBehaviour
    {
        private static Transform parent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void beforeSceneLoad()
        {
            // We allow pooled objects to be seen, but not edited!
            SpawningPool.DefaultHideFlags = HideFlags.NotEditable;
            // Create parent for pooled objects as they are visible!
            parent = UnityExtensions.CreateGameObjectAndComponent<PoolManager>(nameof(PoolManager), isDontDestroyOnLoad: true).transform;
        }

        public static void AddPrefab(string key, GameObject prefab)
        {
            SpawningPool.AddPrefab(key, prefab);
        }

        public static GameObject CreateFromCache(string key)
        {
            var pooledObject =  SpawningPool.CreateFromCache(key);
            var pooledTransform = pooledObject.transform;
            if (pooledTransform.parent == null)
            {
                pooledTransform.parent = parent;
            }
            return pooledObject;
        }

        public static bool ReturnToCache(GameObject pooledObject, string key)
        {
            return pooledObject.ReturnToCache(key);
        }
    }
}