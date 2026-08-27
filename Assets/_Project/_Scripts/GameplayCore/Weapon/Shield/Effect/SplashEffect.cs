using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class SplashEffect : MonoBehaviour, IShieldEffect
    {
        [Header("Splash Settings")]
        [Tooltip("스플래시 반경 (m)")]
        [SerializeField] private float _splashRadius = 3f;

        [Tooltip("스플래시 데미지")]
        [SerializeField] private float _splashDamage = 20f;

        [Tooltip("몬스터 레이어")]
        [SerializeField] private LayerMask _enemyLayer;

        [Header("VFX")]
        [SerializeField] private VFXType _vfxType = VFXType.SplashShield;
        [SerializeField] private float _vfxDuration = 1.5f;

        [Header("Sound")]
        [SerializeField] private AudioClip _splashSfx;
        [SerializeField] private float _volume = 1f;

        public void OnBlock(ProjectileBase projectile, Vector3 hitPosition, Vector3 hitNormal)
        {
            // 투사체 제거
            projectile.ReleaseOrDestroy();

            // VFX
            if (_vfxType != VFXType.None && VFXPoolManager.Instance != null)
                VFXPoolManager.Instance.SpawnVFX(_vfxType, hitPosition, Quaternion.identity, _vfxDuration);

            SoundManager.Instance.PlaySFX(_splashSfx, _volume);

            // 주변 몬스터에게 데미지
            Collider[] hits = Physics.OverlapSphere(hitPosition, _splashRadius, _enemyLayer);
            int hitCount = 0;

            foreach (var hit in hits)
            {
                var monster = hit.GetComponent<MonsterBase>()
                           ?? hit.GetComponentInParent<MonsterBase>();

                if (monster == null) continue;

                monster.Health.TakeDamage(_splashDamage);

                if (DamagePopupManager.Instance != null)
                {
                    DamagePopupManager.Instance.Show(monster.transform.position, _splashDamage);
                }

                hitCount++;
            }

            Debug.Log($"[SplashEffect] 스플래시 데미지 {_splashDamage} → {hitCount}개 몬스터 적중");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.2f);
            Gizmos.DrawSphere(transform.position, _splashRadius);
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 1f);
            Gizmos.DrawWireSphere(transform.position, _splashRadius);
        }
    }
}