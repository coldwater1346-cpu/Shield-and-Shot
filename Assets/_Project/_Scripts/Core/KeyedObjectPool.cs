using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.Core
{
    public class KeyedObjectPool<TKey, T> where T : Component
    {
        private readonly Dictionary<TKey, GenericObjectPool<T>> _pools = new();
        private readonly Dictionary<TKey, T> _prefabByKey = new();
        private readonly Transform _root;

        public KeyedObjectPool(Transform root)
        {
            _root = root;
        }

        public void Prewarm(TKey key, T prefab, int initialSize, Transform parentOverride = null)
        {
            if (prefab == null) return;

            _prefabByKey[key] = prefab;
            ResolvePool(key, prefab).Prewarm(initialSize, parentOverride);
        }

        public T Get(TKey key, Vector3 position, Quaternion rotation, Transform parentOverride = null)
        {
            GenericObjectPool<T> pool = ResolvePool(key);
            return pool?.Get(position, rotation, parentOverride);
        }

        public T Get(TKey key, Transform parentOverride = null)
        {
            return ResolvePool(key)?.Get(parentOverride);
        }

        public void Return(TKey key, T instance, Transform parentOverride = null)
        {
            if (instance == null) return;

            if (_pools.TryGetValue(key, out GenericObjectPool<T> pool))
                pool.Return(instance, parentOverride);
            else
                Object.Destroy(instance.gameObject);
        }

        public void RegisterPrefab(TKey key, T prefab) => _prefabByKey[key] = prefab;

        private GenericObjectPool<T> ResolvePool(TKey key, T prefabHint = null)
        {
            if (_pools.TryGetValue(key, out GenericObjectPool<T> pool))
                return pool;

            T prefab = prefabHint != null ? prefabHint
                : (_prefabByKey.TryGetValue(key, out T registered) ? registered : null);

            if (prefab == null)
            {
                Debug.LogWarning($"[KeyedObjectPool] 키 '{key}'에 등록된 프리팹이 없습니다.");
                return null;
            }

            pool = new GenericObjectPool<T>(prefab, _root);
            _pools[key] = pool;
            return pool;
        }
    }
}