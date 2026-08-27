using Shield_Shot.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Pool
{
    public class AutoReturnToPool : MonoBehaviour
    {
        private GameObject _prefab;
        private ParticleSystem _ps;

        public void Init(GameObject prefab)
        {
            _prefab = prefab;
            _ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (_ps == null || _prefab == null) return;
            if (!_ps.IsAlive())
                PoolManager.Instance.Push(_prefab, gameObject);
        }
    }
}