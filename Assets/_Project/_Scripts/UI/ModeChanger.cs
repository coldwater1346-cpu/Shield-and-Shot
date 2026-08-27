using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class ModeChanger : MonoBehaviour
    {
        [SerializeField] private Image _modeBtn;
        [SerializeField] private Sprite _storySprite;
        [SerializeField] private Sprite _infinitySprite;

        [SerializeField] private GameObject _storyPanel;
        [SerializeField] private GameObject _infinityPanel;

        private bool _isInfinityMode = false;

        public void OnToggleModeClicked()
        {
            _isInfinityMode = !_isInfinityMode;

            if(_isInfinityMode)
            {
                EnterInfinityMode();
            }
            else
            {
                EnterStoryMode();
            }
        }

        private void EnterInfinityMode()
        {
            _modeBtn.sprite = _infinitySprite;

            _storyPanel.SetActive(false);
            _infinityPanel.SetActive(true);

            Debug.Log("무한모드 진입");
        }

        private void EnterStoryMode()
        {
            _modeBtn.sprite = _storySprite;

            _storyPanel.SetActive(true);
            _infinityPanel.SetActive(false);

            Debug.Log("스토리모드 진입");
        }
    }
}
