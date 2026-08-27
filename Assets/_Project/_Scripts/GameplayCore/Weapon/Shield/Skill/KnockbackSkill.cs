using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class KnockbackSkill : ShieldSkillBase
    {
        [Header("Knockback Settings")]
        [Tooltip("넉백 탐지 반경 (m)")]
        [SerializeField] private float _radius = 10f;

        [Tooltip("넉백 속도 (m/s)")]
        [SerializeField] private float _knockbackSpeed = 20f;

        [Tooltip("넉백 지속 시간 (초)")]
        [SerializeField] private float _knockbackDuration = 0.4f;

        [Tooltip("Enemy 레이어 마스크")]
        [SerializeField] private LayerMask _enemyLayer;

        [Header("VFX")]
        [Tooltip("스킬 발동 VFX 타입")]
        [SerializeField] private VFXType _vfxType = VFXType.KnockbackShield;

        [Tooltip("VFX 자동 반환 시간 (초)")]
        [SerializeField] private float _vfxDuration = 2f;

        [Header("Sound")]
        [SerializeField] private AudioClip _konckbackClip;
        [SerializeField] private float _volume = 1f;

        public override void Activate()
        {
            Vector3 origin = transform.position;

            // 스킬 VFX 재생
            if (_vfxType != VFXType.None && VFXPoolManager.Instance != null)
                VFXPoolManager.Instance.SpawnVFX(_vfxType, origin, Quaternion.Euler(90f, 0f, 0f), _vfxDuration);

            SoundManager.Instance.PlaySFX(_konckbackClip, _volume);

            // 반경 내 Enemy 레이어 오브젝트 탐색
            Collider[] hits = Physics.OverlapSphere(origin, _radius, _enemyLayer);
            int knockedCount = 0;

            foreach (var hit in hits)
            {
                MonsterBase monster = hit.GetComponent<MonsterBase>()
                                   ?? hit.GetComponentInParent<MonsterBase>();

                if (monster == null || monster.Movement == null) continue;

                // 방패 중심 → 몬스터 방향으로 넉백
                Vector3 dir = (hit.transform.position - origin).normalized;
                dir.y = 0f; // 수평 방향만

                monster.Movement.ApplyKnockback(dir * _knockbackSpeed, _knockbackDuration);
                knockedCount++;
            }

            Debug.Log($"[KnockbackSkill] 넉백 발동. 반경={_radius}m, 대상={knockedCount}개");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, _radius);
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}