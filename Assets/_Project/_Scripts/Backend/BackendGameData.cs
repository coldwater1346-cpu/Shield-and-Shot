using BackEnd;
using LitJson; // 뒤끝 데이터 파싱용
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Shield_Shot.DataManagement.InventorySystem;
using Shield_Shot.DataManagement;
using System.Linq;

namespace Shield_Shot.NetworkCore
{
    // = 인벤토리에서 사용하는 아이템 세이브 데이터 
    [System.Serializable]
    public class SaveItemInfo
    {
        public string itemId;       // 아이템 고유 코드 (예: weapon_01)
        public string uniqueId;     // 인벤토리 내 개별 식별용 GUID
        public int enhanceLevel;    // 강화 수치
        public bool isEquipped;     // 장착 여부
        public ItemPropertyType property;
        public WeaponSkillType skillType; // 무기 스킬 타입 추가

    }

    // 유저 한 명의 모든 세이브 데이터를 담는 바구니
    public class UserData
    {
        public int gold = 0;
        public int diamond = 0;
        public int clearStageStep = 1; // 클리어한 스테이지 단계
        public int profileId = 0; // 프로필 이미지 ID 추가
        public int frameId = 0; // 프로필 프레임 ID 추가

        // 무한 모드 최고 웨이브
        public int highestWave = 0;

        // 실제 인벤토리 리스트 (기존 구조 연동)
        public List<SaveItemInfo> ownedItems = new List<SaveItemInfo>();
    }

    public class BackendGameData
    {
        private static BackendGameData _instance = null;
        public static BackendGameData Instance
        {
            get
            {
                if (_instance == null) _instance = new BackendGameData();
                return _instance;
            }
        }

        public static UserData userData;
        private string gameDataRowInDate = string.Empty; // 서버 수정용 고유 주소값

        // 1. 최초 데이터 생성 (회원가입 직후 혹은 세이브가 없을 때 최초 1번 실행)
        public void GameDataInsert()
        {
            if (userData == null)
            {
                userData = new UserData();
            }

            // 1. 기본 재화 및 스테이지 정보 할당 (기존 JSON의 상단 필드와 매치)
            userData.gold = 200;
            userData.diamond = 10;
            userData.clearStageStep = 0;
            userData.ownedItems.Clear();
            //프로필 이미지 번호 
            userData.profileId = 0;
            userData.frameId = 0;
            userData.highestWave = 0;

            // 2. 초기 지급 아이템 목록 생성 및 가방에 추가 (기존 JSON의 ownedItems 배열과 매치)

            // 첫 번째 무기 (WP_01)
            SaveItemInfo item1 = new SaveItemInfo();
            item1.itemId = "WP_01";
            item1.uniqueId = "default-weapon-01";
            item1.enhanceLevel = 0;
            item1.isEquipped = true;
            userData.ownedItems.Add(item1);

            // 두 번째 무기 (WP_02)
            SaveItemInfo item2 = new SaveItemInfo();
            item2.itemId = "WP_02";
            item2.uniqueId = "default-weapon-02";
            item2.enhanceLevel = 0;
            item2.isEquipped = true;
            userData.ownedItems.Add(item2);

            // 세 번째 방패 (SH_01)
            SaveItemInfo item3 = new SaveItemInfo();
            item3.itemId = "SH_01";
            item3.uniqueId = "default-shield-03";
            item3.enhanceLevel = 0;
            item3.isEquipped = true;
            userData.ownedItems.Add(item3);


            //강화 체험용 아이템 추가2
            SaveItemInfo item4 = new SaveItemInfo();
            item4.itemId = "WP_01";
            item4.uniqueId = "default-weapon-05";
            item4.enhanceLevel = 0;
            item4.isEquipped = false;
            userData.ownedItems.Add(item4);






            //// 주입할 아이템의 총 개수 (27개 혹은 n개)
            //int totalCount = 27;

            //for (int i = 1; i <= totalCount; i++)
            //{
            //    SaveItemInfo testItem = new SaveItemInfo();

            //    // WP_01, WP_02 ... WP_27 순서대로 자동 생성 (:D2는 두 자리 숫자로 포맷팅)
            //    testItem.itemId = $"WP_{i:D2}";

            //    // 인벤토리 식별용 고유 ID
            //    testItem.uniqueId = $"test-weapon-{i:D2}";
            //    testItem.enhanceLevel = 0;

            //    // 장착 여부 설정 (필요 시 주석 해제)
            //    if (i == 1 || i == 2 || i == 3)
            //    {
            //        // testItem.isEquipped = true;
            //    }
            //    else
            //    {
            //        // testItem.isEquipped = false;
            //    }

            //    userData.ownedItems.Add(testItem);
            //}
            //// [테스트 빌드] 서버에 있는 WP_01, WP_02, WP_03 아이디를 순서대로 n개 주입.
            //for (int i = 1; i <= 30; i++)
            //{
            //    SaveItemInfo testItem = new SaveItemInfo();

            //    // 반복
            //    int remainder = i % 3;
            //    if (remainder == 1)
            //    {
            //        testItem.itemId = "WP_01";
            //    }
            //    else if (remainder == 2)
            //    {
            //        testItem.itemId = "WP_02";
            //    }
            //    else // remainder == 0 일 때
            //    {
            //        testItem.itemId = "WP_03";
            //    }

            //    // 인벤토리 식별용 고유 ID는 1~20까지 안전하게 부여
            //    testItem.uniqueId = $"test-weapon-{i:D2}";
            //    testItem.enhanceLevel = 0;


            //    if (i == 1 || i == 2 || i == 3)
            //    {
            //       // testItem.isEquipped = true;
            //    }
            //    else
            //    {
            //        //testItem.isEquipped = false;
            //    }

            //    userData.ownedItems.Add(testItem);





            // 3. 서버에 보낼 규격 패키지(Param) 생성 후 데이터 삽입
            Param param = new Param();
            param.Add("gold", userData.gold);
            param.Add("diamond", userData.diamond);
            param.Add("clearStageStep", userData.clearStageStep);
            param.Add("profileId", userData.profileId);
            param.Add("frameId", userData.frameId);
            param.Add("ownedItems", userData.ownedItems);
            param.Add("highestWave", userData.highestWave);


            Debug.Log("서버에 신규 유저 초기 데이터를 생성합니다.");
            var bro = Backend.GameData.Insert("USER_DATA", param);

            if (bro.IsSuccess())
            {
                Debug.Log("초기 데이터 생성 성공.");
                gameDataRowInDate = bro.GetInDate();
            }
            else
            {
                Debug.LogError("초기 데이터 생성 실패 : " + bro);
            }
        }

