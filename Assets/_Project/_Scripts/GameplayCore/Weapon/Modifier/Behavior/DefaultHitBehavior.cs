using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Common;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class DefaultHitBehavior : IHitBehavior, ICopyableHitBehavior
    {
        public float Damage { get; set; }
        private readonly AudioClip _hitSfx;
        private readonly float _volume;

        public DefaultHitBehavior(float damage, AudioClip hitSfx = null, float volume = 1f)
        {
            Damage = damage;
            _hitSfx = hitSfx;
            _volume = Mathf.Clamp01(volume);
        }

        public IHitBehavior CreateCopy()
        {
            return new DefaultHitBehavior(Damage, _hitSfx, _volume);
        }

        public void OnHit(ProjectileBase projectile, Collider targetInfo)
        {
            if (targetInfo.TryGetComponent(out ITakeDamage damageable))
            {
                Debug.Log($"[DefaultHitBehavior] {targetInfo.name}에게 {Damage} 데미지 적용 시도.");
                damageable.TakeDamage(Damage);
            }
            else
            {
                Debug.LogWarning($"[DefaultHitBehavior] {targetInfo.name}에 ITakeDamage 컴포넌트가 없음! 데미지 미적용.");
            }

            bool isCriticalHit = projectile.IsCritical;
            if (DamagePopupManager.Instance != null)
                DamagePopupManager.Instance.Show(targetInfo.transform.position, Damage, isCriticalHit);

            if (_hitSfx != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(_hitSfx, _volume);
            }
        }
    }
}