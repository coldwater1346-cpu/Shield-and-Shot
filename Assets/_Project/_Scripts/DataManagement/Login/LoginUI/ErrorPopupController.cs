using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Shield_Shot.NetworkCore.UI
{
    public class ErrorPopupController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI errorText; // 에러 메시지 텍스트
        [SerializeField] private Button okButton;

        [Header("Sound")]
        [Tooltip("입력값 누락, 로그인 실패 등 에러 팝업이 뜰 때 재생할 사운드")]
        [SerializeField] private AudioClip errorSfx;
        [SerializeField, Range(0f, 1f)] private float errorSfxVolume = 0.5f;

        private void Awake()
        {
            
            if (okButton != null)
            {
                okButton.onClick.AddListener(ClosePopup);
            }
        }

        private void Start()
        {
            
            ClosePopup();
        }

        
        public void ShowError(string errorMessage)
        {
            if (errorText != null)
            {
                errorText.text = errorMessage;
            }
            gameObject.SetActive(true);

            if (Shield_Shot.Audio.SoundManager.Instance != null)
                Shield_Shot.Audio.SoundManager.Instance.PlayUI(errorSfx, errorSfxVolume);
        }

       
        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (okButton != null)
            {
                okButton.onClick.RemoveListener(ClosePopup);
            }
        }
    }
}