using UnityEngine;

namespace Shield_Shot.Core
{
    public class PoolManager : PersistentSingleton<PoolManager>
    {
        private KeyedObjectPool<GameObject, Transform> _pool;

        private KeyedObjectPool<GameObject, Transform> Pool
        {
            get
            {
                _pool ??= new KeyedObjectPool<GameObject, Transform>(transform);
                return _pool;
            }
        }

        public void CreatePool(GameObject prefab, int poolSize, Transform parent = null)
        {
            if (prefab == null) return;
            Pool.Prewarm(prefab, prefab.transform, poolSize, parent);
        }

        public GameObject Pop(GameObject prefab, Transform parent = null)
        {
            if (prefab == null) return null;

            Pool.RegisterPrefab(prefab, prefab.transform);
            Transform instance = Pool.Get(prefab, parent);
            return instance != null ? instance.gameObject : null;
        }

        public void Push(GameObject prefab, GameObject obj, Transform parent = null)
        {
            if (prefab == null || obj == null) return;
            Pool.Return(prefab, obj.transform, parent);
        }
    }
}