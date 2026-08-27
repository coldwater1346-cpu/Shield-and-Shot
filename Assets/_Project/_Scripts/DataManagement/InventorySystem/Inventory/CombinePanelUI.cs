using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class CombinePanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private CombineSlotListUI _slotListUI;

        //[Header("Result Items")]
        //[SerializeField] private WeaponItemData[] _resultWeaponDatas;

        [Header("Selected Item UI")]
        [SerializeField] private ViewSlotUI _targetViewSlot;
        [SerializeField] private ViewSlotUI _materialViewSlot;

        [Header("Button")]
        [SerializeField] private Button _combineButton;

        [Header("Result UI")]
        [SerializeField] private InventorySlotUI _resultSlotUI;
        [SerializeField] private ItemInfoPanelUI _itemInfoPanelUI;

        [Header("Combine Effects (Pre-placed UI)")]
        [SerializeField] private GameObject combineEffectObject;  // 합성 이펙트 오브젝트

       
        [SerializeField] private Color successEffectColor = Color.red;
        [SerializeField] private Color failureEffectColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);

        private Item _resultItem;

        private readonly List<Item> _combineItems = new List<Item>();

        private Item _targetItem;
        private Item _materialItem;

        private InventorySlotUI _targetSlotUI;
        private InventorySlotUI _materialSlotUI;

        private void Awake()
        {
            if (_inventoryManager == null)
                _inventoryManager = FindFirstObjectByType<InventoryManager>();

            if (_slotListUI == null)
                _slotListUI = FindFirstObjectByType<CombineSlotListUI>();

            if (_itemInfoPanelUI == null)
                _itemInfoPanelUI = FindFirstObjectByType<ItemInfoPanelUI>();

            ClearSelectedSlots();
            ClearResultSlot();
        }

        private void OnEnable()
        {
            if (_inventoryManager != null)
                _inventoryManager.OnInventoryChanged += RefreshUI;

            if (_combineButton != null)
                _combineButton.onClick.AddListener(OnClickCombineButton);

            if (_slotListUI != null)
                _slotListUI.OnClickSlot += OnClickSlot;

            RefreshUI();
        }

        private void OnDisable()
        {
            if (_inventoryManager != null)
                _inventoryManager.OnInventoryChanged -= RefreshUI;

            if (_combineButton != null)
                _combineButton.onClick.RemoveListener(OnClickCombineButton);

            if (_slotListUI != null)
                _slotListUI.OnClickSlot -= OnClickSlot;
        }

        private void Start()
        {
            //// 시작 시 이펙트가 붙어있는 Particle System의 원래 색상을 저장
            //if (combineEffectObject != null)
            //{
            //    var mainParticle = combineEffectObject.GetComponent<ParticleSystem>();
            //    if (mainParticle != null)
            //    {
            //        _originalEffectColor = mainParticle.main.startColor.color;
            //    }
            //}
        }

        private void RefreshUI()
        {
            if (_inventoryManager == null || _slotListUI == null)
                return;

            ClearSelection();

            _combineItems.Clear();

            foreach (Item item in _inventoryManager.Items)
            {
                if (item == null)
                    continue;

                if (!item.CanEnhance)
                    _combineItems.Add(item);
            }

            _slotListUI.DrawSlots(_combineItems);
        }

        private void OnClickSlot(InventorySlotUI slotUI, Item item)
        {
            if (item == null)
                return;

            if (_targetItem == null)
            {
                SetTargetItem(slotUI, item);
                return;
            }

            if (_targetSlotUI == slotUI)
            {
                ClearSelection();
                return;
            }

            if (_materialSlotUI == slotUI)
            {
                ClearMaterialItem();
                return;
            }

            if (_materialItem == null)
            {
                if (item == _targetItem)
                {
                    Debug.LogWarning("합성 대상과 재료는 같은 아이템일 수 없습니다.");
                    return;
                }

                if (_inventoryManager.IsEquippedItem(item))
                {
                    Debug.LogWarning("장착 중인 아이템은 합성 재료로 사용할 수 없습니다.");
                    return;
                }

                if (item.ItemData.itemID != _targetItem.ItemData.itemID)
                {
                    Debug.LogWarning("같은 ID의 아이템만 합성할 수 있습니다.");
                    return;
                }

                SetMaterialItem(slotUI, item);
                return;
            }

            ClearSelection();
            SetTargetItem(slotUI, item);
        }

        private void OnClickCombineButton()
        {
            if (_targetItem == null || _materialItem == null) return;

            // 인벤토리 매니저에게 두 재료를 합성 
            Item combineResult = _inventoryManager.ProcessItemCombine(_targetItem, _materialItem);

            if (combineResult != null)
            {
                //  [성공] -> 결과 슬롯에 등록하고 이펙트 연출
                SetResultSlot(combineResult);
                Debug.Log($"[UI] {combineResult.ItemData.ItemName} 합성 성공 연출 시작!");
                PlayCombineEffect(true);
            }
            else
            {
                // null이 왔다면 [실패] -> 결과 슬롯을 비우고 파괴 연출
                ClearResultSlot();
                Debug.Log("[UI] 재료 파괴 연출 시작!");
                PlayCombineEffect(false);
            }

            // 공통 화면 갱신
            ClearSelection();
            RefreshUI();
        }

        //private WeaponItemData GetRandomNextGradeWeaponData(Item targetItem)
        //{
        //    if (_resultWeaponDatas == null || _resultWeaponDatas.Length == 0)
        //        return null;

        //    List<WeaponItemData> candidates = new List<WeaponItemData>();

        //    int nextGrade = (int)targetItem.ItemData.ItemGradeType + 1;

        //    foreach (WeaponItemData data in _resultWeaponDatas)
        //    {
        //        if (data == null)
        //            continue;

        //        if ((int)data.ItemGradeType == nextGrade)
        //            candidates.Add(data);
        //    }

        //    if (candidates.Count == 0)
        //        return null;

        //    int randomIndex = Random.Range(0, candidates.Count);
        //    return candidates[randomIndex];
        //}

        private void SetTargetItem(InventorySlotUI slotUI, Item item)
        {
            if (_targetSlotUI != null)
                _targetSlotUI.SetHighlight(false);

            _targetSlotUI = slotUI;
            _targetItem = item;

            _targetSlotUI.SetHighlight(true);

            if (_targetViewSlot != null)
                _targetViewSlot.SetItem(item);
        }

        private void SetMaterialItem(InventorySlotUI slotUI, Item item)
        {
            if (_materialSlotUI != null)
                _materialSlotUI.SetHighlight(false);

            _materialSlotUI = slotUI;
            _materialItem = item;

            _materialSlotUI.SetHighlight(true);

            if (_materialViewSlot != null)
                _materialViewSlot.SetItem(item);
        }

        private void ClearMaterialItem()
        {
            if (_materialSlotUI != null)
                _materialSlotUI.SetHighlight(false);

            _materialSlotUI = null;
            _materialItem = null;

            if (_materialViewSlot != null)
                _materialViewSlot.Clear();
        }

        private void ClearSelection()
        {
            ClearSelectedHighlights();
            ClearSelectedSlots();

            _targetItem = null;
            _materialItem = null;
        }

        private void ClearSelectedHighlights()
        {
            if (_targetSlotUI != null)
                _targetSlotUI.SetHighlight(false);

            if (_materialSlotUI != null)
                _materialSlotUI.SetHighlight(false);

            _targetSlotUI = null;
            _materialSlotUI = null;
        }

        private void ClearSelectedSlots()
        {
            if (_targetViewSlot != null)
                _targetViewSlot.Clear();

            if (_materialViewSlot != null)
                _materialViewSlot.Clear();
        }

        private void SetResultSlot(Item item)
        {
            _resultItem = item;

            if (_resultSlotUI == null)
                return;

            _resultSlotUI.gameObject.SetActive(true);
            _resultSlotUI.SetSlot(item, OnClickResultSlot);
        }

        private void OnClickResultSlot(InventorySlotUI slotUI, Item item)
        {
            if (_resultItem == null)
                return;

            if (_itemInfoPanelUI == null)
                return;

            _itemInfoPanelUI.gameObject.SetActive(true);
            _itemInfoPanelUI.Show(_resultItem);
            ClearResultSlot();
        }

        private void ClearResultSlot()
        {
            _resultItem = null;

            if (_resultSlotUI == null)
                return;

            _resultSlotUI.SetSlot(null, null);
            _resultSlotUI.gameObject.SetActive(false);
        }

        private void PlayCombineEffect(bool isSuccess)
        {
            if (combineEffectObject == null) return;

            // 1. 오브젝트를 끄고 색상을 변경한 뒤 켭니다.
            combineEffectObject.SetActive(false);

            //  인스펙터에서 지정한 정확한 색상을 넘겨줍니다.
            Color targetColor = isSuccess ? successEffectColor : failureEffectColor;
            SetEffectColor(targetColor);

            combineEffectObject.SetActive(true);

            // 2. 코드로 자식 파티클들을 순회하며 확실하게 재생
            ParticleSystem[] particles = combineEffectObject.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Clear(); 
                ps.Play();  // 새 색상으로 재생
            }

            RectTransform rectTransform = combineEffectObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, -10f);
            }

            CancelInvoke(nameof(DisableCombineEffect));
            Invoke(nameof(DisableCombineEffect), 3.0f);
        }

        private void SetEffectColor(Color color)
        {
            if (combineEffectObject == null) return;

            ParticleSystem[] particleSystems = combineEffectObject.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem ps in particleSystems)
            {
                var main = ps.main;

                //  startColor의 모드를 단색(Color) 모드로 강제 고정하면서 색상을 주입합니다.
                main.startColor = new ParticleSystem.MinMaxGradient(color);
            }
        }

        private void DisableCombineEffect()
        {
            if (combineEffectObject != null)
            {
                combineEffectObject.SetActive(false);
            }
        }
        public void ClearPanelSelection()
        {
            ClearSelection();

            
            ClearResultSlot();
        }

    }
}