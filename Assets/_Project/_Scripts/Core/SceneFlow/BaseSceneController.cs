using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shield_Shot.Core.SceneFlow
{
    public abstract class BaseSceneController : MonoBehaviour
    {
        [SerializeField] private SceneType _sceneType = SceneType.None;

        public SceneType SceneType => _sceneType;
        public string SceneName { get; private set; }
        public bool IsInitialized { get; private set; }
        public SceneTransitionData TransitionData { get; private set; }

        protected virtual void Awake()
        {
            SceneName = SceneManager.GetActiveScene().name;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            SceneTransitionData transitionData = SceneFlowManager.Instance != null
    ? SceneFlowManager.Instance.ConsumeTransitionData()
    : null;

            Initialize(transitionData);
        }

        public void Initialize(SceneTransitionData transitionData)
        {
            if (IsInitialized)
            {
                return;
            }

            TransitionData = transitionData;
            IsInitialized = true;

            OnBeforeEnterScene();
            OnEnterScene(transitionData);
            OnAfterEnterScene();
        }

        public void Cleanup()
        {
            if (!IsInitialized)
            {
                return;
            }

            OnBeforeExitScene();
            OnExitScene();
            OnAfterExitScene();

            TransitionData = null;
            IsInitialized = false;
        }

        protected virtual void OnBeforeEnterScene()
        {
        }

        protected abstract void OnEnterScene(SceneTransitionData transitionData);

        protected virtual void OnAfterEnterScene()
        {
        }

        protected virtual void OnBeforeExitScene()
        {
        }

        protected virtual void OnExitScene()
        {
        }

        protected virtual void OnAfterExitScene()
        {
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }
    }

}
