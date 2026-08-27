using BackEnd;
using Shield_Shot.DataManagement;
using Shield_Shot.InputSystem.Data;
using Shield_Shot.DataManagement.InventorySystem;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


namespace Shield_Shot.DataManagement.DataParsing
{
    //가장 먼저 실행
    [DefaultExecutionOrder(-100)]
    public class ItemDataParsingManager : MonoBehaviour
    {
        public static ItemDataParsingManager Instance { get; private set; }


        //아이템 정보 테이블 딕셔너리
        private readonly Dictionary<string, WeaponItemData> _weaponTable = new Dictionary<string, WeaponItemData>();
        private readonly Dictionary<string, ShieldItemData> _shieldTable = new Dictionary<string, ShieldItemData>();

        // 강화 비용 테이블 딕셔너리 : 레벨(int)을 Key로, 코스트 데이터를 Value로 가짐
        private readonly Dictionary<int, EnhanceCostData> _enhanceCostTable = new Dictionary<int, EnhanceCostData>();

        //판매 금액 테이블 딕셔너리 : 레벨(강화수치)를 key, 등급별 금액을 value
        private readonly Dictionary<int, ItemPriceData> _priceTable = new Dictionary<int, ItemPriceData>();

        //합성 확률 데이터를 저장할 딕셔너리 (Key: 타겟 등급, Value: 확률 데이터)
        private readonly Dictionary<ItemGradeType, ItemCombineData> _combineTable = new Dictionary<ItemGradeType, ItemCombineData>();

        //등급별 속성 부여 확률 딕셔너리
        private readonly Dictionary<ItemGradeType, PropertyRateData> _propertyRateTable = new Dictionary<ItemGradeType, PropertyRateData>();

        // 1차 뽑기 확률 테이블 로드 완료 여부 플래그
        private bool _isGachaTableLoaded = false;

        public bool IsGachaTableLoaded => _isGachaTableLoaded;



        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            InitializeLinks();
        }


        private void InitializeLinks()
        {
            LinkTable(_weaponTable);
        }

        private void LinkTable<T>(Dictionary<string, T> table) where T : ITableData
        {
            foreach (T data in table.Values)
            {
                data.LinkData(this);
            }
        }

        //Id로 무기데이터 가져오기
        public WeaponItemData GetWeaponData(string id)
        {
            if (_weaponTable.TryGetValue(id, out WeaponItemData data))
                return data;

            Debug.Log($" WeaponData 없음 : {id}");

            return null;
        }

        public ShieldItemData GetShieldData(string id)
        {
            if (_shieldTable.TryGetValue(id, out ShieldItemData data))
                return data;

            Debug.Log($" ShieldData 없음 : {id}");

            return null;
        }
        /// <summary>
        /// 무기/방패 테이블을 모두 조회하여 해당 ID의 아이템 데이터(ItemData)를 반환합니다.
        /// </summary>
        public ItemData GetItemData(string id)
        {
            // 1. 무기 테이블에서 먼저 찾아보기
            if (_weaponTable.TryGetValue(id, out WeaponItemData weaponData))
            {
                return weaponData; // 자식 객체를 부모(ItemData) 타입으로 안전하게 반환
            }

            // 2. 없으면 방패 테이블에서 찾아보기
            if (_shieldTable.TryGetValue(id, out ShieldItemData shieldData))
            {
                return shieldData; // 자식 객체를 부모(ItemData) 타입으로 안전하게 반환
            }

            Debug.LogError($"[Gacha Error] 테이블에서 아이템 데이터를 찾을 수 없습니다: {id}");
            return null; // 찾지 못했다면 안전하게 null 반환
        }

