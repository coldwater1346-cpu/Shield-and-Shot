using Shield_Shot.DataManagement;
using Shield_Shot.NetworkCore;
using Shield_Shot.UI.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shield_Shot.UI.Components
{
    public class LogoutPopupUI : UIPopupBase
    {
        [SerializeField] private Button _confirmBtn;
        [SerializeField] private Button _closeBtn;

        private void Awake()
        {
            _confirmBtn.onClick.AddListener(ExecuteLogout);
            _closeBtn.onClick.AddListener(Close);
        }

        private void ExecuteLogout()
        {
            Debug.Log("로그아웃 및 타이틀 씬 이동");
            //TODO: 로그아웃 로직 및 씬전환 추가

            BackendLogin.Instance.Logout();

            PlayerDataManager.Instance.ClearData();

            Close();

            SceneManager.LoadScene("00_Intro");
        }
    }
}

