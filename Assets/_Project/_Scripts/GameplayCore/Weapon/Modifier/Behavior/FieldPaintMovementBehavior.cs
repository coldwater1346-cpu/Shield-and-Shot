using Shield_Shot.GameplayCore.Field;
using Shield_Shot.GameplayCore.Monster.Movement;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class FieldPaintMovementBehavior : IMovementBehavior
    {
        private readonly ElementPaintContext _paintContext;
        private readonly float _duration;
        private readonly float _radius;

        public FieldPaintMovementBehavior(ElementType element, float duration, float radius)
            : this(new ElementPaintContext(element), duration, radius)
        {
        }

        public FieldPaintMovementBehavior(ElementType element, int elementLevel, float duration, float radius)
            : this(new ElementPaintContext(element, elementLevel), duration, radius)
        {
        }

        public FieldPaintMovementBehavior(ElementPaintContext paintContext, float duration, float radius)
        {
            _paintContext = paintContext;
            _duration = Mathf.Max(0f, duration);
            _radius = Mathf.Max(0f, radius);
        }

        public void UpdateMovement(Transform projectileTransform, ref Vector3 velocity, float deltaTime)
        {
            if (projectileTransform == null ||
                _paintContext.Element == ElementType.None ||
                _duration <= 0f ||
                ElementFieldGrid.Instance == null)
            {
                return;
            }

            if (_radius > 0f)
            {
                ElementFieldGrid.Instance.PaintCircle(projectileTransform.position, _paintContext, _duration, _radius);
                return;
            }

            ElementFieldGrid.Instance.Paint(projectileTransform.position, _paintContext, _duration);
        }
    }

    public sealed class ProjectileMaterialOverrideBehavior : IMovementBehavior
    {
        private readonly Material _material;
        private readonly bool _replaceAllMaterialSlots;
        private bool _applied;

        public ProjectileMaterialOverrideBehavior(Material material, bool replaceAllMaterialSlots)
        {
            _material = material;
            _replaceAllMaterialSlots = replaceAllMaterialSlots;
        }

        public void UpdateMovement(Transform projectileTransform, ref Vector3 velocity, float deltaTime)
        {
            if (_applied || _material == null || projectileTransform == null)
            {
                return;
            }

            ProjectileBase projectile = projectileTransform.GetComponent<ProjectileBase>();
            if (projectile == null)
            {
                return;
            }

            projectile.ApplyMaterialOverride(_material, _replaceAllMaterialSlots);
            _applied = true;
        }
    }

    public sealed class WindFieldProjectileBoostBehavior : IMovementBehavior
    {
        private readonly float _boostDuration;
        private readonly float _speedMultiplier;

        private float _remainingBoostTime;
        private float _appliedMultiplier = 1f;

        public bool IsBoostActive => _remainingBoostTime > 0f;

        public WindFieldProjectileBoostBehavior(int level, float baseBoostDuration, float durationPerLevel, float baseSpeedMultiplier, float speedMultiplierPerLevel)
        {
            int safeLevel = Mathf.Max(1, level);
            _boostDuration = Mathf.Max(0f, baseBoostDuration + durationPerLevel * (safeLevel - 1));
            _speedMultiplier = Mathf.Max(1f, baseSpeedMultiplier + speedMultiplierPerLevel * (safeLevel - 1));
        }

        public void UpdateMovement(Transform projectileTransform, ref Vector3 velocity, float deltaTime)
        {
            if (projectileTransform == null || velocity.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            ElementFieldGrid grid = ElementFieldGrid.Instance;
            if (grid != null &&
                grid.TryGetCellData(projectileTransform.position, out ElementFieldCellData cellData) &&
                cellData.IsActive &&
                cellData.CurrentElement == ElementType.Wind)
            {
                _remainingBoostTime = _boostDuration;
            }
            else if (_remainingBoostTime > 0f)
            {
                _remainingBoostTime -= deltaTime;
            }

            float targetMultiplier = IsBoostActive ? _speedMultiplier : 1f;
            if (Mathf.Approximately(targetMultiplier, _appliedMultiplier))
            {
                return;
            }

            float speed = velocity.magnitude;
            Vector3 direction = velocity / speed;
            float baseSpeed = speed / Mathf.Max(0.0001f, _appliedMultiplier);
            velocity = direction * baseSpeed * targetMultiplier;
            _appliedMultiplier = targetMultiplier;
        }
    }

    public sealed class WindBoostKnockbackHitBehavior : IHitBehavior
    {
        private readonly WindFieldProjectileBoostBehavior _boostBehavior;
        private readonly float _knockbackSpeed;
        private readonly float _knockbackDuration;

        public WindBoostKnockbackHitBehavior(WindFieldProjectileBoostBehavior boostBehavior, int level, float baseKnockbackSpeed, float knockbackSpeedPerLevel, float knockbackDuration)
        {
            _boostBehavior = boostBehavior;
            int safeLevel = Mathf.Max(1, level);
            _knockbackSpeed = Mathf.Max(0f, baseKnockbackSpeed + knockbackSpeedPerLevel * (safeLevel - 1));
            _knockbackDuration = Mathf.Max(0f, knockbackDuration);
        }

        public void OnHit(ProjectileBase projectile, Collider targetInfo)
        {
            if (projectile == null ||
                targetInfo == null ||
                _boostBehavior == null ||
                !_boostBehavior.IsBoostActive ||
                _knockbackSpeed <= 0f ||
                _knockbackDuration <= 0f)
            {
                return;
            }

            MovementComponent movement = targetInfo.GetComponentInParent<MovementComponent>();
            if (movement == null)
            {
                return;
            }

            Vector3 direction = projectile.Direction;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = projectile.transform.forward;
                direction.y = 0f;
            }

            movement.ApplyKnockback(direction.normalized * _knockbackSpeed, _knockbackDuration);
        }
    }
}
