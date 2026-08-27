using Shield_Shot.UI.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class TopMenuBarUI : MonoBehaviour
    {
        [SerializeField] private Image _profileImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private Button _settingBtn;
        [SerializeField] private Button _homeBtn;
        [SerializeField] private Button _userInfoBtn;
        [SerializeField] private Button _mailBoxBtn;

        private void Start()
        {
            _settingBtn.onClick.AddListener(() => UIManager.Instance.ShowPopup("Setting"));
            _homeBtn.onClick.AddListener(() => UIEventBus.RaiseMenuChanged(MenuType.NONE));
            _userInfoBtn.onClick.AddListener(() => UIManager.Instance.ShowPopup("UserInfo"));
            _mailBoxBtn.onClick.AddListener(() => UIManager.Instance.ShowPopup("MailBox"));

            UserDataTestKHJ.Instance.OnProfileDataChanged += UpdateDisplay;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            // 테스트용
            _profileImage.sprite = UserDataTestKHJ.Instance.CurrentProfile;
            _frameImage.sprite = UserDataTestKHJ.Instance.CurrentFrame;
        }
    }
}

