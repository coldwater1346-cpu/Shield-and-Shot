using System.Collections.Generic;
using Shield_Shot.InputSystem.Data; // WeaponType 위치에 맞게 네임스페이스 확인 필요
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class ProjectileManager : MonoBehaviour
    {
        public static ProjectileManager Instance { get; private set; }

        [System.Serializable]
        public struct PoolSetup
        {
            public WeaponType weaponType;
            public ProjectileBase projectilePrefab;
            public int initialPoolSize;
        }

        [Header("Projectile Pool Configurations")]
        [SerializeField] private List<PoolSetup> poolSetups = new List<PoolSetup>();

        // 런타임에 WeaponType별 풀을 관리할 딕셔너리
        private readonly Dictionary<WeaponType, ProjectileObjectPool> _poolDictionary = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 씬 전환 시 파괴 방지가 필요하다면 활성화 (선택 사항)
            // DontDestroyOnLoad(gameObject);

            GeneratePools();
        }

        private void GeneratePools()
        {
            foreach (var setup in poolSetups)
            {
                if (setup.projectilePrefab == null)
                {
                    Debug.LogWarning($"[ProjectileManager] {setup.weaponType}의 프리팹이 비어있다.");
                    continue;
                }

                if (_poolDictionary.ContainsKey(setup.weaponType))
                {
                    Debug.LogWarning($"[ProjectileManager] {setup.weaponType} 풀이 이미 중복 등록되어 있다.");
                    continue;
                }

                // 매니저 하위에 개별 풀용 게임오브젝트 동적 생성
                GameObject poolObj = new GameObject($"Pool_{setup.weaponType}");
                poolObj.transform.SetParent(transform);

                ProjectileObjectPool poolComponent = poolObj.AddComponent<ProjectileObjectPool>();
                poolComponent.Initialize(setup.projectilePrefab, setup.initialPoolSize);

                _poolDictionary.Add(setup.weaponType, poolComponent);
            }
        }

        // 무기 스크립트에서 호출할 투사체 대여 함수
        public ProjectileBase GetProjectile(WeaponType type, Vector3 position, Quaternion rotation)
        {
            if (_poolDictionary.TryGetValue(type, out var pool))
            {
                return pool.Get(position, rotation);
            }

            Debug.LogError($"[ProjectileManager] {type} 타입에 매핑된 오브젝트 풀이 존재하지 않는다!");
            return null;
        }

        public bool TryGetPool(WeaponType type, out ProjectileObjectPool pool)
        {
            return _poolDictionary.TryGetValue(type, out pool);
        }

        public bool TryGetProjectileCollisionRadius(WeaponType type, out float radius)
        {
            for (int i = 0; i < poolSetups.Count; i++)
            {
                PoolSetup setup = poolSetups[i];

                if (setup.weaponType != type || setup.projectilePrefab == null)
                {
                    continue;
                }

                radius = setup.projectilePrefab.ProjectileRadius;
                return radius > 0f;
            }

            radius = 0f;
            return false;
        }
    }
}
