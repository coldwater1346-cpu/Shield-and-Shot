using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Shield_Shot.Core
{
    public class AdManager : PersistentSingleton<AdManager>
    {
        [Header("[ 광고 단위 ID 설정 ]")]
        private string _reviveAdUnitId;
        private string _gachaAdUnitId;

        private RewardedAd _reviveAd; // 부활 광고 객체
        private RewardedAd _gachaAd;  // 무료 뽑기 광고 객체

        private bool _isGachaRewardEarned = false; // 광고 시청 완료 플래그
        private bool _isReviveRewardEarned = false; // 부활 광고 시청 완료 플래그
       
        private Action _onGachaClosedCallback = null; // 광고 닫힐 때 실행할 가챠 함수 
        private Action _onReviveClosedCallback = null; // 광고 닫힐 때 실행할 부활 함수

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IPHONE)
            // 1. 유니티 에디터 또는 윈도우/Mac PC 빌드 환경 (구글 제공 테스트 ID 사용)
            _reviveAdUnitId = "ca-app-pub-3929537019022852/9114913586";
            _gachaAdUnitId = "ca-app-pub-3929537019022852/7233067263";
            Debug.Log("이 장치는 PC/에디터 환경입니다. 테스트 광고 ID를 세팅합니다.");

#elif UNITY_ANDROID
        // 2. 안드로이드 실제 빌드 환경 (본인의 애드몹 ID 입력)
        _reviveAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // Google rewarded test ad unit
        _gachaAdUnitId = "ca-app-pub-3940256099942544/5224354917";  // Google rewarded test ad unit

#elif UNITY_IPHONE
        // 3. iOS 실제 빌드 환경 (본인의 애드몹 ID 입력)
        _reviveAdUnitId = "ca-app-pub-XXXXXX/XXXXXX"; // 여기에 부활 광고 iOS ID 입력
        _gachaAdUnitId = "ca-app-pub-XXXXXX/XXXXXX";  // 여기에 뽑기 광고 iOS ID 입력
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _reviveAd?.Destroy();
            _reviveAd = null;

            _gachaAd?.Destroy();
            _gachaAd = null;
        }

        #region 부활 광고 (Revive Ad) 로직

        private void Start()
        {
            if (Instance != this) return;

            // 구글 모바일 광고 SDK 초기화
            MobileAds.Initialize((InitializationStatus status) =>
            {
                Debug.Log("구글 애드몹 SDK 초기화 완료");
                // 초기화 성공 후 두 광고를 미리 로드(Preload)
                LoadReviveAd();
                LoadGachaAd();
            });
        }
        public void LoadReviveAd()
        {
            if (_reviveAd != null) { _reviveAd.Destroy(); _reviveAd = null; }
            var adRequest = new AdRequest();
            RewardedAd.Load(_reviveAdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("부활 광고 로드 실패: " + error);
                    return;
                }
                _reviveAd = ad;
                Debug.Log("부활 광고 로드 성공");

                _reviveAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("부활 광고 닫힘 -> 게임 복귀");
                    // 광고 창이 닫힌 뒤, 보상 자격이 확인됐다면 부활 실행
                    if (_isReviveRewardEarned && _onReviveClosedCallback != null)
                    {
                        Debug.Log("보상 확인 완료. 광고 닫힘 시점에 부활 실행");
                        _onReviveClosedCallback.Invoke();
                    }
                    // 데이터 초기화 후 재로드
                    _isReviveRewardEarned = false;
                    _onReviveClosedCallback = null;
                    LoadReviveAd();
                };
                _reviveAd.OnAdFullScreenContentFailed += (AdError err) =>
                { Debug.LogError("부활 광고 재생 실패: " + err); LoadReviveAd(); };
            });
        }

        // 부활 버튼에 연결할 함수 (외부 호출용)
        public void ShowReviveAd(Action onRewardSuccess)
        {
            if (_reviveAd != null && _reviveAd.CanShowAd())
            {
                _onReviveClosedCallback = onRewardSuccess;   // 닫힐 때 실행할 부활 함수 저장
                _isReviveRewardEarned = false;               // 플래그 초기화
                _reviveAd.Show((Reward reward) =>
                {
                    Debug.Log("부활 광고 시청 완료 (보상 대기 중)");
                    _isReviveRewardEarned = true;            // 여기선 획득 표시만
                });
            }
            else
            {
                Debug.LogWarning("부활 광고가 아직 준비되지 않았습니다. 다시 로드를 시도합니다.");
                LoadReviveAd();
            }
        }
        #endregion

        #region 무료 뽑기 광고 (Gacha Ad) 로직

        public void LoadGachaAd()
        {
            if (_gachaAd != null) { _gachaAd.Destroy(); _gachaAd = null; }

            var adRequest = new AdRequest();
            RewardedAd.Load(_gachaAdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("뽑기 광고 로드 실패: " + error);
                    return;
                }
                _gachaAd = ad;
                Debug.Log("뽑기 광고 로드 성공");

                // 광고 풀스크린 콘텐츠 이벤트 연결
               
                _gachaAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("뽑기 광고 닫힘 -> 게임 복귀");

                    // 유저가 광고 창을 닫았을 때, 보상 자격이 확인되었다면 가챠 실행
                    if (_isGachaRewardEarned && _onGachaClosedCallback != null)
                    {
                        Debug.Log("보상 확인 완료. 게임 화면 복귀 시점에 가챠 로직 실행");
                        _onGachaClosedCallback.Invoke(); 
                    }

                    // 데이터 초기화 후 광고 재로드
                    _isGachaRewardEarned = false;
                    _onGachaClosedCallback = null;
                    LoadGachaAd();
                };
                _gachaAd.OnAdFullScreenContentFailed += (AdError err) => { Debug.LogError("뽑기 광고 재생 실패: " + err); LoadGachaAd(); };
            });
        }
        // 무료 뽑기 버튼에 연결할 함수 (외부 호출용)
        public void ShowGachaAd(Action onRewardSuccess)
        {
            if (_gachaAd != null && _gachaAd.CanShowAd())
            {
                //가챠 함수를 임시 저장
                _onGachaClosedCallback = onRewardSuccess;
                _isGachaRewardEarned = false; // 플래그 초기화

                _gachaAd.Show((Reward reward) =>
                {
                    
                    Debug.Log("뽑기 광고 시청 완료 (보상 대기 중)");
                    _isGachaRewardEarned = true;
                });
            }
            else
            {
                Debug.LogWarning("뽑기 광고가 아직 준비되지 않았습니다. 다시 로드를 시도합니다.");
                LoadGachaAd();
            }
        }

        #endregion
    }
}

