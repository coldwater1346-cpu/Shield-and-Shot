using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(fileName = "PierceBehavior", menuName = "ProjectileSystem/Behavior/Pierce")]
    public class PierceBehaviorSO : ProjectileBehaviorSO
    {
        private void OnEnable()
        {
            Priority = 90;
        }

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
        {
            projectile.AddHitBehavior(new PierceHitBehavior(currentLevel), Priority);
        }
    }
}
