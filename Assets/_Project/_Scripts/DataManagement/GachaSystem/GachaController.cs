using BackEnd;
using LitJson;
using Shield_Shot.Core;
using Shield_Shot.DataManagement;
using Shield_Shot.DataManagement.DataParsing;
using Shield_Shot.DataManagement.GachaSystem;
using Shield_Shot.DataManagement.InventorySystem;
using Shield_Shot.NetworkCore;
using Shield_Shot.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Shield_Shot.DataManagement.GachaSystem
{
    public class GachaController : MonoBehaviour
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private GachaResultPopupUI _resultPopupUI;
        [SerializeField] private LobbyCurrencyUI _currencyUI;
        [SerializeField] private DiaLackUI _diaLackPopUI;
        [SerializeField] private VideoPlayer _gachaVideoPlayer;
        [SerializeField] private GameObject _gachaVideoPanel;

        //  영상 종료 후 서버 지연 시 활성화할 로딩 팝업
        [SerializeField] private GameObject _loadingPanel;

        /// <summary>
        /// 뽑기 가공 연산 결과를 묶는 데이터 규격
        /// </summary>
        public struct GachaResultData
        {
            public Item Item;
        }

        // 흐름 제어를 위한 상태 관리 변수들
        private bool isServerResponseReceived = false;   // 서버 응답이 완료되었는지 확인하는 플래그
        private BackendReturnObject savedBro = null;      // 비동기로 수신한 응답 데이터를 임시 저장

        private void Awake()
        {
            if (_inventoryManager == null)
            {
                _inventoryManager = FindFirstObjectByType<InventoryManager>();
            }

            if (_resultPopupUI == null)
            {
                _resultPopupUI = FindFirstObjectByType<GachaResultPopupUI>();
            }
            if (_currencyUI == null)
            {
                _currencyUI = FindFirstObjectByType<LobbyCurrencyUI>();
            }

            if (_diaLackPopUI == null)
            {
                _diaLackPopUI = FindFirstObjectByType<DiaLackUI>();
            }
        }

        //  매개변수에 기본값( = false)
        public void RequestGachaResultFromServer(int count, bool isAdGacha = false)
        {
            // 1. 뽑기 요청 시작 시 플래그 및 데이터 초기화
            isServerResponseReceived = false;
            savedBro = null;

            Param param = new Param();
            param.Add("count", count);
            param.Add("isAd", isAdGacha); //  서버 DTO와 매칭되는 "isAd" 키값으로 데이터

            Debug.Log($"[Gacha] 서버 펑션 호출 - 횟수: {count}, 광고여부: {isAdGacha}");

            // 2. 뒤끝 펑션 비동기 호출 
            Backend.BFunc.InvokeFunction("GachaFunction", param, (bro) =>
            {
                // [서버 응답 시점] 데이터를 캐싱하고 응답 완료 플래그를 올림
                savedBro = bro;
                isServerResponseReceived = true;
            });

            // 3. 서버 요청과 동시에 연출 및 지연 체크 통합 코루틴 시작
            StartCoroutine(PlayGachaAndCheckResponseCoroutine());
        }
        // 영상 패널을 켜고 영상을 재생하는 루틴
        private IEnumerator PlayGachaVideoCoroutine()
        {
            if (_gachaVideoPanel != null)
                _gachaVideoPanel.SetActive(true);

            if (_gachaVideoPlayer != null)
            {
                _gachaVideoPlayer.Stop();
                _gachaVideoPlayer.Play();
            }
            yield return null;
        }

        /// <summary>
        /// 가챠 영상 연출을 진행하며, 3초 후 서버 응답 여부에 따라 로딩 팝업을 제어하는 핵심 루틴
        /// </summary>
        private IEnumerator PlayGachaAndCheckResponseCoroutine()
        {
            // 1. 영상 재생 시작
            yield return StartCoroutine(PlayGachaVideoCoroutine());

            // 2. 서버 응답 속도와 관계없이 뽑기 연출 최소 시간 동안 대기
            yield return new WaitForSeconds(5.0f);

            // 3. 최소 연출 시간이 지났으므로 가챠 연출 영상 OFF
            if (_gachaVideoPlayer != null) _gachaVideoPlayer.Stop();
            if (_gachaVideoPanel != null) _gachaVideoPanel.SetActive(false);

            //] 3초 연출이 끝났는데 아직 서버 응답이 도착하지 않았다면?
            if (isServerResponseReceived == false)
            {

                if (_loadingPanel != null) _loadingPanel.SetActive(true); // 로딩 팝업 ON
            }

            // 4. 서버 응답 플래그가 true가 될 때까지 프레임 단위로 대기 (화면 멈춤 현상 방지)
            while (isServerResponseReceived == false)
            {
                yield return null;
            }

            // 5. 서버 응답 수신 완료! 활성화되었을 수 있는 로딩 팝업 끄기
            if (_loadingPanel != null) _loadingPanel.SetActive(false); // 로딩 팝업 OFF

            // 6. 저장된 응답 데이터를 파싱하고 화면에 결과를 반영
            ProcessGachaResult(savedBro);
        }

        /// <summary>
        ///  수신된 서버 데이터를 파싱하고 클라이언트에 동기화 및 팝업을 처리하는 메서드
        /// </summary>
        private void ProcessGachaResult(BackendReturnObject bro)
        {
            // 서버 응답 실패 처리
            if (bro.IsSuccess() == false)
            {
                _diaLackPopUI.Open();
                Debug.LogError($"[Gacha Error] Function 호출 실패 : {bro}");
                return;
            }

            // 데이터 파싱
            LitJson.JsonData wrapperJson = bro.GetReturnValuetoJSON();
            if (!wrapperJson.Keys.Contains("result")) return;

            string resultString = wrapperJson["result"].ToString();

            if (resultString.StartsWith("\"") && resultString.EndsWith("\""))
            {
                resultString = resultString.Substring(1, resultString.Length - 2);
            }

            resultString = resultString
                .Replace("\\\"", "\"")
                .Replace("\\n", "")
                .Replace("\\r", "")
                .Replace("\\\\", "\\");

            LitJson.JsonData json = LitJson.JsonMapper.ToObject(resultString);

            // 재화 동기화
            if (json.Keys.Contains("updatedDiamond"))
            {
                int serverDiamond = int.Parse(json["updatedDiamond"].ToString());
                PlayerDataManager.Instance.diamond = serverDiamond;
                _currencyUI.RefreshCurrencyUI();
            }

            List<GachaResultData> finalResults = ApplyGachaResultsFromServer(json["items"]);

            // 데이터 동기화 완료 후 결과 팝업 오픈
            if (finalResults.Count > 0)
            {
                _resultPopupUI.Open(finalResults);
            }

            // 로컬 및 서버 최종 게임데이터 업데이트
            BackendGameData.Instance.GameDataUpdateAsync();
        }

        private List<GachaResultData> ApplyGachaResultsFromServer(LitJson.JsonData items)
        {
            List<GachaResultData> finalResults = new List<GachaResultData>();

            if (items == null || items.IsArray == false)
                return finalResults;

            var parsingManager = ItemDataParsingManager.Instance;

            for (int i = 0; i < items.Count; i++)
            {
                string itemId = items[i]["itemId"].ToString();
                string uniqueId = items[i]["uniqueId"].ToString();
                int enhanceLevel = int.Parse(items[i]["enhanceLevel"].ToString());

                // 1. 공통 속성 추출
                ItemPropertyType property = (ItemPropertyType)int.Parse(items[i]["property"].ToString());

                // 2. 서버에서 보낸 스킬 데이터 (Enum 파싱)
                WeaponSkillType skillType = WeaponSkillType.None;
                if (items[i].Keys.Contains("skillType"))
                {
                    skillType = (WeaponSkillType)int.Parse(items[i]["skillType"].ToString());
                }

                ItemData baseItemData = parsingManager.GetItemData(itemId);

                if (baseItemData == null)
                {
                    Debug.LogWarning($"[Gacha Warning] 아이템 데이터 없음: {itemId}");
                    continue;
                }

                Item newItem = null;

                // 3. 무기일 때 타입 캐스팅 후 스킬 주입 생성
                if (baseItemData.ItemType == ItemType.Weapon)
                {
                    WeaponItemData weaponData = baseItemData as WeaponItemData;

                    if (weaponData != null)
                    {
                        newItem = new WeaponItem(
                            weaponData,
                            uniqueId,
                            enhanceLevel,
                            property,
                            skillType
                        );
                    }
                }
                // 4. 방패일 때 예외 처리 생성
                else if (baseItemData.ItemType == ItemType.Shield)
                {
                    ShieldItemData shieldData = baseItemData as ShieldItemData;

                    if (shieldData != null)
                    {
                        newItem = new ShieldItem(
                            shieldData,
                            uniqueId,
                            enhanceLevel,
                            ItemPropertyType.None
                        );
                    }
                }

                if (newItem == null)
                {
                    Debug.LogError($"[Gacha Error] 아이템 생성 실패: {itemId}");
                    continue;
                }

                _inventoryManager.AddItem(newItem);

                finalResults.Add(new GachaResultData
                {
                    Item = newItem
                });
            }

            return finalResults;
        }

      
        //  광고 무료 뽑기 
        public void RequestAdGacha()
        {
            AdManager.Instance.ShowGachaAd(() =>
            {
               
                RequestGachaResultFromServer(1, isAdGacha: true);
            });
        }
    }
}