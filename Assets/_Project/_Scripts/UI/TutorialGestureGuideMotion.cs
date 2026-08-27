using DG.Tweening;
using UnityEngine;

namespace Shield_Shot.UI
{
    public enum TutorialGestureMotionMode
    {
        None,
        WeaponVerticalDrag,
        WeaponDownRepeat,
        WeaponDownHold,
        ShieldArcDrag,
    }

    [RequireComponent(typeof(RectTransform))]
    public class TutorialGestureGuideMotion : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField, Min(0.1f)] private float _cycleDuration = 1.2f;
        [SerializeField, Min(0f)] private float _weaponHoldDuration = 3f;
        [SerializeField] private Ease _motionEase = Ease.OutSine;

        [Header("Weapon Motion")]
        [SerializeField] private Vector2 _weaponCenter = new Vector2(0.75f, 0.5f);
        [SerializeField] private float _weaponVerticalRange = 0.32f;

        [Header("Shield Motion")]
        [SerializeField] private Vector2 _shieldStart = new Vector2(0.18f, 0.42f);
        [SerializeField] private Vector2 _shieldEnd = new Vector2(0.42f, 0.42f);
        [SerializeField] private float _shieldArcHeight = 0.18f;

        [Header("Visual")]
        [SerializeField] private float _weaponRotationZ = 0f;
        [SerializeField] private float _shieldRotationZ = -20f;

        private RectTransform _rectTransform;
        private RectTransform _canvasRect;
        private TutorialGestureMotionMode _mode;
        private Tween _motionTween;
        private float _motionProgress;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        private void OnEnable()
        {
            if (_mode != TutorialGestureMotionMode.None && _canvasRect != null)
            {
                RestartMotionTween();
            }
        }

        private void OnDisable()
        {
            KillMotionTween();
        }

        public void Play(TutorialGestureMotionMode mode, RectTransform canvasRect)
        {
            if (_rectTransform == null)
            {
                _rectTransform = transform as RectTransform;
            }

            if (_mode != mode || _canvasRect != canvasRect)
            {
                _mode = mode;
                _canvasRect = canvasRect;
                RestartMotionTween();
            }

            gameObject.SetActive(mode != TutorialGestureMotionMode.None);
            UpdatePosition(_motionProgress);
        }

        public void Stop()
        {
            _mode = TutorialGestureMotionMode.None;
            KillMotionTween();
            gameObject.SetActive(false);
        }

        private void RestartMotionTween()
        {
            KillMotionTween();

            if (_mode == TutorialGestureMotionMode.None || _canvasRect == null)
            {
                return;
            }

            _motionProgress = 0f;
            UpdatePosition(_motionProgress);

            if (_mode == TutorialGestureMotionMode.ShieldArcDrag)
            {
                _motionTween = DOTween.To(
                    () => _motionProgress,
                    value =>
                    {
                        _motionProgress = value;
                        UpdatePosition(_motionProgress);
                    },
                    1f,
                    _cycleDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
                return;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(
                    () => _motionProgress,
                    value =>
                    {
                        _motionProgress = value;
                        UpdatePosition(_motionProgress);
                    },
                    1f,
                    _cycleDuration)
                .SetEase(_motionEase));

            if (_mode == TutorialGestureMotionMode.WeaponDownHold)
            {
                sequence.AppendInterval(_weaponHoldDuration);
            }

            _motionTween = sequence
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(true);
        }

        private void KillMotionTween()
        {
            if (_motionTween == null)
            {
                return;
            }

            _motionTween.Kill();
            _motionTween = null;
        }

        private void UpdatePosition(float normalizedTime)
        {
            if (_mode == TutorialGestureMotionMode.None || _canvasRect == null || _rectTransform == null)
            {
                return;
            }

            Vector2 viewportPosition = _mode == TutorialGestureMotionMode.ShieldArcDrag
                ? CalculateShieldPosition(normalizedTime)
                : CalculateWeaponPosition(normalizedTime);

            Vector2 canvasSize = _canvasRect.rect.size;
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = new Vector2(
                (viewportPosition.x - 0.5f) * canvasSize.x,
                (viewportPosition.y - 0.5f) * canvasSize.y);

            float rotationZ = _mode == TutorialGestureMotionMode.ShieldArcDrag
                ? _shieldRotationZ
                : _weaponRotationZ;
            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        }

        private Vector2 CalculateWeaponPosition(float t)
        {
            float y = _weaponCenter.y + Mathf.Lerp(_weaponVerticalRange * 0.5f, -_weaponVerticalRange * 0.5f, t);
            return new Vector2(_weaponCenter.x, Mathf.Clamp01(y));
        }

        private Vector2 CalculateShieldPosition(float t)
        {
            Vector2 position = Vector2.Lerp(_shieldStart, _shieldEnd, t);
            position.y += Mathf.Sin(t * Mathf.PI) * _shieldArcHeight;
            return new Vector2(Mathf.Clamp01(position.x), Mathf.Clamp01(position.y));
        }
    }
}
