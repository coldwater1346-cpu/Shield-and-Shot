using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    // 투사체에 적용 가능한 증강 특성 인터페이스.
    public interface IProjectileModifier
    {
        /// 투사체 발사 직전 호출.
        /// 이벤트 구독 등록 및 초기 상태 설정.
        /// ReturnToPool() 시 이벤트 자동 초기화되므로 별도 해제 불필요.
        void Apply(ProjectileBase projectile);

        /// 발사마다 새 상태로 초기화된 복사본을 반환.
        /// ReflectModifier처럼 remainingCount 같은 런타임 상태를 가진 경우
        /// 같은 인스턴스를 여러 투사체에 재사용하면 카운트가 꼬이므로
        /// 반드시 Clone()으로 새 인스턴스를 만들어 Apply()에 넘긴다.
        IProjectileModifier Clone();
    }

    // 활성화된 IProjectileModifier 목록을 보관하고 발사 직전 투사체에 주입.
    public class ProjectileModifierApplier : MonoBehaviour
    {
        private readonly List<IProjectileModifier> activeModifiers = new();
        private IProjectileModifier _nextShotModifier;

        public bool HasNextShotModifier => _nextShotModifier != null;

        public void SetNextShotModifier(IProjectileModifier modifier)
        {
            _nextShotModifier = modifier;
        }

        public void ClearNextShotModifier()
        {
            _nextShotModifier = null;
        }

        public void AddModifier(IProjectileModifier modifier)
        {
            if (!activeModifiers.Contains(modifier))
                activeModifiers.Add(modifier);
        }

        public void RemoveModifier(IProjectileModifier modifier) => activeModifiers.Remove(modifier);
        public void ClearModifiers() => activeModifiers.Clear();

        // 각 modifier를 Clone()해서 새 인스턴스로 Apply.
        /// 투사체마다 독립적인 런타임 상태(반사 횟수 등)를 보장.
        public void ApplyAll(ProjectileBase projectile)
        {
            foreach (var modifier in activeModifiers)
            {
                modifier.Clone().Apply(projectile);
            }

            if (_nextShotModifier == null)
            {
                return;
            }

            _nextShotModifier.Clone().Apply(projectile);
            _nextShotModifier = null;
        }
    }
}