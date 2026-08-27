using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.Core
{
    public class PooledReleaseScheduler<TKey, T>
    {
        private readonly List<(TKey key, T instance, float expireAtTime)> _pending = new();

        public void Schedule(TKey key, T instance, float delay)
        {
            _pending.Add((key, instance, Time.time + delay));
        }

        public void Tick(Action<TKey, T> onExpired)
        {
            if (_pending.Count == 0) return;

            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                if (Time.time < _pending[i].expireAtTime) continue;

                var (key, instance, _) = _pending[i];
                _pending.RemoveAt(i);
                onExpired(key, instance);
            }
        }

        public void CancelAll() => _pending.Clear();
    }
}