using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using ProjectM.UI;
using UnityEngine.UI;

namespace Eclipse.Services;
internal static class DebugService
{
    const int MAX_CHILDREN = 20;
    const int MAX_COMPONENTS = 20;
    const int MAX_HITS = 20;
    const int MAX_SUBTAB_TEXT = 10;

    static bool bloodcraftDebugEnabled;

    static readonly string[] TabLabels =
    [
        "Equipment",
        "Crafting",
        "Blood Pool",
        "Attributes"
    ];

    /// <summary>
    /// Returns whether Bloodcraft-specific debug logging is enabled.
    /// </summary>
    public static bool BloodcraftDebugEnabled => bloodcraftDebugEnabled;

    /// <summary>
    /// Toggles verbose Bloodcraft UI debugging.
    /// </summary>
    public static void ToggleBloodcraftDebug()
    {
        bloodcraftDebugEnabled = !bloodcraftDebugEnabled;
        Core.Log.LogInfo($"[Debug UI] Bloodcraft debug {(bloodcraftDebugEnabled ? "enabled" : "disabled")}.");

        if (bloodcraftDebugEnabled)
        {
            DumpBloodcraftSubTabs("toggle");
        }
    }

    /// <summary>
    /// Logs the setup state for a Bloodcraft sub-tab button.
    /// </summary>
    /// <param name="buttonObject">The button GameObject.</param>
    /// <param name="label">The label text assigned.</param>
    /// <param name="primaryLabel">The primary TMP label used.</param>
    /// <param name="labels">All TMP labels found under the button.</param>
    /// <param name="localizedLabels">LocalizedText components found under the button.</param>
    /// <param name="usedFallbackLabel">True if a fallback label was created.</param>
    public static void LogBloodcraftSubTabSetup(GameObject buttonObject, string label, TMP_Text primaryLabel,
        TMP_Text[] labels, LocalizedText[] localizedLabels, bool usedFallbackLabel)
    {
        if (buttonObject == null)
        {
            return;
        }

        int labelCount = labels?.Length ?? 0;
        int localizedCount = localizedLabels?.Length ?? 0;
        string primaryPath = primaryLabel != null ? GetPath(primaryLabel.transform) : "none";
        Core.Log.LogInfo($"[Debug UI] Sub-tab '{buttonObject.name}' label='{label}' labels={labelCount} localized={localizedCount} fallback={usedFallbackLabel} primary={primaryPath}");

        if (bloodcraftDebugEnabled || usedFallbackLabel || labelCount == 0)
        {
            DumpSubTabDetails(buttonObject.transform, "setup");
        }
    }

    /// <summary>
    /// Dumps Bloodcraft sub-tab button and label diagnostics.
    /// </summary>
    /// <param name="context">Context label for the dump.</param>
    public static void DumpBloodcraftSubTabs(string context = "manual")
    {
        try
        {
            Core.Log.LogInfo($"[Debug UI] Bloodcraft sub-tab dump ({context}).");
            List<Transform> subTabs = FindTransformsByPrefix("BloodcraftSubTab_", "CharacterInventorySubMenu(Clone)");
            if (subTabs.Count == 0)
            {
                Core.Log.LogWarning("[Debug UI] No Bloodcraft sub-tab buttons found.");
                return;
            }

            for (int i = 0; i < subTabs.Count; i++)
            {
                DumpSubTabDetails(subTabs[i], context);
            }
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[Debug UI] Failed to dump Bloodcraft sub-tabs: {ex}");
        }
    }

    /// <summary>
    /// Logs detailed diagnostics for a sub-tab button.
    /// </summary>
    /// <param name="button">The button transform.</param>
    /// <param name="context">Context label for the dump.</param>
    static void DumpSubTabDetails(Transform button, string context)
    {
        if (button == null)
        {
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        Core.Log.LogInfo($"[Debug UI] Sub-tab '{button.name}' ({context}) at {GetPath(button)} active={button.gameObject.activeInHierarchy} size={FormatRect(rect)}");

        RectTransform parentRect = button.parent != null ? button.parent.GetComponent<RectTransform>() : null;
        if (parentRect != null)
        {
            Core.Log.LogInfo($"[Debug UI]   Parent rect {parentRect.name} size={FormatRect(parentRect)}");
        }

        CanvasGroup[] groups = button.GetComponentsInParent<CanvasGroup>(true);
        for (int i = 0; i < groups.Length; i++)
        {
            CanvasGroup group = groups[i];
            if (group == null)
            {
                continue;
            }

            Core.Log.LogInfo($"[Debug UI]   CanvasGroup[{i}] alpha={group.alpha:0.##} interactable={group.interactable} blocksRaycasts={group.blocksRaycasts}");
        }

        Canvas[] canvases = button.GetComponentsInParent<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null)
            {
                continue;
            }

            Core.Log.LogInfo($"[Debug UI]   Canvas[{i}] name={canvas.name} overrideSorting={canvas.overrideSorting} order={canvas.sortingOrder} layer={canvas.sortingLayerName}");
        }

