using Shield_Shot.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Render
{
    public class PooledVfxInstance : MonoBehaviour, IPoolable
    {
        private ParticleSystem _particleSystem;
        private bool _resolved;

        private void ResolveParticleSystem()
        {
            if (_resolved) return;
            _particleSystem = GetComponent<ParticleSystem>() ?? GetComponentInChildren<ParticleSystem>();
            _resolved = true;
        }

        public void OnSpawnedFromPool()
        {
            ResolveParticleSystem();
            if (_particleSystem == null) return;

            _particleSystem.Simulate(0f, true, true);
            _particleSystem.Play(true);
        }

        public void OnReturnedToPool()
        {
            
        }
    }
}