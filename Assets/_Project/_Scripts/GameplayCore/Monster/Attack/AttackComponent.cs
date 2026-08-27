using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Wave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Attack
{
    public class AttackComponent : MonoBehaviour
    {
        private List<BossAttackRule> _patterns = new();
        private MonsterBase _monster;

        public bool IsReady { get; private set; } = true;

        private void Awake() => _monster = GetComponent<MonsterBase>();

        public void SetPatterns(List<BossAttackRule> patterns) => _patterns = patterns;

        public void TryExecuteRandom()
        {
            if (!IsReady || _patterns.Count == 0)
            {
                Debug.LogWarning("[AttackComponent] 패턴 없음 또는 발사 중");
                return;
            }

            var rule = _patterns[Random.Range(0, _patterns.Count)];
            var pattern = rule.attackPattern as IAttackBehavior;
            if (pattern == null)
            {
                Debug.LogWarning($"[AttackComponent] IAttackBehavior 미구현: {rule.attackPattern}");
                return;
            }

            StartCoroutine(Execute(pattern));
        }

        private IEnumerator Execute(IAttackBehavior pattern)
        {
            IsReady = false;
            yield return StartCoroutine(pattern.AttackRoutine(_monster));
            IsReady = true;
        }
    }
}