using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class ItemSlotInfo : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        public int SlotIndex;

        private Sprite _mySprite;
        public Sprite GetSprite() => _mySprite;

        public event Action<ItemSlotInfo> OnSlotClicked;

        public int Setup(int index, Sprite itemSprite)
        {
            SlotIndex = index;
            _mySprite = itemSprite;
            _icon.sprite = itemSprite;
            GetComponent<Button>().onClick.AddListener(() => OnSlotClicked?.Invoke(this));
            return SlotIndex;
        }       
        
    }
}

