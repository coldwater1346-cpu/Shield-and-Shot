using System.Collections.Generic;
using Shield_Shot.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shield_Shot.GameplayCore.Render
{
    public enum VFXType
    {
        None, Hit, Reflect, Block, FieldClear, Charging, ChainLightning, FullCharging,
        MuzzleFlash, MonsterDeath, MonsterTrait, KnockbackShield, SplashShield,
        Invincibility, SlowField, FireField, WindField
    }

    public class VFXPoolManager : MonoBehaviour
    {
        public static VFXPoolManager Instance { get; private set; }

        private const string ResourcesPrefabPath = "Prefabs/VFXPoolManager";

        [Header("Pool Setup")]
        [Tooltip("인스펙터에서 각 Enum 키에 매핑할 프리팹과 초기 생성 개수를 세팅합니다.")]
        [SerializeField] private List<VFXPoolSetup> _prewarmList = new List<VFXPoolSetup>();

        private KeyedObjectPool<VFXType, PooledVfxInstance> _pool;
        private readonly PooledReleaseScheduler<VFXType, PooledVfxInstance> _releaseScheduler = new();
        private readonly Dictionary<PooledVfxInstance, VFXType> _activeInstances = new();

        public static VFXPoolManager EnsureInstance()
        {
            if (Instance != null) return Instance;

            GameObject prefab = Resources.Load<GameObject>(ResourcesPrefabPath);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                instance.name = prefab.name;
                return instance.GetComponent<VFXPoolManager>();
            }

            Debug.LogWarning($"[VFXPoolManager] Resources/{ResourcesPrefabPath} prefab is missing.");
            return null;
        }

        [System.Serializable]
        public struct VFXPoolSetup
        {
            public VFXType vfxType;
            public GameObject prefab;
            public int size;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                _pool = new KeyedObjectPool<VFXType, PooledVfxInstance>(transform);
                SceneManager.sceneUnloaded += HandleSceneUnloaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            if (_activeInstances.Count == 0) return;

            List<(VFXType type, PooledVfxInstance instance)> leftovers = new(_activeInstances.Count);
            foreach (var kvp in _activeInstances)
                leftovers.Add((kvp.Value, kvp.Key));

            foreach (var (type, instance) in leftovers)
                ReturnToPool(type, instance);
        }

        private void Start()
        {
            foreach (var setup in _prewarmList)
            {
                if (setup.prefab == null || setup.vfxType == VFXType.None) continue;

                // VFX 프리팹 루트에 PooledVfxInstance가 없으면 여기서 자동으로 붙여준다.
                // (프리팹 에셋마다 손으로 컴포넌트를 추가할 필요가 없다. 런타임에 딱 한 번,
                //  인스펙터에 연결된 원본 프리팹 참조에 컴포넌트를 추가하는 것뿐이라 .prefab 파일에는
                //  저장되지 않는다 — 원치 않으면 아래 대신 프리팹에 직접 미리 붙여두고 이 블록은 지워도 된다.)
                PooledVfxInstance prefabComponent = setup.prefab.GetComponent<PooledVfxInstance>();
                if (prefabComponent == null)
                    prefabComponent = setup.prefab.AddComponent<PooledVfxInstance>();

                _pool.Prewarm(setup.vfxType, prefabComponent, setup.size);
            }
        }

        public GameObject SpawnVFX(VFXType type, Vector3 position, Quaternion rotation, float autoReleaseTime = 0f)
        {
            if (type == VFXType.None) return null;

            PooledVfxInstance instance = _pool.Get(type, position, rotation);
            if (instance == null)
            {
                Debug.LogWarning($"[VFXPoolManager] ⚠️ {type} 타입의 풀이 존재하지 않거나 준비되지 않았습니다.");
                return null;
            }

            _activeInstances[instance] = type;

            if (autoReleaseTime > 0f)
            {
                _releaseScheduler.Schedule(type, instance, autoReleaseTime);
            }

            return instance.gameObject;
        }

        private void Update()
        {
            _releaseScheduler.Tick(ReturnToPool);
        }

        public void ReturnToPool(VFXType type, GameObject obj)
        {
            if (obj == null) return;
            ReturnToPool(type, obj.GetComponent<PooledVfxInstance>());
        }

        private void ReturnToPool(VFXType type, PooledVfxInstance instance)
        {
            if (instance == null) return;

            _activeInstances.Remove(instance);

            if (!instance.gameObject.activeSelf) return;
            _pool.Return(type, instance);
        }
    }
}