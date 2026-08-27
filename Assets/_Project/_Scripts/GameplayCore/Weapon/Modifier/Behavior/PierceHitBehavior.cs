using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class PierceHitBehavior : IHitBehavior, ICopyableHitBehavior, IProjectileHitSurvivalBehavior
    {
        private int _pierceCount;

        public int PierceCount
        {
            get => _pierceCount;
            set => _pierceCount = value;
        }

        public PierceHitBehavior(int level)
        {
            _pierceCount = Mathf.Max(0, level) + 1;
        }

        public IHitBehavior CreateCopy()
        {
            PierceHitBehavior copy = new PierceHitBehavior(0);
            copy.PierceCount = _pierceCount;
            return copy;
        }

        public void OnHit(ProjectileBase projectile, Collider targetInfo)
        {
            // 1. 관통 횟수 차감
            _pierceCount--;

            // 2. 관통 횟수를 다 소모했다면 투사체 소멸
            if (_pierceCount <= 0)
            {
                projectile.ReleaseOrDestroy();
                Debug.Log("관통 횟수를 모두 소모하여 투사체가 소멸했습니다.");
            }
            else
            {
                Debug.Log($"적을 관통했습니다! 남은 관통 횟수: {_pierceCount}");
            }
        }
    }
}