        // 2. 세이브 데이터 불러오기 (게임 시작 및 로그인 성공 시 호출)
        public void GameDataGet()
        {
            Debug.Log("서버에서 게임 정보를 조회합니다.");
            var bro = Backend.GameData.GetMyData("USER_DATA", new Where());

            if (!bro.IsSuccess())
            {
                Debug.LogError("게임 정보 조회 실패 : " + bro);
                return;
            }

            JsonData gameDataJson = bro.FlattenRows();

            // 데이터가 없다면 신규 유저이므로 초기 데이터 생성(Insert) 진행
            if (gameDataJson.Count <= 0)
            {
                Debug.LogWarning("기존 데이터가 없습니다. 새로 생성합니다.");
                GameDataInsert();
                return;
            }

            // 데이터가 있다면 가방(userData)에 파싱해서 매핑
            gameDataRowInDate = gameDataJson[0]["inDate"].ToString();
            userData = new UserData();

            userData.gold = int.Parse(gameDataJson[0]["gold"].ToString());
            userData.diamond = int.Parse(gameDataJson[0]["diamond"].ToString());
            userData.clearStageStep = int.Parse(gameDataJson[0]["clearStageStep"].ToString());
            //프로필 이미지 번호 추가
            userData.profileId = int.Parse(gameDataJson[0]["profileId"].ToString());
            userData.frameId = int.Parse(gameDataJson[0]["frameId"].ToString());

            // 복잡한 인벤토리 리스트(List<SaveItemInfo>)를 뒤끝 리턴값에서 객체로 자동 변환
            string itemsJson = gameDataJson[0]["ownedItems"].ToJson();
            userData.ownedItems = JsonUtility.FromJson<SerializationWrapper<SaveItemInfo>>("{\"items\":" + itemsJson + "}").items;

            JsonData row = gameDataJson[0];

            if (row.Keys.Contains("highestWave"))
            {
                userData.highestWave =
                    int.Parse(row["highestWave"].ToString());
            }
            else
            {
                // 기존 계정 호환
                userData.highestWave = 0;
            }

            if (userData.ownedItems != null)
            {
                //
            }
            Debug.Log("서버 데이터 불러오기 완료.");
        }

