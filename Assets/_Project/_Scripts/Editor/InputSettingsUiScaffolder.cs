#if UNITY_EDITOR
using TMPro;
using Shield_Shot.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.EditorTools
{
    internal static class InputSettingsUiScaffolder
    {
        private const string PrefabPath =
            "Assets/_Project/_Prefabs/UI/Scene_InGamePrefab/Panel_Pause.prefab";

        private const string LayoutName = "InputSettingsV2Layout";
        private const string RuntimeFontPath =
            "Assets/_Project/Fonts/NotoSansKR-Medium SDF.asset";
        private const string EditorFontPath =
            "Assets/_Project/Editor/UnusedFonts/NotoSansKR-Medium SDF.asset";
        private const string RuntimeFontSourcePath =
            "Assets/_Project/Fonts/NotoSansKR-Medium.ttf";
        private const string EditorFontSourcePath =
            "Assets/_Project/Editor/FontSources/NotoSansKR-Medium.ttf";

        private static TMP_FontAsset notoSansKrMedium;

        [InitializeOnLoadMethod]
        private static void ScheduleInitialBuild()
        {
            EditorApplication.delayCall += BuildIfMissing;
        }

        [MenuItem("Tools/Shield Shot/UI/Rebuild Input Settings V2 Layout")]
        private static void Rebuild()
        {
            Build(rebuild: true);
        }

        private static void BuildIfMissing()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);

            try
            {
                Transform inputPanel = FindDeep(prefabRoot.transform, "Panel_Input");
                Transform layout = inputPanel != null ? inputPanel.Find(LayoutName) : null;
                LayoutElement layoutElement = layout != null ? layout.GetComponent<LayoutElement>() : null;
                Slider ratioSlider = layout != null
                    ? FindDeep(layout, "RatioSlider")?.GetComponent<Slider>()
                    : null;
                InputSettingsPanelUI controller = layout != null
                    ? layout.GetComponent<InputSettingsPanelUI>()
                    : null;
                bool usesRequestedFont = layout != null &&
                                         AllTextUsesRequestedFont(layout);

                // 구버전 레이아웃은 Panel_Input의 HorizontalLayoutGroup에 의해
                // 왼쪽으로 밀리거나 Slider가 Raycast를 받지 못하므로 다시 만든다.
                bool requiresRepair = layout != null &&
                                      (layoutElement == null ||
                                       !layoutElement.ignoreLayout ||
                                       ratioSlider == null ||
                                       ratioSlider.targetGraphic == null ||
                                       !ratioSlider.targetGraphic.raycastTarget ||
                                       controller == null ||
                                       !usesRequestedFont);

                if (layout == null || requiresRepair)
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                    prefabRoot = null;
                    Build(rebuild: requiresRepair);
                }
            }
            finally
            {
                if (prefabRoot != null)
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }

        private static void Build(bool rebuild)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);

            try
            {
                Transform inputPanel = FindDeep(prefabRoot.transform, "Panel_Input");
                if (inputPanel == null)
                {
                    Debug.LogError($"[Input Settings UI] Panel_Input을 찾지 못했습니다: {PrefabPath}");
                    return;
                }

                Transform existingLayout = inputPanel.Find(LayoutName);
                if (existingLayout != null)
                {
                    if (!rebuild)
                    {
                        return;
                    }

                    Object.DestroyImmediate(existingLayout.gameObject);
                }

                Button basicTemplate = FindDeep(inputPanel, "Basic")?.GetComponent<Button>();
                Button swapTemplate = FindDeep(inputPanel, "Swap")?.GetComponent<Button>();
                notoSansKrMedium = ResolveRuntimeFont();

                if (basicTemplate != null)
                {
                    basicTemplate.gameObject.SetActive(false);
                }

                if (swapTemplate != null)
                {
                    swapTemplate.gameObject.SetActive(false);
                }

                RectTransform layout = CreateRect(LayoutName, inputPanel);
                LayoutElement layoutElement = layout.gameObject.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;
                Stretch(layout, 28f, 28f, 20f, 20f);

                CreatePreview(
                    layout,
                    out RectTransform previewFirst,
                    out RectTransform previewSecond,
                    out RectTransform previewBoundary,
                    out TMP_Text previewFirstText,
                    out TMP_Text previewSecondText);

                RectTransform controls = CreateRect("Controls", layout);
                SetAnchoredRect(controls, 0.04f, 0.08f, 0.96f, 0.56f);

                CreateChoiceRow(
                    controls,
                    "SplitDirectionGroup",
                    "분할 방향",
                    "LeftRightButton",
                    "좌우",
                    "TopBottomButton",
                    "상하",
                    0.68f,
                    0.98f,
                    basicTemplate,
                    swapTemplate,
                    out Button leftRightButton,
                    out Button topBottomButton);

                CreateChoiceRow(
                    controls,
                    "ShieldPositionGroup",
                    "방패 위치",
                    "FirstRegionButton",
                    "왼쪽",
                    "SecondRegionButton",
                    "오른쪽",
                    0.36f,
                    0.66f,
                    basicTemplate,
                    swapTemplate,
                    out Button firstRegionButton,
                    out Button secondRegionButton);

                CreateRatioRow(
                    controls,
                    0.04f,
                    0.33f,
                    out Slider ratioSlider,
                    out TMP_Text ratioValueText);

                RectTransform actions = CreateRect("ActionGroup", layout);
                SetAnchoredRect(actions, 0.18f, 0.005f, 0.82f, 0.10f);
                Button resetButton = CreateStyledButton(
                    actions, "ResetButton", "기본값", 0f, 0.47f, basicTemplate);
                Button applyButton = CreateStyledButton(
                    actions, "ApplyButton", "적용", 0.53f, 1f, swapTemplate ?? basicTemplate);

                InputSettingsPanelUI controller =
                    layout.gameObject.AddComponent<InputSettingsPanelUI>();
                controller.Configure(
                    leftRightButton,
                    topBottomButton,
                    firstRegionButton,
                    secondRegionButton,
                    ratioSlider,
                    ratioValueText,
                    previewFirst,
                    previewSecond,
                    previewBoundary,
                    previewFirstText,
                    previewSecondText,
                    resetButton,
                    applyButton);

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                Debug.Log($"[Input Settings UI] 기본 UI 틀 생성 완료: {PrefabPath}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void CreatePreview(
            RectTransform layout,
            out RectTransform first,
            out RectTransform second,
            out RectTransform boundary,
            out TMP_Text firstText,
            out TMP_Text secondText)
        {
            RectTransform group = CreateRect("PreviewGroup", layout);
            SetAnchoredRect(group, 0.04f, 0.58f, 0.96f, 0.99f);

            CreateLabel(group, "TitleText", "입력 영역 미리보기", 0f, 0.84f, 1f, 1f, 34f);

            RectTransform frame = CreateImage(
                "PreviewFrame",
                group,
                new Color(0.17f, 0.12f, 0.07f, 0.75f));
            SetAnchoredRect(frame, 0.13f, 0.02f, 0.87f, 0.82f);

            first = CreateImage(
                "FirstRegion",
                frame,
                new Color(0.22f, 0.48f, 0.30f, 0.92f));
            SetAnchoredRect(first, 0.02f, 0.03f, 0.49f, 0.97f);
            firstText = CreateLabel(first, "FirstRegionText", "방패", 0f, 0f, 1f, 1f, 32f);

            second = CreateImage(
                "SecondRegion",
                frame,
                new Color(0.57f, 0.31f, 0.16f, 0.92f));
            SetAnchoredRect(second, 0.51f, 0.03f, 0.98f, 0.97f);
            secondText = CreateLabel(second, "SecondRegionText", "공격", 0f, 0f, 1f, 1f, 32f);

            boundary = CreateImage(
                "BoundaryLine",
                frame,
                new Color(0.95f, 0.82f, 0.40f, 1f));
            SetAnchoredRect(boundary, 0.495f, 0.03f, 0.505f, 0.97f);
        }

        private static void CreateChoiceRow(
            RectTransform parent,
            string groupName,
            string label,
            string firstName,
            string firstText,
            string secondName,
            string secondText,
            float minY,
            float maxY,
            Button firstTemplate,
            Button secondTemplate,
            out Button firstButton,
            out Button secondButton)
        {
            RectTransform group = CreateRect(groupName, parent);
            SetAnchoredRect(group, 0f, minY, 1f, maxY);

            CreateLabel(group, "Label", label, 0f, 0f, 0.30f, 1f, 28f);
            firstButton = CreateStyledButton(
                group, firstName, firstText, 0.34f, 0.64f, firstTemplate);
            secondButton = CreateStyledButton(
                group, secondName, secondText, 0.68f, 0.98f, secondTemplate ?? firstTemplate);
        }

        private static void CreateRatioRow(
            RectTransform parent,
            float minY,
            float maxY,
            out Slider slider,
            out TMP_Text ratioText)
        {
            RectTransform group = CreateRect("ShieldRatioGroup", parent);
            SetAnchoredRect(group, 0f, minY, 1f, maxY);

            CreateLabel(group, "Label", "방패 영역 크기", 0f, 0f, 0.30f, 1f, 28f);

            RectTransform sliderRect = CreateRect("RatioSlider", group);
            SetAnchoredRect(sliderRect, 0.34f, 0.20f, 0.82f, 0.80f);

            RectTransform background = CreateImage(
                "Background",
                sliderRect,
                new Color(0.22f, 0.16f, 0.10f, 1f));
            background.GetComponent<Image>().raycastTarget = true;
            Stretch(background, 0f, 0f, 13f, 13f);

            RectTransform fillArea = CreateRect("Fill Area", sliderRect);
            Stretch(fillArea, 11f, 11f, 13f, 13f);
            RectTransform fill = CreateImage("Fill", fillArea, new Color(0.32f, 0.62f, 0.31f, 1f));
            Stretch(fill, 0f, 0f, 0f, 0f);

            RectTransform handleArea = CreateRect("Handle Slide Area", sliderRect);
            Stretch(handleArea, 12f, 12f, 0f, 0f);
            RectTransform handle = CreateImage("Handle", handleArea, new Color(0.96f, 0.79f, 0.32f, 1f));
            handle.GetComponent<Image>().raycastTarget = true;
            handle.anchorMin = new Vector2(0.5f, 0.5f);
            handle.anchorMax = new Vector2(0.5f, 0.5f);
            handle.sizeDelta = new Vector2(34f, 52f);

            slider = sliderRect.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = 0.3f;
            slider.maxValue = 0.7f;
            slider.value = 0.5f;
            slider.wholeNumbers = false;

            ratioText = CreateLabel(group, "RatioValueText", "50%", 0.84f, 0f, 1f, 1f, 28f);
        }

        private static Button CreateStyledButton(
            RectTransform parent,
            string name,
            string text,
            float minX,
            float maxX,
            Button template)
        {
            GameObject buttonObject;

            if (template != null)
            {
                buttonObject = Object.Instantiate(template.gameObject, parent, false);
                buttonObject.name = name;
                buttonObject.SetActive(true);
            }
            else
            {
                RectTransform rect = CreateImage(name, parent, new Color(0.20f, 0.43f, 0.19f, 1f));
                buttonObject = rect.gameObject;
                buttonObject.AddComponent<Button>();
            }

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            SetAnchoredRect(buttonRect, minX, 0.10f, maxX, 0.90f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();

            TMP_Text label = buttonObject.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                label = CreateLabel(buttonRect, "Label", text, 0f, 0f, 1f, 1f, 28f);
            }

            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.font = notoSansKrMedium;
            return button;
        }

        private static TextMeshProUGUI CreateLabel(
            RectTransform parent,
            string name,
            string text,
            float minX,
            float minY,
            float maxX,
            float maxY,
            float fontSize)
        {
            RectTransform rect = CreateRect(name, parent);
            SetAnchoredRect(rect, minX, minY, maxX, maxY);

            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.font = notoSansKrMedium;
            label.color = new Color(0.96f, 0.91f, 0.78f, 1f);
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform CreateImage(string name, Transform parent, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.layer = parent.gameObject.layer;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            return rect;
        }

        private static void SetAnchoredRect(
            RectTransform rect,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static void Stretch(
            RectTransform rect,
            float left,
            float right,
            float bottom,
            float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindDeep(root.GetChild(i), objectName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static TMP_FontAsset ResolveRuntimeFont()
        {
            TMP_FontAsset runtimeFont =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RuntimeFontPath);
            if (runtimeFont != null)
            {
                return runtimeFont;
            }

            EnsureRuntimeFontFolder();
            MoveAssetIfNeeded(EditorFontSourcePath, RuntimeFontSourcePath);
            MoveAssetIfNeeded(EditorFontPath, RuntimeFontPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            runtimeFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RuntimeFontPath);
            if (runtimeFont == null)
            {
                Debug.LogError(
                    $"[Input Settings UI] NotoSansKR-Medium SDF를 준비하지 못했습니다: {RuntimeFontPath}");
            }

            return runtimeFont;
        }

        private static void EnsureRuntimeFontFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Fonts"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Fonts");
            }
        }

        private static void MoveAssetIfNeeded(string sourcePath, string destinationPath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(destinationPath) != null ||
                AssetDatabase.LoadMainAssetAtPath(sourcePath) == null)
            {
                return;
            }

            string error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[Input Settings UI] 폰트 이동 실패: {error}");
            }
        }

        private static bool AllTextUsesRequestedFont(Transform layout)
        {
            TMP_FontAsset requestedFont =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RuntimeFontPath);
            if (requestedFont == null)
            {
                return false;
            }

            TMP_Text[] labels = layout.GetComponentsInChildren<TMP_Text>(true);
            if (labels.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i].font != requestedFont)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif
