using System.Collections;
using System.Collections.Generic;
using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Common;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.InputSystem.Data;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class ChainLightningHitBehavior : IHitBehavior
    {
        private readonly int _chainCount;
        private readonly float _searchRadius;
        private readonly float _chainDamageFalloff;
        private readonly LayerMask _targetLayer;
        private readonly VFXType _hitVfxType;
        private readonly float _vfxAutoReleaseTime;
        private readonly float _chainDelay;
        private readonly AudioClip _chainSfx;
        private readonly float _volume;

        public ChainLightningHitBehavior(
            int chainCount,
            float searchRadius,
            float chainDamageFalloff,
            LayerMask targetLayer,
            VFXType hitVfxType,
            float vfxAutoReleaseTime,
            float chainDelay,
            AudioClip chainSfx = null,
            float volume = 1f)
        {
            _chainCount = Mathf.Max(0, chainCount);
            _searchRadius = Mathf.Max(0f, searchRadius);
            _chainDamageFalloff = Mathf.Clamp01(chainDamageFalloff);
            _targetLayer = targetLayer;
            _hitVfxType = hitVfxType;
            _vfxAutoReleaseTime = Mathf.Max(0f, vfxAutoReleaseTime);
            _chainDelay = Mathf.Max(0f, chainDelay);
            _chainSfx = chainSfx;
            _volume = Mathf.Clamp01(volume);
        }

        public void OnHit(ProjectileBase projectile, Collider targetInfo)
        {
            if (projectile == null || targetInfo == null) return;

            Transform currentTarget = GetDamageRoot(targetInfo);
            if (currentTarget == null) return;

            HashSet<Transform> hitTargets = new HashSet<Transform>();
            hitTargets.Add(currentTarget);

            Vector3 currentPosition = GetChainOrigin(currentTarget);

            float firstChainDamage = projectile.ProjectileDamage * (1f - _chainDamageFalloff);
            IEnumerator chainRoutine = Co_ExecuteChain(projectile, currentPosition, hitTargets, firstChainDamage);

            if (TimeScaleManager.Instance != null)
                TimeScaleManager.Instance.StartCoroutine(chainRoutine);
            else
                projectile.StartCoroutine(chainRoutine);
        }

        private IEnumerator Co_ExecuteChain(
            ProjectileBase projectile,
            Vector3 currentPosition,
            HashSet<Transform> hitTargets,
            float currentDamage)
        {
            for (int i = 0; i < _chainCount; i++)
            {
                if (_chainDelay > 0f)
                    yield return new WaitForSecondsRealtime(_chainDelay);

                Collider nextCollider = FindNearestTarget(currentPosition, hitTargets);
                if (nextCollider == null) yield break;

                Transform nextTarget = GetDamageRoot(nextCollider);
                if (nextTarget == null) yield break;

                Vector3 nextPosition = GetChainOrigin(nextTarget);

                if (TryGetDamageable(nextTarget, out ITakeDamage damageable))
                {
                    damageable.TakeDamage(currentDamage);
                }

                PlayChainImpact(projectile, nextPosition, nextTarget, currentDamage);

                Debug.DrawLine(currentPosition, nextPosition, Color.cyan, 0.4f);
                Debug.Log($"[ChainLightning] Chain {i + 1}/{_chainCount}: {nextTarget.name}, Damage: {currentDamage}");

                hitTargets.Add(nextTarget);
                currentPosition = nextPosition;
                currentDamage *= 1f - _chainDamageFalloff;
            }
        }

        private void PlayChainImpact(ProjectileBase projectile, Vector3 position, Transform target, float damage)
        {
            bool isCritical = projectile != null && projectile.IsCritical;

            if (DamagePopupManager.Instance != null)
                DamagePopupManager.Instance.Show(position, damage, isCritical);

            if (_hitVfxType != VFXType.None && VFXPoolManager.Instance != null)
            {
                Quaternion rotation = projectile != null ? projectile.transform.rotation : Quaternion.identity;
                VFXPoolManager.Instance.SpawnVFX(_hitVfxType, position, rotation, _vfxAutoReleaseTime);
            }

            if (_chainSfx != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(_chainSfx, _volume);
            }
        }

        private Collider FindNearestTarget(Vector3 origin, HashSet<Transform> excludedTargets)
        {
            Collider[] hits = Physics.OverlapSphere(origin, _searchRadius, _targetLayer);

            Collider nearestCollider = null;
            Transform nearestRoot = null;
            float nearestSqrDistance = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider candidate = hits[i];
                if (candidate == null) continue;

                Transform candidateRoot = GetDamageRoot(candidate);
                if (candidateRoot == null || excludedTargets.Contains(candidateRoot)) continue;
                if (!TryGetDamageable(candidateRoot, out _)) continue;

                float sqrDistance = (GetChainOrigin(candidateRoot) - origin).sqrMagnitude;
                if (sqrDistance >= nearestSqrDistance) continue;

                nearestCollider = candidate;
                nearestRoot = candidateRoot;
                nearestSqrDistance = sqrDistance;
            }

            return nearestRoot != null ? nearestCollider : null;
        }

        private static Transform GetDamageRoot(Collider collider)
        {
            ITakeDamage damageable = collider.GetComponentInParent<ITakeDamage>();
            if (damageable is Component component) return component.transform;

            return collider.attachedRigidbody != null
                ? collider.attachedRigidbody.transform
                : collider.transform.root;
        }

        private static bool TryGetDamageable(Transform target, out ITakeDamage damageable)
        {
            damageable = target.GetComponent<ITakeDamage>();
            if (damageable != null) return true;

            damageable = target.GetComponentInChildren<ITakeDamage>();
            return damageable != null;
        }

        private static Vector3 GetChainOrigin(Transform target)
        {
            Collider collider = target.GetComponentInChildren<Collider>();
            if (collider != null) return collider.bounds.center;

            return target.position;
        }
    }
}