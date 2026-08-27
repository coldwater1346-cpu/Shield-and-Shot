using UnityEngine;
using Shield_Shot.GameplayCore.Weapon.Core;
using Shield_Shot.GameplayCore.Weapon.Projectile;

[CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Active Skill/Explosion Fire", fileName = "ExplosionSkillSO")]
public class ExplosionSkillSO : ActiveWeaponSkillSO
{
    [Header("Explosion Skill Settings")]
    [SerializeField] private GameObject _explosionProjectilePrefab; // 폭발 컴포넌트가 달린 투사체
    [SerializeField] private float _shootForce = 20f;

    public override void Activate(
        MonoBehaviour coroutineHost,
        IProjectileFireHandler fireHandler,
        Transform firePoint,
        Vector3 aimDirection,
        float chargeRatio,
        int level)
    {
        if (firePoint == null || _explosionProjectilePrefab == null) return;

        // 1. 투사체 생성
        GameObject projObj = Instantiate(_explosionProjectilePrefab, firePoint.position, Quaternion.LookRotation(new Vector3(aimDirection.x, 0, aimDirection.y)));

        // 2. 물리 발사 (Rigidbody가 있다면 추가)
        if (projObj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = new Vector3(aimDirection.x, 0, aimDirection.y).normalized * _shootForce;
        }

        // 3. 필요시 fireHandler를 통해 추가적인 로직(사운드/이펙트)만 호출
        // (직접 Instantiate를 했으므로 fireHandler의 기본 사격 로직은 배제한다)
    }
}