        RectMask2D[] masks = button.GetComponentsInParent<RectMask2D>(true);
        for (int i = 0; i < masks.Length; i++)
        {
            RectMask2D mask = masks[i];
            if (mask == null)
            {
                continue;
            }

            Core.Log.LogInfo($"[Debug UI]   RectMask2D[{i}] name={mask.name} enabled={mask.enabled}");
        }

        Mask[] uiMasks = button.GetComponentsInParent<Mask>(true);
        for (int i = 0; i < uiMasks.Length; i++)
        {
            Mask mask = uiMasks[i];
            if (mask == null)
            {
                continue;
            }

            Core.Log.LogInfo($"[Debug UI]   Mask[{i}] name={mask.name} enabled={mask.enabled} showGraphic={mask.showMaskGraphic}");
        }

        TMP_Text[] texts = button.GetComponentsInChildren<TMP_Text>(true);
        if (texts.Length == 0)
        {
            Core.Log.LogWarning("[Debug UI]   No TMP_Text components found under sub-tab.");
        }

        int textLimit = Math.Min(texts.Length, MAX_SUBTAB_TEXT);
        for (int i = 0; i < textLimit; i++)
        {
            LogTmpText(texts[i], i);
        }

        if (texts.Length > MAX_SUBTAB_TEXT)
        {
            Core.Log.LogInfo($"[Debug UI]   ... {texts.Length - MAX_SUBTAB_TEXT} more TMP_Text component(s) not shown.");
        }

