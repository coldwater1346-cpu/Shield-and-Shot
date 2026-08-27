using System.Collections.Generic;
using Shield_Shot.DataManagement;
using Shield_Shot.DataManagement.InventorySystem;
using Shield_Shot.GameplayCore.Augment;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Core;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using Shield_Shot.GameplayCore.Weapon.Shield;
using Shield_Shot.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Shield_Shot.GameplayCore.Weapon
{
    public class WeaponManager : MonoBehaviour
    {
        [Header("Weapon Slots (Lobby Setup)")]
        [Tooltip("로비에서 세팅되어 넘어올 무기 데이터 2개")]
        [SerializeField] private WeaponItem _mainWeaponData;
        [SerializeField] private WeaponItem _subWeaponData;
        [SerializeField] private ShieldItem _activeShieldData;

        [Header("References")]
        [Tooltip("무기가 장착될 캐릭터 손 또는 특정 트랜스폼 위치")]
        [SerializeField] private Transform _weaponMountPoint;
        [SerializeField] private Transform _shieldMountPoint;
        [Tooltip("방패가 회전할 중심축 (비어있으면 이 WeaponManager의 transform을 사용)")]
        [SerializeField] private Transform _playerOrbitCenter;

        [Header("테스트용 (WeaponItemData 직접 연결 전 임시)")]
        [Tooltip("연결 시 _mainWeaponData 대신 이 프리팹을 메인 무기로 사용 (StatCalculator/WeaponBase 수치는 기본값 적용)")]
        [SerializeField] private GameObject _testMainWeaponPrefab;

        [Tooltip("연결 시 _subWeaponData 대신 이 프리팹을 서브 무기로 사용")]
        [SerializeField] private GameObject _testSubWeaponPrefab;

        [Header("테스트용 (ShieldItemData 필드 추가 전 임시)")]
        [Tooltip("ShieldItemData에 ShieldPrefab 외 필드 추가 전 임시 테스트용. 연결 시 ShieldItem 대신 이 프리팹 사용.")]
        [SerializeField] private GameObject _testShieldPrefab;

        [Header("UI")]
        [Tooltip("방패 스킬 버튼 (씬에서 연결)")]
        [SerializeField] private Button _shieldSkillButton;

        [Tooltip("무기 스킬 버튼 (씬에서 연결). 현재 장착 무기(CurrentWeapon.Skill)를 그때그때 참조한다.")]
        [SerializeField] private Button _weaponSkillButton;

        [Header("Input System Bridge")]
        [SerializeField] private PlayerInputReceiver _inputReceiver;

        [Tooltip("방패 제어용 제스처 분석기 (비어있으면 자동으로 찾음)")]
        [SerializeField] private GestureAnalyzer _defendGestureAnalyzer;

        [Header("Base Behaviors")]
        [Tooltip("항상 켜져 있어야 하는 기본 특성 목록 (예: DefaultDamageSO)")]
        [SerializeField] private List<ProjectileBehaviorSO> _baseBehaviors;

        [Header("Element/Property")]
        [Tooltip("무기 Property(Fire/Ice/Lightning/Wind)와 ProjectileBehaviorSO 매핑 테이블")]
        [SerializeField] private ElementBehaviorDatabaseSO _elementBehaviorDatabase;

        [Header("Weapon Skill")]
        [Tooltip("무기 SkillType(뽑기 결과)와 ActiveWeaponSkillSO 매핑 테이블. " +
                 "WeaponItem.SkillType에 따라 무기 장착 시 자동으로 해당 스킬 SO를 찾아 주입한다.")]
        [SerializeField] private WeaponSkillDatabaseSO _weaponSkillDatabase;

        // 현재 PlayerStatus.CurrentBehaviors에 들어가 있는 원소 특성 SO (스왑 시 뺄 대상 추적용)
        private ProjectileBehaviorSO _activeElementBehavior;

        // 게이지 상태(IsGaugeReady)를 매 프레임 확인하기 위해 장착된 방패를 들고 있는다.
        private SkillShield _equippedShield;

        private WeaponBase[] _equippedWeapons = new WeaponBase[2];
        private int _currentWeaponIndex = 0;

        // 무기 슬롯(0=메인, 1=서브)별 스킬 쿨타임. 스킬 SO 자체(공유 에셋)에 상태를 두면 안 되므로
        // "지금 이 슬롯에 꽂혀있는 무기의 스킬이 얼마나 남았는지"를 슬롯 기준으로 따로 들고 있는다.
        private float[] _weaponSkillCooldownTimers = new float[2];

        public WeaponBase CurrentWeapon => _equippedWeapons[_currentWeaponIndex];
        public int CurrentWeaponIndex => _currentWeaponIndex;

        // Input System V2의 방패 입력 Adapter가 현재 장착된 방패를
        // WeaponManager 구현 세부사항에 접근하지 않고 조회하기 위한 계약이다.
        public SkillShield CurrentShield => _equippedShield;

        // 무기 및 방패 스킬 버튼 이펙트에서 사용하는 읽기 전용 상태.
        // 쿨타임의 실제 소유권은 계속 WeaponManager에 유지한다.
        public bool HasCurrentWeaponSkill =>
            CurrentWeapon != null &&
            CurrentWeapon.Skill != null;

        public float CurrentWeaponSkillCooldownRemaining =>
            Mathf.Max(
                0f,
                _weaponSkillCooldownTimers[_currentWeaponIndex]);

        public float CurrentWeaponSkillCooldownDuration =>
            HasCurrentWeaponSkill
                ? CurrentWeapon.Skill.CooldownDuration
                : 0f;

        public bool IsCurrentWeaponSkillReady =>
            HasCurrentWeaponSkill &&
            CurrentWeaponSkillCooldownRemaining <= 0f;

        public float CurrentWeaponSkillCooldownNormalized
        {
            get
            {
                float duration =
                    CurrentWeaponSkillCooldownDuration;

                if (duration <= 0f)
                {
                    return 0f;
                }

                return Mathf.Clamp01(
                    CurrentWeaponSkillCooldownRemaining /
                    duration);
            }
        }

        public bool IsShieldSkillReady =>
            CurrentShield != null &&
            CurrentShield.IsGaugeReady &&
            !CurrentShield.IsSpecialAbilityActive;

        public float ShieldGaugeNormalized
        {
            get
            {
                if (CurrentShield == null ||
                    CurrentShield.MaxGauge <= 0f)
                {
                    return 0f;
                }

                return Mathf.Clamp01(
                    CurrentShield.CurrentGauge /
                    CurrentShield.MaxGauge);
            }
        }

        private void Start()
        {
            _mainWeaponData = PlayerIngameLoadData.MainWeaponItem as WeaponItem;
            _subWeaponData = PlayerIngameLoadData.SubWeaponItem as WeaponItem;
            _activeShieldData = PlayerIngameLoadData.ShieldItem as ShieldItem;

            if (_mainWeaponData == null && _testMainWeaponPrefab == null)
                Debug.LogWarning("[WeaponManager] MainWeaponItem null. 로비에서 SavePlayerLoadData() 호출 여부를 확인하세요.");
            if (_activeShieldData == null && _testShieldPrefab == null)
                Debug.LogWarning("[WeaponManager] ShieldItem null.");

            if (LocalPlayerStatusContext.TryGet(out PlayerStatus playerStatus))
                playerStatus.ResetBaseBehaviors(_baseBehaviors);
            else
                Debug.LogWarning("[WeaponManager] PlayerStatus를 찾지 못해 기본 특성 목록을 초기화하지 못했습니다.");

            InitializeWeapons();
            RegisterEquippedWeaponSkills();
            InitializeShield();
            BindWeaponSkillButton();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            {
                Debug.Log("[WeaponManager] 테스트용 Q 키 입력 감지. 무기 교체를 시도한다.");
                SwapWeapon();
            }

            for (int i = 0; i < _weaponSkillCooldownTimers.Length; i++)
            {
                if (_weaponSkillCooldownTimers[i] > 0f)
                    _weaponSkillCooldownTimers[i] -= Time.deltaTime;
            }

            // 쿨타임/게이지는 매 프레임 바뀌는 값이라, 버튼 클릭 가능 여부도 매 프레임 갱신한다.
            RefreshWeaponSkillButtonInteractable();
            RefreshShieldSkillButtonInteractable();
        }

        private void InitializeWeapons()
        {
            if (_weaponMountPoint == null) _weaponMountPoint = transform;

            // 메인 무기: 테스트 프리팹이 있으면 우선 사용
            if (_testMainWeaponPrefab != null)
            {
                _equippedWeapons[0] = SpawnAndApplyWeaponFromPrefab(_testMainWeaponPrefab, null);
                if (_equippedWeapons[0] != null)
                    _equippedWeapons[0].gameObject.SetActive(true);
            }
            else if (_mainWeaponData != null && _mainWeaponData.WeaponData?.WeaponPrefab != null)
            {
                _equippedWeapons[0] = SpawnAndApplyWeapon(_mainWeaponData);
                if (_equippedWeapons[0] != null)
                {
                    _equippedWeapons[0].gameObject.SetActive(true);
                    _equippedWeapons[0].PlayEquipSound();
                }
            }
            else
            {
                Debug.LogError("[WeaponManager] 메인 무기 데이터/테스트 프리팹이 모두 없습니다!");
            }

            // 서브 무기: 테스트 프리팹이 있으면 우선 사용
            if (_testSubWeaponPrefab != null)
            {
                _equippedWeapons[1] = SpawnAndApplyWeaponFromPrefab(_testSubWeaponPrefab, null);
                if (_equippedWeapons[1] != null)
                    _equippedWeapons[1].gameObject.SetActive(false);
            }
            else if (_subWeaponData != null && _subWeaponData.WeaponData?.WeaponPrefab != null)
            {
                _equippedWeapons[1] = SpawnAndApplyWeapon(_subWeaponData);
                if (_equippedWeapons[1] != null)
                    _equippedWeapons[1].gameObject.SetActive(false);
            }

            _currentWeaponIndex = 0;
            SwitchElementBehavior(_mainWeaponData);

            if (_inputReceiver != null && _equippedWeapons[0] != null)
                _inputReceiver.SetCurrentWeapon(_equippedWeapons[0]);
        }

        private void RegisterEquippedWeaponSkills()
        {
            if (!LocalPlayerStatusContext.TryGet(out PlayerStatus playerStatus))
            {
                Debug.LogWarning("[WeaponManager] PlayerStatus를 찾지 못해 무기 스킬을 등록하지 못했습니다.");
                return;
            }

            for (int i = 0; i < _equippedWeapons.Length; i++)
            {
                if (_equippedWeapons[i] != null && _equippedWeapons[i].Skill != null)
                    playerStatus.EnsureBehaviorRegistered(_equippedWeapons[i].Skill);
            }
        }

        private const int EnhanceLevelsPerAugmentBonus = 7;

        private int CalculateEnhanceAugmentBonus(int enhanceLevel)
        {
            return Mathf.Max(0, enhanceLevel) / EnhanceLevelsPerAugmentBonus;
        }

        private WeaponBase SpawnAndApplyWeapon(WeaponItem data)
        {
            if (data == null || data.WeaponData?.WeaponPrefab == null) return null;
            return SpawnAndApplyWeaponFromPrefab(data.WeaponData.WeaponPrefab, data);
        }

        private WeaponBase SpawnAndApplyWeaponFromPrefab(GameObject prefab, WeaponItem data)
        {
            if (prefab == null) return null;

            GameObject obj = Instantiate(prefab, _weaponMountPoint);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            WeaponBase weapon = obj.GetComponentInChildren<WeaponBase>();
            string debugName = data != null ? data.WeaponData.ItemName : prefab.name;

            if (data != null)
            {
                // 1. StatCalculator에 투사체 스탯 주입 (데미지, 스피드, 배율)
                var statCalc = obj.GetComponentInChildren<Projectile.StatCalculator>();
                if (statCalc != null)
                {
                    statCalc.ApplyWeaponData(data);
                    Debug.Log($"[WeaponManager] StatCalculator 데이터 적용: {debugName}");
                }
                else
                {
                    Debug.LogWarning($"[WeaponManager] StatCalculator 없음: {debugName}");
                }

                // 2. WeaponBase에 무기 수치 주입 (쿨타임, 크리티컬 배율 등)
                if (weapon != null)
                    weapon.ApplyWeaponData(data);

                // 3. 뽑기/합성 결과(SkillType)에 맞는 액티브 스킬 SO를 데이터베이스에서 찾아 주입.
                //    테스트 프리팹(data == null)은 이 과정을 타지 않고 프리팹에 꽂힌 기본 스킬을 그대로 쓴다.
                if (weapon != null && _weaponSkillDatabase != null)
                {
                    ActiveWeaponSkillSO resolvedSkill = _weaponSkillDatabase.GetSkill(data.SkillType);
                    weapon.SetSkill(resolvedSkill);
                    Debug.Log($"[WeaponManager] SkillType={data.SkillType} → Skill={(resolvedSkill != null ? resolvedSkill.BehaviorName : "없음")} 주입: {debugName}");
                }
                else if (weapon != null)
                {
                    Debug.LogWarning($"[WeaponManager] _weaponSkillDatabase가 비어있어 {debugName}의 SkillType({data.SkillType})을 스킬로 변환하지 못했습니다.");
                }
            }
            else
            {
                Debug.Log($"[WeaponManager] 테스트 프리팹 사용: {debugName} (인스펙터 기본값 적용)");
                // WeaponItem 데이터가 없으므로 SkillType을 조회할 수 없다.
                // 이 경우 프리팹에 인스펙터로 꽂아둔 weaponSkill(fallback)이 그대로 사용된다.
            }

            if (weapon != null)
            {
                weapon.Initialize();
            }
            else
            {
                Debug.LogWarning($"[WeaponManager] WeaponBase 없음: {debugName}");
            }

            return weapon;
        }

        private void InitializeShield()
        {
            if (_testShieldPrefab == null &&
                (_activeShieldData == null || _activeShieldData.ShieldData == null))
            {
                Debug.LogWarning("[WeaponManager] _activeShieldData 또는 ShieldData가 누락되어 방패를 생성하지 않는다.");
                return;
            }

            if (_playerOrbitCenter == null) _playerOrbitCenter = transform;

            if (_defendGestureAnalyzer == null)
            {
                GestureAnalyzer[] allAnalyzers = FindObjectsByType<GestureAnalyzer>(FindObjectsSortMode.None);
                foreach (var analyzer in allAnalyzers)
                {
                    if (analyzer.gameObject.name == "Defend_Zone")
                    {
                        _defendGestureAnalyzer = analyzer;
                        break;
                    }
                }

                if (_defendGestureAnalyzer == null)
                {
                    Transform targetTransform = transform.Find("Defend_Zone") ?? transform.root.Find("Defend_Zone");
                    if (targetTransform != null)
                        _defendGestureAnalyzer = targetTransform.GetComponent<GestureAnalyzer>();
                }

                if (_defendGestureAnalyzer == null)
                {
                    Debug.LogError("[WeaponManager] 'Defend_Zone' GestureAnalyzer를 찾을 수 없어 방패 초기화 중단.");
                    return;
                }
            }

            // 테스트용 프리팹이 연결되어 있으면 ShieldItem 대신 사용
            GameObject shieldPrefab = _testShieldPrefab != null
                ? _testShieldPrefab
                : _activeShieldData?.ShieldData?.ShieldPrefab;

            if (shieldPrefab == null)
            {
                Debug.LogWarning("[WeaponManager] ShieldPrefab 없음. 테스트용 프리팹을 연결하거나 ShieldItemData를 확인하세요.");
                return;
            }

            GameObject shieldObj = Instantiate(shieldPrefab, _shieldMountPoint);
            shieldObj.transform.localPosition = Vector3.zero;
            // localRotation 초기화 제거 → 프리팹에 설정된 회전값 유지

            SkillShield shieldBase = shieldObj.GetComponentInChildren<SkillShield>();
            if (shieldBase != null)
            {
                if (_testShieldPrefab != null)
                {
                    // 테스트 프리팹이면 ShieldDataSO 없으므로 궤도/입력 바인딩만 직접 처리
                    var orbitCtrl = shieldObj.GetComponentInChildren<ShieldOrbitController>(true);
                    if (orbitCtrl != null)
                        orbitCtrl.SetupOrbitCenter(_playerOrbitCenter);

                    var binder = shieldObj.GetComponentInChildren<ShieldSystemBinder>(true);
                    if (binder != null)
                        binder.InitializeBinding(_defendGestureAnalyzer, orbitCtrl);

                    Debug.Log("[WeaponManager] 테스트 방패 프리팹 사용: 궤도/입력 바인딩만 적용.");
                }
                else
                {
                    // ShieldItemData 오버로드 사용
                    shieldBase.ApplyShieldData(_activeShieldData.ShieldData, _playerOrbitCenter, _defendGestureAnalyzer);
                }

                shieldBase.Initialize();

                // 게이지 준비 여부(IsGaugeReady)를 매 프레임 확인하기 위해 참조를 들고 있는다.
                _equippedShield = shieldBase;

                if (_inputReceiver != null)
                {
                    _inputReceiver.SetCurrentShield(shieldBase);
                    Debug.Log("[WeaponManager] 방패 주입 및 연동 성공.");
                }

                if (_shieldSkillButton != null)
                {
                    _shieldSkillButton.onClick.RemoveAllListeners();
                    _shieldSkillButton.onClick.AddListener(shieldBase.ActivateSpecialAbility);
                    _shieldSkillButton.interactable = false; // 게이지가 다 찰 때까지는 비활성화 (Update에서 계속 갱신됨)
                    Debug.Log("[WeaponManager] 방패 스킬 버튼 OnClick 동적 할당 완료.");
                }
                else
                {
                    Debug.LogWarning("[WeaponManager] Shield Skill Button이 연결되지 않음.");
                }
            }
            else
            {
                Debug.LogError("[WeaponManager] 방패 프리팹 내부에서 SkillShield를 찾을 수 없다!");
            }
        }

        private void BindWeaponSkillButton()
        {
            if (_weaponSkillButton == null)
            {
                Debug.LogWarning("[WeaponManager] Weapon Skill Button이 연결되지 않음.");
                return;
            }

            _weaponSkillButton.onClick.RemoveAllListeners();
            _weaponSkillButton.onClick.AddListener(TryActivateWeaponSkill);

            RefreshWeaponSkillButtonInteractable();
        }

        private void RefreshWeaponSkillButtonInteractable()
        {
            if (_weaponSkillButton == null) return;

            ActiveWeaponSkillSO skill = CurrentWeapon != null ? CurrentWeapon.Skill : null;
            bool isOnCooldown = _weaponSkillCooldownTimers[_currentWeaponIndex] > 0f;

            _weaponSkillButton.interactable = skill != null && !isOnCooldown;
        }

        private void RefreshShieldSkillButtonInteractable()
        {
            if (_shieldSkillButton == null) return;

            bool isReady = _equippedShield != null &&
                           _equippedShield.IsGaugeReady &&
                           !_equippedShield.IsSpecialAbilityActive;

            _shieldSkillButton.interactable = isReady;
        }

        private void TryActivateWeaponSkill()
        {
            WeaponBase weapon = CurrentWeapon;
            ActiveWeaponSkillSO skill = weapon != null ? weapon.Skill : null;

            if (skill == null)
            {
                Debug.Log("[WeaponManager] 현재 무기에 연결된 스킬이 없습니다.");
                return;
            }

            if (_weaponSkillCooldownTimers[_currentWeaponIndex] > 0f)
            {
                Debug.Log($"[WeaponManager] 무기 스킬 쿨타임 중. 남은 시간: {_weaponSkillCooldownTimers[_currentWeaponIndex]:F1}초");
                return;
            }

            if (weapon.ProjectileFireHandler == null || weapon.FirePoint == null)
            {
                Debug.LogWarning("[WeaponManager] 현재 무기 참조가 없어 스킬을 발동할 수 없습니다.");
                return;
            }

            if (!LocalPlayerStatusContext.TryGet(out PlayerStatus playerStatus))
            {
                Debug.LogWarning("[WeaponManager] PlayerStatus를 찾지 못해 스킬 레벨을 조회할 수 없습니다.");
                return;
            }

            // 무기 장착 시 RegisterEquippedWeaponSkills()에서 최소 1레벨로 등록해두므로
            // 정상적인 상황이면 여기서 0이 나오면 안 된다. 혹시 등록이 누락된 경우를 대비해
            // 방어적으로 한 번 더 보장해준다 (런타임에 무기가 늦게 셋업된 경우 등).
            int level = playerStatus.GetBehaviorLevel(skill);
            if (level <= 0)
            {
                playerStatus.EnsureBehaviorRegistered(skill);
                level = playerStatus.GetBehaviorLevel(skill);
            }

            _weaponSkillCooldownTimers[_currentWeaponIndex] = skill.CooldownDuration;

            skill.Activate(
                this,
                weapon.ProjectileFireHandler,
                weapon.FirePoint,
                weapon.AimDirection,
                weapon.ChargeRatio,
                level);
        }

        private void SwitchElementBehavior(WeaponItem data)
        {
            if (_elementBehaviorDatabase == null) return;
            if (!LocalPlayerStatusContext.TryGet(out PlayerStatus playerStatus)) return;

            ItemPropertyType property = data != null ? data.Property : ItemPropertyType.None;
            ProjectileBehaviorSO newBehaviorSO = _elementBehaviorDatabase.GetBehavior(property);

            // 스왑해도 같은 원소면 굳이 빼고 다시 넣지 않는다 (증강으로 올려둔 레벨 보존)
            if (newBehaviorSO == _activeElementBehavior) return;

            if (_activeElementBehavior != null)
                playerStatus.RemoveBehavior(_activeElementBehavior);

            if (newBehaviorSO != null)
            {
                int enhanceLevel = data != null ? data.EnhanceLevel : 0;
                int bonusLevel = CalculateEnhanceAugmentBonus(enhanceLevel);
                int startLevel = 1 + bonusLevel;

                playerStatus.EnsureBehaviorRegisteredAtLevel(newBehaviorSO, startLevel);

                Debug.Log($"[WeaponManager] 원소 특성({property}) 시작 레벨 = 1 + ({enhanceLevel} / {EnhanceLevelsPerAugmentBonus} = {bonusLevel}) = {startLevel}");
            }

            _activeElementBehavior = newBehaviorSO;
        }

        public void SwapWeapon()
        {
            if (_equippedWeapons[0] == null || _equippedWeapons[1] == null)
            {
                Debug.LogWarning("[WeaponManager] 스왑할 무기가 슬롯에 부족합니다.");
                return;
            }

            _equippedWeapons[_currentWeaponIndex].Deactivate();
            _equippedWeapons[_currentWeaponIndex].gameObject.SetActive(false);

            _currentWeaponIndex = _currentWeaponIndex == 0 ? 1 : 0;

            WeaponItem newData = _currentWeaponIndex == 0 ? _mainWeaponData : _subWeaponData;
            SwitchElementBehavior(newData);

            if (CameraFXManager.Instance != null)
                CameraFXManager.Instance.ResetZoom();

            _equippedWeapons[_currentWeaponIndex].gameObject.SetActive(true);
            _equippedWeapons[_currentWeaponIndex].PlayEquipSound();

            if (_inputReceiver != null)
                _inputReceiver.SetCurrentWeapon(_equippedWeapons[_currentWeaponIndex]);

            // 무기가 바뀌었으니 스킬 버튼도 "지금 들고 있는 무기" 기준으로 인터랙터블 상태를 갱신한다.
            RefreshWeaponSkillButtonInteractable();

            Debug.Log($"[WeaponManager] 무기 스왑 완료. 현재 장착: {_equippedWeapons[_currentWeaponIndex].Type}");
        }

        #region 로비 연동용 공개 메서드
        public void SetLobbyWeapons(WeaponItem main, WeaponItem sub, ShieldItem shield = null)
        {
            _mainWeaponData = main;
            _subWeaponData = sub;
            if (shield != null)
                _activeShieldData = shield;
        }
        #endregion
    }
}