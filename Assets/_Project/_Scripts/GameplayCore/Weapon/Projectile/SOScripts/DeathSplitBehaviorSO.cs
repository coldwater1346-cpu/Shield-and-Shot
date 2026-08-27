using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Projectile/Behavior/DeathSplit")]
    public class DeathSplitBehaviorSO : ProjectileBehaviorSO
    {
        [Header("Death Split")]
        [Tooltip("처치 시 사방으로 흩어지는 화살 수 (레벨1 기준값). 레벨업마다 1씩 증가.")]
        [SerializeField, Min(1)] private int baseChildCount = 2;

        [Tooltip("분열된 화살의 데미지 배율 (원본 대비). 0.3 = 30%")]
        [SerializeField, Range(0f, 1f)] private float damageMultiplier = 0.3f;

        private void OnEnable()
        {
            // DefaultDamageSO가 데미지를 100번 우선순위로 적용하니, 그 이후(처치 판정 확정 후) 실행되어야 함
            Priority = 150;
        }

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
        {
            int level = Mathf.Max(1, currentLevel);
            int childCount = baseChildCount + (level - 1); // Lv1=2, Lv2=3, Lv3=4 ...

            projectile.AddHitBehavior(new DeathSplitHitBehavior(childCount, damageMultiplier), Priority);
        }
    }
}