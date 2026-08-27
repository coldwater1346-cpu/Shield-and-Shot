
//using Shield_Shot.DataManagement.InventorySystem;
//using Shield_Shot.DataManagement.Json;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;

//namespace Shield_Shot.DataManagement.Json
//{
//    public class DataManager : MonoBehaviour
//    {
//        [SerializeField] private PlayerDataManager playerDataManager;
//        [SerializeField] private InventoryManager inventoryManager;

//        private string _saveFilePath;
//        private bool _isDataLoaded = false; // 데이터를 성공적으로 로드했을 때만 저장 허용

//        private void Awake()
//        {
//            _saveFilePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

//            if (playerDataManager == null) 
//                playerDataManager = FindFirstObjectByType<PlayerDataManager>();
//            if (inventoryManager == null) 
//                inventoryManager = FindFirstObjectByType<InventoryManager>();

//            // 게임 시작 시 무조건 로드 프로세스 실행
//            LoadGameData();
//        }
//        private void OnEnable()
//        {
//            // 인벤토리의 데이터가 바뀌면 자동으로 저장하도록 등록
//            if (inventoryManager != null)
//            {
//                inventoryManager.OnInventoryChanged += SaveGameData;
//            }
//        }

//        private void OnDisable()
//        {
//            // 메모리 누수 방지 해제
//            if (inventoryManager != null)
//            {
//                inventoryManager.OnInventoryChanged -= SaveGameData;
//            }
//        }
//        public void LoadGameData()
//        {
//            //  저장 파일이 아예 없다면 
//            if (!File.Exists(_saveFilePath))
//            {
//                Debug.LogWarning(" 세이브 파일이 없습니다. 기본 데이터를 파일로 생성합니다.");

//                // 기본 제공  JSON 데이터 정의
//                string defaultJson = @"{
//    ""gold"": 1000,
//    ""diamond"": 0,
//    ""currentStage"": 1,
//    ""ownedItems"": [
//        {
//            ""itemId"": ""WP_01"",
//            ""uniqueId"": ""default-weapon-01"",
//            ""enhanceLevel"": 0,
//            ""isEquipped"": true
//        },
//        {
//            ""itemId"": ""WP_02"",
//            ""uniqueId"": ""default-weapon-02"",
//            ""enhanceLevel"": 0,
//            ""isEquipped"": true
//        },
// {
//            ""itemId"": ""SH_01"",
//            ""uniqueId"": ""default-shield-01"",
//            ""enhanceLevel"": 0,
//            ""isEquipped"": true
//        }
//    ]
//}";

//                // 최초 1회 파일 생성
//                File.WriteAllText(_saveFilePath, defaultJson);
//                Debug.Log(" 최초 세이브 파일 생성 완료!");
//            }

//            // 인벤토리 리스트에 로드된 아이템 데이터 넣기
//            string jsonString = File.ReadAllText(_saveFilePath);
//            TotalUserData loadedData = JsonUtility.FromJson<TotalUserData>(jsonString);

//            Debug.Log($"골드: {loadedData.gold}, 아이템 수: {loadedData.ownedItems.Count}");

//            // 인벤토리 매니저에게 실체화 명령
//            if (inventoryManager != null)
//            {
//                inventoryManager.LoadInventoryFromSaveData(loadedData.ownedItems);
//            }

            
//            _isDataLoaded = true;
//        }

//        public void SaveGameData()
//        {
            
//            if (!_isDataLoaded) return;

//            //  저장용 데이터를 담을 클래스 생성
//            TotalUserData saveData = new TotalUserData();

            
//            if (playerDataManager != null)
//            {

//                saveData.gold = 1000;
//                saveData.diamond = 0;
//                saveData.currentStage = 1;
//            }

            
//            if (inventoryManager != null && inventoryManager.Items != null)
//            {
//                saveData.ownedItems = new List<SaveItemInfo>();

//                foreach (Item item in inventoryManager.Items)
//                {
//                    if (item == null) continue; 

//                    SaveItemInfo itemInfo = new SaveItemInfo();
//                    itemInfo.itemId = item.ItemData.Id;         // 스크립터블 오브젝트 고유 ID (WP_01 등)
//                    itemInfo.uniqueId = item.UniqueID;          // 인스턴스 고유 GUID
//                    itemInfo.enhanceLevel = item.EnhanceLevel;  // 현재 강화 레벨
//                    itemInfo.isEquipped = item.IsEquipped;      // 바뀐 장착 여부 상태 

//                    saveData.ownedItems.Add(itemInfo);
//                }
//            }

//            // 5. JSON 문자열로 변환하여 덮어쓰기
//            string jsonString = JsonUtility.ToJson(saveData, true);
//            File.WriteAllText(_saveFilePath, jsonString);

//            Debug.Log($" 실시간 데이터 저장  (아이템 수: {saveData.ownedItems.Count})");
//        }
//        //private void OnApplicationQuit()
//        //{
//        //    SaveGameData();
//        //}
//    }
//}