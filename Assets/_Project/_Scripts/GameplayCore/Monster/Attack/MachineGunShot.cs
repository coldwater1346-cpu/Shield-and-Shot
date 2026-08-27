using System.Collections;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Attack
{
    [CreateAssetMenu(menuName = "Monster/Attack/MachineGunShot")]
    public class MachineGunShot : ScriptableObject, IAttackBehavior
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private float _projectileSpeed = 15f;
        [SerializeField] private MonsterAttackPoints.AttackOrigin _origin = MonsterAttackPoints.AttackOrigin.FRONT;

        [Header("Machine Gun")]
        [Tooltip("총 발사 횟수")]
        [SerializeField, Min(1)] private int _shotCount = 10;

        [Tooltip("발사 간격 (초)")]
        [SerializeField, Min(0f)] private float _shotInterval = 0.08f;

        [Tooltip("중심에서 좌우 최소 퍼짐 각도")]
        [SerializeField, Range(0f, 90f)] private float _minAngle = 30f;

        [Tooltip("중심에서 좌우 최대 퍼짐 각도")]
        [SerializeField, Range(0f, 90f)] private float _maxAngle = 60f;

        [Tooltip("전체 공격 지속 시간 (발사 완료 후 남은 시간 대기)")]
        [SerializeField, Min(0f)] private float _attackDuration = 2f;

        [Header("Reflect Behavior")]
        [Tooltip("투사체에 주입할 반사 특성 SO (없으면 반사 없음)")]
        [SerializeField] private ProjectileBehaviorSO _reflectBehaviorSO;

        [Tooltip("반사 특성 레벨")]
        [SerializeField, Min(1)] private int _reflectLevel = 1;

        public IEnumerator AttackRoutine(MonsterBase monster)
        {
            if (_projectilePrefab == null)
            {
                Debug.LogWarning("[MachineGunShot] Projectile Prefab이 할당되지 않음");
                yield break;
            }

            var points = monster.AttackPoints.GetPoints(_origin);
            var wait = new WaitForSeconds(_shotInterval);
            float startTime = Time.time;

            for (int i = 0; i < _shotCount; i++)
            {
                // 매 발마다 랜덤 각도 (부호도 랜덤 → 좌/우 랜덤)
                float magnitude = Random.Range(_minAngle, _maxAngle);
                float angle = magnitude * (Random.value < 0.5f ? -1f : 1f);

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
                        {
                            _reflectBehaviorSO.InjectBehavior(projectile, _reflectLevel);
                        }
                    }
                    else
                    {
                        go.GetComponent<Rigidbody>()?.AddForce(dir.normalized * _projectileSpeed, ForceMode.Impulse);
                    }
                }

                if (i < _shotCount - 1)
                    yield return wait;
            }

            float remaining = _attackDuration - (Time.time - startTime);
            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);
        }
    }
}