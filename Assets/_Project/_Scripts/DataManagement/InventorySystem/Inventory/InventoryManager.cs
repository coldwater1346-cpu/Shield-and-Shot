using Shield_Shot.DataManagement.DataParsing;
using Shield_Shot.NetworkCore;

//using Shield_Shot.DataManagement.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("���� Ŭ��")]
        [SerializeField] private AudioClip _combineSfx; //  �ռ� ȿ����
        [SerializeField, Range(0f, 1f)] private float _combineSfxVolume = 0.5f;

        [SerializeField] private AudioClip _equipSfx;
        [SerializeField, Range(0f, 1f)] private float _equipSfxVolume = 0.5f;

        // --- �̱��� �ν��Ͻ� ���� �߰� ---
        private static InventoryManager _instance;
        public static InventoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<InventoryManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("InventoryManager");
                        _instance = go.AddComponent<InventoryManager>();
                    }
                }
                return _instance;
            }
        }

        private bool _isInitialized;

        // �κ��丮 ���� �̺�Ʈ (������ �߰�, ����, ���� ���� ��)
        public event Action OnInventoryChanged;

        // �κ��丮�� ���Ե� ��ü ������ ����Ʈ ����
        private List<Item> _items = new List<Item>();
        public List<Item> Items => _items;

        private void Awake()
        {
            // --- �̱��� �ߺ� �˻� �� �� ��ȯ �� �ı� ���� ���� ---
            if (_instance != null && _instance != this)
            {
                // �̹� �ٸ� ������ �Ѿ�� �κ��丮 �Ŵ����� �ִٸ�, ���� ������� �� �ı��մϴ�.
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // �� ������Ʈ�� �ֻ��� ������Ʈ(�θ� ���� ����)���� DontDestroyOnLoad�� �۵��մϴ�.
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // ���� �ʱ�ȭ ��� �ڵ� �߰� ����
            if (_isInitialized) return;
            _isInitialized = true;
        }

        /// <summary>
        /// �α��� ���� �� ���������� ȣ��Ǿ� �����͸� �����ϴ� �Լ�
        /// </summary>
        public void InitInventoryData()
        {
            if (BackendGameData.userData != null)
            {
                LoadInventoryFromSaveData(BackendGameData.userData.ownedItems);
                Debug.Log("[InventoryManager] �α��� ������ �κ��丮 ������ ���� ���� �Ϸ�!");

                // �̺�Ʈ �����ڵ鿡�� �����Ͱ� ����Ǿ����� �˸�
                OnInventoryChanged?.Invoke();
            }
            else
            {
                Debug.LogError("[InventoryManager] ���� ����: BackendGameData.userData�� null�Դϴ�.");
            }
        }



        // �κ��丮�� �� ������ ȹ��/�߰�
        public void AddItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("�߰��Ϸ��� �������� �������� �ʽ��ϴ�(null).");
                return;
            }

            _items.Add(item);
            OnInventoryChanged?.Invoke();
        }

        // �κ��丮���� ������ ���� (������, �Ǹ� ��)
        public void RemoveItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("�����Ϸ��� �������� �������� �ʽ��ϴ�(null).");
                return;
            }

            if (_items.Remove(item))
            {
                OnInventoryChanged?.Invoke();
            }
        }

        // �������� ���� ���� Ȯ��
        //  �������� ���� ���°� 'None'�� �ƴ����� �Ǵ�
        public bool IsEquippedItem(Item item)
        {
            if (item == null) return false;
            return item.CurrentSlotType != EquipSlotType.None;
        }

        // ������ ���� �Լ�
        public void EquipItem(Item item, EquipSlotType targetSlot)
        {
            if (item == null || !_items.Contains(item)) return;

            //������ �������� '����'�ε�, ���� ���Կ� ������� �ϸ� ����
            if (item is WeaponItem && targetSlot == EquipSlotType.Shield)
            {
                Debug.LogWarning($"[���� ����] ����� ���� ���Կ� ������ �� �����ϴ�. ������: {item.ItemData.ItemName}");
                return;
            }

            //  ������ �������� '����'�ε�, ���� ���Կ� ������� �ϸ� ����
            if (item is ShieldItem && (targetSlot == EquipSlotType.MainWeapon || targetSlot == EquipSlotType.SubWeapon))
            {
                Debug.LogWarning($"[���� ����] ���д� ���� ���Կ� ������ �� �����ϴ�. ������: {item.ItemData.ItemName}");
                return;
            }

            // [���� ����Ī] �̹� ��ǥ ����(targetSlot)�� �����ϰ� �ִ� ���� �������� �ִٸ� ���� ����(None) ó��
            Item oldItem = _items.Find(x => x != null && x.CurrentSlotType == targetSlot);
            if (oldItem != null)
            {
                oldItem.CurrentSlotType = EquipSlotType.None;
            }

            // �� �������� Ÿ�� ���� ��ũ ����
            item.CurrentSlotType = targetSlot;

            //������ ������(IsEquipped�� true�� ��)�� ����Ʈ�� �׻� ������ ������ ����
            SortInventory();

            if (Shield_Shot.Audio.SoundManager.Instance != null)
                Shield_Shot.Audio.SoundManager.Instance.PlayUI(_equipSfx, _equipSfxVolume);

            OnInventoryChanged?.Invoke();

            //�񵿱� ���ӵ����� ������Ʈ
            BackendGameData.Instance.GameDataUpdateAsync();
        }

        // ������ �ռ� �ý��� (��� 2���� �Ҹ��Ͽ� ����� 1�� ȹ��)
        public void CombineItems(Item targetItem, Item materialItem, Item resultItem)
        {
            if (targetItem == null || materialItem == null || resultItem == null)
            {
                Debug.LogWarning("�ռ��� �ʿ��� ������ ������ �����մϴ�.");
                return;
            }

            bool removedTarget = _items.Remove(targetItem);
            bool removedMaterial = _items.Remove(materialItem);

            if (!removedTarget || !removedMaterial)
            {
                Debug.LogWarning("�κ��丮���� �ռ� ��Ḧ ã�� �� ���� �ռ��� �����߽��ϴ�.");
                return;
            }

            _items.Add(resultItem);
            OnInventoryChanged?.Invoke();

            //�񵿱� ���ӵ����� ������Ʈ
            BackendGameData.Instance.GameDataUpdateAsync();
        }
        // ���̺� ������(�ڳ�)�κ��� ���� ����Ʈ�� �޾ƿ� �κ��丮 ����
        public void LoadInventoryFromSaveData(List<SaveItemInfo> savedItems)
        {
            // ���޹��� ���� ������ ����Ʈ ��ü�� ����ִٸ� ó�� �ߴ�
            if (savedItems == null)
            {
                Debug.LogWarning("�������� �ҷ��� ������ ����Ʈ�� ����ֽ��ϴ�.");
                return;
            }

            // ���� �ΰ��� �޸� ����Ʈ ����
            _items.Clear();

            // --- [�߰�] ������ ������ ������ ���� ���� ī���� ���� ---
            int equippedWeaponCount = 0;

            foreach (SaveItemInfo info in savedItems)
            {
                if (info == null || string.IsNullOrEmpty(info.itemId))
                {
                    continue; // �� ���̺� ������ ��ŵ
                }

                // 1. ������ ID�� "WP"�� �����ϸ� ���� ��ü�� ����
                if (info.itemId.StartsWith("WP"))
                {
                    //  ���� �������� ���� ������ ID�� ��Ȯ�� ���� �������� Ȯ��
                    Debug.Log($"[�κ��丮 �ε� üũ] �������� �о�� ID: '{info.itemId}'");
                    WeaponItemData weaponData = ItemDataParsingManager.Instance.GetWeaponData(info.itemId);


                    if (weaponData != null)
                    {
                        // �����ڸ� ����Ͽ� ���� ID�� ��ȭ ��ġ ����
                        WeaponItem newWeapon = new WeaponItem(weaponData, info.uniqueId, info.enhanceLevel, info.property, info.skillType);


                        // --- [����] ���̺� �����Ϳ� ����ִ� ���� ���� ���°� ���� ����ȭ ---
                        if (info.isEquipped)
                        {
                            equippedWeaponCount++;

                            if (equippedWeaponCount == 1)
                            {
                                // ù ��°�� �߰ߵ� ���� ����� �ֹ��� ���� ��ġ
                                newWeapon.CurrentSlotType = EquipSlotType.MainWeapon;
                                Debug.Log($"[�κ��丮 ����] �ֹ���(Main) ���� ����: {weaponData.ItemName}");
                            }
                            else if (equippedWeaponCount == 2)
                            {
                                // �� ��°�� �߰ߵ� ���� ����� �������� ���� ��ġ
                                newWeapon.CurrentSlotType = EquipSlotType.SubWeapon;
                                Debug.Log($"[�κ��丮 ����] ��������(Sub) ���� ����: {weaponData.ItemName}");
                            }
                            else
                            {
                                // Ȥ�ó� 3�� �̻� ���� ���·� ����Ǿ� �Ѿ���� ���� ���� �������� None ó��
                                newWeapon.CurrentSlotType = EquipSlotType.None;
                            }
                        }
                        else
                        {
                            newWeapon.CurrentSlotType = EquipSlotType.None;
                        }

                        _items.Add(newWeapon);
                    }
                    else
                    {
                        // �������� ��, ��ȹ ���̺�(��ųʸ�)�� ��ϵ� ��ü ���� ������ �� ������ Ȯ��
                        // (���� �� ������ 0����� �α��� ������ ��Ʈ �Ľ� ��ü�� ������ ��!)
                        int totalWeaponCount = ItemDataParsingManager.Instance.GetWeaponTableCount();

                        Debug.LogError($"[LoadError] ���� '{info.itemId}' ã�� ����. ���� ��ȹ ���̺��� ��ϵ� �� ���� ����: {totalWeaponCount}��");
                    }
                }

                // 2. ������ ID�� "SH"�� �����ϸ� ���� ��ü�� ����
                else if (info.itemId.StartsWith("SH"))
                {
                    ShieldItemData shieldData = ItemDataParsingManager.Instance.GetShieldData(info.itemId) as ShieldItemData;

                    if (shieldData != null)
                    {
                        // �����ڸ� ����Ͽ� ���� ID�� ��ȭ ��ġ ����
                        ShieldItem newShield = new ShieldItem(shieldData, info.uniqueId, info.enhanceLevel, info.property);
                       

                        if (info.isEquipped)
                            newShield.CurrentSlotType = EquipSlotType.Shield; // ���� �������� ����
                        else
                            newShield.CurrentSlotType = EquipSlotType.None;

                        _items.Add(newShield);
                    }
                    else
                    {
                        Debug.LogError($"[LoadError] ���� {info.itemId} �����͸� ��ȹ ���̺����� ã�� �� �����ϴ�.");
                    }
                }
            }

            // �ε尡 ������ ���� �� ���� �����۵��� �κ��丮 ������ ���̵��� ���� ����
            SortInventory();

            // �κ��丮 UI ���� �̺�Ʈ ȣ��
            OnInventoryChanged?.Invoke();
        }

        //���� �����۵��� �κ��丮 ������ ���̵��� ����
        private void SortInventory()
        {
            _items.Sort((x, y) =>
            {
                if (x == null && y == null) return 0;
                if (x == null) return 1;
                if (y == null) return -1;

                // [1����] ���� ���� �� (������ �� ������)
                int equippedCompare = y.IsEquipped.CompareTo(x.IsEquipped);

                // �� �������� ���� ���°� �ٸ��ٸ� (�ϳ��� ����, �ϳ��� ������) �� ������ ����
                if (equippedCompare != 0)
                {
                    return equippedCompare;
                }

                // [2����] ���� ���°� ���ٸ� (�� �� �����̰ų�, �� �� �������̸�)
                // ���� ����Ǿ� �ִ� slotIndex ��ȣ�� ���� ��(��������) ������ ������ ����
                return x.SlotIndex.CompareTo(y.SlotIndex);
            });
        }

        // ������ ��޼� ���� 

        public void SortByGrade()
        {
            Items.Sort((x, y) =>
            {
                // 0. Null ���� ó��
                if (x == null && y == null) return 0;
                if (x == null) return 1;
                if (y == null) return -1;

                // 1����: ���� ���� �� (������ true�� ������ ������ y.CompareTo(x))
                int equipCompare = y.IsEquipped.CompareTo(x.IsEquipped);
                if (equipCompare != 0)
                    return equipCompare;

                // 2����: ��ȭ ���� �� (��ȭ ������ ���� ���� ������ ������ y.CompareTo(x))
                int enhanceCompare = y.EnhanceLevel.CompareTo(x.EnhanceLevel);
                if (enhanceCompare != 0)
                    return enhanceCompare;

                // 3����: ��� �� (��ȭ �������� ���� ��)
                return ((int)y.ItemData.ItemGradeType).CompareTo((int)x.ItemData.ItemGradeType);
            });

            Debug.Log("�κ��丮: [���� �켱 -> ��ȭ������ -> ��޼�] ���� �Ϸ�");
            OnInventoryChanged?.Invoke();
        }


        // 2. ������ ������ ����
        public void SortByType()
        {
            Items.Sort((x, y) =>
            {
                // 0. Null ���� ó��
                if (x == null && y == null) return 0;
                if (x == null) return 1;
                if (y == null) return -1;

                // 1����: ���� ���� �� (������ true�� ������ ������)
                int equipCompare = y.IsEquipped.CompareTo(x.IsEquipped);
                if (equipCompare != 0)
                    return equipCompare;

                // 2����: ��ȭ ���� �� (��ȭ ������ ���� ���� ������ ������ y.CompareTo(x))
                int enhanceCompare = y.EnhanceLevel.CompareTo(x.EnhanceLevel);
                if (enhanceCompare != 0)
                    return enhanceCompare;

                // 3����: ������ ����(Ÿ��) �� (��ȭ �������� ���� ��)
                int typeCompare = x.ItemData.ItemType.CompareTo(y.ItemData.ItemType);
                if (typeCompare != 0)
                    return typeCompare;

                // 4����: ��� �� (�������� ���� ��)
                return ((int)y.ItemData.ItemGradeType).CompareTo((int)x.ItemData.ItemGradeType);
            });

            Debug.Log("�κ��丮: [���� �켱 -> ��ȭ������ -> ������] ���� �Ϸ�");
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// �ռ� ��ϵ� ����� ������� [��������, �ο��� �Ӽ�]�� ����ؼ� ����ִ� �Լ�
        /// </summary>
        public (bool isSuccess, ItemPropertyType rolledProperty) EvaluateCombineResult(ItemGradeType grade)
        {
            ItemCombineData config = ItemDataParsingManager.Instance.GetCombineData(grade);

            if (config == null)
            {
                Debug.LogError($"[Combine] {grade} ����� �ռ� ���̺��� �����ϴ�.");
                return (false, ItemPropertyType.None);
            }

            // 1��: �ռ� ���� ���� (float �ֻ����� ����)
            bool isSuccess = UnityEngine.Random.Range(0f, 100f) < config.SuccessRate;
            ItemPropertyType finalProperty = ItemPropertyType.None;

            // 2��: �������� ���� �Ӽ� �ο� Ȯ�� ��� (float �ֻ����� �����Ͽ� �Ҽ��� ����)
            if (isSuccess)
            {
                if (UnityEngine.Random.Range(0f, 100f) < config.PropertyApplyRate)
                {
                    finalProperty = ApplyRandomProperty();
                }
            }

            return (isSuccess, finalProperty);
        }

        // �Ӽ� �ο� ���� �� ������ �Ӽ�(Enum) �ο�
        private ItemPropertyType ApplyRandomProperty()
        {
            var allProps = (ItemPropertyType[])System.Enum.GetValues(typeof(ItemPropertyType));
            List<ItemPropertyType> validProps = new List<ItemPropertyType>();

            foreach (var prop in allProps)
            {
                if (prop != ItemPropertyType.None) validProps.Add(prop);
            }

            if (validProps.Count == 0) return ItemPropertyType.None;
            return validProps[UnityEngine.Random.Range(0, validProps.Count)];
        }

        /// <summary>
        /// UI�κ��� ��û�� �޾� ����/���� �ռ��� �ϰ� ó���ϰ� ������ ����ȭ�ϴ� �ٽ� �Լ�
        /// </summary>
        public Item ProcessItemCombine(Item target, Item material)
        {
            if (target == null || material == null) return null;

            // 1. ��� �Լ��� ����� ���� �ֻ��� ��� ȹ��
            ItemGradeType currentGrade = target.ItemData.ItemGradeType;
            var (isSuccess, rolledProperty) = EvaluateCombineResult(currentGrade);

            Item resultItem = null;

            if (isSuccess)
            {
                // [���� ó��] ���� ��� ���
                int nextGradeInt = (int)currentGrade + 1;
                ItemGradeType nextGrade = (ItemGradeType)nextGradeInt;

                //  2. ������ ��ü Ÿ�Կ� ���� �б� ó�� (���� vs ����)
                if (target is WeaponItem)
                {
                    // ������ ���: �Ľ� �Ŵ������� ���� ��� ���� ���� ������ ȹ��
                    WeaponItemData nextWeaponData = ItemDataParsingManager.Instance.GetRandomWeaponDataByGrade(nextGrade);
                    if (nextWeaponData != null)
                    {

                        string newUniqueId = System.Guid.NewGuid().ToString(); // �� ���� ID �߱�
                        int baseEnhanceLevel = 0;                             // �ռ� ���Ķ� 0��

                        resultItem = new WeaponItem(nextWeaponData, newUniqueId, baseEnhanceLevel, rolledProperty);
                    }
                }
                else if (target is ShieldItem) // ���� ������ Ŭ���� Ÿ�� üũ
                {
                    // ������ ���: �Ľ� �Ŵ������� ���� ��� ���� ���� ������ ȹ��
                    ShieldItemData nextShieldData = ItemDataParsingManager.Instance.GetRandomShieldDataByGrade(nextGrade);
                    if (nextShieldData != null)
                    {
                        // ���� ���� ����(4��¥�� ������)
                        string newUniqueId = System.Guid.NewGuid().ToString();
                        int baseEnhanceLevel = 0;

                        resultItem = new ShieldItem(nextShieldData, newUniqueId, baseEnhanceLevel, rolledProperty);
                    }
                }

                // ���������� ��� ������ ��ü�� �����Ǿ��ٸ� �κ��丮 ����
                if (resultItem != null)
                {
                    _items.Remove(target);
                    _items.Remove(material);
                    _items.Add(resultItem);

                    Debug.Log($"[Inventory] �ռ� ����! �� ������ ���� �Ϸ�: {resultItem.ItemData.ItemName} (�Ӽ�: {rolledProperty})");
                }
                else
                {
                    Debug.LogError($"[Inventory] �ռ� ���� �����̾�����, {nextGrade} ����� ��Ʈ �����͸� �ε����� ���߽��ϴ�.");
                    return null;
                }
            }
            else
            {
                //��� �����۸� �κ��丮���� ����
                _items.Remove(material);
                Debug.Log($"[Inventory] �ռ� ����. ��� {material.ItemData.ItemName}�� �ı�.");
            }
            // �ռ� ���� ���� ���
            if (Shield_Shot.Audio.SoundManager.Instance != null)
                Shield_Shot.Audio.SoundManager.Instance.PlayUI(_combineSfx, _combineSfxVolume);
            // ���� ���� �� UI ����
            OnInventoryChanged?.Invoke();
            BackendGameData.Instance.GameDataUpdateAsync();

            return resultItem;
        }

        /// <summary>
        /// �α׾ƿ� �� �κ��丮 �����͸� ������ �ʱ�ȭ�մϴ�.
        /// </summary>
        public void ClearInventory()
        {
            // 1. �޸𸮿� �ִ� ������ ����Ʈ �ʱ�ȭ
            if (_items != null)
            {
                _items.Clear();
            }

            // 2. �ʱ�ȭ ���� �÷��� ���� (�ٽ� �α������� �� �����Ͱ� �ε�ǵ���)
            _isInitialized = false;

            // 3. UI ���� �̺�Ʈ ȣ�� (�κ��丮 â�� �����ų� ��������� ����)
            OnInventoryChanged?.Invoke();

            Debug.Log("[InventoryManager] ��� �κ��丮 �����Ͱ� �ʱ�ȭ�Ǿ����ϴ�.");
        }
    }
}
