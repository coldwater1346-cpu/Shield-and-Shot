using Shield_Shot.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class QuitPopupUI : UIPopupBase
    {
        [SerializeField] private Button _confirmBtn;
        [SerializeField] private Button _closeBtn;

        private void Awake()
        {
            _confirmBtn.onClick.AddListener(ExecuteQuit);
            _closeBtn.onClick.AddListener(Close);
        }

        private void ExecuteQuit()
        {
            Debug.Log("게임종료");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

