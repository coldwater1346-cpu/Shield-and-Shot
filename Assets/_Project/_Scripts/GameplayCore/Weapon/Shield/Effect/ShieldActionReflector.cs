using Shield_Shot.DataManagement.InventorySystem;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    /// <summary>
    /// [레거시] 기존 프리팹 호환용 래퍼.
    /// 새 프리팹은 BlockShield + ReflectEffect를 사용하세요.
    /// </summary>
    [System.Obsolete("ShieldActionReflector는 레거시입니다. BlockShield + ReflectEffect를 사용하세요.")]
    public class ShieldActionReflector : MonoBehaviour, IShieldAction
    {
        private BlockShield _blockShield;
        private ReflectEffect _reflectEffect;

        private void Awake()
        {
            _blockShield = GetComponent<BlockShield>()
                          ?? GetComponentInChildren<BlockShield>();
            _reflectEffect = GetComponent<ReflectEffect>()
                          ?? GetComponentInChildren<ReflectEffect>();

            if (_blockShield == null)
                Debug.LogWarning("[ShieldActionReflector] BlockShield 없음. " +
                                 "프리팹에 BlockShield + ReflectEffect를 추가하세요.");
        }

        /// <summary>ShieldDataSO 기반 주입 (레거시 경로 유지)</summary>
        public void ApplyShieldData(ShieldDataSO data)
        {
            if (data == null) return;
            _blockShield?.ApplyShieldData(data);
            Debug.Log("[ShieldActionReflector] ShieldDataSO → BlockShield로 위임.");
        }

        /// <summary>ShieldItemData 기반 주입 (레거시 경로 유지)</summary>
        public void ApplyShieldData(ShieldItemData data)
        {
            if (data == null) return;
            _blockShield?.ApplyShieldData(data);
            Debug.Log("[ShieldActionReflector] ShieldItemData → BlockShield로 위임.");
        }

        public void OnProjectileHit(ProjectileBase projectile, Vector3 hitNormal)
        {
            if (_blockShield != null)
                _blockShield.OnProjectileHit(projectile, hitNormal);
            else
                projectile.ReleaseOrDestroy(); // 폴백
        }
    }
}