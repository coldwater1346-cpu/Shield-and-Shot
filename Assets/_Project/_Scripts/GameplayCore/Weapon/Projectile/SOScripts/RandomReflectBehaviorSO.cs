using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(fileName = "RandomReflectBehavior", menuName = "ProjectileSystem/Behavior/RandomReflect")]
    public class RandomReflectBehaviorSO : ProjectileBehaviorSO
    {
        private void OnEnable()
        {
            // 분열(10)보다 한참 뒤에 실행되도록 우선순위 배치
            Priority = 50;
        }

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
        {
            // 투사체 마스터에 실제 난반사 행동 인스턴스를 주입
            projectile.AddCollisionBehavior(new RandomReflectCollisionBehavior(currentLevel), Priority);
        }
    }
}