        // 서버에서 받아온 무기 차트 데이터를 파싱하여 테이블에 등록하는 함수
        public void LoadWeaponTableFromServer(string chartId)
        {
            Debug.Log($"{chartId} 무기 차트 서버 다운로드 요청");
            var bro = BackEnd.Backend.Chart.GetChartContents(chartId);

            if (bro.IsSuccess() == false)
            {
                Debug.LogError("무기 차트 로드 실패 : " + bro);
                return;
            }

            _weaponTable.Clear();

            foreach (LitJson.JsonData gameData in bro.FlattenRows())
            {
                // 엑셀에 섞여있는 아이템 중 "Weapon" 타입만 골라내기 위한 필터링
                string itemTypeStr = gameData["ItemType"].ToString();
                if (itemTypeStr != "Weapon")
                    continue;

                // 런타임 아이템 SO 생성
                WeaponItemData data = ScriptableObject.CreateInstance<WeaponItemData>();

                // 1. 스크립트 변수명 = gameData["서버엑셀헤더"]
                data.itemID = gameData["ItemID"].ToString();
                data.ItemName = gameData["ItemName"].ToString();
                data.ItemType = (ItemType)System.Enum.Parse(typeof(ItemType), itemTypeStr);
                data.ItemGradeType = (ItemGradeType)System.Enum.Parse(typeof(ItemGradeType), gameData["ItemGradeType"].ToString());
                data.Description = gameData["Description"].ToString();
                data.BaseDamage = int.Parse(gameData["Damage"].ToString());

                // 크리티컬/차징 관련 스탯도 차트에서 읽어와야 활마다 다른 값이 적용됨
                if (gameData.Keys.Contains("CriticalDamageMultiplier") && gameData["CriticalDamageMultiplier"] != null
                    && float.TryParse(gameData["CriticalDamageMultiplier"].ToString().Trim(), out float critMul))
                {
                    data.CriticalDamageMultiplier = critMul;
                }
                if (gameData.Keys.Contains("MaxDamageMultiplier") && gameData["MaxDamageMultiplier"] != null
                    && float.TryParse(gameData["MaxDamageMultiplier"].ToString().Trim(), out float maxDmgMul))
                {
                    data.MaxDamageMultiplier = maxDmgMul;
                }
                if (gameData.Keys.Contains("MaxSpeedMultiplier") && gameData["MaxSpeedMultiplier"] != null
                    && float.TryParse(gameData["MaxSpeedMultiplier"].ToString().Trim(), out float maxSpdMul))
                {
                    data.MaxSpeedMultiplier = maxSpdMul;
                }
                if (gameData.Keys.Contains("Speed") && gameData["Speed"] != null
                    && float.TryParse(gameData["Speed"].ToString().Trim(), out float baseSpeed))
                {
                    data.BaseSpeed = baseSpeed;
                }
                if (gameData.Keys.Contains("FireRate") && gameData["FireRate"] != null
                    && float.TryParse(gameData["FireRate"].ToString().Trim(), out float fireRate))
                {
                    data.FireRate = fireRate;
                }

                //  [추가] 뒤끝 엑셀에 새로 추가한 WeaponType 열을 읽어서 파싱합니다.
                if (gameData.Keys.Contains("WeaponType") && gameData["WeaponType"] != null)
                {
                    string weaponTypeStr = gameData["WeaponType"].ToString().Trim();


                    if (System.Enum.TryParse(weaponTypeStr, out WeaponType wType))
                    {
                        data.weaponType = wType;
                    }
                    else
                    {
                        data.weaponType = WeaponType.None;
                        Debug.LogWarning($"[Parsing Warning] ItemID {data.itemID}의 WeaponType({weaponTypeStr})을 파싱할 수 없어 None으로 설정합니다.");
                    }
                }
                else
                {
                    data.weaponType = WeaponType.None;
                    Debug.LogError($"[Parsing Error] 엑셀에 'WeaponType' 컬럼이 없거나 데이터가 비어있습니다. ID: {data.itemID}");
                }

                // 2. [리소스 경로 매칭] 글자를 Resources 폴더 안의 실제 에셋으로 로드
                string iconPath = gameData["Icon"].ToString();     // 예: "Sprites/Bow1"
                string prefabPath = gameData["Prefab"].ToString(); // 비어있지 않다면 경로

                data.Icon = Resources.Load<Sprite>(iconPath);
                data.WeaponPrefab = Resources.Load<GameObject>(prefabPath);

                // 3. 중복 검사 및 딕셔너리 대입
                if (_weaponTable.ContainsKey(data.itemID))
                {
                    Debug.Log($"중복 무기 ID 존재 : {data.itemID}");
                    continue;
                }

                _weaponTable.Add(data.itemID, data);
            }

            LinkTable(_weaponTable);
            Debug.Log($"서버 무기 테이블 로드 완료 : {_weaponTable.Count}");
        }

