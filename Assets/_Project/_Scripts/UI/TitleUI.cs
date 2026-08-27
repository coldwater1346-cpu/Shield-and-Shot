using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shield_Shot.UI
{
    public class TitleUI : MonoBehaviour
    {
        [Header("Next Scene Name")]
        [SerializeField] private string _lobbySceneName = "02_Login";

        private bool _isTransitioning = false;

        void Update()
        {
            if (_isTransitioning) return;
            
            if(Input.GetMouseButtonUp(0))
            {
                StartCoroutine(TransitionRoutine());
            }
        }

        private IEnumerator TransitionRoutine()
        {
            _isTransitioning = true;
            Debug.Log("화면 터치 감지 - 로그인 씬 전환");

            Input.ResetInputAxes();

            yield return null;

            SceneManager.LoadScene(_lobbySceneName);
        }


        private void TriggerSceneTransition()
        {
            _isTransitioning = true;
            Debug.Log("화면 터치 감지 - 로그인 씬 전환");

            SceneManager.LoadScene(_lobbySceneName);
        }
    }
}

