using Shield_Shot.Audio;
using Shield_Shot.DataManagement.InventorySystem;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using Shield_Shot.InputSystem;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class BlockShield : MonoBehaviour, IShieldAction
    {
        [Header("Block VFX")]
        [Tooltip("막기 성공 시 재생할 VFX 타입")]
        [SerializeField] private VFXType _blockVfxType = VFXType.Block;
        [SerializeField] private float _blockVfxDuration = 1.5f;

        [Header("Sound")]
        [SerializeField] private AudioClip _blockSfx;
        [SerializeField] private float _volume = 1f;

        private IShieldEffect[] _effects;
        private Collider _shieldCollider;

        private void Awake()
        {
            // 자식에서 모든 추가 효과 수집
            _effects = GetComponentsInChildren<IShieldEffect>();

            _shieldCollider = GetComponent<Collider>()
                           ?? GetComponentInChildren<Collider>();

            Debug.Log($"[BlockShield] 추가 효과 {_effects.Length}개 감지: " +
                      string.Join(", ", System.Array.ConvertAll(_effects, e => e.GetType().Name)));
        }

        public void ApplyShieldData(ShieldDataSO data)
        {
            if (data == null) return;
            // 필요 시 SO 데이터 주입 (VFX 타입 등)
            _blockVfxType = data.VfxType;
            _blockVfxDuration = data.VfxDestroyTime;
        }

        public void ApplyShieldData(ShieldItemData data)
        {
            // TODO: ShieldItemData에 VfxType 추가되면 여기서 주입
            Debug.Log("[BlockShield] ShieldItemData 주입 완료.");
        }

        #region IShieldAction
        public void OnProjectileHit(ProjectileBase projectile, Vector3 hitNormal)
        {
            if (projectile == null) return;

            Vector3 hitPosition = GetHitPosition(projectile);

            // 1. 기본: 막기 VFX
            // hitNormal 재계산: ShieldColliderDetector의 SphereCast 실패 시 대비
            // 투사체 → 방패 표면 가장 가까운 지점 방향을 법선으로 직접 계산
            Vector3 validNormal = hitNormal;
            if (validNormal.sqrMagnitude < 0.0001f || hitNormal == -transform.forward)
            {
                // SphereCast 폴백(-transform.forward) 대신 콜라이더 표면 법선 재계산
                if (_shieldCollider != null)
                {
                    Vector3 closest = _shieldCollider.ClosestPoint(projectile.transform.position);
                    validNormal = (projectile.transform.position - closest);
                    validNormal.y = 0f;
                    validNormal = validNormal.sqrMagnitude > 0.0001f ? validNormal.normalized : -transform.forward;
                }
            }
            hitNormal = validNormal;

            Debug.Log($"[BlockShield] 투사체 감지. hitNormal={hitNormal}, shield.forward={transform.forward}");
            PlayBlockVFX(hitPosition, hitNormal);

            SoundManager.Instance.PlaySFX(_blockSfx, _volume);

            // 2. 추가 효과 순회 (반사, 슬로우, 스플래시 등)
            //    효과가 없으면 투사체 제거, 있으면 각 효과가 투사체 처리 담당
            if (_effects.Length == 0)
            {
                projectile.ReleaseOrDestroy();
            }
            else
            {
                foreach (var effect in _effects)
                    effect.OnBlock(projectile, hitPosition, hitNormal);
            }

            Debug.Log($"[BlockShield] 투사체 차단. 효과 수: {_effects.Length}");
        }


        #endregion

        private Vector3 GetHitPosition(ProjectileBase projectile)
        {
            if (_shieldCollider != null)
                return _shieldCollider.ClosestPoint(projectile.transform.position);

            Vector3 toProjectile = projectile.transform.position - transform.position;
            return transform.position + Vector3.Project(toProjectile, transform.forward);
        }

        private void PlayBlockVFX(Vector3 position, Vector3 hitNormal)
        {
            if (_blockVfxType == VFXType.None) return;
            if (VFXPoolManager.Instance == null) return;

            Quaternion rot = hitNormal != Vector3.zero
                ? Quaternion.LookRotation(hitNormal, Vector3.up)
                : transform.rotation;

            VFXPoolManager.Instance.SpawnVFX(_blockVfxType, position + hitNormal * 0.05f, rot, _blockVfxDuration);
        }

        public void Initialize()
        {
            // 효과 목록 갱신 (런타임 추가 대비)
            _effects = GetComponentsInChildren<IShieldEffect>();
        }

        public void ApplyShieldData(ShieldItemData data, Transform playerTransform, GestureAnalyzer inputAnalyzer)
        {
            ApplyShieldData(data);

            var orbitController = GetComponent<ShieldOrbitController>()
                               ?? GetComponentInChildren<ShieldOrbitController>(true);
            if (orbitController != null)
                orbitController.SetupOrbitCenter(playerTransform);

            var binder = GetComponent<ShieldSystemBinder>()
                      ?? GetComponentInChildren<ShieldSystemBinder>(true);
            if (binder != null)
                binder.InitializeBinding(inputAnalyzer, orbitController);
        }

        public void ApplyShieldData(ShieldDataSO data, Transform playerTransform, GestureAnalyzer inputAnalyzer)
        {
            ApplyShieldData(data);

            var orbitController = GetComponent<ShieldOrbitController>()
                               ?? GetComponentInChildren<ShieldOrbitController>(true);
            if (orbitController != null)
                orbitController.SetupOrbitCenter(playerTransform);

            var binder = GetComponent<ShieldSystemBinder>()
                      ?? GetComponentInChildren<ShieldSystemBinder>(true);
            if (binder != null)
                binder.InitializeBinding(inputAnalyzer, orbitController);
        }
    }
}