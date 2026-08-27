using UnityEngine;
using Shield_Shot.Core;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class ProjectileObjectPool : MonoBehaviour
    {
        private GenericObjectPool<ProjectileBase> _pool;
        private Transform poolRoot;

        // 무기 내부가 아닌 외부 매니저(ProjectileManager)에서 호출하여 런타임에 풀을 셋업하는 진입점
        public void Initialize(ProjectileBase prefab, int size)
        {
            if (prefab == null)
            {
                Debug.LogError($"[{nameof(ProjectileObjectPool)}] Projectile prefab is missing.", this);
                enabled = false;
                return;
            }

            poolRoot = new GameObject($"[Pool] {prefab.name}").transform;
            poolRoot.SetParent(transform);
            poolRoot.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            _pool = new GenericObjectPool<ProjectileBase>(prefab, size, poolRoot);
        }

        public ProjectileBase Get(Vector3 position, Quaternion rotation)
        {
            ProjectileBase projectile = _pool.Get(position, rotation);
            projectile.SetReturnCallback(Return);
            projectile.transform.SetParent(null); // 발사 시 풀 부모 관계를 끊어 무기가 꺼져도 안 지워지게 함
            return projectile;
        }

        public void Return(ProjectileBase projectile)
        {
            if (projectile == null) return;

            _pool.Return(projectile);
            projectile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}