        // 서버에서 받아온 방패 차트 데이터를 파싱하여 테이블에 등록하는 함수
        public void LoadShieldTableFromServer(string chartId)
        {
            Debug.Log($"{chartId} 방패 차트 서버 다운로드 요청");
            var bro = BackEnd.Backend.Chart.GetChartContents(chartId);

            if (bro.IsSuccess() == false)
            {
                Debug.LogError("방패 차트 로드 실패 : " + bro);
                return;
            }

            _shieldTable.Clear();

            foreach (LitJson.JsonData gameData in bro.FlattenRows())
            {
                // 엑셀에 섞여있는 아이템 중 "Shield" 타입만 골라내기 위한 필터링
                string itemTypeStr = gameData["ItemType"].ToString();
                if (itemTypeStr != "Shield")
                    continue;

                ShieldItemData data = ScriptableObject.CreateInstance<ShieldItemData>();

                //  스크립트 필드 규격에 맞춰 대입
                data.itemID = gameData["ItemID"].ToString();
                data.ItemName = gameData["ItemName"].ToString();
                data.ItemType = (ItemType)System.Enum.Parse(typeof(ItemType), itemTypeStr);
                data.ItemGradeType = (ItemGradeType)System.Enum.Parse(typeof(ItemGradeType), gameData["ItemGradeType"].ToString());
                data.Description = gameData["Description"].ToString();

                //// 방패 스크립트에 선언된 고유 필드 명에 맞춰 대입 (예: defense 등)

                string iconPath = gameData["Icon"].ToString();
                string prefabPath = gameData["Prefab"].ToString();

                // 리소스 경로 로드 매칭
                data.Icon = Resources.Load<Sprite>(iconPath);
                data.ShieldPrefab = Resources.Load<GameObject>(prefabPath);

                if (_shieldTable.ContainsKey(data.itemID))
                {
                    Debug.Log($"중복 방패 ID 존재 : {data.itemID}");
                    continue;
                }

                _shieldTable.Add(data.itemID, data);
            }

            LinkTable(_shieldTable);
            Debug.Log($"서버 방패 테이블 로드 완료 : {_shieldTable.Count}");
        }

        // 서버에서 강화 비용 로드하는 메소드
        public void LoadEnhanceCostTableFromServer(string chartId)
        {
            var bro = BackEnd.Backend.Chart.GetChartContents(chartId);
            if (!bro.IsSuccess()) return;

            _enhanceCostTable.Clear();

            foreach (LitJson.JsonData gameData in bro.FlattenRows())
            {
                EnhanceCostData data = new EnhanceCostData
                {
                    level = int.Parse(gameData["ItemEnhanceLevel"].ToString()),
                    cost = int.Parse(gameData["Cost"].ToString())
                };

                // 레벨을 Key로 해서 딕셔너리에 추가
                _enhanceCostTable.Add(data.level, data);
            }
            Debug.Log($"서버 강화 비용 테이블 로드 완료: {_enhanceCostTable.Count}레벨까지 등록됨");
        }

        //  현재 레벨을 넣으면 딕셔너리에서 바로 강화 비용을 찾아서 반환하는 함수
        public int GetEnhanceCost(int currentLevel)
        {
            if (_enhanceCostTable.TryGetValue(currentLevel, out EnhanceCostData data))
            {
                return data.cost;
            }

            //  엑셀에 없는 고레벨 예외 처리 
            return 100 + (currentLevel * 50);
        }

        public int GetWeaponTableCount()
        {
            return _weaponTable != null ? _weaponTable.Count : 0;
        }

