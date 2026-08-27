using UnityEngine;

namespace Shield_Shot.Settings.Data
{
    public enum AudioChannel { BGM, SFX, UI }

    public class SoundSettings
    {
        public float bgmVolume = 0.8f;
        public float sfxVolume = 0.8f;
        public float uiVolume = 0.8f;

        public System.Action<AudioChannel, float> OnVolumeChanged;

        public void SetVolume(AudioChannel channel, float volume)
        {
            volume = Mathf.Clamp01(volume);

            switch (channel)
            {
                case AudioChannel.BGM: bgmVolume = volume; break;
                case AudioChannel.SFX: sfxVolume = volume; break;
                case AudioChannel.UI: uiVolume = volume; break;
            }

            PlayerPrefs.SetFloat($"{channel}_Volume", volume);
            OnVolumeChanged?.Invoke(channel, volume);
        }

        public float GetVolume(AudioChannel channel)
        {
            return channel switch
            {
                AudioChannel.BGM => bgmVolume,
                AudioChannel.SFX => sfxVolume,
                AudioChannel.UI => uiVolume,
                _ => 1f
            };
        }
    }
}