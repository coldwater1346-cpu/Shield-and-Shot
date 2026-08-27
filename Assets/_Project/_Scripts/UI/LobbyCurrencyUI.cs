using Shield_Shot.DataManagement; 
using TMPro; 
using UnityEngine;
using System.Collections;


namespace Shield_Shot.UI
{
    public class LobbyCurrencyUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_Text _goldText;

        [SerializeField] private TMP_Text _diaText;

        private void OnEnable()
        {
            // 1.  이벤트 버스 구독
            UIEventBus.OnCurrencyChanged += RefreshCurrencyUI;        
            
        }

        private void OnDisable()
        {
            // 구독 해제
            UIEventBus.OnCurrencyChanged -= RefreshCurrencyUI;
        }

        private IEnumerator Start()
        {
            // 다른 모든 스크립트의 Awake/OnEnable이 끝날 때까지 딱 1프레임 양보(대기)합니다.
            yield return null;

            // 매니저들이 다 준비되었으니 첫 화면을 이쁘게 그립니다.
            RefreshCurrencyUI();
        }

        //  갱신 함수
        public void RefreshCurrencyUI()
        {
            if (PlayerDataManager.Instance == null) return;

         
            _goldText.text = $"{PlayerDataManager.Instance.gold.ToString("N0")}";
            _diaText.text = $"{PlayerDataManager.Instance.diamond.ToString("N0")}";
        }
    }
}