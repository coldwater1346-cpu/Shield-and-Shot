using Shield_Shot.GameplayCore.Monster.Status;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public sealed class FreezeHitBehavior : IHitBehavior
    {
        public const string FrozenStatusID = "Frozen";

        private readonly float _duration;

        public FreezeHitBehavior(float duration)
        {
            _duration = Mathf.Max(0f, duration);
        }

        public void OnHit(ProjectileBase projectile, Collider targetInfo)
        {
            if (targetInfo == null || _duration <= 0f)
            {
                return;
            }

            StatusEffectController statusController =
                targetInfo.GetComponentInParent<StatusEffectController>();

            if (statusController == null)
            {
                return;
            }

            StatusEffectData frozen = new StatusEffectData(
                statusID: FrozenStatusID,
                type: StatusEffectType.Frozen,
                duration: _duration,
                source: projectile,
                showDamagePopup: false
            );

            statusController.ApplyOrRefresh(frozen);
        }
    }
}
