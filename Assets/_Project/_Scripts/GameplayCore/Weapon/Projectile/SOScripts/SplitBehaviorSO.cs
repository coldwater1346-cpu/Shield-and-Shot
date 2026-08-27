using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

[CreateAssetMenu(fileName = "SplitBehavior", menuName = "ProjectileSystem/Behavior/Split")]
public class SplitBehaviorSO : ProjectileBehaviorSO
{
    private void OnEnable()
    {
        // 분열 특성은 무조건 충돌 연산 중 가장 최우선순위로 실행되어야 함
        Priority = 10;
    }

    public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
    {
        // 투as체 마스터에 실제 분열 행동 인스턴스를 주입
        projectile.AddCollisionBehavior(new SplitCollisionBehavior(currentLevel), Priority);
    }
}