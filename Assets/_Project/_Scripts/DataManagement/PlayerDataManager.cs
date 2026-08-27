using Shield_Shot.DataManagement.InventorySystem;
using UnityEngine;

namespace Shield_Shot.DataManagement
{
    public class PlayerDataManager : MonoBehaviour
    {
        // 어디서나 접근할 수 있도록 싱글톤 프로퍼티 추가
        private static PlayerDataManager _instance;
        public static PlayerDataManager Instance => _instance;

        [SerializeField] private InventoryManager _inventoryManager;

        // 뒤끝 서버 데이터와 연동할 재화 및 진행도 필드 추가
        [Header("Player Game Resource Data")]
        public int gold = 0;
        public int diamond = 0;
        public int clearStageStep = 1;
        public int profileId = 0;
        public int frameId = 0;

        public int highestWave = 0;

        [Header("Database")]
        [SerializeField] private InfoItemDatabase _infoItemDatabase;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            DontDestroyOnLoad(gameObject);
        

            if (_inventoryManager == null)
            {
                _inventoryManager = FindFirstObjectByType<InventoryManager>();
            }
        }

        // 아이템의 슬롯 마크(Enum)를 검색하여 주무기 반환
        public Item GetMainWeapon()
        {
            if (_inventoryManager == null)
            {
                Debug.LogError("[PlayerDataManager] _inventoryManager가 null입니다!");
                return null;
            }
            if (_inventoryManager.Items == null)
            {
                Debug.LogError("[PlayerDataManager] _inventoryManager.Items 리스트가 초기화되지 않았습니다!");
                return null;
            }

            var found = _inventoryManager.Items.Find(x => x != null && x.CurrentSlotType == EquipSlotType.MainWeapon);
            if (found == null)
            {
                Debug.LogWarning("[PlayerDataManager] Items 리스트는 있으나 MainWeapon 슬롯에 장착된 아이템이 없습니다.");
            }
            return found;
        }

        // 보조무기 슬롯 검색
        public Item GetSubWeapon()
        {
            if (_inventoryManager == null || _inventoryManager.Items == null) return null;
            return _inventoryManager.Items.Find(x => x != null && x.CurrentSlotType == EquipSlotType.SubWeapon);
        }

        // 방패 슬롯 검색
        public Item GetShield()
        {
            if (_inventoryManager == null || _inventoryManager.Items == null) return null;
            return _inventoryManager.Items.Find(x => x != null && x.CurrentSlotType == EquipSlotType.Shield);
        }

        // 인게임 진입 시 사용할 데이터에 현재 장착 장비들을 넘겨주는 함수
        public void SavePlayerLoadData()
        {
            PlayerIngameLoadData.MainWeaponItem = GetMainWeapon();
            PlayerIngameLoadData.SubWeaponItem = GetSubWeapon();
            PlayerIngameLoadData.ShieldItem = GetShield();

            Debug.Log("인게임용 장착 데이터 전달 완료");
        }

        /// <summary> 현재 유저의 프로필 스프라이트를 반환 </summary>
        public Sprite GetCurrentProfileSprite()
        {
            if (_infoItemDatabase == null || _infoItemDatabase.prefileImages.Length == 0) return null;

            // 인덱스 범위 초과 예외 
            if (profileId < 0 || profileId >= _infoItemDatabase.prefileImages.Length) return _infoItemDatabase.prefileImages[0];

            return _infoItemDatabase.prefileImages[profileId];
        }

        /// <summary> 현재 유저의 프로필 테두리 스프라이트를 반환 </summary>
        public Sprite GetCurrentFrameSprite()
        {
            if (_infoItemDatabase == null || _infoItemDatabase.frameImages.Length == 0) return null;

            // 인덱스 범위 초과 예외 
            if (frameId < 0 || frameId >= _infoItemDatabase.frameImages.Length) return _infoItemDatabase.frameImages[0];

            return _infoItemDatabase.frameImages[frameId];
        }

        public void ClearData()
        {
            // 1. 단순 재화 및 진행도 초기화
            gold = 0;
            diamond = 0;
            clearStageStep = 1; // 1스테이지부터 시작하는 것이 기본값이라면 1로 설정
            profileId = 0;
            frameId = 0;
            highestWave = 0;

            // 2. 인벤토리 매니저 데이터 정리
            if (_inventoryManager != null)
            {
                // InventoryManager 클래스 내부에 정의된 데이터 초기화 메서드를 호출
                _inventoryManager.ClearInventory();
            }
            else
            {
                Debug.LogWarning("[PlayerDataManager] 초기화할 인벤토리 매니저가 없습니다.");
            }

            // 3. 인게임용 정적 데이터 정리
            PlayerIngameLoadData.MainWeaponItem = null;
            PlayerIngameLoadData.SubWeaponItem = null;
            PlayerIngameLoadData.ShieldItem = null;

            Debug.Log("[PlayerDataManager] 모든 유저 데이터가 초기화되었습니다.");
        }
    }
}