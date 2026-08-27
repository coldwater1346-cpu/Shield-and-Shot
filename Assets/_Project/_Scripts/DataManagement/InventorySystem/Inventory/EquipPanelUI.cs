using UnityEngine;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class EquipPanelUI : MonoBehaviour
    {
        [SerializeField] private InventoryManager _inventoryManager;

        [SerializeField] private ViewSlotUI _weapon1Slot;
        [SerializeField] private ViewSlotUI _weapon2Slot;
        [SerializeField] private ViewSlotUI _shieldSlot;

        private void Awake()
        {
            if (_inventoryManager == null)
                _inventoryManager = FindFirstObjectByType<InventoryManager>();
            //RefreshUI();
        }

        private void Start()
        {
            
            RefreshUI();
            
        }

        private void OnEnable()
        {
            if (_inventoryManager != null)
                _inventoryManager.OnInventoryChanged += RefreshUI;

            
        }

        private void OnDisable()
        {
            if (_inventoryManager != null)
                _inventoryManager.OnInventoryChanged -= RefreshUI;
        }

        private void RefreshUI()
        {
            if (_inventoryManager == null) return;

            // ����Ʈ�� ��ȸ�ϸ� �� ���� ���� ��ũ�� ���� �������� ã���ϴ�.
            Item mainWeapon = _inventoryManager.Items.Find(x => x != null && x.CurrentSlotType == EquipSlotType.MainWeapon);
            Item subWeapon = _inventoryManager.Items.Find(x => x != null && x.CurrentSlotType == EquipSlotType.SubWeapon);
            Item shield = _inventoryManager.Items.Find(x => x != null && x.CurrentSlotType == EquipSlotType.Shield);

            // ���� UI�� ���� ���� (��������� null�� �Ѿ�� �˾Ƽ� �� �������� �׷���)
            if (_weapon1Slot != null) _weapon1Slot.SetItem(mainWeapon);
            if (_weapon2Slot != null) _weapon2Slot.SetItem(subWeapon);
            if (_shieldSlot != null) _shieldSlot.SetItem(shield);
        }


    }
}