using Shield_Shot.GameplayCore.Monster.Status;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;
using Shield_Shot.GameplayCore.Render;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class PoisonHitBehavior : IHitBehavior
    {
        private const string PoisonStatusID = "Poison";

        private readonly float _duration;
        private readonly float _tickInterval;
        private readonly float _damagePerTick;

        private readonly bool _showDamagePopup;
        private readonly VFXType _tickVfxType;
        private readonly float _vfxAutoReleaseTime;

        public PoisonHitBehavior(
            float duration,
            float tickInterval,
            float damagePerTick,
            bool showDamagePopup,
VFXType tickVfxType,
float vfxAutoReleaseTime)
        {
            _duration = Mathf.Max(0f, duration);
            _tickInterval = Mathf.Max(0f, tickInterval);
            _damagePerTick = Mathf.Max(0f, damagePerTick);
            _showDamagePopup = showDamagePopup;
            _tickVfxType = tickVfxType;
            _vfxAutoReleaseTime = Mathf.Max(0f, vfxAutoReleaseTime);
        }

        public void OnHit(ProjectileBase projectile, Collider targetInfo)
        {
            if (targetInfo == null)
            {
                return;
            }

            StatusEffectController statusController =
                targetInfo.GetComponentInParent<StatusEffectController>();

            if (statusController == null)
            {
                Debug.LogWarning($"[PoisonHitBehavior] StatusEffectController가 없습니다. Target: {targetInfo.name}");
                return;
            }

            StatusEffectData poison = new StatusEffectData(
                statusID: PoisonStatusID,
                type: StatusEffectType.Poison,
                duration: _duration,
                tickInterval: _tickInterval,
                damagePerTick: _damagePerTick,
                source: projectile,
                showDamagePopup: _showDamagePopup,
tickVfxType: _tickVfxType,
vfxAutoReleaseTime: _vfxAutoReleaseTime
            );

            statusController.ApplyOrRefresh(poison);
        }
    }
}