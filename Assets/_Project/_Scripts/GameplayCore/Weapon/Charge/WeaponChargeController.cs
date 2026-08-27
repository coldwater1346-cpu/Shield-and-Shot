using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Charge
{
    public class WeaponChargeController : MonoBehaviour
    {
        #region Inspector
        [Header("Settings")]
        [Tooltip("0→1 도달에 걸리는 시간(초)")]
        [SerializeField] private float maxChargeTime = 1.5f;

        [Tooltip("차징이 이 비율 이상이어야 발사로 인정")]
        [SerializeField] private float minFireThreshold = 0.1f;
        #endregion

        #region 상태
        private float chargeRatio;
        private bool isCharging;

        // 현재 차징 비율 (0~1)
        public float ChargeRatio => chargeRatio;

        // 최소 발사 임계값 이상이면 true
        public bool CanFire => chargeRatio >= minFireThreshold;
        #endregion


        #region 공개 메서드
        // 초기화. WeaponBase.Initialize()에서 호출
        public void Initialize()
        {
            chargeRatio = 0f;
            isCharging = false;
        }

        // 매 프레임 deltaTime만큼 차징을 진행
        public void Tick(float deltaTime)
        {
            if (!isCharging)
            {
                isCharging = true;
            }

            chargeRatio = Mathf.Clamp01(chargeRatio + deltaTime / maxChargeTime);
        }

        // 차징 비율을 외부에서 직접 설정
        public void SetChargeRatio(float ratio)
        {
            chargeRatio = Mathf.Clamp01(ratio);
        }

        // 차징 상태를 초기화. Deactivate() 시 호출
        public void Reset()
        {
            chargeRatio = 0f;
            isCharging = false;
        }
        #endregion
    }
}