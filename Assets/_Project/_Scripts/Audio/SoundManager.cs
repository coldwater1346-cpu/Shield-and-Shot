using System.Collections.Generic;
using Shield_Shot.Core; // PoolManager를 참조하기 위해 추가
using Shield_Shot.Settings;
using Shield_Shot.Settings.Data;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Shield_Shot.Audio
{
    public class SoundManager : PersistentSingleton<SoundManager>
    {
        [Header("믹서 그룹 참조")]
        [SerializeField] private AudioMixerGroup bgmGroup;

        [Header("오디오 소스 프리팹 (PoolManager에 등록할 원본)")]
        [SerializeField] private GameObject sfxPrefab;
        [SerializeField] private GameObject uiPrefab;

        [Header("초기 풀링 생성 개수")]
        [SerializeField] private int sfxInitialCount = 15;
        [SerializeField] private int uiInitialCount = 5;

        [Header("BGM Playlists")]
        [Tooltip("BgmGroup별 BGM 리스트. 같은 그룹 안에서 랜덤으로 재생되고, 곡이 끝나면 자동으로 다음 곡을 랜덤 재생한다.")]
        [SerializeField] private List<BgmPlaylistEntry> bgmPlaylists = new List<BgmPlaylistEntry>();

        [Header("BGM Base Volume")]
        [Tooltip("사용자 설정 볼륨(0~1)과는 별개로 BGM에 항상 곱해지는 기본 배율. " +
                 "SFX/BGM을 둘 다 최대로 놓아도 BGM이 SFX보다 상대적으로 크게 들리면 이 값을 낮춰서 밸런스를 맞춘다. " +
                 "예: 0.7이면 설정 볼륨이 100%여도 실제로는 70% 크기로 재생됨.")]
        [SerializeField, Range(0f, 1f)] private float bgmBaseVolumeMultiplier = 0.7f;

        [Header("Scene -> BGM Group 매핑")]
        [Tooltip("씬 이름과 BgmGroup을 매핑한다. 씬이 로드될 때마다 자동으로 해당 그룹의 BGM을 요청한다. " +
                 "(씬마다 별도 트리거 오브젝트를 둘 필요 없음)")]
        [SerializeField] private List<SceneBgmMapping> sceneBgmMappings = new List<SceneBgmMapping>();

        private AudioSource bgmSource;

        // 현재 재생 그룹/리스트/직전 곡 (직전 곡 연속 재생 방지용)
        private BgmGroup? _currentGroup = null;
        private List<AudioClip> _currentPlaylist;
        private AudioClip _lastPlayedClip;

        // 사용 중인 소스들의 실시간 반납 처리를 위한 리스트
        private List<(GameObject prefab, GameObject instance, AudioSource source)> activeSources =
            new List<(GameObject, GameObject, AudioSource)>();

        protected override void Awake()
        {
            base.Awake();

            // 중복 생성되어 곧 파괴될 인스턴스는 이벤트 구독하지 않는다.
            if (Instance != this) return;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (GameSettingsManager.Instance != null)
                GameSettingsManager.Instance.Sound.OnVolumeChanged -= HandleVolumeChanged;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            for (int i = 0; i < sceneBgmMappings.Count; i++)
            {
                if (sceneBgmMappings[i].SceneName == scene.name)
                {
                    RequestBgmGroup(sceneBgmMappings[i].Group);
                    return;
                }
            }

            Debug.LogWarning($"[SoundManager] 씬 '{scene.name}'에 매핑된 BgmGroup이 없습니다.");
        }

        private void Start()
        {
            // 게임 시작 시 범용 PoolManager에게 사운드 그릇들을 미리 만들어 두라고 요청 (Warm-up)
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.CreatePool(sfxPrefab, sfxInitialCount, transform);
                PoolManager.Instance.CreatePool(uiPrefab, uiInitialCount, transform);
            }

            // BGM 소스는 풀링할 필요가 없으므로 자체 생성하여 유지
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.outputAudioMixerGroup = bgmGroup;
            // 개별 곡을 loop하지 않는다 - 곡이 끝나면 Update()에서 감지해서
            // 같은 그룹 리스트 안에서 다른 곡을 랜덤으로 골라 이어 튼다.
            bgmSource.loop = false;

            // 저장된 BGM 볼륨을 즉시 반영하고, 이후 설정이 바뀔 때마다 갱신되도록 구독한다.
            if (GameSettingsManager.Instance != null)
            {
                ApplyBgmVolume(GameSettingsManager.Instance.Sound.GetVolume(AudioChannel.BGM));
                GameSettingsManager.Instance.Sound.OnVolumeChanged += HandleVolumeChanged;
            }
            else
            {
                // 설정 시스템이 없는 테스트 씬 등에서도 기본 배율만큼은 적용된 상태로 시작한다.
                bgmSource.volume = bgmBaseVolumeMultiplier;
            }
        }

        private void HandleVolumeChanged(AudioChannel channel, float volume)
        {
            if (channel != AudioChannel.BGM) return;
            ApplyBgmVolume(volume);
        }

        private void ApplyBgmVolume(float settingsVolume)
        {
            if (bgmSource == null) return;
            bgmSource.volume = Mathf.Clamp01(settingsVolume) * bgmBaseVolumeMultiplier;
        }

        public void SetBgmVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);

            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.Sound.SetVolume(AudioChannel.BGM, volume);
            }
            else
            {
                ApplyBgmVolume(volume);
            }
        }

        public float GetBgmVolume()
        {
            if (GameSettingsManager.Instance != null)
                return GameSettingsManager.Instance.Sound.GetVolume(AudioChannel.BGM);

            return bgmSource != null ? bgmSource.volume : 1f;
        }

        private void Update()
        {
            // 사용 중인 사운드 컴포넌트들을 검사하여 재생이 끝났다면 PoolManager에게 반납(Push)
            for (int i = activeSources.Count - 1; i >= 0; i--)
            {
                var item = activeSources[i];
                if (!item.source.isPlaying)
                {
                    // PoolManager의 규칙에 맞춰 원본 프리팹과 생성된 인스턴스를 함께 넘겨주며 반납
                    PoolManager.Instance.Push(item.prefab, item.instance, transform);
                    activeSources.RemoveAt(i);
                }
            }

            // 현재 재생 중인 BGM이 끝났다면, 같은 그룹 리스트 안에서 다음 곡을 랜덤으로 이어 튼다.
            if (bgmSource != null && !bgmSource.isPlaying &&
                _currentPlaylist != null && _currentPlaylist.Count > 0)
            {
                PlayRandomFromCurrentPlaylist();
            }
        }

        public void RequestBgmGroup(BgmGroup group)
        {
            if (_currentGroup.HasValue && _currentGroup.Value == group)
            {
                // 같은 그룹 - 재생 중인 BGM을 그대로 유지한다.
                return;
            }

            _currentGroup = group;
            _currentPlaylist = GetPlaylist(group);
            _lastPlayedClip = null;

            if (_currentPlaylist == null || _currentPlaylist.Count == 0)
            {
                Debug.LogWarning($"[SoundManager] BgmGroup '{group}'에 등록된 BGM이 없습니다.");
                bgmSource.Stop();
                return;
            }

            PlayRandomFromCurrentPlaylist();
        }

        private List<AudioClip> GetPlaylist(BgmGroup group)
        {
            for (int i = 0; i < bgmPlaylists.Count; i++)
            {
                if (bgmPlaylists[i].Group == group)
                    return bgmPlaylists[i].Clips;
            }

            return null;
        }

        private void PlayRandomFromCurrentPlaylist()
        {
            if (_currentPlaylist == null || _currentPlaylist.Count == 0) return;

            AudioClip next = PickRandomClip(_currentPlaylist, _lastPlayedClip);
            _lastPlayedClip = next;

            bgmSource.clip = next;
            bgmSource.Play();
        }

        private AudioClip PickRandomClip(List<AudioClip> clips, AudioClip exclude)
        {
            if (clips.Count == 1) return clips[0];

            AudioClip picked;
            int guard = 0;
            do
            {
                picked = clips[Random.Range(0, clips.Count)];
                guard++;
            } while (picked == exclude && guard < 10);

            return picked;
        }

        public void PlayBGM(AudioClip clip)
        {
            if (bgmSource == null || clip == null) return;
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            PlaySound(sfxPrefab, clip, volume);
        }

        public void PlayUI(AudioClip clip, float volume = 1f)
        {
            PlaySound(uiPrefab, clip, volume);
        }

        private void PlaySound(GameObject prefab, AudioClip clip, float volume = 1f)
        {
            if (clip == null || PoolManager.Instance == null || prefab == null) return;

            // PoolManager에게 그릇 오브젝트 빌려오기
            GameObject go = PoolManager.Instance.Pop(prefab, transform);
            AudioSource source = go.GetComponent<AudioSource>();

            if (source != null)
            {
                source.clip = clip;
                source.volume = Mathf.Clamp01(volume);
                source.Play();

                // 추적 리스트에 백업
                activeSources.Add((prefab, go, source));
            }
        }
    }

    [System.Serializable]
    public struct BgmPlaylistEntry
    {
        public BgmGroup Group;
        public List<AudioClip> Clips;
    }

    [System.Serializable]
    public struct SceneBgmMapping
    {
        [Tooltip("Unity 씬 이름과 정확히 일치해야 한다 (Build Settings에 등록된 씬 이름).")]
        public string SceneName;
        public BgmGroup Group;
    }
}