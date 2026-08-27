using Shield_Shot.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI.Controllers
{
    public class LobbySceneUIController : MonoBehaviour
    {
        [SerializeField] private UIPopupBase _userInfoPanel;
        [SerializeField] private UIPopupBase _settingPanel;
        [SerializeField] private UIPopupBase _mailBoxPanel;
        [SerializeField] private UIPopupBase _logoutPopup;
        [SerializeField] private UIPopupBase _quitPopup;
        [SerializeField] private UIPopupBase _chapterPopup;

        private void Start()
        {
            UIManager.Instance.ClearRegistry();

            UIManager.Instance.RegisterPopup("UserInfo", _userInfoPanel);
            UIManager.Instance.RegisterPopup("Setting", _settingPanel);
            UIManager.Instance.RegisterPopup("MailBox", _mailBoxPanel);
            UIManager.Instance.RegisterPopup("Logout", _logoutPopup);
            UIManager.Instance.RegisterPopup("Quit", _quitPopup);
            UIManager.Instance.RegisterPopup("Chapter", _chapterPopup);

            _userInfoPanel.Close();
            _settingPanel.Close();
            _mailBoxPanel.Close();
            _logoutPopup.Close();
            _quitPopup.Close();
            _chapterPopup.Close();
        }
    }
}
