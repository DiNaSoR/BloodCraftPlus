using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VDebug.Services;

/// <summary>
/// Live UI Inspector - click any UI element to see its path, components, and properties.
/// Toggle with a button or hotkey. When active, clicking any UI element shows its details.
/// </summary>
internal static class UIInspectorService
{
    static bool _inspectorActive;
    static GameObject _inspectorPanel;
    static TextMeshProUGUI _infoText;
    static GameObject _highlightOverlay;
    static Image _highlightImage;
    static RectTransform _currentTarget;
    static bool _initialized;

    // Inspector panel configuration
    const float PanelWidth = 400f;
    const float PanelHeight = 350f;
    const float Padding = 10f;

    public static bool IsActive => _inspectorActive;

    /// <summary>
    /// Initialize the inspector system.
    /// </summary>
    public static void Initialize(Canvas canvas)
    {
        if (_initialized || canvas == null)
            return;

        try
        {
            CreateInspectorPanel(canvas);
            CreateHighlightOverlay(canvas);
            CreateInspectorBehaviour(canvas.gameObject);
            _initialized = true;
            VDebugLog.Log.LogInfo("[VDebug] UI Inspector initialized.");
        }
        catch (Exception ex)
        {
            VDebugLog.Log.LogWarning($"[VDebug] Failed to initialize UI Inspector: {ex}");
        }
    }

    /// <summary>
    /// Toggle inspector mode on/off.
    /// </summary>
    public static void Toggle()
    {
        _inspectorActive = !_inspectorActive;

        if (_inspectorPanel != null)
            _inspectorPanel.SetActive(_inspectorActive);

        if (!_inspectorActive)
        {
            ClearHighlight();
            _currentTarget = null;
        }

        VDebugLog.Log.LogInfo($"[VDebug] UI Inspector: {(_inspectorActive ? "ENABLED" : "DISABLED")}");
    }

    /// <summary>
    /// Enable inspector mode.
    /// </summary>
    public static void Enable()
    {
        _inspectorActive = true;
        if (_inspectorPanel != null)
            _inspectorPanel.SetActive(true);
    }

    /// <summary>
    /// Disable inspector mode.
    /// </summary>
    public static void Disable()
    {
        _inspectorActive = false;
        if (_inspectorPanel != null)
            _inspectorPanel.SetActive(false);
        ClearHighlight();
        _currentTarget = null;
    }

    /// <summary>
    /// Inspect a specific RectTransform and display its info.
    /// </summary>
    public static void InspectElement(RectTransform target)
    {
        if (target == null)
            return;

        _currentTarget = target;
        UpdateHighlight(target);
        UpdateInfoPanel(target);
    }

    static void CreateInspectorPanel(Canvas canvas)
    {
        // Main panel
        _inspectorPanel = new GameObject("VDebugInspectorPanel");
        _inspectorPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = _inspectorPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);  // Top-right
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(-20, -20);
        panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        // Background
        Image bgImage = _inspectorPanel.AddComponent<Image>();
        bgImage.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

        // Vertical layout
        VerticalLayoutGroup layout = _inspectorPanel.AddComponent<VerticalLayoutGroup>();
        RectOffset pad = new RectOffset();
        pad.left = (int)Padding;
        pad.right = (int)Padding;
        pad.top = (int)Padding;
        pad.bottom = (int)Padding;
        layout.padding = pad;
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // Header
        CreateLabel(_inspectorPanel.transform, "UI Inspector", 16, new Color(0.9f, 0.7f, 0.2f), FontStyles.Bold);
        CreateLabel(_inspectorPanel.transform, "Click any UI element to inspect", 11, new Color(0.6f, 0.6f, 0.6f), FontStyles.Italic);

        // Separator
        CreateSeparator(_inspectorPanel.transform);

        // Info text (scrollable content)
        GameObject scrollViewGo = CreateScrollView(_inspectorPanel.transform);
        _infoText = scrollViewGo.GetComponentInChildren<TextMeshProUGUI>();

        _inspectorPanel.SetActive(false);

        // Add drag handler
        AddDragHandler(_inspectorPanel);
    }

    static void CreateHighlightOverlay(Canvas canvas)
    {
        _highlightOverlay = new GameObject("VDebugHighlight");
        _highlightOverlay.transform.SetParent(canvas.transform, false);

        RectTransform rect = _highlightOverlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(100, 100);

        _highlightImage = _highlightOverlay.AddComponent<Image>();
        _highlightImage.color = new Color(1f, 0.8f, 0.2f, 0.3f);  // Yellow tint
        _highlightImage.raycastTarget = false;

        // Add outline effect
        Outline outline = _highlightOverlay.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.8f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(2, 2);

        _highlightOverlay.SetActive(false);
    }

    static void UpdateHighlight(RectTransform target)
    {
        if (_highlightOverlay == null || target == null)
            return;

        RectTransform highlightRect = _highlightOverlay.GetComponent<RectTransform>();

        // Get world corners of target
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        // Calculate center and size
        Vector3 center = (corners[0] + corners[2]) / 2f;
        Vector3 size = corners[2] - corners[0];

        highlightRect.position = center;
        highlightRect.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));

        _highlightOverlay.SetActive(true);
    }

    static void ClearHighlight()
    {
        if (_highlightOverlay != null)
            _highlightOverlay.SetActive(false);
    }

    static void UpdateInfoPanel(RectTransform target)
    {
        if (_infoText == null || target == null)
            return;

        StringBuilder sb = new StringBuilder(2048);

        // Path
        sb.AppendLine($"<color=#FFD700>Path:</color>");
        sb.AppendLine($"<color=#AAAAAA>{GetPath(target)}</color>");
        sb.AppendLine();

        // GameObject info
        sb.AppendLine($"<color=#FFD700>GameObject:</color>");
        sb.AppendLine($"  Name: <color=#88CCFF>{target.name}</color>");
        sb.AppendLine($"  Active: {(target.gameObject.activeSelf ? "<color=#88FF88>Yes</color>" : "<color=#FF8888>No</color>")}");
        sb.AppendLine($"  Layer: {target.gameObject.layer} ({LayerMask.LayerToName(target.gameObject.layer)})");
        sb.AppendLine();

        // RectTransform
        sb.AppendLine($"<color=#FFD700>RectTransform:</color>");
        sb.AppendLine($"  AnchorMin: ({target.anchorMin.x:F2}, {target.anchorMin.y:F2})");
        sb.AppendLine($"  AnchorMax: ({target.anchorMax.x:F2}, {target.anchorMax.y:F2})");
        sb.AppendLine($"  Pivot: ({target.pivot.x:F2}, {target.pivot.y:F2})");
        sb.AppendLine($"  Position: ({target.anchoredPosition.x:F1}, {target.anchoredPosition.y:F1})");
        sb.AppendLine($"  Size: ({target.sizeDelta.x:F1}, {target.sizeDelta.y:F1})");
        sb.AppendLine($"  Scale: ({target.localScale.x:F2}, {target.localScale.y:F2}, {target.localScale.z:F2})");
        sb.AppendLine($"  Rect: {target.rect.width:F0}x{target.rect.height:F0}");
        sb.AppendLine();

        // Components
        sb.AppendLine($"<color=#FFD700>Components:</color>");
        Component[] components = target.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null) continue;
            string typeName = comp.GetType().Name;
            sb.AppendLine($"  • <color=#CCCCFF>{typeName}</color>");

            // Extra info for specific types
            if (comp is Image img)
            {
                sb.AppendLine($"      Sprite: {(img.sprite != null ? img.sprite.name : "null")}");
                sb.AppendLine($"      Color: {ColorToHex(img.color)}");
            }
            else if (comp is TMP_Text txt)
            {
                string preview = txt.text?.Length > 30 ? txt.text.Substring(0, 30) + "..." : txt.text;
                sb.AppendLine($"      Text: \"{preview}\"");
                sb.AppendLine($"      Font: {txt.font?.name ?? "null"}");
                sb.AppendLine($"      Size: {txt.fontSize:F1}");
            }
            else if (comp is Button btn)
            {
                sb.AppendLine($"      Interactable: {btn.interactable}");
            }
            else if (comp is LayoutGroup lg)
            {
                sb.AppendLine($"      Padding: L{lg.padding.left} R{lg.padding.right} T{lg.padding.top} B{lg.padding.bottom}");
            }
        }

        _infoText.text = sb.ToString();
    }

    static GameObject CreateScrollView(Transform parent)
    {
        // Scroll View Container
        GameObject scrollGo = new GameObject("ScrollView");
        scrollGo.transform.SetParent(parent, false);

        RectTransform scrollRect = scrollGo.AddComponent<RectTransform>();
        scrollRect.sizeDelta = new Vector2(0, 250f);

        // Add mask
        Image scrollBg = scrollGo.AddComponent<Image>();
        scrollBg.color = new Color(0.05f, 0.05f, 0.07f, 0.8f);
        scrollGo.AddComponent<Mask>().showMaskGraphic = true;

        // ScrollRect component
        ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 20f;

        // Content
        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(scrollGo.transform, false);

        RectTransform contentRect = contentGo.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);

        // Content size fitter
        ContentSizeFitter fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;
        scroll.viewport = scrollRect;

        // Text
        TextMeshProUGUI text = contentGo.AddComponent<TextMeshProUGUI>();
        text.fontSize = 11;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;
        text.richText = true;
        text.text = "<color=#888888>Click a UI element to inspect...</color>";

        return scrollGo;
    }

    static void CreateLabel(Transform parent, string text, float fontSize, Color color, FontStyles style)
    {
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(parent, false);

        RectTransform rect = labelGo.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 6);

        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.enableWordWrapping = false;
    }

    static void CreateSeparator(Transform parent)
    {
        GameObject sepGo = new GameObject("Separator");
        sepGo.transform.SetParent(parent, false);

        RectTransform rect = sepGo.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 2);

        Image sepImg = sepGo.AddComponent<Image>();
        sepImg.color = new Color(0.3f, 0.3f, 0.35f, 0.8f);
    }

    static void AddDragHandler(GameObject panel)
    {
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(InspectorDragHandler)))
            ClassInjector.RegisterTypeInIl2Cpp<InspectorDragHandler>();

        panel.AddComponent<InspectorDragHandler>();
    }

    static void CreateInspectorBehaviour(GameObject canvasGo)
    {
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(InspectorBehaviour)))
            ClassInjector.RegisterTypeInIl2Cpp<InspectorBehaviour>();

        canvasGo.AddComponent<InspectorBehaviour>();
    }

    static string GetPath(Transform transform)
    {
        if (transform == null) return "null";

        StringBuilder sb = new StringBuilder(256);
        sb.Append(transform.name);

        Transform parent = transform.parent;
        int depth = 0;
        while (parent != null && depth < 20)
        {
            sb.Insert(0, '/');
            sb.Insert(0, parent.name);
            parent = parent.parent;
            depth++;
        }

        return sb.ToString();
    }

    static string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
    }

    /// <summary>
    /// Behaviour that handles click detection for the inspector.
    /// </summary>
    class InspectorBehaviour : MonoBehaviour
    {
        void Update()
        {
            if (!_inspectorActive)
                return;

            // Right-click to exit inspector mode
            if (Input.GetMouseButtonDown(1))
            {
                Disable();
                return;
            }

            // Left-click to inspect
            if (Input.GetMouseButtonDown(0))
            {
                // Don't inspect if clicking on the inspector panel itself
                if (_inspectorPanel != null)
                {
                    RectTransform panelRect = _inspectorPanel.GetComponent<RectTransform>();
                    if (RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition))
                        return;
                }

                RectTransform hit = RaycastUI();
                if (hit != null)
                {
                    InspectElement(hit);
                }
            }

            // Update highlight position if target moved
            if (_currentTarget != null && _highlightOverlay != null && _highlightOverlay.activeSelf)
            {
                UpdateHighlight(_currentTarget);
            }
        }

        RectTransform RaycastUI()
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            Il2CppSystem.Collections.Generic.List<RaycastResult> results = new Il2CppSystem.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            // Skip our own overlay and panel
            for (int i = 0; i < results.Count; i++)
            {
                RaycastResult result = results[i];
                if (result.gameObject == null)
                    continue;

                // Skip VDebug elements
                if (result.gameObject.name.StartsWith("VDebug"))
                    continue;

                RectTransform rect = result.gameObject.GetComponent<RectTransform>();
                if (rect != null)
                    return rect;
            }

            return null;
        }
    }

    /// <summary>
    /// Drag handler for the inspector panel.
    /// </summary>
    class InspectorDragHandler : MonoBehaviour
    {
        RectTransform _rect;
        bool _dragging;
        Vector2 _offset;

        void Start()
        {
            _rect = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(_rect, Input.mousePosition))
                {
                    Vector3[] corners = new Vector3[4];
                    _rect.GetWorldCorners(corners);

                    // Only drag from header area (top 40px)
                    if (Input.mousePosition.y > corners[1].y - 40f)
                    {
                        _dragging = true;
                        _offset = (Vector2)_rect.position - (Vector2)Input.mousePosition;
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                _dragging = false;
            }

            if (_dragging)
            {
                _rect.position = (Vector2)Input.mousePosition + _offset;
            }
        }
    }
}
