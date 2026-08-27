using System;
using BackEnd;
using LitJson;
using Shield_Shot.DataManagement;
using Shield_Shot.NetworkCore;
using Shield_Shot.UI;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Currency UI")]
    [SerializeField] private LobbyCurrencyUI _currencyUI;

    [Header("Loading UI")]
    [SerializeField] private GameObject _loadingPanel;

    [Header("Purchase Result Popup")]
    [SerializeField] private GameObject _purchaseResultPopup;
    [SerializeField] private TextMeshProUGUI _purchaseResultText;

    private bool _isPurchasing;

    private void Awake()
    {
        if (_currencyUI == null)
        {
            _currencyUI = FindFirstObjectByType<LobbyCurrencyUI>();
        }

        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(false);
        }

        if (_purchaseResultPopup != null)
        {
            _purchaseResultPopup.SetActive(false);
        }
    }

    /// <summary>
    /// 서버 펑션을 호출하여 상품 구매를 처리한다.
    /// </summary>
    /// <param name="productId">
    /// 서버 차트에 등록된 상품 ID
    /// </param>
    public void PurchaseProduct(string productId)
    {
        // 구매 버튼 연속 클릭 방지
        if (_isPurchasing)
        {
            Debug.LogWarning("[Shop] 이미 구매 요청을 처리하고 있습니다.");
            return;
        }

        if (string.IsNullOrEmpty(productId))
        {
            Debug.LogError("[Shop] 상품 ID가 비어 있습니다.");
            ShowPurchaseResult("구매에 실패했습니다!");
            return;
        }

        _isPurchasing = true;

        // 이전 결과 팝업 닫기
        if (_purchaseResultPopup != null)
        {
            _purchaseResultPopup.SetActive(false);
        }

        // 로딩창 표시
        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
        }

        Param param = new Param();
        param.Add("shopProductId", productId);

        Debug.Log($"[Shop] 서버 펑션 호출 시작 - 상품 ID: {productId}");

        Backend.BFunc.InvokeFunction("ShopFunction", param, bro =>
        {
            try
            {
                // 서버 통신 자체가 실패한 경우
                if (!bro.IsSuccess())
                {
                    Debug.LogError(
                        $"[Shop Error] 뒤끝 펑션 통신 실패: " +
                        $"StatusCode={bro.GetStatusCode()}, " +
                        $"Message={bro.GetMessage()}");

                    ShowPurchaseResult("구매에 실패했습니다!");
                    return;
                }

                JsonData wrapperJson = bro.GetReturnValuetoJSON();

                // result 데이터가 없는 경우
                if (wrapperJson == null ||
                    !wrapperJson.Keys.Contains("result"))
                {
                    Debug.LogError(
                        "[Shop Error] 서버 응답에 result 값이 없습니다.");

                    ShowPurchaseResult("구매에 실패했습니다!");
                    return;
                }

                string resultString = wrapperJson["result"].ToString();

                if (string.IsNullOrEmpty(resultString))
                {
                    Debug.LogError(
                        "[Shop Error] 서버 result 값이 비어 있습니다.");

                    ShowPurchaseResult("구매에 실패했습니다!");
                    return;
                }

                // 뒤끝 펑션 반환 문자열 정리
                if (resultString.StartsWith("\"") &&
                    resultString.EndsWith("\""))
                {
                    resultString = resultString.Substring(
                        1,
                        resultString.Length - 2);
                }

                resultString = resultString
                    .Replace("\\\"", "\"")
                    .Replace("\\n", "")
                    .Replace("\\r", "")
                    .Replace("\\\\", "\\");

                JsonData json = JsonMapper.ToObject(resultString);

                if (json == null ||
                    !json.Keys.Contains("success"))
                {
                    Debug.LogError(
                        "[Shop Error] 서버 응답에 success 값이 없습니다.");

                    ShowPurchaseResult("구매에 실패했습니다!");
                    return;
                }

                bool isSuccess =
                    bool.Parse(json["success"].ToString());

                // 다이아 부족 등의 서버 구매 실패
                if (!isSuccess)
                {
                    string errorMessage =
                        json.Keys.Contains("errorMessage")
                            ? json["errorMessage"].ToString()
                            : "구매 실패";

                    Debug.LogWarning(
                        $"[Shop Warning] 구매 실패: {errorMessage}");

                    ShowPurchaseResult("다이아가 부족합니다!");
                    return;
                }

                // 구매 성공 응답에 재화 정보가 없는 경우
                if (!json.Keys.Contains("updatedDiamond") ||
                    !json.Keys.Contains("updatedGold"))
                {
                    Debug.LogError(
                        "[Shop Error] 갱신된 재화 정보가 없습니다.");

                    ShowPurchaseResult("구매에 실패했습니다!");
                    return;
                }

                int serverDiamond =
                    int.Parse(json["updatedDiamond"].ToString());

                int serverGold =
                    int.Parse(json["updatedGold"].ToString());

                // 서버 데이터를 클라이언트 데이터에 반영
                PlayerDataManager.Instance.diamond = serverDiamond;
                PlayerDataManager.Instance.gold = serverGold;

                // 갱신된 게임 데이터 저장
                if (BackendGameData.Instance != null)
                {
                    BackendGameData.Instance.GameDataUpdateAsync();
                }

                // 로비 재화 UI 갱신
                if (_currencyUI != null)
                {
                    _currencyUI.RefreshCurrencyUI();
                }

                ShowPurchaseResult("구매 완료되었습니다!");

                Debug.Log(
                    $"[Shop Success] 구매 완료. " +
                    $"다이아: {serverDiamond}, 골드: {serverGold}");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[Shop Error] 구매 응답 처리 중 예외 발생\n" +
                    $"{exception}");

                ShowPurchaseResult("구매에 실패했습니다!");
            }
            finally
            {
                // 성공, 실패, 예외와 관계없이 항상 실행
                _isPurchasing = false;

                if (_loadingPanel != null)
                {
                    _loadingPanel.SetActive(false);
                }
            }
        });
    }

    /// <summary>
    /// 구매 결과 팝업을 열고 안내 문구를 출력한다.
    /// </summary>
    private void ShowPurchaseResult(string message)
    {
        if (_purchaseResultText != null)
        {
            _purchaseResultText.text = message;
        }

        if (_purchaseResultPopup != null)
        {
            _purchaseResultPopup.SetActive(true);
        }
    }

    /// <summary>
    /// 구매 결과 팝업의 확인 또는 닫기 버튼에 연결한다.
    /// </summary>
    public void ClosePurchaseResultPopup()
    {
        if (_purchaseResultPopup != null)
        {
            _purchaseResultPopup.SetActive(false);
        }
    }
}