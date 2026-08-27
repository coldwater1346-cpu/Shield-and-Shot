using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI.Core
{    
    public enum AlertType
    {
        System,
        Error,
        Reward
    }

    public class AlertSystem : MonoBehaviour
    {
        private static AlertSystem _instance;

        [SerializeField] private GameObject _alertContainer;
        [SerializeField] private TextMeshProUGUI _alertTitle;
        [SerializeField] private TextMeshProUGUI _alertMessage;
        [SerializeField] private Button _confirmBtn;

        [SerializeField] private GameObject _toastContainer;
        [SerializeField] private TextMeshProUGUI _toastMessage;

        private Coroutine _toastCoroutine;

        private void Awake()
        {
            if(_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _alertContainer.SetActive(false);
            _toastContainer.SetActive(false);

            _confirmBtn.onClick.AddListener(() => _alertContainer.SetActive(false));
            DontDestroyOnLoad(gameObject);
        }

        public static void Alert(string title, string message,  AlertType type)
        {
            if (_instance == null) return;
            _instance._alertTitle.text = title;
            _instance._alertMessage.text = message;
            _instance._alertContainer.SetActive(true);
        }

        public static void ShowToast(string message, float duration)
        {
            if (_instance == null) return;
            if (_instance._toastCoroutine != null) _instance.StopCoroutine(_instance._toastCoroutine);

            _instance._toastMessage.text = message;
            _instance._toastContainer.SetActive(true);
            _instance._toastCoroutine = _instance.StartCoroutine(_instance.HideToastAfter(duration));
        }

        private IEnumerator HideToastAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            _toastContainer.SetActive(false);
        }


    }
}

