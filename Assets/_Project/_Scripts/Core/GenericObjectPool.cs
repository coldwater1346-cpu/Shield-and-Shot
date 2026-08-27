using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.Core
{
    public class GenericObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _root;
        private readonly Queue<T> _available = new();
        private readonly HashSet<T> _pooled = new(); // 중복 반환 방지

        public GenericObjectPool(T prefab, Transform root)
        {
            _prefab = prefab;
            _root = root;
        }

        public GenericObjectPool(T prefab, int initialSize, Transform root) : this(prefab, root)
        {
            Prewarm(initialSize);
        }

        public void Prewarm(int count, Transform parentOverride = null)
        {
            for (int i = 0; i < count; i++)
            {
                Return(CreateNew(), parentOverride);
            }
        }

        public T Get(Vector3 position, Quaternion rotation, Transform parentOverride = null)
        {
            T instance = Get(parentOverride);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public T Get(Transform parentOverride = null)
        {
            T instance = _available.Count > 0 ? _available.Dequeue() : CreateNew();
            _pooled.Remove(instance);

            if (parentOverride != null) instance.transform.SetParent(parentOverride);
            instance.gameObject.SetActive(true);

            if (instance is IPoolable poolable) poolable.OnSpawnedFromPool();
            return instance;
        }

        public void Return(T instance, Transform parentOverride = null)
        {
            if (instance == null || _pooled.Contains(instance)) return;

            if (instance is IPoolable poolable) poolable.OnReturnedToPool();

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(parentOverride != null ? parentOverride : _root);

            _pooled.Add(instance);
            _available.Enqueue(instance);
        }

        private T CreateNew()
        {
            T instance = Object.Instantiate(_prefab, _root);
            instance.gameObject.SetActive(false);
            return instance;
        }
    }
}