using Shield_Shot.Settings;
using Shield_Shot.Settings.Data;
using UnityEngine;
using UnityEngine.Audio;

namespace Shield_Shot.Audio
{
    public class AudioMixerController : PersistentSingleton<AudioMixerController>
    {
        [SerializeField] private AudioMixer audioMixer;

        private void Start()
        {
            if (Instance != this) return;

            if (GameSettingsManager.Instance != null && GameSettingsManager.Instance.Sound != null)
            {
                GameSettingsManager.Instance.Sound.OnVolumeChanged += SetChannelVolume;

                var audioSet = GameSettingsManager.Instance.Sound;
                SetChannelVolume(AudioChannel.BGM, audioSet.bgmVolume);
                SetChannelVolume(AudioChannel.SFX, audioSet.sfxVolume);
                SetChannelVolume(AudioChannel.UI, audioSet.uiVolume);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (GameSettingsManager.Instance != null && GameSettingsManager.Instance.Sound != null)
                GameSettingsManager.Instance.Sound.OnVolumeChanged -= SetChannelVolume;
        }

        private void SetChannelVolume(AudioChannel channel, float volume)
        {
            if (audioMixer == null) return;

            float db = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
            string parameterName = $"{channel}_Volume";
            audioMixer.SetFloat(parameterName, db);
        }
    }
}