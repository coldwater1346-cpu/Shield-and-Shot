using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    /// <summary>
    /// [레거시] 기존 프리팹 호환용 래퍼.
    /// 새 프리팹은 BlockShield를 직접 사용하세요.
    /// </summary>
    [System.Obsolete("ShieldActionBlocker는 레거시입니다. BlockShield를 사용하세요.")]
    public class ShieldActionBlocker : ShieldBase, IShieldAction
    {
        private BlockShield _blockShield;

        protected override void Awake()
        {
            base.Awake();
            _blockShield = GetComponent<BlockShield>()
                        ?? GetComponentInChildren<BlockShield>();

            if (_blockShield == null)
                Debug.LogWarning("[ShieldActionBlocker] BlockShield 없음. " +
                                 "프리팹에 BlockShield 컴포넌트를 추가하세요.");
        }

        public void OnProjectileHit(ProjectileBase projectile, Vector3 hitNormal)
        {
            if (_blockShield != null)
                _blockShield.OnProjectileHit(projectile, hitNormal);
            else
                projectile.ReleaseOrDestroy(); // 폴백
        }

        protected override void OnProjectileHit_Internal(ProjectileBase projectile, Vector3 hitNormal)
            => OnProjectileHit(projectile, hitNormal);
    }
}