using System;
using System.Collections;
using System.Collections.Generic;
using Shield_Shot.InputSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class InGameTutorialGuideUI : MonoBehaviour, IInputGate
    {
        private enum FocusScreenZone
        {
            None,
            Left,
            Right,
        }

        [Serializable]
        private class TutorialStep
        {
            [TextArea]
            public string message;
            public GameObject focusObject;
            public string autoFocusLayerName;
            public FocusScreenZone screenZone;
            public Color screenZoneColor = new Color(1f, 1f, 1f, 0.25f);
            public TutorialGestureMotionMode gestureMotionMode;

            [NonSerialized]
            public GameObject autoFocusTarget;
        }

        private const string SeenKey = "InGameTutorialGuide.Seen";

        [Header("Root")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("Controls")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _closeButton;

        [Header("Options")]
        [SerializeField] private bool _showOnlyOnce = true;
        [SerializeField] private bool _pauseWhileShowing = true;
        [SerializeField] private bool _markSeenWhenSkipped = true;
        [SerializeField] private bool _advanceOnAnyClick = true;

        [Header("Auto Focus")]
        [SerializeField] private bool _resolveAutoFocusTargetsOnShow = true;
        [SerializeField] private float _autoFocusResolveTimeout = 3f;
        [SerializeField] private float _autoFocusResolveInterval = 0.1f;
        [SerializeField] private GameObject _autoFocusMarker;
        [SerializeField] private Image _screenZoneOverlay;
        [SerializeField] private Vector3 _autoFocusMarkerWorldOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Canvas _markerCanvas;
        [SerializeField] private bool _logAutoFocusDebug;

        [Header("Steps")]
        [SerializeField]
        private List<TutorialStep> _steps = new()
        {
            new TutorialStep
            {
                message = "Drag in the weapon area to fire the bow.",
                autoFocusLayerName = "PvpWeapon",
                screenZone = FocusScreenZone.Right,
                screenZoneColor = new Color(1f, 0f, 0f, 0.35f),
                gestureMotionMode = TutorialGestureMotionMode.WeaponDownRepeat,
            },
            new TutorialStep
            {
                message = "Bows and guns can charge attacks in the weapon area.",
                autoFocusLayerName = "PvpWeapon",
                screenZone = FocusScreenZone.Right,
                screenZoneColor = new Color(1f, 0f, 0f, 0.35f),
                gestureMotionMode = TutorialGestureMotionMode.WeaponDownHold,
            },
            new TutorialStep
            {
                message = "Move left and right in the shield area to defend.",
                autoFocusLayerName = "PvpShield",
                screenZone = FocusScreenZone.Left,
                screenZoneColor = new Color(0f, 0.35f, 1f, 0.35f),
                gestureMotionMode = TutorialGestureMotionMode.ShieldArcDrag,
            },
            new TutorialStep { message = "Defeat enemies and begin your adventure." },
        };

        private int _stepIndex;
        private float _previousTimeScale = 1f;
        private bool _isShowing;
        private GameObject _currentAutoFocusTarget;
        private Coroutine _autoFocusResolveRoutine;
        private Coroutine _showAfterAutoFocusRoutine;
        private GameObject _autoFocusMarkerInstance;
        private Image _screenZoneOverlayInstance;
        private TutorialStep _currentStep;
        private float _acceptAdvanceInputAtRealtime;

        public bool CanReceiveInput => !_isShowing;

        private void Awake()
        {
            HideImmediate();
        }

        private void OnEnable()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(Complete);
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Complete);

            StopShowAfterAutoFocusRoutine();

            if (_isShowing)
            {
                StopAutoFocusResolveRoutine();
                ResumeGame();
                _isShowing = false;
                HideImmediate();
            }
        }

        public bool ShouldShow()
        {
            return !_showOnlyOnce || PlayerPrefs.GetInt(SeenKey, 0) == 0;
        }

        public void Show()
        {
            Show(false);
        }

        public void ShowIgnoringSeen()
        {
            Show(true);
        }

        public void Show(bool ignoreSeen)
        {
            if (!ignoreSeen && !ShouldShow())
            {
                return;
            }

            if (_resolveAutoFocusTargetsOnShow && !ResolveAutoFocusTargets())
            {
                StartShowAfterAutoFocusRoutine(ignoreSeen);
                return;
            }

            ShowImmediate();
        }

        private void ShowImmediate()
        {
            _isShowing = true;
            _stepIndex = 0;
            _acceptAdvanceInputAtRealtime = Time.unscaledTime + 0.15f;

            if (_root != null)
            {
                _root.SetActive(true);
            }

            if (_pauseWhileShowing)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            if (_resolveAutoFocusTargetsOnShow)
            {
                StartAutoFocusResolveRoutine();
            }

            RefreshStep();
        }

        public void ShowNextStep()
        {
            if (!_isShowing)
            {
                return;
            }

            _stepIndex++;
            if (_stepIndex >= _steps.Count)
            {
                Complete();
                return;
            }

            _acceptAdvanceInputAtRealtime = Time.unscaledTime + 0.1f;
            RefreshStep();
        }

        public void Skip()
        {
            if (_markSeenWhenSkipped)
            {
                MarkSeen();
            }

            Close();
        }

        public void Complete()
        {
            MarkSeen();
            Close();
        }

        public void ResetSeenState()
        {
            PlayerPrefs.DeleteKey(SeenKey);
        }

        private void RefreshStep()
        {
            TutorialStep step = _steps.Count > 0 ? _steps[Mathf.Clamp(_stepIndex, 0, _steps.Count - 1)] : null;
            _currentStep = step;
            _currentAutoFocusTarget = step != null ? step.autoFocusTarget : null;

            if (_messageText != null)
            {
                _messageText.text = step != null ? step.message : string.Empty;
            }

            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i].focusObject != null)
                {
                    _steps[i].focusObject.SetActive(i == _stepIndex);
                }
            }

            if (_nextButton != null) _nextButton.gameObject.SetActive(false);
            if (_skipButton != null) _skipButton.gameObject.SetActive(false);
            if (_closeButton != null) _closeButton.gameObject.SetActive(true);

            RefreshScreenZoneOverlay(step);
            RefreshAutoFocusMarker();
        }

        private void Update()
        {
            if (!_isShowing || !_advanceOnAnyClick || Time.unscaledTime < _acceptAdvanceInputAtRealtime)
            {
                return;
            }

            if (!TryGetAdvanceClickScreenPosition(out Vector2 screenPosition))
            {
                return;
            }

            if (IsScreenPositionOnCloseButton(screenPosition))
            {
                return;
            }

            ShowNextStep();
        }

        private void LateUpdate()
        {
            if (_isShowing)
            {
                RefreshAutoFocusMarker();
            }
        }

        private bool TryGetAdvanceClickScreenPosition(out Vector2 screenPosition)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screenPosition = touch.position;
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }

            screenPosition = default;
            return false;
        }

        private bool IsScreenPositionOnCloseButton(Vector2 screenPosition)
        {
            if (_closeButton == null || !_closeButton.gameObject.activeInHierarchy)
            {
                return false;
            }

            RectTransform closeRect = _closeButton.transform as RectTransform;
            if (closeRect == null)
            {
                return false;
            }

            Canvas closeCanvas = closeRect.GetComponentInParent<Canvas>();
            Camera uiCamera = null;
            if (closeCanvas != null && closeCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = closeCanvas.worldCamera;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(closeRect, screenPosition, uiCamera);
        }

        private void Close()
        {
            if (!_isShowing)
            {
                return;
            }

            _isShowing = false;
            StopShowAfterAutoFocusRoutine();
            StopAutoFocusResolveRoutine();
            ResumeGame();
            HideImmediate();
        }

        private void HideImmediate()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }

            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i].focusObject != null)
                {
                    _steps[i].focusObject.SetActive(false);
                }
            }

            if (_autoFocusMarker != null)
            {
                GetAutoFocusMarkerInstance()?.SetActive(false);
            }

            if (_screenZoneOverlayInstance != null)
            {
                _screenZoneOverlayInstance.gameObject.SetActive(false);
            }
        }

        private void ResumeGame()
        {
            if (_pauseWhileShowing)
            {
                Time.timeScale = _previousTimeScale;
            }
        }

        private static void MarkSeen()
        {
            PlayerPrefs.SetInt(SeenKey, 1);
            PlayerPrefs.Save();
        }

        private void StartAutoFocusResolveRoutine()
        {
            StopAutoFocusResolveRoutine();
            _autoFocusResolveRoutine = StartCoroutine(ResolveAutoFocusTargetsRoutine());
        }

        private void StopAutoFocusResolveRoutine()
        {
            if (_autoFocusResolveRoutine == null)
            {
                return;
            }

            StopCoroutine(_autoFocusResolveRoutine);
            _autoFocusResolveRoutine = null;
        }

        private IEnumerator ResolveAutoFocusTargetsRoutine()
        {
            float elapsed = 0f;
            while (elapsed <= _autoFocusResolveTimeout)
            {
                bool allResolved = ResolveAutoFocusTargets();
                RefreshStep();

                if (allResolved)
                {
                    _autoFocusResolveRoutine = null;
                    yield break;
                }

                elapsed += _autoFocusResolveInterval;
                yield return new WaitForSecondsRealtime(_autoFocusResolveInterval);
            }

            ResolveAutoFocusTargets();
            RefreshStep();
            _autoFocusResolveRoutine = null;
        }

        private bool ResolveAutoFocusTargets()
        {
            bool allResolved = true;
            for (int i = 0; i < _steps.Count; i++)
            {
                TutorialStep step = _steps[i];
                if (step == null || string.IsNullOrWhiteSpace(step.autoFocusLayerName))
                {
                    continue;
                }

                if (step.autoFocusTarget != null)
                {
                    continue;
                }

                step.autoFocusTarget = FindActiveObjectByLayerName(step.autoFocusLayerName);
                if (step.autoFocusTarget == null)
                {
                    allResolved = false;
                }
            }

            return allResolved;
        }

        private void StartShowAfterAutoFocusRoutine(bool ignoreSeen)
        {
            StopShowAfterAutoFocusRoutine();
            _showAfterAutoFocusRoutine = StartCoroutine(ShowAfterAutoFocusResolvedRoutine(ignoreSeen));
        }

        private void StopShowAfterAutoFocusRoutine()
        {
            if (_showAfterAutoFocusRoutine == null)
            {
                return;
            }

            StopCoroutine(_showAfterAutoFocusRoutine);
            _showAfterAutoFocusRoutine = null;
        }

        private IEnumerator ShowAfterAutoFocusResolvedRoutine(bool ignoreSeen)
        {
            float elapsed = 0f;
            while (elapsed <= _autoFocusResolveTimeout)
            {
                if (ResolveAutoFocusTargets())
                {
                    break;
                }

                elapsed += _autoFocusResolveInterval;
                yield return new WaitForSecondsRealtime(_autoFocusResolveInterval);
            }

            _showAfterAutoFocusRoutine = null;

            if (!ignoreSeen && !ShouldShow())
            {
                yield break;
            }

            ShowImmediate();
        }

        private void RefreshAutoFocusMarker()
        {
            GameObject marker = GetAutoFocusMarkerInstance();
            if (marker == null)
            {
                return;
            }

            TutorialGestureGuideMotion gestureMotion = marker.GetComponent<TutorialGestureGuideMotion>();
            if (gestureMotion != null)
            {
                marker.SetActive(true);
                marker.transform.SetAsLastSibling();

                Canvas gestureCanvas = ResolveMarkerCanvas(marker.transform as RectTransform);
                RectTransform gestureCanvasRect = gestureCanvas != null ? gestureCanvas.transform as RectTransform : null;
                gestureMotion.Play(ResolveGestureMotionMode(_currentStep, _stepIndex), gestureCanvasRect);
                return;
            }

            if (_currentAutoFocusTarget == null)
            {
                marker.SetActive(false);
                return;
            }

            marker.SetActive(true);
            marker.transform.SetAsLastSibling();

            RectTransform markerRect = marker.transform as RectTransform;
            if (markerRect == null)
            {
                marker.transform.position = _currentAutoFocusTarget.transform.position + _autoFocusMarkerWorldOffset;
                return;
            }

            Camera camera = _worldCamera != null ? _worldCamera : Camera.main;
            if (camera == null)
            {
                markerRect.position = _currentAutoFocusTarget.transform.position + _autoFocusMarkerWorldOffset;
                return;
            }

            Vector3 screenPosition = camera.WorldToScreenPoint(_currentAutoFocusTarget.transform.position + _autoFocusMarkerWorldOffset);
            if (screenPosition.z < 0f)
            {
                marker.SetActive(false);
                return;
            }

            Canvas canvas = ResolveMarkerCanvas(markerRect);
            RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
            if (canvasRect != null)
            {
                Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, uiCamera, out Vector2 localPoint))
                {
                    markerRect.SetParent(canvasRect, false);
                    markerRect.anchoredPosition = localPoint;
                }
                else
                {
                    markerRect.position = screenPosition;
                }
            }
            else
            {
                markerRect.position = screenPosition;
            }

            if (_logAutoFocusDebug)
            {
                Debug.Log($"[InGameTutorialGuideUI] Marker target={_currentAutoFocusTarget.name}, screen={screenPosition}, marker={marker.name}");
            }
        }

        private void RefreshScreenZoneOverlay(TutorialStep step)
        {
            Image overlay = GetScreenZoneOverlayInstance();
            if (overlay == null)
            {
                return;
            }

            FocusScreenZone screenZone = ResolveScreenZone(step);
            if (step == null || screenZone == FocusScreenZone.None)
            {
                overlay.gameObject.SetActive(false);
                return;
            }

            overlay.gameObject.SetActive(true);
            overlay.raycastTarget = false;
            overlay.color = ResolveScreenZoneColor(step, screenZone);
            overlay.transform.SetAsFirstSibling();

            RectTransform rect = overlay.rectTransform;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            switch (screenZone)
            {
                case FocusScreenZone.Left:
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    break;
                case FocusScreenZone.Right:
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    break;
                default:
                    overlay.gameObject.SetActive(false);
                    break;
            }
        }

        private Image GetScreenZoneOverlayInstance()
        {
            if (_screenZoneOverlayInstance != null)
            {
                return _screenZoneOverlayInstance;
            }

            Canvas canvas = ResolveMarkerCanvas(null);
            Transform parent = canvas != null ? canvas.transform : transform;

            if (_screenZoneOverlay != null)
            {
                _screenZoneOverlayInstance = _screenZoneOverlay.gameObject.scene.IsValid()
                    ? _screenZoneOverlay
                    : Instantiate(_screenZoneOverlay, parent);
            }
            else
            {
                GameObject overlayObject = new GameObject(
                    "TutorialScreenZoneOverlay",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));

                overlayObject.transform.SetParent(parent, false);
                _screenZoneOverlayInstance = overlayObject.GetComponent<Image>();
            }

            _screenZoneOverlayInstance.raycastTarget = false;
            return _screenZoneOverlayInstance;
        }

        private static FocusScreenZone ResolveScreenZone(TutorialStep step)
        {
            if (step == null)
            {
                return FocusScreenZone.None;
            }

            if (step.screenZone != FocusScreenZone.None)
            {
                return step.screenZone;
            }

            if (ContainsIgnoreCase(step.autoFocusLayerName, "Weapon"))
            {
                return FocusScreenZone.Right;
            }

            if (ContainsIgnoreCase(step.autoFocusLayerName, "Shield"))
            {
                return FocusScreenZone.Left;
            }

            return FocusScreenZone.None;
        }

        private static Color ResolveScreenZoneColor(TutorialStep step, FocusScreenZone screenZone)
        {
            if (step != null && step.screenZoneColor.a > 0f)
            {
                return step.screenZoneColor;
            }

            return screenZone == FocusScreenZone.Right
                ? new Color(1f, 0f, 0f, 0.35f)
                : new Color(0f, 0.35f, 1f, 0.35f);
        }

        private static TutorialGestureMotionMode ResolveGestureMotionMode(TutorialStep step, int stepIndex)
        {
            if (step != null && step.gestureMotionMode != TutorialGestureMotionMode.None)
            {
                return step.gestureMotionMode;
            }

            FocusScreenZone screenZone = ResolveScreenZone(step);
            if (screenZone == FocusScreenZone.Right)
            {
                return stepIndex == 1
                    ? TutorialGestureMotionMode.WeaponDownHold
                    : TutorialGestureMotionMode.WeaponDownRepeat;
            }

            if (screenZone == FocusScreenZone.Left)
            {
                return TutorialGestureMotionMode.ShieldArcDrag;
            }

            return TutorialGestureMotionMode.None;
        }

        private GameObject GetAutoFocusMarkerInstance()
        {
            if (_autoFocusMarker == null)
            {
                return null;
            }

            if (_autoFocusMarkerInstance != null)
            {
                return _autoFocusMarkerInstance;
            }

            if (_autoFocusMarker.scene.IsValid())
            {
                _autoFocusMarkerInstance = _autoFocusMarker;
                return _autoFocusMarkerInstance;
            }

            Canvas canvas = ResolveMarkerCanvas(null);
            Transform parent = canvas != null ? canvas.transform : transform;
            _autoFocusMarkerInstance = Instantiate(_autoFocusMarker, parent);
            _autoFocusMarkerInstance.name = $"{_autoFocusMarker.name}_Runtime";
            return _autoFocusMarkerInstance;
        }

        private Canvas ResolveMarkerCanvas(RectTransform markerRect)
        {
            if (_markerCanvas != null)
            {
                return _markerCanvas;
            }

            Canvas markerParentCanvas = markerRect != null ? markerRect.GetComponentInParent<Canvas>() : null;
            if (markerParentCanvas != null)
            {
                _markerCanvas = markerParentCanvas;
                return _markerCanvas;
            }

            Canvas rootCanvas = _root != null ? _root.GetComponentInParent<Canvas>() : null;
            if (rootCanvas != null)
            {
                _markerCanvas = rootCanvas;
                return _markerCanvas;
            }

            _markerCanvas = GetComponentInParent<Canvas>();
            if (_markerCanvas != null)
            {
                return _markerCanvas;
            }

            _markerCanvas = FindFirstObjectByType<Canvas>();
            return _markerCanvas;
        }

        private static GameObject FindActiveObjectByLayerName(string layerName)
        {
            int layer = ResolveLayer(layerName);
            if (layer < 0)
            {
                Debug.LogWarning($"[InGameTutorialGuideUI] Layer not found: {layerName}");
                return null;
            }

            Transform[] transforms = FindObjectsByType<Transform>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            GameObject fallback = null;
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || candidate.gameObject.layer != layer)
                {
                    continue;
                }

                if (fallback == null)
                {
                    fallback = candidate.gameObject;
                }

                if (ContainsIgnoreCase(candidate.name, "Local") ||
                    ContainsIgnoreCase(candidate.root.name, "Local"))
                {
                    return candidate.gameObject;
                }
            }

            return fallback;
        }

        private static int ResolveLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                return layer;
            }

            string normalized = layerName.Replace(" ", string.Empty);
            string[] candidates =
            {
                normalized,
                normalized.ToLowerInvariant(),
                normalized.ToUpperInvariant(),
                ToPascalLayerName(normalized),
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                layer = LayerMask.NameToLayer(candidates[i]);
                if (layer >= 0)
                {
                    return layer;
                }
            }

            return -1;
        }

        private static string ToPascalLayerName(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return layerName;
            }

            if (layerName.Equals("pvpweapon", StringComparison.OrdinalIgnoreCase))
            {
                return "PvpWeapon";
            }

            if (layerName.Equals("pvpshield", StringComparison.OrdinalIgnoreCase))
            {
                return "PvpShield";
            }

            return char.ToUpperInvariant(layerName[0]) + layerName.Substring(1).ToLowerInvariant();
        }

        private static bool ContainsIgnoreCase(string source, string value)
        {
            return !string.IsNullOrEmpty(source) &&
                   !string.IsNullOrEmpty(value) &&
                   source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
