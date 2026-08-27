using Shield_Shot.Core;                               // PoolManager
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Difficulty;    // MonsterUnitPoolSO
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Pool
{
    /// 몬스터 인스턴스 공급자. 공용 PoolManager에서 꺼내 몬스터용 세팅(SourcePrefab·반환콜백·리셋)을
    /// 입혀 내주고, 회수한다. 실제 풀링은 PoolManager에 위임.
    public class MonsterFactory : MonoBehaviour
    {
        public static MonsterFactory Instance { get; private set; }

        [Tooltip("풀링된 몬스터를 담을 부모(선택). 비우면 PoolManager 기본 부모.")]
        [SerializeField] private Transform _monsterRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── 꺼내기 ────────────────────────────────────────
        public MonsterBase Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject go = PoolManager.Instance.Pop(prefab, _monsterRoot);
            if (go == null)
            {
                Debug.LogError($"[MonsterPool] Pop 실패: {prefab?.name}");
                return null;
            }

            go.transform.SetPositionAndRotation(position, rotation);
            if (!go.activeSelf) go.SetActive(true);

            var monster = go.GetComponent<MonsterBase>();
            monster.SourcePrefab = prefab;        // 반환 시 키로 사용
            monster.OnReturnToPool = Return;      // 기본 반환 경로(AliveTracker가 덮어씀)
            return monster;
        }

        // ── 되돌리기 ──────────────────────────────────────
        public void Return(MonsterBase monster)
        {
            if (monster == null) return;

            // 공용 PoolManager에 리셋 훅이 없으므로 몬스터 리셋은 어댑터가 처리
            monster.GetComponent<AnimComponent>()?.ResetAnim();

            PoolManager.Instance.Push(monster.SourcePrefab, monster.gameObject, _monsterRoot);
        }

        // ── 프리워밍 (난이도 구조) ────────────────────────
        public void Prewarm(MonsterUnitPoolSO pool, int perPrefab = 5)
        {
            if (pool == null) return;

            foreach (var u in pool.AllUnits)
            {
                if (u == null || u.Prefab == null) continue;
                PoolManager.Instance.CreatePool(u.Prefab, u.IsBoss ? 1 : perPrefab, _monsterRoot);
            }
        }
    }
}