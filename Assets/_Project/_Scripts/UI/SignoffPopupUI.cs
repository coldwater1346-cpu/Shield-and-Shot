using BackEnd;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignoffUI : MonoBehaviour
{
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private Button _closeBtn;


    private void Awake()
    {
        _confirmBtn.onClick.AddListener(ExecuteSignoff);
        _closeBtn.onClick.AddListener(Close);
    }


    private void ExecuteSignoff()
    {
        // 1. 뒤끝 서버 계정 즉시 탈퇴 요청
        Backend.BMember.WithdrawAccount();

        // 2. [게스트 계정용] 기기(PlayerPrefs)에 저장된 게스트 고유 ID 삭제
       
        Backend.BMember.DeleteGuestInfo();

        // 3. [공통] 현재 로그인 세션 종료 및 로컬 액세스 토큰 초기화
        Backend.BMember.Logout();

        Debug.Log("회원탈퇴 및 타이틀 씬 이동 완료");

        Close();

        SceneManager.LoadScene("00_Intro");
    }

    private void Close()
    {
     gameObject.SetActive(false);
    }
}

