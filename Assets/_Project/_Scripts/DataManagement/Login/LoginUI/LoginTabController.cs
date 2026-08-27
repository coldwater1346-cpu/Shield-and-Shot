using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class LoginTabController : MonoBehaviour
    {


        [Header("Panel")]
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _signUpPanel;

        [Header("Button")]
        [SerializeField] private Button _loginTabBtn;
        [SerializeField] private Button _signUpTabBtn;

      



        private void Awake()
        {
            if( _loginPanel != null )
            {
                _loginTabBtn.onClick.AddListener(ShowLoginPanel);
            }
            if( _signUpPanel != null )
            {
                _signUpTabBtn.onClick.AddListener(ShowSignUpPanel);
            }
        }
        private void Start()
        {
            ShowLoginPanel();
        }
        private void OnDestroy()
        {
            if (_loginTabBtn != null)
                _loginTabBtn.onClick.RemoveListener(ShowLoginPanel);
            if (_signUpTabBtn != null) 
                _signUpTabBtn.onClick.RemoveListener(ShowSignUpPanel);
        }

        public void ShowLoginPanel()
        {
            _loginPanel.SetActive(true);
            _signUpPanel.SetActive(false);
           
        }

        public void ShowSignUpPanel()
        {
            _loginPanel.SetActive(false);
            _signUpPanel.SetActive(true);
           
        }
    }
}