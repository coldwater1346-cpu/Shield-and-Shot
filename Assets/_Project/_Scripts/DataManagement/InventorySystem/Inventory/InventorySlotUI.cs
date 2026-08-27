using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Slot UI Elements")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _highlightImage;
        [SerializeField] private Image _equippedHighlightImage;
        [SerializeField] private TMP_Text _enhanceLevelText;

        

        private Item _currentItem;
        private Action<InventorySlotUI, Item> _onClickSlot;

        public void SetSlot(Item item, Action<InventorySlotUI, Item> clickAction)
        {
            _currentItem = item;
            _onClickSlot = clickAction;

            SetHighlight(false);

            if (_currentItem == null)
            {
                _iconImage.enabled = false;
                _iconImage.sprite = null;

                _enhanceLevelText.enabled = false;
                _enhanceLevelText.text = "";

                if (_equippedHighlightImage != null)
                {
                    _equippedHighlightImage.gameObject.SetActive(false);
                }

                if (_backgroundImage != null)
                {

                    _backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }
            else
            {
                // ] 기획 데이터(ItemData)나 아이콘이 통째로 비어있는 경우 (서버에 데이터가 없을 때)
                if (_currentItem.ItemData == null || _currentItem.ItemData.Icon == null)
                {
                    _iconImage.enabled = false;
                }
                else
                {
                    _iconImage.enabled = true;
                    _iconImage.sprite = _currentItem.ItemData.Icon;
                }

                _enhanceLevelText.enabled = true;

                if (_currentItem is WeaponItem weaponItem)
                {
                    _enhanceLevelText.text = $"+{weaponItem.EnhanceLevel}";
                }
                else if (_currentItem is ShieldItem shieldItem)
                {
                    _enhanceLevelText.text = $"+{shieldItem.EnhanceLevel}";
                }

                if (_equippedHighlightImage != null)
                {
                    _equippedHighlightImage.gameObject.SetActive(_currentItem.IsEquipped);
                }

                SetBackgroundGradeColor();
            }
        }

        //  등급별 배경색 
        private void SetBackgroundGradeColor()
        {
            if (_backgroundImage == null) return;

            // 만약 서버에 등록 안 된 더미 아이템이라 ItemData가 null이면 기본 회색 투명으로 강제 지정
            if (_currentItem == null || _currentItem.ItemData == null)
            {
                _backgroundImage.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
                return;
            }

            // 모든 색상의 마지막 값(A = 알파값)을 0.5f로 고정

            switch (_currentItem.ItemData.ItemGradeType) // 대소문자 규격에 맞춤
            {
                case ItemGradeType.C:
                    _backgroundImage.color = new Color(0.6f, 0.6f, 0.6f, 0.5f); // 회색
                    break;
                case ItemGradeType.UC:
                    _backgroundImage.color = new Color(0.4f, 0.8f, 0.4f, 0.5f); // 연두
                    break;
                case ItemGradeType.Rare:
                    _backgroundImage.color = new Color(0.3f, 0.65f, 0.9f, 0.5f); // 하늘
                    break;
                case ItemGradeType.SR:
                    _backgroundImage.color = new Color(0.6f, 0.3f, 0.8f, 0.5f); // 보라
                    break;
                case ItemGradeType.SSR:
                    _backgroundImage.color = new Color(0.95f, 0.85f, 0.2f, 0.5f); // 노랑
                    break;
                case ItemGradeType.UR:
                    _backgroundImage.color = new Color(0.85f, 0.2f, 0.2f, 0.5f); // 빨강
                    break;
                default:
                    _backgroundImage.color = new Color(1f, 1f, 1f, 0.5f);
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_currentItem == null)
                return;

            // 1. 기존 클릭 알림 코드 유지
            _onClickSlot?.Invoke(this, _currentItem);

            // ==========================================================
            //  클릭 시 프리팹을 가져와서 3D 매니저에 전달
            // ==========================================================
            if (Item3DPreviewManager.Instance != null)
            {
                GameObject targetPrefab = GetItemPrefab(_currentItem);

                if (targetPrefab != null)
                {
                    Item3DPreviewManager.Instance.SetPreviewItem(targetPrefab);
                }
            }
        }

        /// <summary>
        /// 아이템의 타입(무기/방패)을 체크하여 알맞은 3D 프리팹을 반환하는 함수
        /// </summary>
        private GameObject GetItemPrefab(Item item)
        {
            if (item == null || item.ItemData == null)
                return null;

            // Case A: 무기 아이템일 때
            if (item is WeaponItem && item.ItemData is WeaponItemData weaponData)
            {
                return weaponData.WeaponPrefab;
            }

            // Case B: 방패 아이템일 때 (클래스명과 변수명이 맞는지 확인해 주세요!)
            if (item is ShieldItem && item.ItemData is ShieldItemData shieldData)
            {
                return shieldData.ShieldPrefab;
            }

            return null;
        }

        public void SetHighlight(bool active)
        {
            if (_highlightImage != null)
            {
                _highlightImage.enabled = active;
            }
        }
    }
}