using System.Collections;
using UnityEngine;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Weapon.Projectile;

namespace Shield_Shot.GameplayCore.Monster.Attack
{
    [CreateAssetMenu(menuName = "Monster/Attack/SingleShot")]
    public class SingleShot : ScriptableObject, IAttackBehavior
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private MonsterAttackPoints.AttackOrigin _origin = MonsterAttackPoints.AttackOrigin.FRONT;
        [SerializeField] private float _projectileSpeed = 10f;
        [SerializeField] private float _attackCooldown = 0.5f;
        [SerializeField] private float _attackDuration = 3f;

        [Header("Reflect Behavior")]
        [Tooltip("투사체에 주입할 반사 특성 SO (없으면 반사 없음)")]
        [SerializeField] private ProjectileBehaviorSO _reflectBehaviorSO;
        [Tooltip("반사 특성 레벨")]
        [SerializeField, Min(1)] private int _reflectLevel = 1;

        public IEnumerator AttackRoutine(MonsterBase monster)
        {
            if (_projectilePrefab == null)
            {
                Debug.LogWarning("[SingleShot] Projectile Prefab이 할당되지 않음");
                yield break;
            }

            var points = monster.AttackPoints.GetPoints(_origin);

            float elapsed = 0f;
            while (elapsed < _attackDuration)
            {
                foreach (var point in points)
                {
                    if (point == null) continue;

                    var go = Object.Instantiate(_projectilePrefab, point.position, point.rotation);
                    var projectile = go.GetComponent<ProjectileBase>();
                    if (projectile != null)
                    {
                        projectile.Velocity = point.forward * _projectileSpeed;
                        if (_reflectBehaviorSO != null)
                            _reflectBehaviorSO.InjectBehavior(projectile, _reflectLevel);
                    }
                    else
                    {
                        go.GetComponent<Rigidbody>()?.AddForce(point.forward * _projectileSpeed, ForceMode.Impulse);
                    }
                }

                elapsed += _attackCooldown;
                yield return new WaitForSeconds(_attackCooldown);
            }
        }
    }
}