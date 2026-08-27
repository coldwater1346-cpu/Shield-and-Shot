using Shield_Shot.Audio;
using Shield_Shot.DataManagement.Login;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shield_Shot.DataManagement.Login
{
    public class PostLoginInitializer : MonoBehaviour
    {
        [Header("Login Success Sound")]
        [SerializeField]
        private AudioClip loginSuccessSfx;

        [SerializeField, Range(0f, 1f)]
        private float loginSuccessSfxVolume = 0.5f;

        [Header("Next Scene")]
        [SerializeField]
        private string lobbySceneName = "03_Lobby";

        private readonly ServerChartLoader _chartLoader =
            new ServerChartLoader();

        private readonly UserDataLoader _userDataLoader =
            new UserDataLoader();

        private readonly InventoryInitializer _inventoryInitializer =
            new InventoryInitializer();

        public void Initialize(
            Action<InitializationResult> onComplete)
        {
            try
            {
                PlayLoginSound();

                // 1. 공용 서버 차트 데이터
                _chartLoader.LoadAll();

                // 2. 로그인한 사용자 데이터
                _userDataLoader.Load();

                // 3. 사용자 데이터 기반 인벤토리 구성
                _inventoryInitializer.Initialize();

                // 4. 로비 이동
                SceneManager.LoadScene(lobbySceneName);

                onComplete?.Invoke(
                    InitializationResult.Success());
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[PostLoginInitializer] 초기화 실패: {ex}");

                onComplete?.Invoke(
                    InitializationResult.Failure(
                        ex.Message));
            }
        }

        private void PlayLoginSound()
        {
            if (loginSuccessSfx == null)
            {
                return;
            }

            if (SoundManager.Instance == null)
            {
                Debug.LogWarning(
                    "[PostLoginInitializer] " +
                    "SoundManager.Instance가 없습니다.");

                return;
            }

            SoundManager.Instance.PlayUI(
                loginSuccessSfx,
                loginSuccessSfxVolume);
        }
    }
}