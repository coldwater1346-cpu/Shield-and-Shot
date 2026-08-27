using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shield_Shot.Core.SceneFlow
{
    public sealed class SceneFlowManager : PersistentSingleton<SceneFlowManager>
    {
        private SceneTransitionData _pendingTransitionData;
        private bool _isLoading;
        private AsyncOperation _operation;

        public bool IsLoading => _isLoading;
        public float GetLoadingProgress() => _operation != null ? _operation.progress : 0f;
        
        public void LoadScene(string sceneName, SceneTransitionData transitionData)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[SceneFlowManager] Scene loading is already in progress.");
                return;
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[SceneFlowManager] Scene name is empty.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName, transitionData));
        }

        public SceneTransitionData ConsumeTransitionData()
        {
            SceneTransitionData data = _pendingTransitionData;
            _pendingTransitionData = null;
            return data;
        }

        private IEnumerator LoadSceneRoutine(string sceneName, SceneTransitionData transitionData)
        {
            if(_isLoading)
            {
                Debug.LogWarning($"[SceneFlowManager] Loading is in progress. Waiting for completion...");
                yield return new WaitUntil(() => !_isLoading);
            }
            
            _isLoading = true;
            _pendingTransitionData = transitionData;

            _operation = SceneManager.LoadSceneAsync(sceneName);
            if (_operation == null)
            {
                Debug.LogError($"[SceneFlowManager] Failed to load scene: {sceneName}");
                _isLoading = false;
                yield break;
            }

            while (!_operation.isDone)
            {
                yield return null;
            }

            _isLoading = false;
            _operation = null;
        }
    }
}