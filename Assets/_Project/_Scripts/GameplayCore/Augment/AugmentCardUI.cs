using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using TMPro;

public class AugmentCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Component Bindings")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _glowEffect;

    private ProjectileBehaviorSO _boundSO;
    // PopupUI의 OnCardSelected 껍데기 함수와 규격을 맞춘 델리게이트 콜백
    private System.Action<int, string> _onSelectedCallback;

    private int _slotIndex;
    private Vector3 _targetLocalPosition;
    private Vector3 _originalScale = Vector3.one;

    /// <summary>
    /// AugmentPopupUI에서 호출하는 규격에 맞게 매개변수 인터페이스를 통일했습니다.
    /// </summary>
    public void SetupUICardSlot(ProjectileBehaviorSO so, int slotIndex, int currentLevel, System.Action<int, string> onSelected)
    {
        // 1. 자동 할당 방어 코드 (CanvasGroup)
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _boundSO = so;
        _slotIndex = slotIndex;
        _onSelectedCallback = onSelected;

        // 배치 좌표 연산
        _targetLocalPosition = slotIndex == 1 ? new Vector3(-280f, 0f, 0f) : new Vector3(280f, 0f, 0f);

        // 초기 상태 리셋
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = true;
        if (_glowEffect != null) _glowEffect.gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.zero;

        // 🔥 [방어막 1] 데이터가 null(빈 선택지)일 경우, 밑에 코드가 실행되지 않도록 여기서 즉시 컷트합니다.
        if (so == null)
        {
            if (_titleText != null) _titleText.text = "공백 증강";
            if (_descriptionText != null) _descriptionText.text = "더 이상 선택할 수 있는 증강 특성이 없습니다.";
            _canvasGroup.DOFade(1f, 0.4f).SetUpdate(true);
            return;
        }

        // 🔥 [방어막 2] 인스펙터 컴포넌트 할당 누락 체크 (54번째 줄 에러 완벽 원천 차단)
        if (_titleText == null || _descriptionText == null)
        {
            Debug.LogError($"<color=red>🚨 [AugmentCardUI] {gameObject.name}의 TitleText 또는 DescriptionText 컴포넌트가 인스펙터에서 연결되지 않았습니다! </color>");
            return;
        }

        // 위 방어막들을 다 통과해야 안전하게 텍스트 대입 진입
        _titleText.text = string.IsNullOrWhiteSpace(so.BehaviorName) ? so.name : so.BehaviorName;
        _descriptionText.text = so.GetDynamicDescription(currentLevel);

        if (_iconImage != null && so.Icon != null) // 아이콘 변수명이 다를 수 있으니 체크 후 주입
        {
            _iconImage.sprite = so.Icon;
        }

        // Phase 2: DoTween 등장 연출 실행
        transform.DOLocalMove(_targetLocalPosition, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        transform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        _canvasGroup.DOFade(1f, 0.4f).SetUpdate(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_boundSO == null) return;
        transform.DOKill();
        transform.DOScale(_originalScale * 1.08f, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);
        transform.DOLocalMove(_targetLocalPosition + new Vector3(0, 20f, 0), 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);

        //_glowEffect.gameObject.SetActive(true);
        //_glowEffect.DOFade(1f, 0.15f).From(0f).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_boundSO == null) return;
        transform.DOKill();
        transform.DOScale(_originalScale, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);
        transform.DOLocalMove(_targetLocalPosition, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);

        //_glowEffect.DOFade(0f, 0.15f).SetUpdate(true).OnComplete(() => _glowEffect.gameObject.SetActive(false));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_boundSO == null) return;

        // 클릭 연출 도중 중복 클릭 방지 차단 연설
        _canvasGroup.blocksRaycasts = false;

        // PopupUI 마스터에게 보고하여 전체 카드 카드 오케스트라 제어 유도
        string behaviorID = _boundSO != null ? _boundSO.BehaviorID : string.Empty;
        _onSelectedCallback?.Invoke(_slotIndex, behaviorID);
    }

    public void PlayFadeOutSequence()
    {
        transform.DOKill();
        _canvasGroup.blocksRaycasts = false;
        transform.DOLocalMove(_targetLocalPosition + new Vector3(0, -50f, 0), 0.3f).SetEase(Ease.InCubic).SetUpdate(true);
        _canvasGroup.DOFade(0f, 0.3f).SetUpdate(true);
    }

    public void PlaySelectedSequence(System.Action onComplete)
    {
        transform.DOKill();
        _canvasGroup.blocksRaycasts = false;
        //_glowEffect.gameObject.SetActive(true);

        Sequence selectSeq = DOTween.Sequence().SetUpdate(true);
        selectSeq.Append(transform.DOScale(_originalScale * 1.2f, 0.15f).SetEase(Ease.OutQuad))
                 .Append(transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InQuad))
                 .Join(_canvasGroup.DOFade(0f, 0.25f))
                 .OnComplete(() => onComplete?.Invoke());
    }
}
