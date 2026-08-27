using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class ConfirmPopupUI : MonoBehaviour
    {

        [SerializeField] private GameObject _confirmPopup;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private Action _onConfirm;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnClickConfirmButton);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(OnClickConfirmButton);

            if (_cancelButton != null)
                _cancelButton.onClick.RemoveListener(Close);
        }

        public void Open(string message, Action onConfirm)
        {
            _onConfirm = onConfirm;



            _confirmPopup.SetActive(true);
        }

        private void OnClickConfirmButton()
        {
            _onConfirm?.Invoke();
            Close();
        }

        private void Close()
        {
            _onConfirm = null;
            _confirmPopup.SetActive(false);
        }
    }
}