using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.Core.SceneFlow
{
    public sealed class SceneTransitionData
    {
        public SceneType FromScene { get; }
        public SceneType ToScene { get; }
        public SceneTransitionReason Reason { get; }
        public DateTime CreatedAt { get; }

        private readonly Dictionary<string, object> _payloads = new();

        public SceneTransitionData(
    SceneType fromScene,
    SceneType toScene,
    SceneTransitionReason reason)
        {
            FromScene = fromScene;
            ToScene = toScene;
            Reason = reason;
            CreatedAt = DateTime.UtcNow;
        }
        public void Set<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            _payloads[key] = value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            if (!_payloads.TryGetValue(key, out object rawValue))
            {
                return false;
            }

            if (rawValue is not T typedValue)
            {
                return false;
            }

            value = typedValue;
            return true;
        }
        public bool Has(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && _payloads.ContainsKey(key);
        }
    }
}
