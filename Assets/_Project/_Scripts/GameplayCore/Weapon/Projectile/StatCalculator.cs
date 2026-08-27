using UnityEngine;
using Shield_Shot.DataManagement.InventorySystem;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    // 최종 투사체 스탯을 담는 순수 데이터 구조체
    public readonly struct ProjectileStats
    {
        public readonly float Damage;
        public readonly float Speed;

        public ProjectileStats(float damage, float speed)
        {
            Damage = damage;
            Speed = speed;
        }
    }

    public class StatCalculator : MonoBehaviour
    {
        public WeaponItem WeaponData { get; private set; }
        public float DamageMultiplier { get; set; } = 1f;

        private float _baseDamage = 10f;
        private float _baseSpeed = 15f;
        private float _maxDamageMultiplier = 2f;
        private float _maxSpeedMultiplier = 1.5f;
        private float _criticalMultiplier = 2.0f;

        public float MaxDamageMultiplier => _maxDamageMultiplier;
        public float CriticalMultiplier => _criticalMultiplier;

        public void ApplyRawStats(
            float damage,
            float speed,
            float maxDamageMultiplier,
            float maxSpeedMultiplier,
            float criticalMultiplier)
        {
            WeaponData = null;
            _baseDamage = damage;
            _baseSpeed = speed;
            _maxDamageMultiplier = maxDamageMultiplier;
            _maxSpeedMultiplier = maxSpeedMultiplier;
            _criticalMultiplier = criticalMultiplier;
        }

        public void ApplyWeaponData(WeaponItem data)
        {
            if (data == null)
            {
                Debug.LogError($"[{nameof(StatCalculator)}] WeaponItem이 null.");
                return;
            }

            WeaponData = data;
            _baseDamage = data.FinalDamage;
            _baseSpeed = data.FinalSpeed;
            _maxDamageMultiplier = data.FinalMaxDamageMultiplier;
            _maxSpeedMultiplier = data.FinalMaxSpeedMultiplier;
            _criticalMultiplier = data.FinalCriticalDamageMultiplier;

            Debug.Log($"[{gameObject.name}] StatCalculator 적용. " +
                      $"Damage={_baseDamage}, Speed={_baseSpeed}, " +
                      $"MaxDmgMul={_maxDamageMultiplier}, MaxSpdMul={_maxSpeedMultiplier}, CritMul={_criticalMultiplier}");
        }

        public ProjectileStats Calculate(float chargeRatio, bool isCritical = false)
        {
            float t = Mathf.Clamp01(chargeRatio);

            float damage = _baseDamage * Mathf.Lerp(1f, _maxDamageMultiplier, t);
            float speed = _baseSpeed * Mathf.Lerp(1f, _maxSpeedMultiplier, t);

            if (isCritical)
                damage *= _criticalMultiplier;

            damage *= DamageMultiplier;

            return new ProjectileStats(damage, speed);
        }
    }
}
