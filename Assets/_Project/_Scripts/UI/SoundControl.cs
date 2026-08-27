using Shield_Shot.Settings;
using Shield_Shot.Settings.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class SoundControl : MonoBehaviour
    {
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite _onSprite;
        [SerializeField] private Sprite _offSprite;
        [SerializeField] private AudioChannel _channel;

        private float _lastVolume = 1f;

        private void Awake()
        {
            _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            GetComponentInChildren<Button>().onClick.AddListener(ToggleMute);
        }

        private void Start()
        {
            float saved = GameSettingsManager.Instance != null
                ? GameSettingsManager.Instance.Sound.GetVolume(_channel)
                : 1f;

            _volumeSlider.SetValueWithoutNotify(saved);
            UpdateSoundUI(saved);
        }

        private void OnVolumeChanged(float value)
        {
            UpdateSoundUI(value);

            if (GameSettingsManager.Instance != null)
                GameSettingsManager.Instance.Sound.SetVolume(_channel, value);
        }

        public void ToggleMute()
        {
            if (_volumeSlider.value > 0)
            {
                _lastVolume = _volumeSlider.value;
                _volumeSlider.value = 0;
            }
            else
            {
                _volumeSlider.value = _lastVolume > 0 ? _lastVolume : 0.5f;
            }
        }

        private void UpdateSoundUI(float value)
        {
            _iconImage.sprite = value > 0 ? _onSprite : _offSprite;
        }
    }
}