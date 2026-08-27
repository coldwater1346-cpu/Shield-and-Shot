using System.Collections;
using UnityEngine;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Weapon.Projectile;

namespace Shield_Shot.GameplayCore.Monster.Attack
{
    [CreateAssetMenu(menuName = "Monster/Attack/SpreadShot")]
    public class SpreadShot : ScriptableObject, IAttackBehavior
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private float _projectileSpeed = 10f;
        [SerializeField] private MonsterAttackPoints.AttackOrigin _origin = MonsterAttackPoints.AttackOrigin.FRONT;

        [Header("Spread")]
        [SerializeField] private float _attackDuration = 3f;
        [SerializeField, Min(1)] private int _shotCount = 5;
        [SerializeField, Min(0f)] private float _shotInterval = 0.5f;
        [SerializeField, Range(0f, 180f)] private float _spreadAngle = 60f;
        [SerializeField] private bool _rightToLeft = false;

        [Header("Reflect Behavior")]
        [Tooltip("투사체에 주입할 반사 특성 SO (없으면 반사 없음)")]
        [SerializeField] private ProjectileBehaviorSO _reflectBehaviorSO;
        [Tooltip("반사 특성 레벨")]
        [SerializeField, Min(1)] private int _reflectLevel = 1;

        public IEnumerator AttackRoutine(MonsterBase monster)
        {
            if (_projectilePrefab == null)
            {
                Debug.LogWarning("[SpreadShot] Projectile Prefab이 할당되지 않음");
                yield break;
            }

            var points = monster.AttackPoints.GetPoints(_origin);
            var wait = new WaitForSeconds(_shotInterval);

            for (int i = 0; i < _shotCount; i++)
            {
                float t = _shotCount == 1 ? 0.5f : (float)i / (_shotCount - 1);
                if (_rightToLeft) t = 1f - t;

                float angle = Mathf.Lerp(-_spreadAngle * 0.5f, _spreadAngle * 0.5f, t);

                foreach (var point in points)
                {
                    if (point == null) continue;

                    Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * point.forward;
                    var go = Object.Instantiate(_projectilePrefab, point.position, Quaternion.LookRotation(dir));
                    var projectile = go.GetComponent<ProjectileBase>();
                    if (projectile != null)
                    {
                        projectile.Velocity = dir.normalized * _projectileSpeed;
                        if (_reflectBehaviorSO != null)
                            _reflectBehaviorSO.InjectBehavior(projectile, _reflectLevel);
                    }
                    else
                    {
                        go.GetComponent<Rigidbody>()?.AddForce(dir.normalized * _projectileSpeed, ForceMode.Impulse);
                    }
                }

                if (i < _shotCount - 1)
                    yield return wait;
            }
        }
    }
}