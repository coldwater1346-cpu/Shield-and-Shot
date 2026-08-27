using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    public class AliveTracker : MonoBehaviour
    {
        public int AliveCount { get; private set; }
        public event System.Action<int> CountChanged;

        private readonly HashSet<MonsterBase> _active = new();

        public void Reset()
        {
            DespawnAll();   // 재시작 시 남은 것부터 정리
        }

        public void Register(MonsterBase monster)
        {
            monster.OnReturnToPool = OnMonsterReturn;
            _active.Add(monster);
            AliveCount = _active.Count;
            CountChanged?.Invoke(AliveCount);
        }

        private void OnMonsterReturn(MonsterBase m)
        {
            _active.Remove(m);
            MonsterFactory.Instance.Return(m);   // (이름 바꿨으면 MonsterFactory)
            AliveCount = _active.Count;
            CountChanged?.Invoke(AliveCount);
        }

        /// 활성 몬스터 전부 풀로 반환(비활성). 재시작·씬 전환 시 호출.
        public void DespawnAll()
        {
            // 복사본 순회 (반환 중 _active 변경 방지)
            foreach (var m in new List<MonsterBase>(_active))
            {
                if (m == null) continue;
                m.OnReturnToPool = null;                 // 콜백 재진입 방지

                if (Shield_Shot.Core.PoolManager.Instance != null && m.SourcePrefab != null)
                    Shield_Shot.Core.PoolManager.Instance.Push(m.SourcePrefab, m.gameObject);
                else
                    m.gameObject.SetActive(false);       // 폴백
            }
            _active.Clear();
            AliveCount = 0;
            CountChanged?.Invoke(0);
        }

        // 씬 언로드/오브젝트 파괴 시 안전망
        private void OnDisable() => DespawnAll();
    }
}