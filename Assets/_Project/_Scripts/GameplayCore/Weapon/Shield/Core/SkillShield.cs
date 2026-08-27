using Shield_Shot.DataManagement.InventorySystem;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using Shield_Shot.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class SkillShield : ShieldBase, ISpecialShield
    {
        [Header("Special Ability")]
        [Tooltip("방패 스킬 컴포넌트 (ShieldSkillBase 상속). 비어있으면 자식에서 자동 탐색.")]
        [SerializeField] private ShieldSkillBase _skill;

        [Header("References")]
        [SerializeField] private Collider targetInfo;

        private bool _isSpecialActive = false;
        private IShieldAction _shieldAction;

        public bool IsSpecialAbilityActive => _isSpecialActive;

        protected override void Awake()
        {
            base.Awake();

            if (_shieldAction == null)
                _shieldAction = GetComponent<IShieldAction>()
                             ?? GetComponentInChildren<IShieldAction>();

            if (_skill == null)
                _skill = GetComponentInChildren<ShieldSkillBase>(true);

            if (targetInfo == null)
                targetInfo = GetComponentInChildren<Collider>(true) ?? GetComponent<Collider>();
        }

        // 임시: W키로 스킬 발동
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame)
            {
                Debug.Log("[SkillShield] [테스트] W키 입력");
                ActivateSpecialAbility();
            }
        }

        public void Initialize()
        {
            ResetGauge();
            _isSpecialActive = false;

            if (_shieldAction == null)
                _shieldAction = GetComponent<IShieldAction>()
                             ?? GetComponentInChildren<IShieldAction>();

            if (_skill == null)
                _skill = GetComponentInChildren<ShieldSkillBase>(true);

            if (targetInfo == null)
                targetInfo = GetComponentInChildren<Collider>(true) ?? GetComponent<Collider>();

            // BlockShield의 _effects 목록 갱신 (Awake 타이밍 이슈 방지)
            var blockShield = GetComponent<BlockShield>()
                           ?? GetComponentInChildren<BlockShield>();
            blockShield?.Initialize();

            Debug.Log($"[SkillShield] Initialize 완료. ShieldAction={_shieldAction?.GetType().Name}, Skill={(_skill != null ? _skill.GetType().Name : "없음")}");
        }

        #region ApplyShieldData 오버로드
        public void ApplyShieldData(ShieldItemData data, Transform playerTransform, GestureAnalyzer inputAnalyzer)
        {
            if (data == null) return;

            // TODO: ShieldItemData에 MaxGauge 추가되면 교체
            // _maxGauge = data.MaxGauge;

            if (_shieldAction == null)
                _shieldAction = GetComponent<IShieldAction>() ?? GetComponentInChildren<IShieldAction>(true);

            if (_shieldAction == null)
            {
                Debug.LogWarning("[SkillShield] IShieldAction 없음 → BlockShield 자동 추가");
                _shieldAction = gameObject.AddComponent<BlockShield>();
            }

            if (_shieldAction is BlockShield blockShield)
                blockShield.ApplyShieldData(data);

            BindOrbitAndInput(playerTransform, inputAnalyzer);
            Debug.Log("[SkillShield] ShieldItemData 기반 바인딩 완료.");
        }

        public void ApplyShieldData(ShieldDataSO data, Transform playerTransform, GestureAnalyzer inputAnalyzer)
        {
            if (data == null) return;

            _maxGauge = data.MaxGauge;

            var blockShield = GetComponent<BlockShield>()
                           ?? GetComponentInChildren<BlockShield>();
            blockShield?.ApplyShieldData(data);

            BindOrbitAndInput(playerTransform, inputAnalyzer);
            Debug.Log("[SkillShield] ShieldDataSO 기반 바인딩 완료.");
        }

        private void BindOrbitAndInput(Transform playerTransform, GestureAnalyzer inputAnalyzer)
        {
            var orbitController = GetComponent<ShieldOrbitController>()
                               ?? GetComponentInChildren<ShieldOrbitController>(true);
            if (orbitController != null)
                orbitController.SetupOrbitCenter(playerTransform);

            var binder = GetComponent<ShieldSystemBinder>()
                      ?? GetComponentInChildren<ShieldSystemBinder>(true);
            if (binder != null)
                binder.InitializeBinding(inputAnalyzer, orbitController);
        }
        #endregion

        protected override void OnProjectileHit_Internal(ProjectileBase projectile, Vector3 hitNormal)
        {
            _shieldAction?.OnProjectileHit(projectile, hitNormal);
            ChargeGauge(10f);
        }

        #region ISpecialShield
        public override void ChargeGauge(float amount)
        {
            if (_isSpecialActive) return;

            if (_skill == null) return;

            base.ChargeGauge(amount);
        }

        public void ActivateSpecialAbility()
        {
            if (_skill == null)
            {
                Debug.LogWarning("[SkillShield] 연결된 스킬이 없어 발동할 수 없습니다.");
                return;
            }

            if (_isSpecialActive)
            {
                Debug.Log("[SkillShield] 스킬 발동 실패 - 이미 사용 중.");
                return;
            }
            if (!IsGaugeReady)
            {
                Debug.Log($"[SkillShield] 스킬 발동 실패 - 게이지 부족 ({CurrentGauge}/{MaxGauge}).");
                return;
            }

            _isSpecialActive = true;
            StopGlow();

            Debug.Log($"[SkillShield] 스킬 발동: {_skill.GetType().Name}");
            _skill.Activate();

            DeactivateSpecialAbility();
        }

        public void DeactivateSpecialAbility()
        {
            if (!_isSpecialActive) return;
            _isSpecialActive = false;
            ResetGauge();
            Debug.Log("[SkillShield] 스킬 종료. 게이지 리셋.");
        }
        #endregion
    }
}