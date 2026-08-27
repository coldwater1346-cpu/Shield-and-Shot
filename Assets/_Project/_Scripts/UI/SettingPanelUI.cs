using UnityEngine;
using UnityEngine.UI;
using Shield_Shot.UI.Core;

namespace Shield_Shot.UI
{    public class SettingPanelUI : UIPopupBase
    {
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _audioBtn;
        [SerializeField] private Button _inputBtn;
        [SerializeField] private Button _logoutBtn;
        [SerializeField] private Button _quitBtn;

        [SerializeField] private GameObject _audioPanel;
        [SerializeField] private GameObject _inputPanel;

        private void Awake()
        {
            _closeBtn.onClick.AddListener(() => UIManager.Instance.ClosePopup("Setting"));
            _audioBtn.onClick.AddListener(() => SwitchSubPanel(true));
            _inputBtn.onClick.AddListener(() => SwitchSubPanel(false));
            _logoutBtn.onClick.AddListener(() => UIManager.Instance.ShowPopup("Logout"));
            _quitBtn.onClick.AddListener(() => UIManager.Instance.ShowPopup("Quit"));
        }
        

        private void OnEnable()
        {
            Debug.Log("세팅창 활성화");
            SwitchSubPanel(true);
        }

        private void SwitchSubPanel(bool isAudio)
        {
            _audioPanel.SetActive(isAudio);
            _inputPanel.SetActive(!isAudio);
        }
    }
}

