using Shield_Shot.DataManagement;
using Shield_Shot.GameplayCore.Augment;
using Shield_Shot.NetworkCore;
using Shield_Shot.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public enum SlotType
    {
        Profile,
        Frame
    }

    public class UserInfoPanelUI : UIPopupBase
    {
        [SerializeField] private Button _closeBtn;

        [SerializeField] private InfoItemDatabase _database;
        [SerializeField] private Transform _scrollContent;
        [SerializeField] private GameObject _itemSlotPrefab;

        [SerializeField] private GameObject _actionPanel;
        [SerializeField] private Button _selectBtn;
        [SerializeField] private Button _cancleBtn;

        [SerializeField] private Image _mainProfileImage;
        [SerializeField] private Image _mainFrameImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _idText;

        private SlotType _currentType;
        private ItemSlotInfo _selectedSlot;

        public override void Open()
        {
            base.Open();
            _mainProfileImage.sprite = UserDataTestKHJ.Instance.CurrentProfile;
            _mainFrameImage.sprite = UserDataTestKHJ.Instance.CurrentFrame;
            _nameText.text = UserDataTestKHJ.Instance.UserName;
            
            _idText.text = $"ID: {UserDataTestKHJ.Instance.UserID}";
            OnClickTab(0);
        }

        private void Awake()
        {
            _closeBtn.onClick.AddListener(() => UIManager.Instance.ClosePopup("UserInfo"));

            _actionPanel.SetActive(false);
            _selectBtn.onClick.AddListener(OnConfirmSelection);
            _cancleBtn.onClick.AddListener(() => _actionPanel.SetActive(false));
        }

        public void OnClickTab(int typeIndex)
        {
            _currentType = (SlotType)typeIndex;
            RefreshScrollList(_currentType);
        }

        private void RefreshScrollList(SlotType type)
        {
            foreach(Transform child in _scrollContent)
            {
                Destroy(child.gameObject);
            }

            var items = (type == SlotType.Profile) ? _database.prefileImages : _database.frameImages;

            for(int i = 0; i < items.Length; i++)
            {
                GameObject slotObj = Instantiate(_itemSlotPrefab, _scrollContent);
                var slot = slotObj.GetComponent<ItemSlotInfo>();

                slot.Setup(i, items[i]);

                slot.OnSlotClicked += (clickedSlot) =>
                {
                    _selectedSlot = clickedSlot;
                    _actionPanel.transform.position = clickedSlot.transform.position;
                    _actionPanel.SetActive(true);
                };
            }
        }

        private void OnConfirmSelection()
        {
            if (_selectedSlot == null) return;

            int index = _selectedSlot.SlotIndex;

            if(_currentType == SlotType.Profile)
            {
                _mainProfileImage.sprite = _database.GetProfileSprite(index);
                // 테스트용
                UserDataTestKHJ.Instance.UpdateProfile(_database.GetProfileSprite(index), null);
                // 유저 데이터 업데이트
                PlayerDataManager.Instance.profileId = index;
                BackendGameData.Instance.GameDataUpdateAsync();
            }
            else
            {
                _mainFrameImage.sprite = _database.GetFrameSprite(index);
                // 테스트용
                UserDataTestKHJ.Instance.UpdateProfile(null, _database.GetFrameSprite(index));
                // 유저 데이터 업데이트
                PlayerDataManager.Instance.frameId = index;
                BackendGameData.Instance.GameDataUpdateAsync();
            }

            _actionPanel.SetActive(false);
        }
    }
}

