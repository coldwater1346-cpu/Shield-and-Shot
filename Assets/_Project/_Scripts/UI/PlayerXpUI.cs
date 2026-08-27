using Shield_Shot.GameplayCore.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class PlayerXpUI : MonoBehaviour
    {
        [SerializeField] private Slider _xpSlider;
        [SerializeField] private TextMeshProUGUI _levelText;


        void Start()
        {
            if(PlayerLevelSystem.Instance != null)
            {
                PlayerLevelSystem.Instance.XpChanged += UpdateXpSlider;
                PlayerLevelSystem.Instance.LeveledUp += UpdateLevelUI;

                UpdateLevelUI(PlayerLevelSystem.Instance.Level);
            }
        }

        private void OnDestroy()
        {
            if(PlayerLevelSystem.Instance != null)
            {
                PlayerLevelSystem.Instance.XpChanged -= UpdateXpSlider;
                PlayerLevelSystem.Instance.LeveledUp -= UpdateLevelUI;
            }
        }

        private void UpdateXpSlider(int currentXp, int xpToNext)
        {
            if(_xpSlider != null)
            {
                _xpSlider.maxValue = xpToNext;
                _xpSlider.value = currentXp;
            }
        }

        private void UpdateLevelUI(int newLevel)
        {
            if(_levelText != null)
            {
                _levelText.text = $"{newLevel}";
            }
        }
    }
}
