using Shield_Shot.GameplayCore.Weapon.Projectile;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    [RequireComponent(typeof(Collider))]
    public class ShieldColliderDetector : MonoBehaviour, IShieldColliderDetector
    {
        [Header("Layer Settings")]
        [SerializeField] private LayerMask projectileLayerMask;

        [Header("Defense Angle Settings")]
        [SerializeField, Range(10f, 180f)] private float defenseAngle = 120f;

        [Header("Reflect Cooldown")]
        [Tooltip("반사 후 같은 투사체를 무시할 시간 (초)")]
        [SerializeField] private float reflectCooldown = 0.5f;

        public event System.Action<ProjectileBase, Vector3> OnProjectileDetected;

        private readonly Dictionary<ProjectileBase, float> _recentlyReflected = new();

        private void OnTriggerEnter(Collider other)
        {
            int mask = 1 << other.gameObject.layer;
            if ((projectileLayerMask.value & mask) == 0) return;

            var projectile = other.GetComponent<ProjectileBase>()
                ?? other.GetComponentInParent<ProjectileBase>()
                ?? other.GetComponentInChildren<ProjectileBase>();
            if (projectile == null) return;

            if (IsOnCooldown(projectile)) return;

            if (!IsHitFromFront(projectile)) return;

            Vector3 hitNormal = GetHitNormal(other);
            _recentlyReflected[projectile] = Time.time;
            OnProjectileDetected?.Invoke(projectile, hitNormal);
        }

        private void Update()
        {
            var toRemove = new List<ProjectileBase>();
            foreach (var kvp in _recentlyReflected)
            {
                if (kvp.Key == null || Time.time - kvp.Value > reflectCooldown)
                    toRemove.Add(kvp.Key);
            }
            foreach (var key in toRemove)
                _recentlyReflected.Remove(key);
        }

        private bool IsOnCooldown(ProjectileBase projectile)
        {
            if (!_recentlyReflected.TryGetValue(projectile, out float lastTime)) return false;
            return Time.time - lastTime < reflectCooldown;
        }

        private bool IsHitFromFront(ProjectileBase projectile)
        {
            Vector3 incoming = projectile.Velocity.sqrMagnitude > 0.001f
                ? projectile.Velocity.normalized
                : projectile.transform.forward;
            Vector3 shieldFwd = -transform.forward;

            if (Vector3.Dot(incoming, shieldFwd) >= 0f) return false;

            Vector3 toProj = (projectile.transform.position - transform.position).normalized;
            toProj.y = 0f;
            shieldFwd.y = 0f;
            return Vector3.Angle(shieldFwd.normalized, toProj.normalized) <= defenseAngle * 0.5f;
        }

        private Vector3 GetHitNormal(Collider other)
        {
            var projectile = other.GetComponent<ProjectileBase>()
                ?? other.GetComponentInParent<ProjectileBase>();
            if (projectile == null) return -transform.forward;

            Vector3 dir = projectile.Velocity.sqrMagnitude > 0.001f
                ? projectile.Velocity.normalized
                : projectile.transform.forward;
            Vector3 origin = projectile.transform.position - dir * 0.2f;

            if (Physics.SphereCast(origin, 0.1f, dir, out RaycastHit hit, 2f, 1 << gameObject.layer))
            {
                Vector3 n = hit.normal;
                return Vector3.Dot(n, -transform.forward) < 0f ? -n : n;
            }
            return -transform.forward;
        }
    }
}