        //서버에서 아이템 판매 데이터 파싱하여 테이블에 등록
        public void LoadItemPriceTableFromServer(string chartId)
        {
            var bro = BackEnd.Backend.Chart.GetChartContents(chartId);
            if (!bro.IsSuccess()) return;

            _priceTable.Clear();

            foreach (LitJson.JsonData gameData in bro.FlattenRows())
            {
                // 1. 공백을 0으로 바꿈
                int GetSafeInt(string columnName)
                {
                    if (gameData.Keys.Contains(columnName) && gameData[columnName] != null)
                    {
                        string rawValue = gameData[columnName].ToString().Trim();

                        // 값이 숫자로 변환 가능하면 그 값을 반환
                        if (int.TryParse(rawValue, out int result))
                        {
                            return result;
                        }
                    }
                    // 칸이 비어있거나("") 숫자가 아니면 무조건 0 반환
                    return 0;
                }


                ItemPriceData data = new ItemPriceData
                {
                    ItemEnhanceLevel = GetSafeInt("ItemEnhanceLevel"),
                    c = GetSafeInt("C"),
                    uc = GetSafeInt("UC"),
                    rare = GetSafeInt("Rare"),
                    sr = GetSafeInt("SR"),
                    ssr = GetSafeInt("SSR"),
                    ur = GetSafeInt("UR")
                };

                // 딕셔너리에 추가
                _priceTable.Add(data.ItemEnhanceLevel, data);
            }
            Debug.Log($"서버 아이템 판매 비용 테이블 로드 완료: {_priceTable.Count}레벨까지 등록됨");
        }


        // 현재 레벨과 등급(Enum)을 넣으면 딕셔너리에서 판매 비용을 반환하는 함수
        public int GetItemSalePrice(int currentLevel, ItemGradeType grade)
        {
            // 예외 수식 다 빼고, 테이블에 있는 값만 정확하게 매칭
            if (_priceTable.TryGetValue(currentLevel, out ItemPriceData data))
            {
                return data.GetPriceByGrade(grade);
            }

            Debug.LogError($"[PriceTable] 해당 강화 수치의 데이터가 테이블에 없습니다: {currentLevel}강");
            return 0;
        }

        public int GetItemPriceTableCount()
        {
            return _priceTable != null ? _priceTable.Count : 0;
        }


        /// <summary>
        /// 뒤끝 서버 아이템 합성 확률 테이블(차트)을 로드
        /// </summary>
        public void LoadItemCombineTableFromServer(string chartId)
        {
            var bro = BackEnd.Backend.Chart.GetChartContents(chartId);

            if (!bro.IsSuccess())
            {
                Debug.Log($"CombineTable 로드 실패 : {bro.GetStatusCode()}");
                return;
            }

            _combineTable.Clear();

            foreach (LitJson.JsonData gameData in bro.FlattenRows())
            {
                int GetSafeInt(string columnName)
                {
                    if (gameData.Keys.Contains(columnName) && gameData[columnName] != null)
                    {
                        string rawValue = gameData[columnName].ToString().Trim();
                        if (int.TryParse(rawValue, out int result))
                        {
                            return result;
                        }
                    }
                    return 0;
                }
                // 엑셀의 TargetGrade 문자열을 ItemGradeType으로 파싱
                string gradeStr = gameData["TargetGrade"].ToString().Trim();
                if (!System.Enum.TryParse(gradeStr, out ItemGradeType gradeType))
                {
                    Debug.LogWarning($"[CombineTable] 알 수 없는 등급 문자열 감지: {gradeStr}");
                    continue;
                }

                // 데이터 객체 조립
                ItemCombineData combineData = new ItemCombineData
                {
                    TargetGrade = gradeType,
                    SuccessRate = GetSafeInt("SuccessRate"),
                    PropertyApplyRate = GetSafeInt("PropertyApplyRate")
                };

                // 딕셔너리에 등록
                if (!_combineTable.ContainsKey(gradeType))
                {
                    _combineTable.Add(gradeType, combineData);
                }
            }

            Debug.Log("[CombineTable] 서버 합성 확률 테이블 로드 완료");
        }

        /// <summary>
        /// 합성 확률 데이터를 가져오는 메소드
        /// </summary>
        public ItemCombineData GetCombineData(ItemGradeType grade)
        {
            if (_combineTable.TryGetValue(grade, out var data))
            {
                return data;
            }

            Debug.LogWarning($"[CombineTable] 해당 등급의 합성 데이터가 테이블에 없습니다: {grade}");
            return null;
        }

