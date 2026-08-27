using System.Collections;
using Shield_Shot.GameplayCore.Network;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Shield_Shot.Core
{
    public sealed class ApplicationLifecycleManager : MonoBehaviour
    {
        public static ApplicationLifecycleManager Instance { get; private set; }

        [SerializeField] private float _quitDelaySeconds = 0.5f;

        private bool _isQuitting;
        private bool _allowQuit;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            Application.wantsToQuit += OnWantsToQuit;
            Application.quitting += OnQuitting;
        }

        private void OnDisable()
        {
            Application.wantsToQuit -= OnWantsToQuit;
            Application.quitting -= OnQuitting;
        }

        public void QuitGame()
        {
            if (_isQuitting)
            {
                return;
            }

            StartCoroutine(QuitRoutine());
        }

        private bool OnWantsToQuit()
        {
            if (_allowQuit)
            {
                return true;
            }

            if (!_isQuitting)
            {
                StartCoroutine(QuitRoutine());
            }

            return false;
        }

        private void OnQuitting()
        {
            ShutdownNetwork();
        }

        private IEnumerator QuitRoutine()
        {
            _isQuitting = true;

            ShutdownNetwork();

            if (_quitDelaySeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(_quitDelaySeconds);
            }

            _allowQuit = true;

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void ShutdownNetwork()
        {
            if (NetworkMatchManager.Instance != null)
            {
                NetworkMatchManager.Instance.ShutdownNetwork();
            }
        }
    }
}