        // 3. 데이터 덮어쓰기 (스테이지 클리어, 획득/소비 시점 호출)
        public void GameDataUpdate()
        {
            if (userData == null) return;

            Param param = new Param();
            param.Add("gold", userData.gold);
            //param.Add("diamond", userData.diamond);
            param.Add("clearStageStep", userData.clearStageStep);
            //프로필 이미지 번호 추가
            param.Add("profileId", userData.profileId);
            param.Add("frameId", userData.frameId);
            param.Add("ownedItems", userData.ownedItems);

            param.Add("highestWave", userData.highestWave);

            BackendReturnObject bro = null;

            if (string.IsNullOrEmpty(gameDataRowInDate))
            {
                bro = Backend.GameData.Update("USER_DATA", new Where(), param);
            }
            else
            {
                bro = Backend.GameData.UpdateV2("USER_DATA", gameDataRowInDate, Backend.UserInDate, param);
            }

            if (bro.IsSuccess())
            {
                Debug.Log("서버에 최신 세이브 데이터 업로드 완료.");
            }
            else
            {
                Debug.LogError("서버 데이터 업로드 실패 : " + bro);
            }
        }
        // 매개변수를 완전히 없애고, 호출되면 무조건 현재 로컬의 최신 데이터를 가져와 저장하는 함수
        public void GameDataUpdateAsync()
        {
            if (userData == null) return;

            // 1. 저장 직전, PlayerDataManager(로컬)의 최신 데이터들을 뒤끝 가방(userData)으로 싹 복사
            var localData = PlayerDataManager.Instance;

            userData.gold = localData.gold;
            userData.diamond = localData.diamond;
            userData.clearStageStep = localData.clearStageStep;
            userData.profileId = localData.profileId;
            userData.frameId = localData.frameId;
            userData.highestWave = localData.highestWave;

            // 2. 인벤토리 리스트도 현재 로컬의 최신 상태를 SaveItemInfo 규격으로 파싱해서 대입
            userData.ownedItems.Clear();

            // inventoryManager나 playerDataManager를 통해 현재 인게임 아이템 리스트를 가져옵니다.
            // 여기서는 inventoryManager가 실시간 리스트(_items)를 들고 있으므로 이를 활용합니다.
            var inventory = UnityEngine.Object.FindFirstObjectByType<InventoryManager>();
            if (inventory != null)
            {
                int i = 0; //  인덱스용 변수를 루프 밖 선언

                foreach (var item in inventory.Items)
                {
                    if (item == null) continue;

                    SaveItemInfo info = new SaveItemInfo();
                    info.itemId = item.ItemData.itemID;
                    info.uniqueId = item.UniqueID;
                    info.enhanceLevel = item.EnhanceLevel;
                    info.isEquipped = item.IsEquipped;
                    info.property = item.Property;

                    if (item.ItemData.ItemType == ItemType.Weapon)
                    {

                        var weapon = item as WeaponItem;

                        if (weapon != null)
                        {

                            info.skillType = weapon.SkillType;
                        }
                        else
                        {
                            info.skillType = WeaponSkillType.None;
                        }
                    }
                    else
                    {

                        info.skillType = WeaponSkillType.None;
                    }

                    userData.ownedItems.Add(info);

                    i++; // 아이템을 하나 담았으니 다음 순서를 위해 번호 증가
                }
            }

            // 3. 최신화된 가방 내용을 Param에 담기
            Param param = new Param();
            param.Add("gold", userData.gold);
            //param.Add("diamond", userData.diamond);
            param.Add("clearStageStep", userData.clearStageStep);
            //프로필 이미지 번호 추가
            param.Add("profileId", userData.profileId);
            param.Add("frameId", userData.frameId);
            param.Add("ownedItems", userData.ownedItems);
            param.Add("highestWave", userData.highestWave);

            // 4. 뒤끝 서버에 비동기 업로드 요청
            if (string.IsNullOrEmpty(gameDataRowInDate))
            {
                Backend.GameData.Update("USER_DATA", new Where(), param, callback =>
                {
                    if (callback.IsSuccess()) Debug.Log("서버 비동기 저장 완료");
                    else Debug.LogError("서버 비동기 저장 실패: " + callback);
                });
            }
            else
            {
                Backend.GameData.UpdateV2("USER_DATA", gameDataRowInDate, Backend.UserInDate, param, callback =>
                {
                    if (callback.IsSuccess()) Debug.Log("서버 비동기 저장 완료(V2)");
                    else Debug.LogError("서버 비동기 저장 실패: " + callback);
                });
            }
        }

        // 뒤끝의 리스트 구조를 유니티 JsonUtility로 파싱하기 위한 간단한 헬퍼 클래스
        [System.Serializable]
        public class SerializationWrapper<T>
        {
            public List<T> items;
        }
    }

}