        /// <summary>
        /// 특정 등급의 무기 데이터 중 하나를 무작위로 가져옵니다.
        /// </summary>
        public WeaponItemData GetRandomWeaponDataByGrade(ItemGradeType grade)
        {
            List<WeaponItemData> candidates = new List<WeaponItemData>();

            // 예시: 파싱 매니저가 가진 전체 무기 딕셔너리나 리스트를 순회하며 등급 매칭
            foreach (var weapon in _weaponTable.Values)
            {
                if (weapon.ItemGradeType == grade)
                    candidates.Add(weapon);
            }

            if (candidates.Count == 0) return null;
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        /// <summary>
        /// 특정 등급의 방패 데이터 중 하나를 무작위로 가져옵니다.
        /// </summary>
        public ShieldItemData GetRandomShieldDataByGrade(ItemGradeType grade)
        {
            List<ShieldItemData> candidates = new List<ShieldItemData>();

            // 예시: 파싱 매니저가 가진 전체 방패 딕셔너리나 리스트를 순회하며 등급 매칭
            foreach (var shield in _shieldTable.Values)
            {
                if (shield.ItemGradeType == grade)
                    candidates.Add(shield);
            }

            if (candidates.Count == 0) return null;
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }


        /// <summary>
        /// 등급별 속성 부여 확률테이블 로드
        /// </summary>

        public void LoadPropertyRateTableFromServer(string chartId)
        {
            Debug.Log($"{chartId} 속성 확률 차트 서버 다운로드 요청");
            var bro = BackEnd.Backend.Chart.GetChartContents(chartId);

            if (bro.IsSuccess() == false)
            {
                Debug.LogError($"속성 확률 차트 로드 실패 : {bro.GetStatusCode()}");
                return;
            }

            _propertyRateTable.Clear();

            //  백분율(소수점) 파싱을 위한 안전한 float 변환 함수
            float GetSafeFloat(LitJson.JsonData data, string columnName)
            {
                if (data.Keys.Contains(columnName) && data[columnName] != null)
                {
                    string rawValue = data[columnName].ToString().Trim();
                    if (float.TryParse(rawValue, out float result))
                    {
                        return result;
                    }
                }
                return 0f;
            }
            foreach (LitJson.JsonData gameData in bro.FlattenRows())
            {
                string gradeStr = gameData["ItemGrade"].ToString().Trim();
                if (!System.Enum.TryParse(gradeStr, out ItemGradeType gradeType))
                {
                    Debug.LogWarning($"[Property Parsing] 알 수 없는 등급 문자열: {gradeStr}");
                    continue;
                }

                // 백분율 데이터를 'Rates' 배열에 순서대로 조립
                PropertyRateData rateData = new PropertyRateData();
                rateData.Grade = gradeType;

                // 인덱스 규칙을 이넘(Enum) 순서와 100% 일치시킵니다.
                rateData.Rates[0] = GetSafeFloat(gameData, "NoneRate");
                rateData.Rates[1] = GetSafeFloat(gameData, "FireRate");
                rateData.Rates[2] = GetSafeFloat(gameData, "IceRate");
                rateData.Rates[3] = GetSafeFloat(gameData, "LightningRate");
                rateData.Rates[4] = GetSafeFloat(gameData, "WindRate");

                if (!_propertyRateTable.ContainsKey(gradeType))
                {
                    _propertyRateTable.Add(gradeType, rateData);
                }
            }

            Debug.Log($"서버 속성 확률 테이블 로드 완료 : {_propertyRateTable.Count}개 등급 데이터 확보");
        }
        /// <summary>
        /// 등급에 매칭되는 속성 확률 데이터를 반환
        /// </summary>
        public PropertyRateData GetPropertyRateData(ItemGradeType grade)
        {
            if (_propertyRateTable.TryGetValue(grade, out PropertyRateData data))
            {
                return data;
            }

            Debug.LogError($"[Gacha Error] {grade} 등급에 해당하는 속성 확률 데이터가 테이블에 없습니다");
            return null;
        }
        public void LoadGachaProbabilityTable(string gachaUuid)
        {
            Debug.Log($"[{gachaUuid}] 뒤끝 뽑기 확률 테이블 서버 검증 요청...");

            // 확률 관리 전용 내장 함수로 테이블 존재 여부 확인 및 로드
            var bro = Backend.Probability.GetProbability(gachaUuid);

            if (bro.IsSuccess() == false)
            {
                Debug.LogError($"[Gacha Load Error] 뽑기 확률 파일 로드 실패 : {bro.GetStatusCode()} - {bro.GetErrorCode()}");
                _isGachaTableLoaded = false;
                return;
            }

            _isGachaTableLoaded = true;
            Debug.Log("뒤끝 뽑기 확률 테이블 검증 및 로드 완료.");
        }


    }
}