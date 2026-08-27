using DG.Tweening;
using Shield_Shot.Core.SceneFlow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class LoadingUIAnimation : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI[] _letters;
        [SerializeField] RectTransform _spinner;
        [SerializeField] Slider _loadingBar;
        [SerializeField] private CanvasGroup _fadeGroup;

        private Vector2[] _startPositions;
        private bool _isFading = false;

        private Sequence _waveSequence;

        private void Awake()
        {
            _startPositions = new Vector2[_letters.Length];
            for(int i = 0; i < _letters.Length; i++)
            {
                _startPositions[i] = _letters[i].rectTransform.anchoredPosition;
            }

            if (_fadeGroup != null) _fadeGroup.alpha = 0f;
        }

        void Start()
        {
            LoadingWave();
            SpinnerRotate();
        }

        private void Update()
        {
            if (!SceneFlowManager.Instance.IsLoading) return;

            float progress = SceneFlowManager.Instance.GetLoadingProgress();
            _loadingBar.value = Mathf.Clamp01(progress / 0.9f);

            if(_loadingBar.value >= 0.99f && !_isFading)
            {
                _isFading = true;
                StartFadeOut();
            }
        }

        private void StartFadeOut()
        {
            if(_fadeGroup != null)
            {
                _fadeGroup.DOFade(1f, 0.5f);
            }
        }

        private void LoadingWave()
        {
            _waveSequence?.Kill();

            _waveSequence = DOTween.Sequence();

            for (int i = 0; i < _letters.Length; i++)
            {
                int index = i;

                if (_letters[index] == null) continue;
                var rect = _letters[index].rectTransform;

                _waveSequence.AppendInterval(0.1f);

                _waveSequence.Append(
                    rect.DOAnchorPosY(_startPositions[index].y + 20f, 0.5f)
                        .SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            if(rect != null)
                            {
                                rect.DOAnchorPosY(_startPositions[index].y, 0.5f)
                                .SetEase(Ease.InOutSine);
                            }
                        })
                );
            }
            _waveSequence.SetLoops(-1, LoopType.Restart);
        }

        private void SpinnerRotate()
        {
            _spinner
                .DORotate(new Vector3(0, 0, -360), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear);
        }

        private void OnDestroy()
        {
            _waveSequence?.Kill();

            if (_spinner != null) _spinner.DOKill();
            if(_fadeGroup != null) _fadeGroup.DOKill();
        }
    }
}
