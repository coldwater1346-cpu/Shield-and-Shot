using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class ReflectEffect : MonoBehaviour, IShieldEffect
    {
        [Header("Reflect Settings")]
        [SerializeField] private float _speedMultiplier = 1f;
        [SerializeField] private float _vfxDestroyTime = 1.5f;

        [Header("VFX")]
        [SerializeField] private VFXType _vfxType = VFXType.Reflect;

        private Collider _shieldCollider;
        private Vector3 _lastDebugSpawnPos;
        private Vector3 _lastDebugHitNormal;

        private void Awake()
        {
            _shieldCollider = GetComponent<Collider>()
                           ?? GetComponentInParent<Collider>()
                           ?? GetComponentInChildren<Collider>();
        }

        public void OnBlock(ProjectileBase projectile, Vector3 hitPosition, Vector3 hitNormal)
        {
            if (projectile == null) return;

            // 기존 ShieldActionReflector와 동일한 로직
            Vector3 incomingVelocity = projectile.Velocity;
            if (incomingVelocity == Vector3.zero)
                incomingVelocity = projectile.transform.forward * projectile.BaseSpeed;

            // 방패가 Y축 180도 회전된 상태로 소환되므로 -forward가 실제 앞면
            Vector3 effectiveNormal = -transform.forward;
            if (Vector3.Dot(incomingVelocity, effectiveNormal) > 0f)
                effectiveNormal = -effectiveNormal;

            Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, effectiveNormal);
            reflectedVelocity.y = 0f;
            reflectedVelocity = reflectedVelocity.normalized * (projectile.BaseSpeed * _speedMultiplier);

            projectile.gameObject.layer = LayerMask.NameToLayer("Projectile");

            MonsterProjectile monsterProjectile = projectile.GetComponent<MonsterProjectile>();
            if (monsterProjectile != null)
            {
                monsterProjectile.SetReflected(true);
            }

            projectile.Velocity = reflectedVelocity;
            projectile.BaseSpeed *= _speedMultiplier;

            if (reflectedVelocity.sqrMagnitude > 0.001f)
                projectile.transform.rotation = Quaternion.LookRotation(reflectedVelocity, Vector3.up);

            // 충돌 위치는 BlockShield에서 넘겨준 hitPosition 사용
            PlayReflectVFX(hitPosition, effectiveNormal);

            Debug.Log($"[ReflectEffect] 반사 성공! 방향: {reflectedVelocity.normalized}");
        }

        private void PlayReflectVFX(Vector3 position, Vector3 hitNormal)
        {
            if (_vfxType == VFXType.None || VFXPoolManager.Instance == null) return;

            Quaternion vfxRotation = hitNormal != Vector3.zero
                ? Quaternion.LookRotation(hitNormal, Vector3.up)
                : transform.rotation;

            Vector3 spawnPosition = position + hitNormal * 0.05f;
            _lastDebugSpawnPos = spawnPosition;
            _lastDebugHitNormal = hitNormal;

            VFXPoolManager.Instance.SpawnVFX(_vfxType, spawnPosition, vfxRotation, _vfxDestroyTime);
        }

        private void OnDrawGizmos()
        {
            if (_lastDebugSpawnPos == Vector3.zero) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_lastDebugSpawnPos, 0.15f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(_lastDebugSpawnPos, _lastDebugHitNormal * 0.5f);
        }
    }
}