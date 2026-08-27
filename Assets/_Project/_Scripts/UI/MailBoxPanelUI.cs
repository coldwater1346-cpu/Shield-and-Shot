using Shield_Shot.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class MailBoxPanelUI : UIPopupBase
    {
        [SerializeField] private Button _closeBtn;

        private void Awake()
        {
            _closeBtn.onClick.AddListener(() => UIManager.Instance.ClosePopup("MailBox"));
        }

    }
}