        LocalizedText[] localized = button.GetComponentsInChildren<LocalizedText>(true);
        for (int i = 0; i < localized.Length; i++)
        {
            LocalizedText local = localized[i];
            if (local == null)
            {
                continue;
            }

            Core.Log.LogInfo($"[Debug UI]   LocalizedText[{i}] name={local.name} enabled={local.enabled}");
        }
    }

    /// <summary>
    /// Logs a TMP text component with layout info.
    /// </summary>
    /// <param name="text">The TMP text component.</param>
    /// <param name="index">The index within the list.</param>
    static void LogTmpText(TMP_Text text, int index)
    {
        if (text == null)
        {
            Core.Log.LogWarning($"[Debug UI]   TMP_Text[{index}] is null.");
            return;
        }

        RectTransform rect = text.rectTransform;
        string value = text.text ?? string.Empty;
        string fontName = text.font != null ? text.font.name : "null";
        string materialName = text.fontMaterial != null ? text.fontMaterial.name : "null";
        Core.Log.LogInfo($"[Debug UI]   TMP_Text[{index}] name={text.name} active={text.gameObject.activeInHierarchy} enabled={text.enabled} maskable={text.maskable} text='{value}' size={FormatRect(rect)} fontSize={text.fontSize:0.##} color={FormatColor(text.color)} font={fontName} material={materialName}");

        if (string.IsNullOrWhiteSpace(value) || text.color.a < 0.1f || rect == null || rect.rect.width < 1f || rect.rect.height < 1f)
        {
            Core.Log.LogWarning($"[Debug UI]   TMP_Text[{index}] may be invisible (text empty, alpha low, or size too small).");
        }
    }

    /// <summary>
    /// Formats a color for logging.
    /// </summary>
    /// <param name="color">The color to format.</param>
    /// <returns>A formatted color string.</returns>
    static string FormatColor(Color color)
    {
        return $"({color.r:0.##},{color.g:0.##},{color.b:0.##},{color.a:0.##})";
    }

    /// <summary>
    /// Formats a rect transform for logging.
    /// </summary>
    /// <param name="rect">The rect transform to format.</param>
    /// <returns>A formatted rect string.</returns>
    static string FormatRect(RectTransform rect)
    {
        if (rect == null)
        {
            return "none";
        }

        Rect r = rect.rect;
        return $"({r.width:0.##}x{r.height:0.##}) pos=({rect.anchoredPosition.x:0.##},{rect.anchoredPosition.y:0.##})";
    }

    /// <summary>
    /// Dumps Character UI hierarchy data to logs to locate tab containers.
    /// </summary>
    public static void DumpCharacterUi()
    {
        try
        {
            Core.Log.LogInfo("[Debug UI] F1 pressed. Scanning Character UI labels...");

            List<TextMeshProUGUI> hits = FindLabelHits();
            if (hits.Count == 0)
            {
                Core.Log.LogWarning("[Debug UI] No Character tab labels found. Open the Character window and press F1 again.");
                return;
            }

            Core.Log.LogInfo($"[Debug UI] Found {hits.Count} label(s).");

            int limit = Math.Min(hits.Count, MAX_HITS);
            for (int i = 0; i < limit; i++)
            {
                LogLabelHit(hits[i]);
            }

            if (hits.Count > MAX_HITS)
            {
                Core.Log.LogInfo($"[Debug UI] Output truncated after {MAX_HITS} hits.");
            }
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[Debug UI] Failed to dump Character UI: {ex}");
        }
    }

    /// <summary>
    /// Dumps component lists for Character tab buttons to identify tab controllers.
    /// </summary>
    public static void DumpCharacterTabComponents()
    {
        try
        {
            Core.Log.LogInfo("[Debug UI] F2 pressed. Dumping Character tab components...");

            Transform tabButtons = FindTransformByName("TabButtons", "CharacterInventorySubMenu(Clone)");
            if (tabButtons == null)
            {
                Core.Log.LogWarning("[Debug UI] TabButtons not found. Open the Character window and press F2 again.");
                return;
            }

            LogComponents(tabButtons, "TabButtons");
            Transform motionRoot = tabButtons.parent;
            if (motionRoot != null)
            {
                LogComponents(motionRoot, "MotionRoot");
            }

            Transform subMenu = FindTransformByName("CharacterInventorySubMenu(Clone)", "HUDCanvas(Clone)");
            if (subMenu != null)
            {
                LogComponents(subMenu, "CharacterInventorySubMenu");
            }

            for (int i = 0; i < tabButtons.childCount; i++)
            {
                Transform child = tabButtons.GetChild(i);
                if (!child.name.Contains("TabButton", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                LogComponents(child, $"TabButtons/{child.name}");
            }
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[Debug UI] Failed to dump Character tab components: {ex}");
        }
    }

    /// <summary>
    /// Finds TextMeshProUGUI nodes that match known Character tab labels.
    /// </summary>
    /// <returns>A list of matching TMP labels.</returns>
    static List<TextMeshProUGUI> FindLabelHits()
    {
        List<TextMeshProUGUI> hits = [];

        foreach (TextMeshProUGUI text in UnityEngine.Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (text == null)
            {
                continue;
            }

            string value = text.text?.Trim();
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (MatchesTabLabel(value))
            {
                hits.Add(text);
            }
        }

        return hits;
    }

    /// <summary>
    /// Checks whether a label matches known Character tab text.
    /// </summary>
    /// <param name="text">The label text to check.</param>
    /// <returns>True if the label matches a known tab.</returns>
    static bool MatchesTabLabel(string text)
    {
        for (int i = 0; i < TabLabels.Length; i++)
        {
            if (text.Contains(TabLabels[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Logs the hierarchy around a label hit to help identify tab containers.
    /// </summary>
    /// <param name="text">The TMP label component.</param>
    static void LogLabelHit(TextMeshProUGUI text)
    {
        Transform transform = text.transform;
        Core.Log.LogInfo($"[Debug UI] Label '{text.text}' at {GetPath(transform)} (active={transform.gameObject.activeInHierarchy})");

        Transform parent = transform.parent;
        if (parent != null)
        {
            Core.Log.LogInfo($"[Debug UI] Parent: {GetPath(parent)} (children={parent.childCount})");
            LogChildren(parent);
        }

        Transform grandParent = parent?.parent;
        if (grandParent != null)
        {
            Core.Log.LogInfo($"[Debug UI] Grandparent: {GetPath(grandParent)} (children={grandParent.childCount})");
            LogChildren(grandParent);
        }
    }

    /// <summary>
    /// Logs the immediate children of a transform for context.
    /// </summary>
    /// <param name="parent">The parent transform.</param>
    static void LogChildren(Transform parent)
    {
        int limit = Math.Min(parent.childCount, MAX_CHILDREN);
        for (int i = 0; i < limit; i++)
        {
            Transform child = parent.GetChild(i);
            Core.Log.LogInfo($"[Debug UI]   child[{i}]: {child.name} (active={child.gameObject.activeSelf})");
        }

        if (parent.childCount > MAX_CHILDREN)
        {
            Core.Log.LogInfo($"[Debug UI]   ... {parent.childCount - MAX_CHILDREN} more child(ren) not shown.");
        }
    }

    /// <summary>
    /// Logs components attached to a transform.
    /// </summary>
    /// <param name="target">The transform to inspect.</param>
    /// <param name="label">A label for log context.</param>
    static void LogComponents(Transform target, string label)
    {
        if (target == null)
        {
            return;
        }

        Component[] components = target.GetComponents<Component>();
        Core.Log.LogInfo($"[Debug UI] Components on {label} at {GetPath(target)}: {components.Length}");

        int limit = Math.Min(components.Length, MAX_COMPONENTS);
        for (int i = 0; i < limit; i++)
        {
            Component component = components[i];
            string typeName = component?.GetType().FullName ?? "null";
            Core.Log.LogInfo($"[Debug UI]   [{i}] {typeName}");
        }

        if (components.Length > MAX_COMPONENTS)
        {
            Core.Log.LogInfo($"[Debug UI]   ... {components.Length - MAX_COMPONENTS} more component(s) not shown.");
        }
    }

    /// <summary>
    /// Finds a transform by name with an optional ancestor constraint.
    /// </summary>
    /// <param name="name">The transform name to search for.</param>
    /// <param name="ancestorName">Optional ancestor name to require.</param>
    /// <returns>The matching transform if found.</returns>
    static Transform FindTransformByName(string name, string ancestorName)
    {
        Transform fallback = null;

        foreach (Transform transform in UnityEngine.Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform == null || transform.name != name)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(ancestorName) && !HasAncestor(transform, ancestorName))
            {
                continue;
            }

            if (transform.gameObject.activeInHierarchy)
            {
                return transform;
            }

            fallback ??= transform;
        }

        return fallback;
    }

    /// <summary>
    /// Finds transforms by name prefix with an optional ancestor constraint.
    /// </summary>
    /// <param name="prefix">The name prefix to search for.</param>
    /// <param name="ancestorName">Optional ancestor name to require.</param>
    /// <returns>A list of matching transforms.</returns>
    static List<Transform> FindTransformsByPrefix(string prefix, string ancestorName)
    {
        List<Transform> results = [];

        foreach (Transform transform in UnityEngine.Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform == null || !transform.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(ancestorName) && !HasAncestor(transform, ancestorName))
            {
                continue;
            }

            results.Add(transform);
        }

        return results;
    }

    /// <summary>
    /// Checks whether a transform has a named ancestor.
    /// </summary>
    /// <param name="transform">The transform to walk up from.</param>
    /// <param name="ancestorName">The ancestor name to look for.</param>
    /// <returns>True if an ancestor is found.</returns>
    static bool HasAncestor(Transform transform, string ancestorName)
    {
        Transform current = transform.parent;
        while (current != null)
        {
            if (current.name == ancestorName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    /// <summary>
    /// Builds a full path for a transform in the scene hierarchy.
    /// </summary>
    /// <param name="transform">The transform to describe.</param>
    /// <returns>The path from the root to the transform.</returns>
    static string GetPath(Transform transform)
    {
        var sb = new StringBuilder();
        Transform current = transform;

        while (current != null)
        {
            if (sb.Length == 0)
            {
                sb.Insert(0, current.name);
            }
            else
            {
                sb.Insert(0, $"{current.name}/");
            }

            current = current.parent;
        }

        return sb.ToString();
    }

    #region Equipment Tab Hierarchy Dump

    const int MAX_DEPTH = 10;

    /// <summary>
    /// Dumps the full Equipment tab hierarchy with RectTransform data.
    /// Press F3 with Character menu open to trigger.
    /// </summary>
    public static void DumpEquipmentTabHierarchy()
    {
        try
        {
            Core.Log.LogInfo("[Debug UI] F3 pressed. Dumping Equipment tab hierarchy...");

            Transform equipmentTab = FindTransformByName("EquipmentTab", "CharacterInventorySubMenu(Clone)");
            if (equipmentTab == null)
            {
                Core.Log.LogWarning("[Debug UI] EquipmentTab not found. Open the Character menu Equipment tab and press F3.");
                return;
            }

            Core.Log.LogInfo("[Debug UI] === EQUIPMENT TAB HIERARCHY START ===");
            DumpTransformHierarchy(equipmentTab, 0);
            Core.Log.LogInfo("[Debug UI] === EQUIPMENT TAB HIERARCHY END ===");
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[Debug UI] Failed to dump Equipment tab hierarchy: {ex}");
        }
    }

    /// <summary>
    /// Recursively dumps a transform and all children with full RectTransform data.
    /// </summary>
    /// <param name="transform">The transform to dump.</param>
    /// <param name="depth">Current recursion depth.</param>
    static void DumpTransformHierarchy(Transform transform, int depth)
    {
        if (transform == null || depth > MAX_DEPTH)
        {
            return;
        }

        string indent = new(' ', depth * 2);
        RectTransform rt = transform.GetComponent<RectTransform>();

        if (rt != null)
        {
            // Full RectTransform data for 1:1 HTML/CSS recreation
            Core.Log.LogInfo($"[Debug UI] {indent}{transform.name} | " +
                $"pos:({rt.anchoredPosition.x:F1},{rt.anchoredPosition.y:F1}) " +
                $"size:({rt.sizeDelta.x:F1},{rt.sizeDelta.y:F1}) " +
                $"anchor:({rt.anchorMin.x:F2},{rt.anchorMin.y:F2})-({rt.anchorMax.x:F2},{rt.anchorMax.y:F2}) " +
                $"pivot:({rt.pivot.x:F2},{rt.pivot.y:F2}) " +
                $"active:{transform.gameObject.activeSelf}");

            // Log additional layout components if present
            DumpLayoutComponents(transform, indent);
        }
        else
        {
            Core.Log.LogInfo($"[Debug UI] {indent}{transform.name} | (no RectTransform) active:{transform.gameObject.activeSelf}");
        }

        // Recurse into children
        for (int i = 0; i < transform.childCount; i++)
        {
            DumpTransformHierarchy(transform.GetChild(i), depth + 1);
        }
    }

    /// <summary>
    /// Dumps layout-related components (LayoutGroup, LayoutElement, Image, Text).
    /// </summary>
    /// <param name="transform">The transform to inspect.</param>
    /// <param name="indent">Current indentation string.</param>
    static void DumpLayoutComponents(Transform transform, string indent)
    {
        // Check for VerticalLayoutGroup
        var vlg = transform.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            Core.Log.LogInfo($"[Debug UI] {indent}  [VLG] spacing:{vlg.spacing:F1} padding:(L:{vlg.padding.left},R:{vlg.padding.right},T:{vlg.padding.top},B:{vlg.padding.bottom}) childAlign:{vlg.childAlignment}");
        }

        // Check for HorizontalLayoutGroup
        var hlg = transform.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            Core.Log.LogInfo($"[Debug UI] {indent}  [HLG] spacing:{hlg.spacing:F1} padding:(L:{hlg.padding.left},R:{hlg.padding.right},T:{hlg.padding.top},B:{hlg.padding.bottom}) childAlign:{hlg.childAlignment}");
        }

        // Check for GridLayoutGroup
        var glg = transform.GetComponent<GridLayoutGroup>();
        if (glg != null)
        {
            Core.Log.LogInfo($"[Debug UI] {indent}  [GLG] cellSize:({glg.cellSize.x:F1},{glg.cellSize.y:F1}) spacing:({glg.spacing.x:F1},{glg.spacing.y:F1}) constraint:{glg.constraint} constraintCount:{glg.constraintCount}");
        }

        // Check for LayoutElement
        var le = transform.GetComponent<LayoutElement>();
        if (le != null && (le.preferredWidth > 0 || le.preferredHeight > 0 || le.minWidth > 0 || le.minHeight > 0))
        {
            Core.Log.LogInfo($"[Debug UI] {indent}  [LE] min:({le.minWidth:F1},{le.minHeight:F1}) pref:({le.preferredWidth:F1},{le.preferredHeight:F1}) flex:({le.flexibleWidth:F1},{le.flexibleHeight:F1})");
        }

        // Check for Image (for backgrounds, fills)
        var img = transform.GetComponent<Image>();
        if (img != null)
        {
            string spriteName = img.sprite != null ? img.sprite.name : "none";
            Core.Log.LogInfo($"[Debug UI] {indent}  [IMG] sprite:{spriteName} color:{FormatColor(img.color)} type:{img.type} fillAmount:{img.fillAmount:F2}");
        }

        // Check for TMP_Text
        var tmp = transform.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            string textPreview = tmp.text?.Length > 20 ? tmp.text.Substring(0, 20) + "..." : tmp.text ?? "";
            Core.Log.LogInfo($"[Debug UI] {indent}  [TMP] text:\"{textPreview}\" fontSize:{tmp.fontSize:F1} color:{FormatColor(tmp.color)} align:{tmp.alignment}");
        }
    }

    #endregion
}
