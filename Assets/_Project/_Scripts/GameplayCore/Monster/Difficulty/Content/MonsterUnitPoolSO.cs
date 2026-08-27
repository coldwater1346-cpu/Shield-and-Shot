using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 등장 가능한 몬스터를 "테마 그룹(UnitGroup)" 여러 개로 나눠 담는다.
    /// 스테이지 시작 시 PickGroup 으로 그룹 하나가 뽑히고, 그 스테이지의 모든 웨이브는
    /// 선택된 그룹 안의 몬스터/보스만으로 구성된다.
    /// ※ 여기 "그룹"은 테마 묶음(UnitGroup), 스폰 대형인 MonsterGroupPlan 과는 다른 개념.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterUnitPool", menuName = "Monster/Difficulty/Monster Unit Pool")]
    public class MonsterUnitPoolSO : ScriptableObject
    {
        [System.Serializable]
        public class UnitGroup
        {
            [Tooltip("식별용 이름 (예: 언데드, 기계)")]
            public string groupName = "Group";
            [Min(0f)]
            [Tooltip("스테이지 시작 시 이 그룹이 뽑힐 가중치")]
            public float weight = 1f;
            [SerializeField] private List<MonsterUnitSO> _units = new();

            public IReadOnlyList<MonsterUnitSO> Units => _units;
            public bool HasAny => _units != null && _units.Count > 0;

            public void GetCandidates(int difficulty, int budget, List<MonsterUnitSO> outList)
            {
                outList.Clear();
                if (_units == null) return;
                foreach (var u in _units)
                    if (u != null && u.IsAvailable(difficulty, budget))
                        outList.Add(u);
            }

            public MonsterUnitSO PickBoss(int difficulty)
            {
                var buf = new List<MonsterUnitSO>();
                if (_units != null)
                    foreach (var u in _units)
                        if (u != null && u.IsBossAvailable(difficulty))
                            buf.Add(u);
                return buf.Count == 0 ? null : buf[Random.Range(0, buf.Count)];
            }
        }

        [SerializeField] private List<UnitGroup> _groups = new();
        public IReadOnlyList<UnitGroup> Groups => _groups;

        /// <summary>스테이지 시작 시 그룹 하나를 가중 랜덤으로 선택. 비어있으면 null.</summary>
        public UnitGroup PickGroup()
        {
            float total = 0f;
            foreach (var g in _groups)
                if (g != null && g.HasAny) total += Mathf.Max(0f, g.weight);

            if (total <= 0f)
            {
                foreach (var g in _groups)
                    if (g != null && g.HasAny) return g;
                return null;
            }

            float r = Random.Range(0f, total);
            float cum = 0f;
            foreach (var g in _groups)
            {
                if (g == null || !g.HasAny) continue;
                cum += Mathf.Max(0f, g.weight);
                if (r <= cum) return g;
            }
            return null;
        }

        /// <summary>프리워밍/검증용: 모든 그룹의 모든 유닛.</summary>
        public IEnumerable<MonsterUnitSO> AllUnits
        {
            get
            {
                foreach (var g in _groups)
                {
                    if (g == null) continue;
                    foreach (var u in g.Units)
                        if (u != null) yield return u;
                }
            }
        }
    }
}