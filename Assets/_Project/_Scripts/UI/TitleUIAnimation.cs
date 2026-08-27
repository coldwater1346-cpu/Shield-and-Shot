using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace Shield_Shot.UI
{
    public class TitleUIAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _titleLogo;
        [SerializeField] private CanvasGroup _startText;

        [SerializeField] private float _dropDuration = 1f;
        [SerializeField] private float _fadeDuration = 0.8f;

        void Start()
        {
            Vector2 originPos = _titleLogo.anchoredPosition;
            _titleLogo.anchoredPosition = new Vector2(originPos.x, originPos.y + 1500f);

            _titleLogo.DOAnchorPos(originPos, _dropDuration).SetEase(Ease.OutBack);

            _startText.alpha = 0f;
            _startText.DOFade(1f, _fadeDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }
}
