using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shield_Shot.Core.SceneFlow
{
    public sealed class IntroSceneController : BaseSceneController
    {
        [Header("Intro")]
        [SerializeField] private string _titleSceneName = "01_Title";
        [SerializeField] private float _minIntroDuration = 1f;

        private Coroutine _introRoutine;

        protected override void OnEnterScene(SceneTransitionData transitionData)
        {
            _introRoutine = StartCoroutine(PlayIntroRoutine());
        }

        protected override void OnExitScene()
        {
            if (_introRoutine != null)
            {
                StopCoroutine(_introRoutine);
                _introRoutine = null;
            }
        }

        private IEnumerator PlayIntroRoutine()
        {
            SceneTransitionData transitionData = new SceneTransitionData(
                fromScene: SceneType.Intro,
                toScene: SceneType.Title,
                reason: SceneTransitionReason.IntroToTitle);

            if(SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(_titleSceneName, transitionData);

                float timer = 0f;
                while(timer < _minIntroDuration)
                {
                    timer += Time.unscaledDeltaTime;
                    yield return null;
                }

                while(SceneFlowManager.Instance.IsLoading)
                {
                    yield return null;
                }

                yield break;
            }

            yield return new WaitForSecondsRealtime(_minIntroDuration);
            Debug.LogWarning("[IntroSceneController] SceneFlowManager is missing. Falling back to SceneManager.LoadScene.");
            SceneManager.LoadScene(_titleSceneName);
        }


        //private IEnumerator PlayIntroRoutine()
        //{
        //    if (_minIntroDuration > 0f)
        //    {
        //        yield return new WaitForSecondsRealtime(_minIntroDuration);
        //    }

        //    LoadTitleScene();
        //}

        //private void LoadTitleScene()
        //{
        //    SceneTransitionData transitionData = new SceneTransitionData(
        //        fromScene: SceneType.Intro,
        //        toScene: SceneType.Title,
        //        reason: SceneTransitionReason.IntroToTitle);

        //    if (SceneFlowManager.Instance != null)
        //    {
        //        SceneFlowManager.Instance.LoadScene(_titleSceneName, transitionData);
        //        return;
        //    }

        //    Debug.LogWarning("[IntroSceneController] SceneFlowManager is missing. Falling back to SceneManager.LoadScene.");
        //    SceneManager.LoadScene(_titleSceneName);
        //}
    }
}
