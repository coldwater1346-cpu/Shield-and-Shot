using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Aim
{
    public class WeaponRotationController : MonoBehaviour
    {
        #region Inspector
        [Header("Settings")]
        [Tooltip("회전 보간 속도. 0이면 즉시 회전.")]
        [SerializeField] private float rotationLerpSpeed = 0f;
        #endregion

        #region 공개 메서드

        public void SyncRotation(Vector3 aimDirection)
        {
            // 스크린 XY → 월드 XZ 변환
            Vector3 worldDir = AimDirectionUtility.ToWorldXZ(aimDirection);

            if (worldDir.sqrMagnitude < 0.0001f) return;

            Quaternion targetRot = Quaternion.LookRotation(worldDir, Vector3.up);

            transform.rotation = rotationLerpSpeed <= 0f
                ? targetRot
                : Quaternion.Lerp(transform.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);
        }

        public void Reset()
        {
            transform.rotation = Quaternion.identity;
        }
        #endregion
    }
}
