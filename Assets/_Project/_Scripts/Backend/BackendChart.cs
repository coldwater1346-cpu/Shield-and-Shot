//using BackEnd;
//using LitJson; // 뒤끝 내부 Json 파서 라이브러리
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;



//namespace Shield_Shot.NetworkCore
//{
//    public class BackendChart 
//    {
//      private static BackendChart instance;

//        public static BackendChart Instance
//        {
//            get
//            {
//                if(instance == null)
//                    instance = new BackendChart();
//                return instance;
//            }
            
//        }

//        public void ChartGet(string chartId)
//        {
//            Debug.Log($"{chartId}의 서버 차트 불러오기를 요청합니다.");

//            // 뒤끝 서버에서 차트 내용(Json 형태) 가져오기
//            var bro = Backend.Chart.GetChartContents(chartId);

//            if (bro.IsSuccess() == false)
//            {
//                Debug.LogError($"{chartId}의 차트를 불러오는 중 에러가 발생했습니다: " + bro);
//                return;
//            }

//            Debug.Log("서버 차트 불러오기 성공. 데이터 매핑을 시작합니다.");

//            // 뒤끝이 제공하는 납작한 로우(FlattenRows) 반복문 돌리기
//            foreach (JsonData gameData in bro.FlattenRows())
//            {
//                try
//                {
//                    // 1. 서버에서 내려온 순수 데이터 파싱 (헤더 대소문자 주의!)
//                    string itemId = gameData["ItemID"].ToString();
//                    string itemName = gameData["ItemName"].ToString();
//                    string itemTypeStr = gameData["ItemType"].ToString();
//                    string itemGradeStr = gameData["ItemGradeType"].ToString();
//                    string iconName = gameData["Icon"].ToString();
//                    string description = gameData["Description"].ToString();

//                    // 숫자가 비어있거나 소수점일 수 있으므로 안전하게 float/int 파싱
//                    float damage = float.Parse(gameData["Damage"].ToString());
//                    string prefabName = gameData["Prefab"].ToString();

//                    // ------------------------------------------------------------------
//                    // 2. 기존 DataParsingManager 딕셔너리에 데이터 적재하기
//                    // ------------------------------------------------------------------
//                    // 현재 프로젝트에서 사용 중인 '기획 데이터 저장용 SO' 또는 ' 구조에 맞춰 대입합니다.
//                    // 아래는 예시 구조이며, 실제 프로젝트의 클래스명(예: ItemData 등)으로 매핑하시면 됩니다.

//                    /*
//                    WeaponItemData newBaseData = new WeaponItemData();
//                    newBaseData.Id = itemId;
//                    newBaseData.Name = itemName;
//                    newBaseData.Damage = damage;
//                    // ... 나머지 변수들 대입 ...

//                    // 로컬 딕셔너리에 추가하여 인게임에서 원본으로 쓰도록 보관
//                    if (!ItemDictionary.ContainsKey(itemId))
//                    {
//                        ItemDictionary.Add(itemId, newBaseData);
//                    }
//                    */

//                    // 디버그 확인용 로그 생성
//                    StringBuilder content = new StringBuilder();
//                    content.AppendLine($"[서버 아이템 로드] {itemId} : {itemName}");
//                    content.AppendLine($"타입: {itemTypeStr} | 등급: {itemGradeStr} | 공격력: {damage}");
//                    content.AppendLine($"프리팹: {prefabName} | 설명: {description}");
//                    Debug.Log(content.ToString());
//                }
//                catch (Exception e)
//                {
//                    // 특정 행에 오타가 있거나 빈 칸이 있어 파싱 에러가 나더라도 팅기지 않고 넘어가도록 안전장치
//                    Debug.LogError($"차트 행 파싱 중 오류 발생 (아이템ID 체크 필요): {e.Message}");
//                }
//            }

//            Debug.Log("모든 서버 기획 차트 데이터 메모리 적재 완료!");
//        }
//    }
//}