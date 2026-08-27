using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Core
{
    [CreateAssetMenu(fileName = "MonsterDataSO", menuName = "Monster/MonsterDataSO")]
    public class MonsterDataSO : ScriptableObject
    {
        [Header("Prefab")]
        [SerializeField] private GameObject _monsterPrefab; // 프리팹 추가

        [Header("Stats")]
        [SerializeField] private float _maxHP;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _attackDamage;

        // 프로퍼티 노출
        public GameObject MonsterPrefab => _monsterPrefab;
        public float MaxHP => _maxHP;
        public float MoveSpeed => _moveSpeed;
        public float AttackDamage => _attackDamage;

        /// <summary>
        /// 뒤끝 서버에서 파싱한 데이터를 SO에 주입해주는 초기화 메서드
        /// </summary>
        public void Initialize(GameObject prefab, float maxHP, float moveSpeed, float attackDamage)
        {
            _monsterPrefab = prefab;
            _maxHP = maxHP;
            _moveSpeed = moveSpeed;
            _attackDamage = attackDamage;
        }
    }
}