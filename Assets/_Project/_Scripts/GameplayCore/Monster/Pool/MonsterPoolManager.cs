using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.GameplayCore.Monster.Stage;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Pool
{
    public class MonsterPoolManager : MonoBehaviour
    {
        public static MonsterPoolManager Instance { get; private set; }
        // 프리펩 → 대기 중인 몬스터 큐
        private readonly Dictionary<GameObject, Queue<MonsterBase>> _pool = new();
        // 몬스터 → 원본 프리펩 (반환 시 사용)
        private readonly Dictionary<MonsterBase, GameObject> _prefabMap = new();

        private Transform _poolRoot;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(StageSO stage)
        {
            _poolRoot = new GameObject("[MonsterPool]").transform;

            var needed = new Dictionary<GameObject, int>();

            foreach (var wave in stage.Waves)
            {
                var prefabsInWave = new HashSet<GameObject>();
                foreach (var group in wave.SpawnGroups)
                    if (group.MonsterPrefab != null)
                        prefabsInWave.Add(group.MonsterPrefab);

                foreach (var prefab in prefabsInWave)
                {
                    if (!needed.ContainsKey(prefab)) needed[prefab] = 0;
                    needed[prefab] = Mathf.Max(needed[prefab], wave.TotalSpawnCount);
                }

                // ── 보스 추가 ──────────────────────────────
                if (wave.IsBossWave && wave.BossGroup.MonsterPrefab != null)
                {
                    var bossPrefab = wave.BossGroup.MonsterPrefab;
                    if (!needed.ContainsKey(bossPrefab)) needed[bossPrefab] = 0;
                    needed[bossPrefab] = Mathf.Max(needed[bossPrefab], 1);
                }
            }

            foreach (var (prefab, count) in needed)
            {
                var queue = new Queue<MonsterBase>();
                for (int i = 0; i < count; i++)
                    queue.Enqueue(CreateNew(prefab));
                _pool[prefab] = queue;
                Debug.Log($"[Pool] {prefab.name} x{count} 준비 완료");
            }
        }

        public MonsterBase Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            MonsterBase monster;

            if (_pool.TryGetValue(prefab, out var queue) && queue.Count > 0)
            {
                monster = queue.Dequeue();
            }
            else
            {
                Debug.LogWarning($"[Pool] {prefab.name} 풀 부족 → 즉시 생성");
                monster = CreateNew(prefab);
                if (!_pool.ContainsKey(prefab))
                    _pool[prefab] = new Queue<MonsterBase>();
            }

            monster.transform.SetPositionAndRotation(position, rotation);
            monster.gameObject.SetActive(true);
            monster.SourcePrefab = prefab;
            monster.OnReturnToPool = Return;   // 죽으면 자동 반환
            return monster;
        }

        public void Return(MonsterBase monster)
        {
            monster.GetComponent<AnimComponent>()?.ResetAnim();

            monster.gameObject.SetActive(false);

            if (_prefabMap.TryGetValue(monster, out var prefab) &&
                _pool.TryGetValue(prefab, out var queue))
                queue.Enqueue(monster);
        }

        private MonsterBase CreateNew(GameObject prefab)
        {
            var go = Instantiate(prefab, _poolRoot);
            var monster = go.GetComponent<MonsterBase>();
            go.SetActive(false);
            _prefabMap[monster] = prefab;
            return monster;
        }


        // 난이도 구조용 풀 프리워밍 (MonsterUnitPoolSO)
        public void Prewarm(MonsterUnitPoolSO pool, int perPrefab = 5)
        {
            if (pool == null) return;
            if (_poolRoot == null) _poolRoot = new GameObject("[MonsterPool]").transform;

            void Warm(GameObject prefab, int count)
            {
                if (prefab == null) return;
                if (!_pool.TryGetValue(prefab, out var q)) { q = new Queue<MonsterBase>(); _pool[prefab] = q; }
                for (int i = 0; i < count; i++) q.Enqueue(CreateNew(prefab));
            }

            foreach (var u in pool.AllUnits)
            {
                if (u == null || u.Prefab == null) continue;
                Warm(u.Prefab, u.IsBoss ? 1 : perPrefab);
            }
        }

    }

}