//using BackEnd;
//using Shield_Shot.DataManagement;
//using System;
//using UnityEngine;

//public class UserDataTestKHJ : MonoBehaviour
//{
//    public static UserDataTestKHJ Instance;

//    public Sprite CurrentProfile;
//    public Sprite CurrentFrame;
//    public string UserName = Backend.UserNickName;

//    public static BackendReturnObject bro = Backend.BMember.GetUserInfoV2();
//    public string UserID = bro.GetReturnValuetoJSON()["row"]["gamerId"].ToString();

//    public event Action OnProfileDataChanged;

//    private void Awake()
//    {
//        Instance = this;
//        CurrentProfile = PlayerDataManager.Instance.GetCurrentProfileSprite();
//        CurrentFrame = PlayerDataManager.Instance.GetCurrentFrameSprite();
//        Debug.Log(UserName);
//        Debug.Log(UserID);
//    }

//    private void Start()
//    {

//        UpdateProfile(CurrentProfile, CurrentFrame);
//    }

//    public void UpdateProfile(Sprite profile, Sprite frame)
//    {
//        if (profile != null) CurrentProfile = profile;
//        if (frame != null) CurrentFrame = frame;

//        OnProfileDataChanged?.Invoke();
//    }
//}

using BackEnd;
using System;
using UnityEngine;
using Shield_Shot.DataManagement;

public class UserDataTestKHJ : MonoBehaviour
{
    public static UserDataTestKHJ Instance;

    public Sprite CurrentProfile;
    public Sprite CurrentFrame;

    // [수정] 선언할 때 뒤끝 함수를 호출하지 말고, 그냥 타입만 선언합니다.
    public string UserName;
    public string UserID;

    public event Action OnProfileDataChanged;

    private void Awake()
    {
        Instance = this;
        // ❌ 이 자리(Awake)에서는 아직 로그인이 안 되었을 확률이 높아 
        // 여기서 Backend 관련 정보를 가져오거나 로그를 찍으면 빈 값으로 나옵니다.
    }

    private void Start()
    {
        // [수정] 게임이 시작되고 로그인 처리가 완료된 시점(Start)에 값을 가져옵니다.
        LoadUserInfo();
    }

    public void LoadUserInfo()
    {
        // 1. 뒤끝에 저장된 닉네임 가져오기
        UserName = Backend.UserNickName;

        // 2. 뒤끝 회원 상세 정보에서 gamerId 파싱하기
        BackendReturnObject bro = Backend.BMember.GetUserInfoV2();
        if (bro.IsSuccess())
        {
            var json = bro.GetReturnValuetoJSON();
            if (json.Keys.Contains("row"))
            {
                UserID = json["row"]["gamerId"].ToString();
            }
        }

        // 3. SO 데이터베이스 이미지 로드
        if (PlayerDataManager.Instance != null)
        {
            CurrentProfile = PlayerDataManager.Instance.GetCurrentProfileSprite();
            CurrentFrame = PlayerDataManager.Instance.GetCurrentFrameSprite();
        }

        // [확인] 모든 값이 변수에 채워진 '이 시점'에 로그를 찍어야 정상 출력됩니다.
        Debug.Log($"[로그 확인] 닉네임: {UserName}");
        Debug.Log($"[로그 확인] 유저ID: {UserID}");

        // UI에 알림
        UpdateProfile(CurrentProfile, CurrentFrame);
    }

    public void UpdateProfile(Sprite profile, Sprite frame)
    {
        if (profile != null) CurrentProfile = profile;
        if (frame != null) CurrentFrame = frame;

        OnProfileDataChanged?.Invoke();
    }
}
