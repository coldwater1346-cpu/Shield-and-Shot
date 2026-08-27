using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

[CreateAssetMenu(fileName = "StandardReflectBehaviorSO", menuName = "ProjectileSystem/Behavior/StandardReflect")]
public class StandardReflectBehaviorSO : ProjectileBehaviorSO
{
    private void OnEnable()
    {
        Priority = 40;
    }

    public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
    {
        projectile.AddCollisionBehavior(new StandardReflectCollisionBehavior(currentLevel), Priority);
    }
}
