using Shield_Shot.DataManagement.DataParsing;
using Shield_Shot.NetworkCore;
using Shield_Shot.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class ItemInfoPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager _inventoryManager;

        [Header("UI Elements")]
        [SerializeField] private GameObject _itemInfoPopup;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _gradeText;
        [SerializeField] private TMP_Text _enhanceLevelText;
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_Text _skillText;

        // 1. 판매 금액을 보여줄 텍스트 추가
        [SerializeField] private TMP_Text _salePriceText;

        //  아이템의 부여된 속성을 보여줄 텍스트 필드
        [SerializeField] private TMP_Text _propertyText;

        [Header("Equip Select Highlights")]
        [SerializeField] private EquipViewSlotClickUI[] _equipSlotHighlights;

        [Header("Button")]
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _closeButton;

        // 2. 판매 버튼 추가
        [SerializeField] private Button _sellButton;

        private Item _currentItem;

        private void Start()
        {
            if (_inventoryManager == null)
                _inventoryManager = FindFirstObjectByType<InventoryManager>();

            _itemInfoPopup.SetActive(false);
            HideEquipSlotHighlights();
        }

        private void OnEnable()
        {
            _equipButton.onClick.AddListener(OnClickEquipButton);
            _closeButton.onClick.AddListener(OnClickCloseButton);
            _sellButton.onClick.AddListener(OnClickSellButton);
        }

        private void OnDisable()
        {
            _equipButton.onClick.RemoveListener(OnClickEquipButton);
            _closeButton.onClick.RemoveListener(OnClickCloseButton);
            _sellButton.onClick.RemoveListener(OnClickSellButton);
        }

        public void Show(Item item)
        {
            _currentItem = item;

            if (_currentItem == null)
            {
                _itemInfoPopup.SetActive(false);
                return;
            }

            _itemInfoPopup.SetActive(true);

            _iconImage.enabled = true;
            _iconImage.sprite = _currentItem.ItemData.Icon;

            _nameText.text = _currentItem.ItemData.ItemName;

            ItemGradeType gradeType = _currentItem.ItemData.ItemGradeType;
            _gradeText.text = gradeType.ToString();

            // 속성 텍스트 출력 처리
            if (_propertyText != null)
            {
                _propertyText.gameObject.SetActive(true); // 항상 활성화

                if (_currentItem is WeaponItem)
                {
                    //  무기인 경우 속성값 
                    if (_currentItem.Property == ItemPropertyType.None)
                    {
                        _propertyText.text = "None";
                        _propertyText.color = Color.white;
                    }
                    else
                    {
                        _propertyText.text = _currentItem.Property.ToString();

                        switch (_currentItem.Property)
                        {
                            case ItemPropertyType.Fire: _propertyText.color = Color.red; break;
                            case ItemPropertyType.Ice: _propertyText.color = new Color(0.4f, 0.7f, 1f); break; // 하늘색
                            case ItemPropertyType.Lightning: _propertyText.color = Color.yellow; break;
                            case ItemPropertyType.Wind: _propertyText.color = Color.green; break;
                        }
                    }
                }
                else
                {
                   
                    _propertyText.text = "None";
                   // _propertyText.color = Color.white;
                }
            }

            //  2. 무기 스킬 / 방패 설명(디스크립션) 분기 처리
            if (_skillText != null)
            {
                _skillText.gameObject.SetActive(true);

                if (_currentItem is WeaponItem weapon)
                {
                    _skillText.text = $" {weapon.SkillType.ToString()}";
                }
                else
                {
                    if (_currentItem.ItemData != null && !string.IsNullOrEmpty(_currentItem.ItemData.Description))
                    {
                        _skillText.text = _currentItem.ItemData.Description;
                    }
                    else
                    {
                        _skillText.text = " 없음";
                    }
                }
            }

            //  3. 아이템 판매 가격 조회 및 표시
            int salePrice = ItemDataParsingManager.Instance.GetItemSalePrice(_currentItem.EnhanceLevel, gradeType);
            if (_salePriceText != null)
            {
                _salePriceText.text = $"{salePrice}";
            }

            // 4. 강화 및 데미지 수치 분기 처리
            if (_currentItem is WeaponItem weaponItem)
            {
                _enhanceLevelText.text = $"+{weaponItem.EnhanceLevel}";
                _damageText.text = $"{weaponItem.FinalDamage}";
            }
            else
            {
             
                _enhanceLevelText.text = "";
                _damageText.text = "0"; 
            }
        }


        private void OnClickEquipButton()
        {
            if (_currentItem == null)
                return;

            if (_currentItem.IsEquipped)
            {
                Debug.LogWarning(
                    $"[ItemInfoPanelUI] 이미 장착 중인 아이템입니다: {_currentItem.ItemData.ItemName}");

                return;
            }
            ShowEquipSlotHighlights(_currentItem);
            Close();
        }

        private void OnClickCloseButton()
        {
            Close();
        }

        private void Close()
        {
            _itemInfoPopup.SetActive(false);
        }

        private void ShowEquipSlotHighlights(Item item)
        {
            foreach (EquipViewSlotClickUI highlight in _equipSlotHighlights)
            {
                if (highlight == null)
                    continue;

                highlight.SetEquipTarget(item, _inventoryManager, HideEquipSlotHighlights);
                highlight.gameObject.SetActive(true);
            }
        }

        public  void HideEquipSlotHighlights()
        {
            foreach (EquipViewSlotClickUI highlight in _equipSlotHighlights)
            {
                if (highlight == null)
                    continue;

                highlight.gameObject.SetActive(false);
            }
        }

        // 6. 판매 버튼을 클릭했을 때 발동하는 메소드
        private void OnClickSellButton()
        {
            if (_currentItem == null)
                return;

            // 장착 여부 확인 예외 조건
            if (_currentItem.IsEquipped)
            {
                Debug.LogWarning($"{_currentItem.ItemData.ItemName}은(는) 장착 중이므로 판매할 수 없습니다.");
                return;
            }

            // 테이블 수치와 비교하여 최종 금액 조회
            ItemGradeType gradeType = _currentItem.ItemData.ItemGradeType;
            int salePriceGold = ItemDataParsingManager.Instance.GetItemSalePrice(_currentItem.EnhanceLevel, gradeType);

            if (salePriceGold <= 0)
            {
                Debug.LogError("판매 가격 조회 실패.");
                return;
            }

            // 금액에 맞는 골드 지급 및 UI 갱신 이벤트 호출 (강화 코드 구조 100% 반영)
            PlayerDataManager.Instance.gold += salePriceGold;
            UIEventBus.RaiseCurrencyChanged();
            Debug.Log($"아이템 판매 완료. +{salePriceGold} 골드 | 현재 골드: {PlayerDataManager.Instance.gold}");

            // 인벤토리 매니저를 통해 아이템 삭제 처리
            _inventoryManager.RemoveItem(_currentItem);

            // 팝업 닫기 및 타겟 데이터 초기화
            _currentItem = null;
            Close();

            // 뒤끝 서버 비동기 저장 요청
            BackendGameData.Instance.GameDataUpdateAsync();
            Debug.Log("아이템 판매 정보 뒤끝 서버 비동기 업데이트 호출");
        }
        
        
                


}
}