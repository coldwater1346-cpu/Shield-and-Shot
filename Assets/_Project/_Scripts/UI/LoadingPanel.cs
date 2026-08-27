using DG.Tweening;
using Shield_Shot.Core.SceneFlow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class LoadingPanel : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI[] _letters;
        [SerializeField] RectTransform _spinner;

        private Vector2[] _startPositions;

        private void Awake()
        {
            _startPositions = new Vector2[_letters.Length];
            for (int i = 0; i < _letters.Length; i++)
            {
                _startPositions[i] = _letters[i].rectTransform.anchoredPosition;
            }

        }

        void Start()
        {
            LoadingWave();
            SpinnerRotate();
        }


        private void LoadingWave()
        {
            Sequence waveSequence = DOTween.Sequence();

            for (int i = 0; i < _letters.Length; i++)
            {
                int index = i;
                var rect = _letters[index].rectTransform;

                waveSequence.AppendInterval(0.1f);

                waveSequence.Append(
                    rect.DOAnchorPosY(_startPositions[index].y + 20f, 0.5f)
                        .SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            rect.DOAnchorPosY(_startPositions[index].y, 0.5f)
                                .SetEase(Ease.InOutSine);
                        })
                );
            }

            waveSequence.SetLoops(-1, LoopType.Restart);
        }

        private void SpinnerRotate()
        {
            _spinner
                .DORotate(new Vector3(0, 0, -360), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear);
        }
    }
}
