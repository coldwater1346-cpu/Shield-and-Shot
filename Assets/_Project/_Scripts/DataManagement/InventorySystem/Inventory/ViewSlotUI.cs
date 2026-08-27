using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class ViewSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _enhanceLevelText;

        private void Awake()
        {
            //Clear();
        }
        private void Start()
        {
            //Clear();
        }

      
      

        //�� ������ �����۾�����,��ȭ��ġ ����
        public void SetItem(Item item)
        {
            if (_iconImage == null)
            {
                Debug.LogWarning("[ViewSlotUI] IconImage ���� �� ��");
                return;
            }

            if (item == null || item.ItemData == null || item.ItemData.Icon == null)
            {
                Clear();
                return;
            }

            _iconImage.enabled = true;
            _iconImage.sprite = item.ItemData.Icon;

            SetEnhanceLevel(item);
        }

        //�� ���� �ʱ�ȭ
        public void Clear()
        {
            if (_iconImage != null)
            {
                _iconImage.enabled = false;
                _iconImage.sprite = null;
            }

            if (_enhanceLevelText != null)
            {
                _enhanceLevelText.enabled = false;
                _enhanceLevelText.text = "";
            }
        }

        private void SetEnhanceLevel(Item item)
        {
            if (_enhanceLevelText == null)
                return;

            if (item is WeaponItem weapon)
            {
                _enhanceLevelText.enabled = true;
                _enhanceLevelText.text = $"+{weapon.EnhanceLevel}";
            }
            else if (item is ShieldItem shield)
            {
                _enhanceLevelText.enabled = true;
                _enhanceLevelText.text = $"+{shield.EnhanceLevel}";
            }
            else
            {
                _enhanceLevelText.enabled = false;
                _enhanceLevelText.text = "";
            }
        }
    }
}