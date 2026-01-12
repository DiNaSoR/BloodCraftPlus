using Eclipse.Utilities;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProjectM.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Stunlock.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ProjectM;
using Unity.Entities;
using static Eclipse.Services.CanvasService.ConfigureHUD;
using static Eclipse.Services.CanvasService.DataHUD;
using static Eclipse.Services.CanvasService.UtilitiesHUD;
using static Eclipse.Services.DataService;
using Eclipse.Services.CharacterMenu;

namespace Eclipse.Services;

/// <summary>
/// Adds the Bloodcraft tab to the Character menu and renders Bloodcraft data inside it.
/// </summary>
internal static class CharacterMenuService
{
    const string CharacterMenuName = "CharacterInventorySubMenu(Clone)";
    const string CharacterMenuRootName = "CharacterMenu(Clone)";
    const string CharacterMenuRootAltName = "CharacterMenu";
    const string TabButtonsPath = "MotionRoot/TabButtons";
    const string TabContentPath = "Scroll View/Viewport/AttributesTabContent";
    const string AttributesTabButtonName = "AttributesTabButton";
    const string BloodcraftTabButtonName = "BloodcraftTabButton";
    const string BloodcraftTabName = "BloodcraftTab";
    const float HeaderFontScale = 1.3f;
    const float SubHeaderFontScale = 0.87f;
    const float EntryFontScale = 0.87f;
    const float SubTabFontScale = 0.5f;
    const float SubTabHeightScale = 1.0f;
    const int ContentPaddingLeft = 24;
    const int ContentPaddingRight = 24;
    const int ContentPaddingTop = 12;
    const int ContentPaddingBottom = 12;
    const int SubTabPaddingLeft = 24;
    const int SubTabPaddingRight = 24;
    const int SubTabPaddingTop = 0;
    const int SubTabPaddingBottom = 0;
    const float SubTabSpacing = 0f;
    const float ProfessionHeaderFontScale = 0.93f;
    const float ProfessionFontScale = 0.82f;
    const float ProfessionRowHeight = 30f;
    const float ProfessionIconSize = 24f;
    const float ProfessionNameWidth = 160f;
    const float ProfessionLevelWidth = 52f;
    const float ProfessionProgressWidth = 160f;
    const float ProfessionProgressHeight = 6f;
    const float ProfessionPercentWidth = 52f;

    static InventorySubMenu inventorySubMenu;
    static RectTransform bloodcraftTab;
    static SimpleStunButton bloodcraftTabButton;
    static Transform contentRoot;
    static TextMeshProUGUI headerText;
    static RectTransform headerDivider;
    static TextMeshProUGUI subHeaderText;
    static Transform entriesRoot;
    static GameObject entryTemplate;
    static TextMeshProUGUI entryStyle;
    static readonly List<TextMeshProUGUI> entries = [];
    static readonly List<SimpleStunButton> entryButtons = [];
    static Transform subTabRoot;
    static readonly List<SimpleStunButton> subTabButtons = [];
    static readonly List<TMP_Text> subTabLabels = [];
    static Transform professionsRoot;
    static Transform professionsListRoot;
    static TextMeshProUGUI professionsStatusText;
    static TextMeshProUGUI professionsSummaryText;
    static readonly List<ProfessionRow> professionRows = [];
    static Transform statBonusesRoot;
    static TextMeshProUGUI statBonusesWeaponText;
    static TextMeshProUGUI statBonusesCountText;
    static Image statBonusesWeaponImage;
    static Transform statBonusesListRoot;
    static readonly List<StatBonusRow> statBonusRows = [];
    static BloodcraftTab activeTab = BloodcraftTab.Prestige;
    static int bloodcraftTabIndex = -1;
    static int lastKnownTabIndex = -1;
    static bool initialized;
    static bool manualActive;
    static bool subTabDiagnosticsLogged;
    static float lastSubTabWidth;
    static float lastSubTabHeight;
    static int lastSubTabCount;
    static float subTabReferenceFontSize;

    // BloodcraftTab enum moved to DataService for shared access

    static readonly List<BloodcraftTab> BloodcraftTabOrder =
    [
        BloodcraftTab.Prestige,
        BloodcraftTab.Exoform,
        BloodcraftTab.Battles,
        BloodcraftTab.StatBonuses,
        BloodcraftTab.Professions
    ];

    static readonly Dictionary<BloodcraftTab, string> BloodcraftTabLabels = new()
    {
        { BloodcraftTab.Prestige, "Prestige" },
        { BloodcraftTab.Exoform, "Exoform" },
        { BloodcraftTab.Battles, "Familiar Battles" },
        { BloodcraftTab.StatBonuses, "Stat Bonuses" },
        { BloodcraftTab.Professions, "Professions" }
    };

    static readonly Dictionary<BloodcraftTab, string> BloodcraftSectionTitles = new()
    {
        { BloodcraftTab.Prestige, "Prestige Leaderboard" },
        { BloodcraftTab.Exoform, "Exoforms" },
        { BloodcraftTab.Battles, "Familiar Battles" },
        { BloodcraftTab.StatBonuses, "Stat Bonuses" },
        { BloodcraftTab.Professions, "Professions" }
    };

    /// <summary>
    /// Initializes the Bloodcraft tab inside the Character menu.
    /// </summary>
    /// <param name="menu">The inventory sub-menu instance.</param>
    public static void TryInitialize(InventorySubMenu menu)
    {
        if (menu == null || !IsCharacterMenu(menu))
        {
            return;
        }

        if (initialized && inventorySubMenu == menu)
        {
            return;
        }

        Reset();
        inventorySubMenu = menu;

        Il2CppReferenceArray<RectTransform> tabs = menu.Tabs;
        Il2CppReferenceArray<SimpleStunButton> tabButtons = menu.TabButtons;
        if (tabs == null || tabButtons == null || tabs.Length == 0 || tabButtons.Length == 0)
        {
            Core.Log.LogWarning("[Bloodcraft Tab] Failed to read existing tabs.");
            return;
        }

        Transform tabButtonsRoot = menu.transform.Find(TabButtonsPath);
        SimpleStunButton templateButton = FindTemplateButton(tabButtonsRoot);
        if (templateButton == null)
        {
            Core.Log.LogWarning("[Bloodcraft Tab] Failed to locate tab button template.");
            return;
        }

        RemoveBloodcraftTabButtons(tabButtonsRoot);
        RemoveBloodcraftTabs(tabs);
        tabButtons = FilterBloodcraftTabButtons(tabButtons);
        tabs = FilterBloodcraftTabs(tabs);

        bloodcraftTabIndex = tabs.Length;
        bloodcraftTabButton = CreateTabButton(templateButton, tabButtonsRoot);
        bloodcraftTab = CreateTabRoot(tabs[tabs.Length - 1], templateButton);

        if (bloodcraftTabButton == null || bloodcraftTab == null || entriesRoot == null || entryTemplate == null)
        {
            Core.Log.LogWarning("[Bloodcraft Tab] Failed to build Bloodcraft tab UI.");
            return;
        }

        menu.TabButtons = AppendTabButton(tabButtons, bloodcraftTabButton);
        menu.Tabs = AppendTab(tabs, bloodcraftTab);
        HookExistingTabButtons(menu.TabButtons);
        ConfigureBloodcraftButton();

        // Initialize the modular CharacterMenu system
        CharacterMenuIntegration.GetOrCreateOrchestrator();

        initialized = true;
        Core.Log.LogInfo("[Bloodcraft Tab] Initialized Bloodcraft tab in Character menu.");
    }

    /// <summary>
    /// Updates the Bloodcraft tab content when the Character menu is active.
    /// </summary>
    public static void Update()
    {
        if (!initialized || inventorySubMenu == null || bloodcraftTab == null)
        {
            return;
        }

        int currentTabIndex = inventorySubMenu.CurrentTab;
        bool isActive = manualActive || currentTabIndex == bloodcraftTabIndex;

        if (manualActive && currentTabIndex != lastKnownTabIndex && currentTabIndex != bloodcraftTabIndex)
        {
            manualActive = false;
            isActive = false;
        }

        bloodcraftTab.gameObject.SetActive(isActive);
        UpdateButtonSelection(isActive);

        if (!isActive)
        {
            lastKnownTabIndex = currentTabIndex;
            return;
        }

        UpdateSubTabSizing();
        ApplyTabVisibility();
        UpdateEntries();
        lastKnownTabIndex = currentTabIndex;
    }

    /// <summary>
    /// Clears cached references when the client UI is destroyed.
    /// </summary>
    public static void Reset()
    {
        // Reset the modular CharacterMenu system
        CharacterMenuIntegration.Reset();

        inventorySubMenu = null;
        bloodcraftTab = null;
        bloodcraftTabButton = null;
        contentRoot = null;
        headerText = null;
        headerDivider = null;
        subHeaderText = null;
        entriesRoot = null;
        entryTemplate = null;
        entryStyle = null;
        entries.Clear();
        entryButtons.Clear();
        subTabRoot = null;
        subTabButtons.Clear();
        subTabLabels.Clear();
        professionsRoot = null;
        professionsListRoot = null;
        professionsStatusText = null;
        professionsSummaryText = null;
        professionRows.Clear();
        statBonusesRoot = null;
        statBonusesWeaponText = null;
        statBonusesCountText = null;
        statBonusesListRoot = null;
        statBonusRows.Clear();
        activeTab = BloodcraftTab.Prestige;
        bloodcraftTabIndex = -1;
        lastKnownTabIndex = -1;
        lastSubTabWidth = 0f;
        lastSubTabHeight = 0f;
        lastSubTabCount = 0;
        subTabReferenceFontSize = 0f;
        initialized = false;
        manualActive = false;
        subTabDiagnosticsLogged = false;
    }

    /// <summary>
    /// Checks whether the provided menu instance belongs to the Character inventory menu.
    /// </summary>
    /// <param name="menu">The menu instance to inspect.</param>
    /// <returns>True when the menu matches the Character menu.</returns>
    static bool IsCharacterMenu(InventorySubMenu menu)
    {
        if (menu == null || menu.gameObject == null)
        {
            return false;
        }

        if (!menu.gameObject.name.Equals(CharacterMenuName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Transform current = menu.transform;
        return HasAncestorNamed(current, CharacterMenuRootName) || HasAncestorNamed(current, CharacterMenuRootAltName);
    }

    /// <summary>
    /// Checks whether the transform has an ancestor with the specified name.
    /// </summary>
    /// <param name="root">The starting transform.</param>
    /// <param name="name">The ancestor name to match.</param>
    /// <returns>True when an ancestor matches the provided name.</returns>
    static bool HasAncestorNamed(Transform root, string name)
    {
        Transform current = root;
        while (current != null)
        {
            if (current.name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    /// <summary>
    /// Finds the existing tab button used as a template for the Bloodcraft tab.
    /// </summary>
    /// <param name="tabButtonsRoot">The root transform containing tab buttons.</param>
    /// <returns>The template tab button or null if not found.</returns>
    static SimpleStunButton FindTemplateButton(Transform tabButtonsRoot)
    {
        if (tabButtonsRoot == null)
        {
            return null;
        }

        Transform templateTransform = tabButtonsRoot.Find(AttributesTabButtonName);
        return templateTransform != null ? templateTransform.GetComponent<SimpleStunButton>() : null;
    }

    /// <summary>
    /// Removes any previously created Bloodcraft tab buttons from the hierarchy.
    /// </summary>
    /// <param name="tabButtonsRoot">The tab button root to scan.</param>
    static void RemoveBloodcraftTabButtons(Transform tabButtonsRoot)
    {
        if (tabButtonsRoot == null)
        {
            return;
        }

        for (int i = tabButtonsRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = tabButtonsRoot.GetChild(i);
            if (child == null)
            {
                continue;
            }

            if (child.name.Equals(BloodcraftTabButtonName, StringComparison.OrdinalIgnoreCase))
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Removes any previously created Bloodcraft tab roots.
    /// </summary>
    /// <param name="tabs">The tab array to scan.</param>
    static void RemoveBloodcraftTabs(Il2CppReferenceArray<RectTransform> tabs)
    {
        if (tabs == null || tabs.Length == 0)
        {
            return;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            RectTransform tab = tabs[i];
            if (tab == null)
            {
                continue;
            }

            if (tab.name.Equals(BloodcraftTabName, StringComparison.OrdinalIgnoreCase))
            {
                UnityEngine.Object.Destroy(tab.gameObject);
            }
        }
    }

    /// <summary>
    /// Filters out Bloodcraft tab buttons from the tab button array.
    /// </summary>
    /// <param name="buttons">The current tab button array.</param>
    /// <returns>A filtered array without Bloodcraft entries.</returns>
    static Il2CppReferenceArray<SimpleStunButton> FilterBloodcraftTabButtons(Il2CppReferenceArray<SimpleStunButton> buttons)
    {
        if (buttons == null || buttons.Length == 0)
        {
            return new Il2CppReferenceArray<SimpleStunButton>(0);
        }

        List<SimpleStunButton> list = [];
        for (int i = 0; i < buttons.Length; i++)
        {
            SimpleStunButton button = buttons[i];
            if (button == null || button.gameObject == null)
            {
                continue;
            }

            if (!button.gameObject.name.Equals(BloodcraftTabButtonName, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(button);
            }
        }

        return ToIl2CppButtonArray(list);
    }

    /// <summary>
    /// Filters out Bloodcraft tab roots from the tab array.
    /// </summary>
    /// <param name="tabs">The current tab array.</param>
    /// <returns>A filtered array without Bloodcraft entries.</returns>
    static Il2CppReferenceArray<RectTransform> FilterBloodcraftTabs(Il2CppReferenceArray<RectTransform> tabs)
    {
        if (tabs == null || tabs.Length == 0)
        {
            return new Il2CppReferenceArray<RectTransform>(0);
        }

        List<RectTransform> list = [];
        for (int i = 0; i < tabs.Length; i++)
        {
            RectTransform tab = tabs[i];
            if (tab == null)
            {
                continue;
            }

            if (!tab.name.Equals(BloodcraftTabName, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(tab);
            }
        }

        return ToIl2CppTabArray(list);
    }

    /// <summary>
    /// Converts a managed list of buttons to an Il2Cpp reference array.
    /// </summary>
    /// <param name="items">The list of buttons to convert.</param>
    /// <returns>A new Il2CppReferenceArray containing the list items.</returns>
    static Il2CppReferenceArray<SimpleStunButton> ToIl2CppButtonArray(List<SimpleStunButton> items)
    {
        var array = new Il2CppReferenceArray<SimpleStunButton>(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            array[i] = items[i];
        }

        return array;
    }

    /// <summary>
    /// Converts a managed list of tabs to an Il2Cpp reference array.
    /// </summary>
    /// <param name="items">The list of tabs to convert.</param>
    /// <returns>A new Il2CppReferenceArray containing the list items.</returns>
    static Il2CppReferenceArray<RectTransform> ToIl2CppTabArray(List<RectTransform> items)
    {
        var array = new Il2CppReferenceArray<RectTransform>(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            array[i] = items[i];
        }

        return array;
    }

    /// <summary>
    /// Clones a tab button and updates its label for the Bloodcraft entry.
    /// </summary>
    /// <param name="templateButton">The existing button to clone.</param>
    /// <param name="parent">The parent transform to attach the new button.</param>
    /// <returns>The created tab button or null on failure.</returns>
    static SimpleStunButton CreateTabButton(SimpleStunButton templateButton, Transform parent)
    {
        if (templateButton == null || parent == null)
        {
            return null;
        }

        GameObject buttonObject = UnityEngine.Object.Instantiate(templateButton.gameObject, parent, false);
        buttonObject.name = BloodcraftTabButtonName;
        buttonObject.SetActive(true);
        buttonObject.transform.SetSiblingIndex(templateButton.transform.GetSiblingIndex() + 1);

        UpdateTabButtonLabel(buttonObject);

        return buttonObject.GetComponent<SimpleStunButton>();
    }

    /// <summary>
    /// Updates the tab button label text to Bloodcraft.
    /// </summary>
    /// <param name="buttonObject">The button object whose label is updated.</param>
    static void UpdateTabButtonLabel(GameObject buttonObject)
    {
        TMP_Text label = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = "Bloodcraft";
        }
    }

    /// <summary>
    /// Creates the Bloodcraft tab root by cloning an existing tab and preparing its content container.
    /// </summary>
    /// <param name="templateTab">The tab to clone for layout and styling.</param>
    /// <returns>The new Bloodcraft tab root transform.</returns>
    static RectTransform CreateTabRoot(RectTransform templateTab, SimpleStunButton templateButton)
    {
        if (templateTab == null)
        {
            return null;
        }

        RectTransform tabRoot = UnityEngine.Object.Instantiate(templateTab, templateTab.parent, false);
        tabRoot.name = BloodcraftTabName;
        tabRoot.gameObject.SetActive(false);

        TextMeshProUGUI referenceText = templateTab.GetComponentInChildren<TextMeshProUGUI>(true);
        contentRoot = tabRoot.Find(TabContentPath) ?? tabRoot;
        ClearChildren(contentRoot);
        contentRoot = CreateContentRoot(contentRoot);

        if (referenceText == null)
        {
            Core.Log.LogWarning("[Bloodcraft Tab] Failed to find reference text.");
            return tabRoot;
        }

        if (contentRoot == null)
        {
            Core.Log.LogWarning("[Bloodcraft Tab] Failed to create content root.");
            return tabRoot;
        }

        subTabRoot = CreateSubTabBar(contentRoot, referenceText, templateButton);

        Transform bodyRoot = CreatePaddedSectionRoot(contentRoot, "BloodcraftBodyRoot");
        if (bodyRoot == null)
        {
            bodyRoot = contentRoot;
        }

        headerText = CreateSectionHeader(bodyRoot, referenceText, ResolveSectionTitle(activeTab));
        headerDivider = CreateDividerLine(bodyRoot);
        subHeaderText = null;

        entriesRoot = CreateEntriesRoot(bodyRoot);
        if (entriesRoot != null)
        {
            entryTemplate = CreateEntryTemplate(entriesRoot, referenceText);
        }

        professionsRoot = CreateProfessionPanel(bodyRoot, referenceText);
        statBonusesRoot = CreateStatBonusesPanel(bodyRoot, referenceText);
        return tabRoot;
    }

    /// <summary>
    /// Removes all child objects under a parent transform.
    /// </summary>
    /// <param name="root">The parent transform to clear.</param>
    static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.Destroy(root.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Ensures the target transform has a vertical layout with size fitting for entries.
    /// </summary>
    /// <param name="root">The transform to configure.</param>
    /// <param name="paddingLeft">The left padding applied to the layout.</param>
    /// <param name="paddingRight">The right padding applied to the layout.</param>
    /// <param name="paddingTop">The top padding applied to the layout.</param>
    /// <param name="paddingBottom">The bottom padding applied to the layout.</param>
    /// <param name="spacing">The spacing between child elements.</param>
    static void EnsureVerticalLayout(Transform root, int paddingLeft = 0, int paddingRight = 0,
        int paddingTop = 0, int paddingBottom = 0, float spacing = 6f)
    {
        if (root == null)
        {
            return;
        }

        if (root.Equals(null))
        {
            return;
        }

        try
        {
            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = spacing;
            layout.padding = CreatePadding(paddingLeft, paddingRight, paddingTop, paddingBottom);

            ContentSizeFitter fitter = root.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        catch (Exception ex)
        {
            Core.Log.LogWarning($"[Bloodcraft Tab] Failed to configure vertical layout: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a RectOffset using the provided padding values.
    /// </summary>
    /// <param name="left">The left padding.</param>
    /// <param name="right">The right padding.</param>
    /// <param name="top">The top padding.</param>
    /// <param name="bottom">The bottom padding.</param>
    /// <returns>The configured padding instance.</returns>
    static RectOffset CreatePadding(int left, int right, int top, int bottom)
    {
        RectOffset padding = new();
        padding.left = left;
        padding.right = right;
        padding.top = top;
        padding.bottom = bottom;
        return padding;
    }

    /// <summary>
    /// Creates the root container for Bloodcraft content.
    /// </summary>
    /// <param name="parent">The parent transform to attach the container.</param>
    /// <returns>The container transform.</returns>
    static Transform CreateContentRoot(Transform parent)
    {
        if (parent == null || parent.Equals(null))
        {
            return null;
        }

        RectTransform rectTransform = CreateRectTransformObject("BloodcraftContentRoot", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        EnsureVerticalLayout(rectTransform, spacing: 10f);
        return rectTransform;
    }

    /// <summary>
    /// Creates a padded section root for grouped content.
    /// </summary>
    /// <param name="parent">The parent transform to attach the section.</param>
    /// <param name="name">The section name.</param>
    /// <returns>The section transform.</returns>
    static Transform CreatePaddedSectionRoot(Transform parent, string name)
    {
        if (parent == null || parent.Equals(null))
        {
            return null;
        }

        RectTransform rectTransform = CreateRectTransformObject(name, parent);
        if (rectTransform == null)
        {
            return null;
        }

        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        EnsureVerticalLayout(rectTransform, ContentPaddingLeft, ContentPaddingRight, ContentPaddingTop, ContentPaddingBottom);
        return rectTransform;
    }

    /// <summary>
    /// Creates a RectTransform-backed GameObject under a parent.
    /// </summary>
    /// <param name="name">The name of the GameObject.</param>
    /// <param name="parent">The parent transform.</param>
    /// <returns>The created RectTransform.</returns>
    static RectTransform CreateRectTransformObject(string name, Transform parent)
    {
        if (parent == null || parent.Equals(null))
        {
            return null;
        }

        var components = new Il2CppReferenceArray<Il2CppSystem.Type>(1);
        components[0] = Il2CppType.Of<RectTransform>();
        GameObject obj = new(name, components);
        obj.transform.SetParent(parent, false);
        return obj.GetComponent<RectTransform>();
    }

    /// <summary>
    /// Creates a reusable entry template for Bloodcraft tab rows.
    /// </summary>
    /// <param name="parent">The parent transform to attach the template.</param>
    /// <param name="reference">A reference text used to copy styling.</param>
    /// <returns>The entry template GameObject.</returns>
    static GameObject CreateEntryTemplate(Transform parent, TextMeshProUGUI reference)
    {
        RectTransform rectTransform = CreateRectTransformObject("BloodcraftEntryTemplate", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI text = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        CopyTextStyle(reference, text);
        text.fontSize = reference.fontSize * EntryFontScale;
        text.alignment = TextAlignmentOptions.Center;
        text.text = string.Empty;
        text.raycastTarget = true;
        text.enableAutoSizing = false;
        text.enableWordWrapping = false;
        text.richText = true;
        entryStyle = text;

        LayoutElement referenceLayout = reference.GetComponent<LayoutElement>();
        if (referenceLayout != null)
        {
            LayoutElement layout = rectTransform.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = referenceLayout.minHeight;
            layout.preferredHeight = referenceLayout.preferredHeight;
            layout.preferredWidth = referenceLayout.preferredWidth;
            layout.flexibleHeight = referenceLayout.flexibleHeight;
        }

        rectTransform.gameObject.SetActive(false);
        return rectTransform.gameObject;
    }

    /// <summary>
    /// Creates a container for text entries within the Bloodcraft tab.
    /// </summary>
    /// <param name="parent">The parent transform to attach the container.</param>
    /// <returns>The entries root transform.</returns>
    static Transform CreateEntriesRoot(Transform parent)
    {
        if (parent == null || parent.Equals(null))
        {
            return null;
        }

        RectTransform rectTransform = CreateRectTransformObject("BloodcraftEntries", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        EnsureVerticalLayout(rectTransform);
        return rectTransform;
    }

    /// <summary>
    /// Creates the main header text for the Bloodcraft tab.
    /// </summary>
    /// <param name="parent">The parent transform to attach the header.</param>
    /// <param name="reference">A reference text used to copy styling.</param>
    /// <param name="text">The header text.</param>
    /// <returns>The created header text component.</returns>
    static TextMeshProUGUI CreateSectionHeader(Transform parent, TextMeshProUGUI reference, string text)
    {
        TextMeshProUGUI header = CreateTextElement(parent, "BloodcraftHeader", reference, HeaderFontScale, FontStyles.Bold);
        if (header == null)
        {
            return null;
        }
        header.text = text;
        header.alignment = TextAlignmentOptions.Center;
        return header;
    }

    /// <summary>
    /// Creates the sub-header text for the Bloodcraft tab.
    /// </summary>
    /// <param name="parent">The parent transform to attach the sub-header.</param>
    /// <param name="reference">A reference text used to copy styling.</param>
    /// <param name="text">The sub-header text.</param>
    /// <returns>The created sub-header text component.</returns>
    static TextMeshProUGUI CreateSectionSubHeader(Transform parent, TextMeshProUGUI reference, string text)
    {
        TextMeshProUGUI subHeader = CreateTextElement(parent, "BloodcraftSubHeader", reference, SubHeaderFontScale, FontStyles.Normal);
        if (subHeader == null)
        {
            return null;
        }
        subHeader.text = text;
        subHeader.alignment = TextAlignmentOptions.Center;
        Color color = reference.color;
        subHeader.color = new Color(color.r, color.g, color.b, 0.8f);
        return subHeader;
    }

    /// <summary>
    /// Creates a styled text element based on an existing TMP reference.
    /// </summary>
    /// <param name="parent">The parent transform to attach the text.</param>
    /// <param name="name">The GameObject name.</param>
    /// <param name="reference">The reference text to copy styling.</param>
    /// <param name="scale">Font scale multiplier.</param>
    /// <param name="style">Font style to apply.</param>
    /// <returns>The created text component.</returns>
    static TextMeshProUGUI CreateTextElement(Transform parent, string name, TextMeshProUGUI reference, float scale, FontStyles style)
    {
        RectTransform rectTransform = CreateRectTransformObject(name, parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);

        TextMeshProUGUI text = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        CopyTextStyle(reference, text);
        text.fontSize = reference.fontSize * scale;
        text.fontStyle = style;
        text.enableAutoSizing = false;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>
    /// Updates the sub-tab widths to evenly fill the available bar width.
    /// </summary>
    static void UpdateSubTabSizing()
    {
        if (subTabRoot == null || subTabButtons.Count == 0)
        {
            return;
        }

        RectTransform barRect = subTabRoot as RectTransform ?? subTabRoot.GetComponent<RectTransform>();
        if (barRect == null)
        {
            return;
        }

        float barWidth = barRect.rect.width;
        if (barWidth <= 0f)
        {
            return;
        }

        int count = subTabButtons.Count;
        float totalSpacing = SubTabSpacing * Math.Max(0, count - 1);
        float availableWidth = barWidth - SubTabPaddingLeft - SubTabPaddingRight - totalSpacing;
        if (availableWidth <= 0f)
        {
            return;
        }

        float targetWidth = availableWidth / count;
        if (Mathf.Abs(targetWidth - lastSubTabWidth) < 0.25f && lastSubTabCount == count)
        {
            return;
        }

        lastSubTabWidth = targetWidth;
        lastSubTabCount = count;

        for (int i = 0; i < subTabButtons.Count; i++)
        {
            SimpleStunButton button = subTabButtons[i];
            if (button == null)
            {
                continue;
            }

            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                continue;
            }

            LayoutElement layout = rectTransform.GetComponent<LayoutElement>() ?? rectTransform.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = targetWidth;
            layout.preferredWidth = targetWidth;
            layout.flexibleWidth = 0f;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        }

        float targetHeight = barRect.rect.height;
        if (targetHeight > 1f && Mathf.Abs(targetHeight - lastSubTabHeight) >= 0.25f)
        {
            lastSubTabHeight = targetHeight;
            float desiredFontSize = subTabReferenceFontSize > 0f ? subTabReferenceFontSize * SubTabFontScale : 0f;
            ApplySubTabTextSizing(subTabLabels, targetHeight, desiredFontSize);
        }

        LayoutRebuilder.MarkLayoutForRebuild(barRect);
    }

    /// <summary>
    /// Creates the horizontal Bloodcraft sub-tab bar.
    /// </summary>
    /// <param name="parent">The parent transform to attach the bar.</param>
    /// <param name="reference">A reference text used for label sizing.</param>
    /// <param name="templateButton">The button template to clone.</param>
    /// <returns>The sub-tab bar root transform.</returns>
    static Transform CreateSubTabBar(Transform parent, TextMeshProUGUI reference, SimpleStunButton templateButton)
    {
        if (templateButton == null || parent == null)
        {
            return null;
        }

        RectTransform rectTransform = CreateRectTransformObject("BloodcraftTabBar", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        HorizontalLayoutGroup layout = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.padding = CreatePadding(SubTabPaddingLeft, SubTabPaddingRight, SubTabPaddingTop, SubTabPaddingBottom);
        layout.spacing = SubTabSpacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        RectTransform templateRect = templateButton.GetComponent<RectTransform>();
        float baseHeight = templateRect != null && templateRect.rect.height > 0f ? templateRect.rect.height : 36f;
        float targetHeight = baseHeight * SubTabHeightScale + SubTabPaddingTop + SubTabPaddingBottom;
        LayoutElement barLayout = rectTransform.gameObject.AddComponent<LayoutElement>();
        barLayout.minHeight = targetHeight;
        barLayout.preferredHeight = targetHeight;

        subTabReferenceFontSize = reference != null ? reference.fontSize : 0f;

        subTabButtons.Clear();
        subTabLabels.Clear();

        for (int i = 0; i < BloodcraftTabOrder.Count; i++)
        {
            BloodcraftTab tab = BloodcraftTabOrder[i];
            string label = BloodcraftTabLabels.TryGetValue(tab, out string tabLabel) ? tabLabel : tab.ToString();
            CreateSubTabButton(templateButton, rectTransform, reference, tab, label, i);
        }

        return rectTransform;
    }

    /// <summary>
    /// Creates a single Bloodcraft sub-tab button.
    /// </summary>
    /// <param name="templateButton">The button template to clone.</param>
    /// <param name="parent">The parent transform to attach the button.</param>
    /// <param name="reference">The reference text for sizing.</param>
    /// <param name="tab">The tab associated with the button.</param>
    /// <param name="label">The label to display.</param>
    /// <param name="index">The sibling index for ordering.</param>
    static void CreateSubTabButton(SimpleStunButton templateButton, Transform parent, TextMeshProUGUI reference,
        BloodcraftTab tab, string label, int index)
    {
        GameObject buttonObject = UnityEngine.Object.Instantiate(templateButton.gameObject, parent, false);
        buttonObject.name = $"BloodcraftSubTab_{tab}";
        buttonObject.SetActive(true);
        buttonObject.transform.SetSiblingIndex(index);

        SimpleStunButton button = buttonObject.GetComponent<SimpleStunButton>() ?? buttonObject.AddComponent<SimpleStunButton>();
        button.onClick.RemoveAllListeners();
        BloodcraftTab capturedTab = tab;
        button.onClick.AddListener((UnityAction)(() => activeTab = capturedTab));

        TMP_Text primaryLabel = null;
        TMP_Text[] labels = buttonObject.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            TMP_Text tmpLabel = labels[i];
            if (tmpLabel == null)
            {
                continue;
            }

            tmpLabel.text = label;
            tmpLabel.enableAutoSizing = false;
            tmpLabel.enableWordWrapping = false;
            tmpLabel.overflowMode = TextOverflowModes.Ellipsis;
            tmpLabel.alignment = TextAlignmentOptions.Center;
            tmpLabel.fontSize = reference.fontSize * SubTabFontScale;
            tmpLabel.color = new Color(reference.color.r, reference.color.g, reference.color.b, 1f);
            tmpLabel.enabled = true;
            tmpLabel.gameObject.SetActive(true);
            tmpLabel.maskable = false;
            tmpLabel.margin = Vector4.zero;
            ConfigureSubTabLabelRect(tmpLabel);

            if (primaryLabel == null)
            {
                primaryLabel = tmpLabel;
            }
        }

        LocalizedText[] localizedLabels = buttonObject.GetComponentsInChildren<LocalizedText>(true);
        for (int i = 0; i < localizedLabels.Length; i++)
        {
            LocalizedText localized = localizedLabels[i];
            if (localized == null)
            {
                continue;
            }

            localized.ForceSet(label);
            localized.enabled = false;
        }

        Transform labelParent = buttonObject.transform;
        if (labels.Length > 0 && labels[0] != null && labels[0].transform.parent != null)
        {
            labelParent = labels[0].transform.parent;
            RectTransform labelParentRect = labelParent as RectTransform ?? labelParent.GetComponent<RectTransform>();
            if (labelParentRect == null || labelParentRect.rect.width <= 1f || labelParentRect.rect.height <= 1f)
            {
                labelParent = buttonObject.transform;
            }
        }

        bool usedFallbackLabel = false;
        TMP_Text fallbackLabel = CreateSubTabLabel(labelParent, reference, label);
        if (fallbackLabel != null)
        {
            usedFallbackLabel = true;
            primaryLabel = fallbackLabel;
        }

        if (usedFallbackLabel && labels.Length > 0)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                TMP_Text tmpLabel = labels[i];
                if (tmpLabel == null)
                {
                    continue;
                }

                tmpLabel.enabled = false;
                tmpLabel.gameObject.SetActive(false);
            }
        }

        DebugService.LogBloodcraftSubTabSetup(buttonObject, label, primaryLabel, labels, localizedLabels, usedFallbackLabel);

        StretchSubTabGraphics(buttonObject);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        RectTransform templateRect = templateButton.GetComponent<RectTransform>();
        float baseHeight = templateRect != null && templateRect.rect.height > 0f ? templateRect.rect.height : 36f;
        float targetHeight = baseHeight * SubTabHeightScale;
        rectTransform.localScale = Vector3.one;

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>() ?? buttonObject.AddComponent<LayoutElement>();
        layout.minWidth = 0f;
        layout.preferredWidth = 0f;
        layout.flexibleWidth = 1f;
        layout.minHeight = targetHeight;
        layout.preferredHeight = targetHeight;
        layout.flexibleHeight = 0f;

        float desiredFontSize = reference.fontSize * SubTabFontScale;
        ApplySubTabTextSizing(labels, fallbackLabel, targetHeight, desiredFontSize);

        subTabButtons.Add(button);
        subTabLabels.Add(primaryLabel);
    }

    /// <summary>
    /// Creates a fallback label for a sub-tab button when the template label is missing.
    /// </summary>
    /// <param name="parent">The parent transform for the label.</param>
    /// <param name="reference">Reference text used for styling.</param>
    /// <param name="label">The label text to display.</param>
    /// <returns>The created TMP text component.</returns>
    static TMP_Text CreateSubTabLabel(Transform parent, TextMeshProUGUI reference, string label)
    {
        if (parent == null || parent.Equals(null) || reference == null)
        {
            return null;
        }

        RectTransform rectTransform = CreateRectTransformObject("BloodcraftSubTabLabel", parent);
        if (rectTransform == null)
        {
            return null;
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.SetAsLastSibling();

        LayoutElement layout = rectTransform.gameObject.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        TextMeshProUGUI text = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        CopyTextStyle(reference, text);
        text.fontSize = reference.fontSize * SubTabFontScale;
        text.fontStyle = FontStyles.Normal;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = false;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.maskable = false;
        text.raycastTarget = false;
        text.text = label;
        text.enabled = true;
        text.margin = Vector4.zero;
        ConfigureSubTabLabelRect(text);

        Color color = reference.color;
        if (color.a < 0.1f)
        {
            color.a = 1f;
        }

        text.color = color;
        return text;
    }

    /// <summary>
    /// Stretches sub-tab background graphics so adjacent tabs touch without visual gaps.
    /// </summary>
    /// <param name="buttonObject">The sub-tab button root.</param>
    static void StretchSubTabGraphics(GameObject buttonObject)
    {
        if (buttonObject == null)
        {
            return;
        }

        Image[] images = buttonObject.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image == null)
            {
                continue;
            }

            RectTransform rectTransform = image.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                continue;
            }

            bool shouldStretch = rectTransform.transform == buttonObject.transform;
            if (!shouldStretch)
            {
                string name = rectTransform.gameObject.name;
                shouldStretch = !string.IsNullOrWhiteSpace(name)
                    && name.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (!shouldStretch)
            {
                continue;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            if (rectTransform.transform != buttonObject.transform)
            {
                rectTransform.SetAsFirstSibling();
            }
        }
    }

    static void ApplySubTabTextSizing(TMP_Text[] templateLabels, TMP_Text fallbackLabel, float targetHeight, float desiredFontSize)
    {
        float finalFontSize = ResolveSubTabFontSize(targetHeight, desiredFontSize);

        for (int i = 0; i < templateLabels.Length; i++)
        {
            TMP_Text label = templateLabels[i];
            if (label == null)
            {
                continue;
            }

            ApplySubTabLabelStyle(label, finalFontSize);
        }

        if (fallbackLabel != null)
        {
            ApplySubTabLabelStyle(fallbackLabel, finalFontSize);
        }
    }

    static void ApplySubTabTextSizing(IReadOnlyList<TMP_Text> labels, float targetHeight, float desiredFontSize)
    {
        if (labels == null || labels.Count == 0)
        {
            return;
        }

        float finalFontSize = ResolveSubTabFontSize(targetHeight, desiredFontSize);
        for (int i = 0; i < labels.Count; i++)
        {
            TMP_Text label = labels[i];
            if (label == null)
            {
                continue;
            }

            ApplySubTabLabelStyle(label, finalFontSize);
        }
    }

    static float ResolveSubTabFontSize(float targetHeight, float desiredFontSize)
    {
        float maxFontSize = Mathf.Max(10f, targetHeight * 0.65f);
        if (desiredFontSize <= 0f)
        {
            return maxFontSize;
        }

        return Mathf.Min(desiredFontSize, maxFontSize);
    }

    static void ApplySubTabLabelStyle(TMP_Text label, float fontSize)
    {
        if (label == null)
        {
            return;
        }

        label.fontSize = fontSize;
        label.enableAutoSizing = false;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.margin = Vector4.zero;
        ConfigureSubTabLabelRect(label);
    }

    static void ConfigureSubTabLabelRect(TMP_Text label)
    {
        RectTransform rectTransform = label?.rectTransform;
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// Creates the professions panel container.
    /// </summary>
    /// <param name="parent">The parent transform to attach the panel.</param>
    /// <param name="reference">Reference text used to style labels.</param>
    /// <returns>The professions panel root transform.</returns>
    static Transform CreateProfessionPanel(Transform parent, TextMeshProUGUI reference)
    {
        RectTransform rectTransform = CreateRectTransformObject("BloodcraftProfessions", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        EnsureVerticalLayout(rectTransform);

        professionsStatusText = CreateSectionSubHeader(rectTransform, reference, string.Empty);
        CreateProfessionHeaderRow(rectTransform, reference);
        professionsListRoot = CreateProfessionListRoot(rectTransform);
        _ = CreateDividerLine(rectTransform);
        professionsSummaryText = CreateSectionSubHeader(rectTransform, reference, string.Empty);

        rectTransform.gameObject.SetActive(false);
        return rectTransform;
    }

    /// <summary>
    /// Creates the stat bonuses panel for weapon expertise stat selection.
    /// </summary>
    /// <param name="parent">The parent transform to attach the panel.</param>
    /// <param name="reference">Reference text used to style labels.</param>
    /// <returns>The panel's root transform.</returns>
    static Transform CreateStatBonusesPanel(Transform parent, TextMeshProUGUI reference)
    {
        RectTransform rectTransform = CreateRectTransformObject("BloodcraftStatBonuses", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        EnsureVerticalLayout(rectTransform);

        // Weapon header section
        CreateStatBonusesWeaponHeader(rectTransform, reference);
        _ = CreateDividerLine(rectTransform);

        // Stats list container
        statBonusesListRoot = CreateStatBonusesListRoot(rectTransform);

        // Hint text
        TextMeshProUGUI hintText = CreateSectionSubHeader(rectTransform, reference, "Click a stat to toggle selection.");
        if (hintText != null)
        {
            hintText.alpha = 0.5f;
            hintText.alignment = TextAlignmentOptions.Center;
        }

        rectTransform.gameObject.SetActive(false);
        return rectTransform;
    }

    /// <summary>
    /// Creates the weapon header section for stat bonuses.
    /// </summary>
    /// <param name="parent">The parent transform.</param>
    /// <param name="reference">Reference text for styling.</param>
    static void CreateStatBonusesWeaponHeader(Transform parent, TextMeshProUGUI reference)
    {
        RectTransform rectTransform = CreateRectTransformObject("WeaponHeader", parent);
        if (rectTransform == null)
        {
            return;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        HorizontalLayoutGroup layout = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 12f;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.padding = CreatePadding(12, 12, 8, 8);

        LayoutElement rowLayout = rectTransform.gameObject.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 48f;
        rowLayout.minHeight = 48f;

        // Weapon icon placeholder
        RectTransform iconRect = CreateRectTransformObject("WeaponIcon", rectTransform);
        if (iconRect != null)
        {
            iconRect.sizeDelta = new Vector2(40f, 40f);
            Image iconImage = iconRect.gameObject.AddComponent<Image>();
            iconImage.color = new Color(1f, 1f, 1f, 0.2f);
            statBonusesWeaponImage = iconImage;
            iconImage.raycastTarget = false;
            LayoutElement iconLayout = iconRect.gameObject.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 40f;
            iconLayout.preferredHeight = 40f;
        }

        // Weapon info container
        RectTransform infoRect = CreateRectTransformObject("WeaponInfo", rectTransform);
        if (infoRect != null)
        {
            VerticalLayoutGroup infoLayout = infoRect.gameObject.AddComponent<VerticalLayoutGroup>();
            infoLayout.childAlignment = TextAnchor.MiddleLeft;
            infoLayout.spacing = 2f;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            infoLayout.childControlHeight = true;
            infoLayout.childControlWidth = true;

            LayoutElement infoElement = infoRect.gameObject.AddComponent<LayoutElement>();
            infoElement.flexibleWidth = 1f;

            // Weapon name text
            statBonusesWeaponText = CreateText(infoRect, reference, "No Weapon Equipped", reference.fontSize * 1.1f, TextAlignmentOptions.Left);
            if (statBonusesWeaponText != null)
            {
                statBonusesWeaponText.color = new Color(0.95f, 0.84f, 0.7f, 1f); // Gold-ish color
            }

            // Selection count text
            statBonusesCountText = CreateText(infoRect, reference, "0 / 0 Bonuses Selected", reference.fontSize * 0.85f, TextAlignmentOptions.Left);
            if (statBonusesCountText != null)
            {
                statBonusesCountText.alpha = 0.7f;
            }
        }
    }

    /// <summary>
    /// Creates the stat bonuses list container.
    /// </summary>
    /// <param name="parent">The parent transform.</param>
    /// <returns>The list root transform.</returns>
    static Transform CreateStatBonusesListRoot(Transform parent)
    {
        RectTransform rectTransform = CreateRectTransformObject("StatBonusList", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.spacing = 4f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.padding = CreatePadding(0, 0, 6, 6);

        ContentSizeFitter fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        return rectTransform;
    }

    /// <summary>
    /// Creates a divider line for section separation.
    /// </summary>
    /// <param name="parent">The parent transform to attach the divider.</param>
    static RectTransform CreateDividerLine(Transform parent)
    {
        RectTransform rectTransform = CreateRectTransformObject("BloodcraftDivider", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);

        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.sprite = ResolveProgressBackgroundSprite();
        image.color = new Color(1f, 1f, 1f, 0.2f);
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;

        LayoutElement layout = rectTransform.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 2f;
        layout.minHeight = 2f;
        return rectTransform;
    }

    /// <summary>
    /// Creates the profession header row.
    /// </summary>
    /// <param name="parent">The parent transform to attach the row.</param>
    /// <param name="reference">Reference text used to style labels.</param>
    static void CreateProfessionHeaderRow(Transform parent, TextMeshProUGUI reference)
    {
        RectTransform rectTransform = CreateRectTransformObject("ProfessionHeaderRow", parent);
        if (rectTransform == null)
        {
            return;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        HorizontalLayoutGroup layout = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 12f;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        LayoutElement rowLayout = rectTransform.gameObject.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = ProfessionRowHeight;
        rowLayout.minHeight = ProfessionRowHeight;

        AddSpacer(rectTransform, ProfessionIconSize);
        CreateHeaderLabel(rectTransform, reference, "Profession", ProfessionNameWidth, TextAlignmentOptions.Left);
        CreateHeaderLabel(rectTransform, reference, "Level", ProfessionLevelWidth, TextAlignmentOptions.Center);
        CreateHeaderLabel(rectTransform, reference, "Progress", 0f, TextAlignmentOptions.Left, 1f);
        AddSpacer(rectTransform, ProfessionPercentWidth);
    }

    /// <summary>
    /// Creates the professions list container.
    /// </summary>
    /// <param name="parent">The parent transform to attach the list.</param>
    /// <returns>The list root transform.</returns>
    static Transform CreateProfessionListRoot(Transform parent)
    {
        RectTransform rectTransform = CreateRectTransformObject("ProfessionList", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        EnsureVerticalLayout(rectTransform);
        return rectTransform;
    }

    /// <summary>
    /// Adds a fixed-width spacer to a layout.
    /// </summary>
    /// <param name="parent">The parent transform to attach the spacer.</param>
    /// <param name="width">The width for the spacer.</param>
    static void AddSpacer(Transform parent, float width)
    {
        RectTransform rectTransform = CreateRectTransformObject("Spacer", parent);
        if (rectTransform == null)
        {
            return;
        }

        LayoutElement layout = rectTransform.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.preferredHeight = ProfessionRowHeight;
    }

    /// <summary>
    /// Creates a header label with fixed width.
    /// </summary>
    /// <param name="parent">The parent transform to attach the label.</param>
    /// <param name="reference">Reference text used to style labels.</param>
    /// <param name="text">Label text.</param>
    /// <param name="width">Preferred width.</param>
    static TextMeshProUGUI CreateHeaderLabel(Transform parent, TextMeshProUGUI reference, string text, float width,
        TextAlignmentOptions alignment, float flexibleWidth = 0f)
    {
        TextMeshProUGUI label = CreateTextElement(parent, $"Header_{text}", reference, ProfessionHeaderFontScale, FontStyles.Bold);
        if (label == null)
        {
            return null;
        }
        label.text = text;
        label.alignment = alignment;

        LayoutElement layout = label.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.flexibleWidth = flexibleWidth;
        layout.preferredHeight = ProfessionRowHeight;
        layout.minHeight = ProfessionRowHeight;
        return label;
    }

    /// <summary>
    /// Resolves a sprite for progress bar backgrounds.
    /// </summary>
    /// <returns>The resolved sprite or null.</returns>
    static Sprite ResolveProgressBackgroundSprite()
    {
        if (_alchemyFill != null && _alchemyFill.sprite != null)
        {
            return _alchemyFill.sprite;
        }

        if (_alchemyProgressFill != null && _alchemyProgressFill.sprite != null)
        {
            return _alchemyProgressFill.sprite;
        }

        if (Sprites.TryGetValue("Attribute_TierIndicator_Fixed", out Sprite fallback))
        {
            return fallback;
        }

        return null;
    }

    /// <summary>
    /// Resolves a sprite for progress bar fills.
    /// </summary>
    /// <returns>The resolved sprite or null.</returns>
    static Sprite ResolveProgressFillSprite()
    {
        if (_alchemyProgressFill != null && _alchemyProgressFill.sprite != null)
        {
            return _alchemyProgressFill.sprite;
        }

        if (_alchemyFill != null && _alchemyFill.sprite != null)
        {
            return _alchemyFill.sprite;
        }

        if (Sprites.TryGetValue("Attribute_TierIndicator_Fixed", out Sprite fallback))
        {
            return fallback;
        }

        return null;
    }

    /// <summary>
    /// Resolves the icon sprite for a profession.
    /// </summary>
    /// <param name="profession">The profession to resolve.</param>
    /// <returns>The profession icon sprite or null.</returns>
    static Sprite ResolveProfessionIcon(Profession profession)
    {
        if (ProfessionIcons.TryGetValue(profession, out string spriteName) && Sprites.TryGetValue(spriteName, out Sprite sprite))
        {
            return sprite;
        }

        return null;
    }

    /// <summary>
    /// Copies text styling settings from a source to a target TMP text component.
    /// </summary>
    /// <param name="source">The reference text component.</param>
    /// <param name="target">The text component to update.</param>
    static void CopyTextStyle(TextMeshProUGUI source, TextMeshProUGUI target)
    {
        target.font = source.font;
        target.fontSize = source.fontSize;
        target.fontStyle = source.fontStyle;
        target.color = source.color;
        target.alignment = source.alignment;
        target.margin = source.margin;
        target.overflowMode = source.overflowMode;
    }

    /// <summary>
    /// Appends a button to the tab button array.
    /// </summary>
    /// <param name="buttons">The existing tab button array.</param>
    /// <param name="newButton">The new button to append.</param>
    /// <returns>A resized array containing the new button.</returns>
    static Il2CppReferenceArray<SimpleStunButton> AppendTabButton(Il2CppReferenceArray<SimpleStunButton> buttons, SimpleStunButton newButton)
    {
        int length = buttons.Length;
        var expanded = new Il2CppReferenceArray<SimpleStunButton>(length + 1);

        for (int i = 0; i < length; i++)
        {
            expanded[i] = buttons[i];
        }

        expanded[length] = newButton;
        return expanded;
    }

    /// <summary>
    /// Appends a tab root to the tab array.
    /// </summary>
    /// <param name="tabs">The existing tab array.</param>
    /// <param name="newTab">The new tab to append.</param>
    /// <returns>A resized array containing the new tab.</returns>
    static Il2CppReferenceArray<RectTransform> AppendTab(Il2CppReferenceArray<RectTransform> tabs, RectTransform newTab)
    {
        int length = tabs.Length;
        var expanded = new Il2CppReferenceArray<RectTransform>(length + 1);

        for (int i = 0; i < length; i++)
        {
            expanded[i] = tabs[i];
        }

        expanded[length] = newTab;
        return expanded;
    }

    /// <summary>
    /// Hooks the existing tab buttons to clear manual Bloodcraft selection.
    /// </summary>
    /// <param name="tabButtons">The existing tab buttons.</param>
    static void HookExistingTabButtons(Il2CppReferenceArray<SimpleStunButton> tabButtons)
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            SimpleStunButton button = tabButtons[i];
            if (button == null)
            {
                continue;
            }

            button.onClick.AddListener((UnityAction)(() => manualActive = false));
        }
    }

    /// <summary>
    /// Binds click handling to the Bloodcraft tab button.
    /// </summary>
    static void ConfigureBloodcraftButton()
    {
        if (bloodcraftTabButton == null)
        {
            return;
        }

        bloodcraftTabButton.onClick.RemoveAllListeners();
        bloodcraftTabButton.onClick.AddListener((UnityAction)ActivateBloodcraftTab);
    }

    /// <summary>
    /// Activates the Bloodcraft tab, falling back to manual mode if needed.
    /// </summary>
    static void ActivateBloodcraftTab()
    {
        if (inventorySubMenu == null)
        {
            return;
        }

        manualActive = false;
        int previousTab = inventorySubMenu.CurrentTab;
        inventorySubMenu.CurrentTab = bloodcraftTabIndex;

        if (inventorySubMenu.CurrentTab != bloodcraftTabIndex)
        {
            manualActive = true;
            inventorySubMenu.CurrentTab = previousTab;
        }

        Update();
    }

    /// <summary>
    /// Updates the visual selection state of the Bloodcraft tab button.
    /// </summary>
    /// <param name="isActive">Whether the Bloodcraft tab is active.</param>
    static void UpdateButtonSelection(bool isActive)
    {
        if (bloodcraftTabButton == null)
        {
            return;
        }

        bloodcraftTabButton.ForceSelect = isActive;
        bloodcraftTabButton.ForceHighlight = isActive;
    }

    /// <summary>
    /// Ensures only the Bloodcraft tab content is visible when active.
    /// </summary>
    static void ApplyTabVisibility()
    {
        if (inventorySubMenu == null)
        {
            return;
        }

        Il2CppReferenceArray<RectTransform> tabs = inventorySubMenu.Tabs;
        if (tabs == null)
        {
            return;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            RectTransform tab = tabs[i];
            if (tab == null)
            {
                continue;
            }

            tab.gameObject.SetActive(i == bloodcraftTabIndex);
        }
    }

    /// <summary>
    /// Updates the Bloodcraft entry list based on the active sub-tab.
    /// </summary>
    static void UpdateEntries()
    {
        if (entriesRoot == null || entryTemplate == null)
        {
            return;
        }

        UpdateSubTabSelection();
        UpdateSectionHeader();

        bool showTextEntries = activeTab != BloodcraftTab.Professions && activeTab != BloodcraftTab.StatBonuses;
        entriesRoot.gameObject.SetActive(showTextEntries);

        if (professionsRoot != null)
        {
            professionsRoot.gameObject.SetActive(activeTab == BloodcraftTab.Professions);
        }

        if (statBonusesRoot != null)
        {
            statBonusesRoot.gameObject.SetActive(activeTab == BloodcraftTab.StatBonuses);
        }

        if (showTextEntries)
        {
            List<BloodcraftEntry> entriesToApply = BuildEntries();
            EnsureEntries(entriesToApply.Count);

            for (int i = 0; i < entriesToApply.Count; i++)
            {
                BloodcraftEntry entry = entriesToApply[i];
                TextMeshProUGUI text = entries[i];
                text.text = entry.Text;
                text.fontSize = entryStyle != null ? entryStyle.fontSize : text.fontSize;
                text.alignment = TextAlignmentOptions.Center;
                text.fontStyle = entry.Style;
                text.color = Color.white;

                if (entry.Action != null)
                {
                    ConfigureActionButton(entryButtons[i], entry.Action, entry.Enabled);
                }
                else
                {
                    ConfigureCommandButton(entryButtons[i], entry.Command, entry.Enabled);
                }
            }
        }
        else if (activeTab == BloodcraftTab.Professions)
        {
            EnsureEntries(0);
            UpdateProfessionPanel();
        }
        else if (activeTab == BloodcraftTab.StatBonuses)
        {
            EnsureEntries(0);
            UpdateStatBonusesPanel();
        }
    }

    /// <summary>
    /// Builds the list of entries to render in the Bloodcraft tab.
    /// </summary>
    /// <returns>A list of entries for the current view.</returns>
    static List<BloodcraftEntry> BuildEntries()
    {
        List<BloodcraftEntry> list = [];

        switch (activeTab)
        {
            case BloodcraftTab.Exoform:
                AppendExoFormEntries(list);
                break;
            case BloodcraftTab.Battles:
                AppendFamiliarBattleEntries(list);
                break;
            case BloodcraftTab.Professions:
                break;
            default:
                AppendPrestigeEntries(list);
                break;
        }

        return list;
    }

    /// <summary>
    /// Appends prestige leaderboard entries to the list.
    /// </summary>
    /// <param name="list">The list to populate.</param>
    static void AppendPrestigeEntries(List<BloodcraftEntry> list)
    {
        if (!_prestigeDataReady)
        {
            list.Add(new BloodcraftEntry("Awaiting prestige data...", FontStyles.Normal));
            return;
        }

        if (!_prestigeSystemEnabled)
        {
            list.Add(new BloodcraftEntry("Prestige system disabled.", FontStyles.Normal));
            return;
        }

        if (!_prestigeLeaderboardEnabled)
        {
            list.Add(new BloodcraftEntry("Prestige leaderboard disabled.", FontStyles.Normal));
            return;
        }

        if (_prestigeLeaderboardOrder.Count == 0)
        {
            list.Add(new BloodcraftEntry("No prestige data available.", FontStyles.Normal));
            return;
        }

        if (_prestigeLeaderboardIndex >= _prestigeLeaderboardOrder.Count)
        {
            _prestigeLeaderboardIndex = 0;
        }

        string typeKey = _prestigeLeaderboardOrder[_prestigeLeaderboardIndex];
        string displayType = SplitPascalCase(typeKey);
        _prestigeLeaderboards.TryGetValue(typeKey, out List<PrestigeLeaderboardEntry> leaderboard);
        leaderboard ??= [];

        list.Add(new BloodcraftEntry("Click type to cycle.", FontStyles.Normal));
        list.Add(new BloodcraftEntry($"Type: {displayType}", FontStyles.Normal, action: CyclePrestigeType, enabled: _prestigeLeaderboardOrder.Count > 1));

        if (leaderboard.Count == 0)
        {
            list.Add(new BloodcraftEntry("No prestige entries yet.", FontStyles.Normal));
            return;
        }

        for (int i = 0; i < leaderboard.Count; i++)
        {
            PrestigeLeaderboardEntry entry = leaderboard[i];
            FontStyles style = i == 0 ? FontStyles.Bold : FontStyles.Normal;
            list.Add(new BloodcraftEntry($"{i + 1} | {entry.Name}: {entry.Value}", style));
        }
    }

    /// <summary>
    /// Appends exoform and shapeshift entries to the list.
    /// </summary>
    /// <param name="list">The list to populate.</param>
    static void AppendExoFormEntries(List<BloodcraftEntry> list)
    {
        if (!_exoFormDataReady)
        {
            list.Add(new BloodcraftEntry("Awaiting exoform data...", FontStyles.Normal));
            return;
        }

        if (!_exoFormEnabled)
        {
            list.Add(new BloodcraftEntry("Exo prestiging disabled.", FontStyles.Normal));
            return;
        }

        string currentForm = string.IsNullOrWhiteSpace(_exoFormCurrentForm)
            ? "None"
            : SplitPascalCase(_exoFormCurrentForm);

        list.Add(new BloodcraftEntry($"Current: {currentForm}", FontStyles.Normal));

        bool canToggleTaunt = _exoFormPrestiges > 0;
        string chargeLine = _exoFormMaxDuration > 0f
            ? $"Charge: {_exoFormCharge:0.0}/{_exoFormMaxDuration:0.0}s"
            : "Charge: --";

        string tauntStatus = _exoFormTauntEnabled ? "<color=green>On</color>" : "<color=red>Off</color>";

        list.Add(new BloodcraftEntry($"Exo Prestiges: {_exoFormPrestiges}", FontStyles.Normal));
        list.Add(new BloodcraftEntry(chargeLine, FontStyles.Normal));
        list.Add(new BloodcraftEntry($"Taunt to Exoform: {tauntStatus}", FontStyles.Normal, command: ".prestige exoform", enabled: canToggleTaunt));
        list.Add(new BloodcraftEntry("Forms", FontStyles.Bold));

        for (int i = 0; i < _exoFormEntries.Count; i++)
        {
            ExoFormEntry form = _exoFormEntries[i];
            string formName = SplitPascalCase(form.FormName);
            string status = form.Unlocked ? "Unlocked" : "Locked";
            FontStyles style = form.FormName.Equals(_exoFormCurrentForm, StringComparison.OrdinalIgnoreCase)
                ? FontStyles.Bold
                : FontStyles.Normal;

            list.Add(new BloodcraftEntry($"{i + 1} | {formName} ({status})", style,
                command: $".prestige sf {form.FormName}", enabled: form.Unlocked));
        }

        ExoFormEntry activeForm = ResolveActiveExoForm();
        if (activeForm != null && activeForm.Abilities.Count > 0)
        {
            list.Add(new BloodcraftEntry("Abilities", FontStyles.Bold));

            foreach (ExoFormAbilityData ability in activeForm.Abilities)
            {
                string abilityName = ResolveAbilityName(ability.AbilityId);
                list.Add(new BloodcraftEntry($" - {abilityName} ({ability.Cooldown:0.0}s)", FontStyles.Normal));
            }
        }
    }

    /// <summary>
    /// Appends familiar battle group entries to the list.
    /// </summary>
    /// <param name="list">The list to populate.</param>
    static void AppendFamiliarBattleEntries(List<BloodcraftEntry> list)
    {
        if (!_familiarBattleDataReady)
        {
            list.Add(new BloodcraftEntry("Awaiting familiar battle data...", FontStyles.Normal));
            return;
        }

        if (!_familiarSystemEnabled)
        {
            list.Add(new BloodcraftEntry("Familiars are disabled.", FontStyles.Normal));
            return;
        }

        if (!_familiarBattlesEnabled)
        {
            list.Add(new BloodcraftEntry("Familiar battles disabled.", FontStyles.Normal));
            return;
        }

        string activeGroupName = string.IsNullOrWhiteSpace(_familiarActiveBattleGroup)
            ? "None"
            : _familiarActiveBattleGroup;

        list.Add(new BloodcraftEntry($"Active group: {activeGroupName}", FontStyles.Normal));

        if (_familiarBattleGroups.Count == 0)
        {
            list.Add(new BloodcraftEntry("No battle groups available.", FontStyles.Normal));
            return;
        }

        list.Add(new BloodcraftEntry("Groups", FontStyles.Bold));

        for (int i = 0; i < _familiarBattleGroups.Count; i++)
        {
            FamiliarBattleGroupData group = _familiarBattleGroups[i];
            bool isActive = group.Name.Equals(_familiarActiveBattleGroup, StringComparison.OrdinalIgnoreCase);
            FontStyles style = isActive ? FontStyles.Bold : FontStyles.Normal;
            string suffix = isActive ? " (Active)" : string.Empty;
            list.Add(new BloodcraftEntry($"{i + 1} | {group.Name}{suffix}", style,
                command: $".fam cbg {group.Name}", enabled: true));
        }

        FamiliarBattleGroupData activeGroup = FindBattleGroup(_familiarActiveBattleGroup) ?? _familiarBattleGroups[0];
        list.Add(new BloodcraftEntry("Slots", FontStyles.Bold));

        for (int i = 0; i < activeGroup.Slots.Count; i++)
        {
            FamiliarBattleSlotData slot = activeGroup.Slots[i];
            string slotText = FormatFamiliarSlot(slot, i + 1);
            list.Add(new BloodcraftEntry(slotText, FontStyles.Normal, command: $".fam sbg {i + 1}", enabled: true));
        }
    }

    /// <summary>
    /// Updates the selection state for Bloodcraft sub-tabs.
    /// </summary>
    static void UpdateSubTabSelection()
    {
        if (subTabButtons.Count == 0)
        {
            return;
        }

        for (int i = 0; i < subTabButtons.Count; i++)
        {
            SimpleStunButton button = subTabButtons[i];
            if (button == null)
            {
                continue;
            }

            BloodcraftTab tab = i < BloodcraftTabOrder.Count ? BloodcraftTabOrder[i] : BloodcraftTab.Prestige;
            bool isActive = tab == activeTab;
            button.ForceSelect = isActive;
            button.ForceHighlight = isActive;

            if (i < subTabLabels.Count && subTabLabels[i] != null)
            {
                subTabLabels[i].fontStyle = FontStyles.Normal;
            }
        }

        if (!subTabDiagnosticsLogged && DebugService.BloodcraftDebugEnabled)
        {
            DebugService.DumpBloodcraftSubTabs("selection");
            subTabDiagnosticsLogged = true;
        }
    }

    /// <summary>
    /// Resolves the section title for the active Bloodcraft tab.
    /// </summary>
    /// <param name="tab">The active tab.</param>
    /// <returns>The section title.</returns>
    static string ResolveSectionTitle(BloodcraftTab tab)
    {
        if (BloodcraftSectionTitles.TryGetValue(tab, out string title))
        {
            return title;
        }

        return BloodcraftTabLabels.TryGetValue(tab, out string label) ? label : tab.ToString();
    }

    /// <summary>
    /// Updates the section header based on the active Bloodcraft tab.
    /// </summary>
    static void UpdateSectionHeader()
    {
        if (headerText == null)
        {
            return;
        }

        string title = ResolveSectionTitle(activeTab);
        
        // User requested removal of sub-tab headers for all Bloodcraft tabs
        title = string.Empty;

        headerText.text = title;
        headerText.gameObject.SetActive(!string.IsNullOrWhiteSpace(title));

        if (headerDivider != null)
        {
            headerDivider.gameObject.SetActive(!string.IsNullOrWhiteSpace(title));
        }

        if (subHeaderText != null)
        {
            subHeaderText.text = string.Empty;
            subHeaderText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the profession panel UI.
    /// </summary>
    static void UpdateProfessionPanel()
    {
        if (professionsRoot == null || professionsListRoot == null)
        {
            return;
        }

        if (!_professionBars)
        {
            if (professionsStatusText != null)
            {
                professionsStatusText.text = "Profession UI disabled.";
                professionsStatusText.gameObject.SetActive(true);
            }

            professionsListRoot.gameObject.SetActive(false);
            if (professionsSummaryText != null)
            {
                professionsSummaryText.text = string.Empty;
            }
            return;
        }

        if (professionsStatusText != null)
        {
            professionsStatusText.text = string.Empty;
            professionsStatusText.gameObject.SetActive(false);
        }

        professionsListRoot.gameObject.SetActive(true);

        List<ProfessionEntry> entries = [..GetProfessionEntries()];
        EnsureProfessionRows(entries.Count);

        int rowCount = Math.Min(entries.Count, professionRows.Count);
        for (int i = 0; i < rowCount; i++)
        {
            UpdateProfessionRow(professionRows[i], entries[i]);
        }

        if (professionsSummaryText != null)
        {
            professionsSummaryText.text = BuildProfessionSummaryText(entries);
        }
    }

    /// <summary>
    /// Updates the stat bonuses panel UI.
    /// </summary>
    static void UpdateStatBonusesPanel()
    {
        if (statBonusesRoot == null || statBonusesListRoot == null)
        {
            return;
        }

        if (DataService._statBonusDataReady && DataService._weaponStatBonusData != null)
        {
            var data = DataService._weaponStatBonusData;

            if (statBonusesWeaponText != null)
            {
                statBonusesWeaponText.text = data.WeaponType;
            }

            /*
            if (statBonusesWeaponImage != null)
            {
                // Try set icon from equipped weapon if it matches
                // Commented out due to build errors finding ItemIcon/Entity types correctly in this context
                // var weaponEntity = Core.LocalCharacter.Read<Equipment>().WeaponSlot.SlotEntity._Entity;
                // if (weaponEntity != Entity.Null && weaponEntity.Has<ItemIcon>())
                // {
                //    statBonusesWeaponImage.sprite = weaponEntity.Read<ItemIcon>().Icon;
                //    statBonusesWeaponImage.color = Color.white;
                // }
            }
            */

            if (statBonusesCountText != null)
            {
                statBonusesCountText.text = $"{data.SelectedStats.Count} / {data.MaxStatChoices} Bonuses Selected";
            }

            List<StatBonusEntry> entries = [];
            foreach (var stat in data.AvailableStats)
            {
                entries.Add(new StatBonusEntry(
                    stat.StatIndex,
                    stat.StatName,
                    stat.Value,
                    stat.IsSelected
                ));
            }

            EnsureStatBonusRows(entries.Count);

            int rowCount = Math.Min(entries.Count, statBonusRows.Count);
            Action<int> onClicked = (index) =>
            {
                Quips.SendCommand($".wep cst {data.WeaponType} {index}");
            };

            for (int i = 0; i < rowCount; i++)
            {
                UpdateStatBonusRow(statBonusRows[i], entries[i], onClicked);
            }
        }
        else
        {
            // Fallback to mock data for development/layout testing
            if (statBonusesWeaponText != null)
            {
                statBonusesWeaponText.text = "Mock Sword";
            }

            if (statBonusesCountText != null)
            {
                statBonusesCountText.text = "0 / 3 Bonuses Selected";
            }

            List<StatBonusEntry> mockEntries =
            [
                new StatBonusEntry(0, "Max Health", 250f, false),
                new StatBonusEntry(1, "Movement Speed", 0.15f, false),
                new StatBonusEntry(2, "Primary Attack Speed", 0.12f, false),
                new StatBonusEntry(3, "Physical Life Leech", 0.05f, false),
                new StatBonusEntry(4, "Spell Life Leech", 0.05f, false),
                new StatBonusEntry(5, "Primary Life Leech", 0.05f, false),
                new StatBonusEntry(6, "Physical Power", 15f, false),
                new StatBonusEntry(7, "Spell Power", 15f, false),
                new StatBonusEntry(8, "Physical Crit Chance", 0.08f, false),
                new StatBonusEntry(9, "Physical Crit Damage", 0.25f, false),
                new StatBonusEntry(10, "Spell Crit Chance", 0.08f, false),
                new StatBonusEntry(11, "Spell Crit Damage", 0.25f, false)
            ];

            EnsureStatBonusRows(mockEntries.Count);

            Action<int> onMockClicked = (statIndex) =>
            {
                Core.Log.LogInfo($"[Mock] Clicked stat index: {statIndex}");
            };

            int rowCount = Math.Min(mockEntries.Count, statBonusRows.Count);
            for (int i = 0; i < rowCount; i++)
            {
                UpdateStatBonusRow(statBonusRows[i], mockEntries[i], onMockClicked);
            }
        }
    }

    /// <summary>
    /// Ensures the stat bonus row list has the requested count.
    /// </summary>
    /// <param name="count">The number of rows required.</param>
    static void EnsureStatBonusRows(int count)
    {
        if (statBonusesListRoot == null)
        {
            return;
        }

        while (statBonusRows.Count < count)
        {
            StatBonusRow row = CreateStatBonusRow(statBonusesListRoot);
            if (row?.Root == null)
            {
                break;
            }
            statBonusRows.Add(row);
        }

        for (int i = 0; i < statBonusRows.Count; i++)
        {
            bool isActive = i < count;
            statBonusRows[i].Root.SetActive(isActive);
        }
    }

    /// <summary>
    /// Creates a stat bonus row UI element.
    /// </summary>
    /// <param name="parent">The parent transform.</param>
    /// <returns>The created row.</returns>
    static StatBonusRow CreateStatBonusRow(Transform parent)
    {
        if (parent == null)
        {
            return null;
        }

        RectTransform rectTransform = CreateRectTransformObject($"StatBonusRow_{statBonusRows.Count + 1}", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // Add background image
        Image bgImage = rectTransform.gameObject.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.25f);
        bgImage.raycastTarget = true;

        HorizontalLayoutGroup layout = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 10f;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.padding = CreatePadding(12, 12, 6, 6);

        LayoutElement rowLayout = rectTransform.gameObject.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 32f;
        rowLayout.minHeight = 32f;

        // Checkbox
        RectTransform checkRect = CreateRectTransformObject("CheckBox", rectTransform);
        Image checkImage = null;
        if (checkRect != null)
        {
            checkRect.sizeDelta = new Vector2(20f, 20f);
            checkImage = checkRect.gameObject.AddComponent<Image>();
            checkImage.color = new Color(0f, 0f, 0f, 0.4f);
            LayoutElement checkLayout = checkRect.gameObject.AddComponent<LayoutElement>();
            checkLayout.preferredWidth = 20f;
            checkLayout.preferredHeight = 20f;
        }

        // Stat name text
        TextMeshProUGUI nameText = CreateSimpleText(rectTransform, "Stat Name", 14f);
        if (nameText != null)
        {
            nameText.alignment = TextAlignmentOptions.Left;
            LayoutElement nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
            nameLayout.flexibleWidth = 1f;
        }

        // Stat value text
        TextMeshProUGUI valueText = CreateSimpleText(rectTransform, "+0%", 14f);
        if (valueText != null)
        {
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.color = new Color(0.31f, 0.82f, 0.33f, 1f); // Green
            LayoutElement valueLayout = valueText.gameObject.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 60f;
        }

        // Add button component for interaction
        SimpleStunButton button = rectTransform.gameObject.AddComponent<SimpleStunButton>();

        return new StatBonusRow(rectTransform.gameObject, checkImage, nameText, valueText, button);
    }

    /// <summary>
    /// Updates a single stat bonus row with data.
    /// </summary>
    /// <param name="row">The row to update.</param>
    /// <param name="entry">The stat data.</param>
    /// <param name="onClicked">Action to invoke on click.</param>
    static void UpdateStatBonusRow(StatBonusRow row, StatBonusEntry entry, Action<int> onClicked = null)
    {
        if (row == null)
        {
            return;
        }

        row.StatIndex = entry.StatIndex;
        row.IsSelected = entry.IsSelected;

        if (row.NameText != null)
        {
            row.NameText.text = entry.StatName;
            row.NameText.alpha = entry.IsSelected ? 1f : 0.6f;
        }

        if (row.ValueText != null)
        {
            row.ValueText.text = FormatStatValue(entry.StatIndex, entry.Value);
            row.ValueText.alpha = entry.IsSelected ? 1f : 0.6f;
        }

        if (row.CheckBox != null)
        {
            // Show checkmark when selected
            row.CheckBox.color = entry.IsSelected 
                ? new Color(0.31f, 0.82f, 0.33f, 1f)  // Green when selected
                : new Color(0f, 0f, 0f, 0.4f);        // Dark when not selected
        }

        // Update background color
        Image bgImage = row.Root.GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = entry.IsSelected 
                ? new Color(0.5f, 0.05f, 0.06f, 0.3f)  // Red tint when selected
                : new Color(0f, 0f, 0f, 0.25f);        // Default dark
        }

        if (row.Button != null)
        {
            row.Button.onClick.RemoveAllListeners();
            if (onClicked != null)
            {
                 row.Button.onClick.AddListener((UnityAction)(() => onClicked(entry.StatIndex)));
            }
        }
    }

    /// <summary>
    /// Formats a stat value for display.
    /// </summary>
    /// <param name="statIndex">The stat type index.</param>
    /// <param name="value">The stat value.</param>
    /// <returns>The formatted value string.</returns>
    static string FormatStatValue(int statIndex, float value)
    {
        // Stats that use percentage format (indices 2-5, 8-11)
        bool isPercentage = statIndex >= 2 && statIndex <= 5 || statIndex >= 8 && statIndex <= 11;
        // Stats that use decimal format (index 1 = movement speed)
        bool isDecimal = statIndex == 1;

        if (isPercentage)
        {
            return $"+{value * 100:0.#}%";
        }
        else if (isDecimal)
        {
            return $"+{value:0.##}";
        }
        else
        {
            return $"+{value:0}";
        }
    }

    /// <summary>
    /// Creates a styled text element from a reference.
    /// </summary>
    static TextMeshProUGUI CreateText(Transform parent, TextMeshProUGUI reference, string content, float fontSize, TextAlignmentOptions alignment)
    {
        RectTransform rectTransform = CreateRectTransformObject("Text", parent);
        if (rectTransform == null) return null;

        TextMeshProUGUI text = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        CopyTextStyle(reference, text);
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        
        return text;
    }

    /// <summary>
    /// Creates a simple text element using the global entry style.
    /// </summary>
    static TextMeshProUGUI CreateSimpleText(Transform parent, string text, float fontSize)
    {
        RectTransform rectTransform = CreateRectTransformObject("Text", parent);
        if (rectTransform == null)
        {
            return null;
        }

        TextMeshProUGUI tmpText = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        if (entryStyle != null)
        {
            CopyTextStyle(entryStyle, tmpText);
        }
        
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.color = Color.white;
        tmpText.raycastTarget = false;

        return tmpText;
    }

    /// <summary>
    /// Ensures the profession row list has the requested count.
    /// </summary>
    /// <param name="count">The number of rows required.</param>
    static void EnsureProfessionRows(int count)
    {
        if (professionsListRoot == null)
        {
            return;
        }

        while (professionRows.Count < count)
        {
            ProfessionRow row = CreateProfessionRow(professionsListRoot);
            if (row == null)
            {
                break;
            }

            professionRows.Add(row);
        }

        for (int i = 0; i < professionRows.Count; i++)
        {
            bool isActive = i < count;
            professionRows[i].Root.SetActive(isActive);
        }
    }

    /// <summary>
    /// Creates a profession row entry with icon, labels, and progress bar.
    /// </summary>
    /// <param name="parent">The parent transform to attach the row.</param>
    /// <returns>The created row.</returns>
    static ProfessionRow CreateProfessionRow(Transform parent)
    {
        TextMeshProUGUI reference = entryStyle ?? headerText;
        if (reference == null)
        {
            return null;
        }
        RectTransform rectTransform = CreateRectTransformObject($"ProfessionRow_{professionRows.Count + 1}", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image background = rectTransform.gameObject.AddComponent<Image>();
        background.sprite = ResolveProgressBackgroundSprite();
        background.color = new Color(0f, 0f, 0f, 0.35f);
        background.type = Image.Type.Sliced;
        background.raycastTarget = false;

        HorizontalLayoutGroup layout = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 12f;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        LayoutElement rowLayout = rectTransform.gameObject.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = ProfessionRowHeight;
        rowLayout.minHeight = ProfessionRowHeight;

        Image icon = CreateProfessionIcon(rectTransform);
        TextMeshProUGUI nameText = CreateRowLabel(rectTransform, reference, ProfessionNameWidth, TextAlignmentOptions.Left);
        TextMeshProUGUI levelText = CreateRowLabel(rectTransform, reference, ProfessionLevelWidth, TextAlignmentOptions.Center);
        Image progressFill = CreateProgressBar(rectTransform, out Image progressBackground);
        TextMeshProUGUI progressText = CreateRowLabel(rectTransform, reference, ProfessionPercentWidth, TextAlignmentOptions.Right);

        return new ProfessionRow(rectTransform.gameObject, icon, nameText, levelText, progressFill, progressBackground, progressText);
    }

    /// <summary>
    /// Updates a profession row with live data.
    /// </summary>
    /// <param name="row">The row to update.</param>
    /// <param name="entry">The profession data.</param>
    static void UpdateProfessionRow(ProfessionRow row, ProfessionEntry entry)
    {
        if (row == null)
        {
            return;
        }

        Sprite iconSprite = ResolveProfessionIcon(entry.Profession);
        if (row.Icon != null)
        {
            if (iconSprite != null)
            {
                row.Icon.sprite = iconSprite;
                row.Icon.enabled = true;
            }
            else
            {
                row.Icon.enabled = false;
            }
        }

        string name = SplitPascalCase(entry.Profession.ToString());
        if (row.NameText != null)
        {
            row.NameText.text = name;
        }

        if (row.LevelText != null)
        {
            row.LevelText.text = entry.Level.ToString(CultureInfo.InvariantCulture);
        }

        float progress = Math.Clamp(entry.Progress, 0f, 1f);
        if (row.ProgressFill != null)
        {
            row.ProgressFill.fillAmount = progress;
        }

        if (row.ProgressText != null)
        {
            row.ProgressText.text = $"{progress * 100f:0}%";
        }

        if (ProfessionColors.TryGetValue(entry.Profession, out Color color))
        {
            Color textColor = new(color.r, color.g, color.b, 1f);
            if (row.NameText != null)
            {
                row.NameText.color = textColor;
            }

            if (row.ProgressFill != null)
            {
                row.ProgressFill.color = new Color(color.r, color.g, color.b, 0.9f);
            }

            if (row.ProgressBackground != null)
            {
                row.ProgressBackground.color = new Color(color.r * 0.2f, color.g * 0.2f, color.b * 0.2f, 0.6f);
            }
        }

        if (row.LevelText != null)
        {
            row.LevelText.color = Color.white;
        }

        if (row.ProgressText != null)
        {
            row.ProgressText.color = new Color(1f, 1f, 1f, 0.85f);
        }
    }

    /// <summary>
    /// Builds the summary text for the profession panel.
    /// </summary>
    /// <param name="entries">The profession entries.</param>
    /// <returns>The summary text.</returns>
    static string BuildProfessionSummaryText(List<ProfessionEntry> entries)
    {
        if (entries.Count == 0)
        {
            return "No profession data available.";
        }

        int totalLevel = 0;
        float totalProgress = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            totalLevel += entries[i].Level;
            totalProgress += entries[i].Progress;
        }

        float averageProgress = totalProgress / entries.Count;
        return $"Total Level: {totalLevel}   Avg Progress: {averageProgress * 100f:0}%";
    }

    /// <summary>
    /// Creates the profession icon image.
    /// </summary>
    /// <param name="parent">The parent transform to attach the icon.</param>
    /// <returns>The image component.</returns>
    static Image CreateProfessionIcon(Transform parent)
    {
        RectTransform rectTransform = CreateRectTransformObject("ProfessionIcon", parent);
        if (rectTransform == null)
        {
            return null;
        }
        rectTransform.sizeDelta = new Vector2(ProfessionIconSize, ProfessionIconSize);

        Image icon = rectTransform.gameObject.AddComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        LayoutElement layout = rectTransform.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = ProfessionIconSize;
        layout.minWidth = ProfessionIconSize;
        layout.preferredHeight = ProfessionIconSize;
        layout.minHeight = ProfessionIconSize;

        return icon;
    }

    /// <summary>
    /// Creates a row label with fixed width.
    /// </summary>
    /// <param name="parent">The parent transform to attach the label.</param>
    /// <param name="reference">Reference text for styling.</param>
    /// <param name="width">Preferred width.</param>
    /// <param name="alignment">Text alignment.</param>
    /// <returns>The label component.</returns>
    static TextMeshProUGUI CreateRowLabel(Transform parent, TextMeshProUGUI reference, float width, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI label = CreateTextElement(parent, "RowLabel", reference, ProfessionFontScale, FontStyles.Normal);
        if (label == null)
        {
            return null;
        }
        label.alignment = alignment;
        label.text = string.Empty;

        LayoutElement layout = label.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.preferredHeight = ProfessionRowHeight;

        return label;
    }

    /// <summary>
    /// Creates a progress bar and returns the fill image.
    /// </summary>
    /// <param name="parent">The parent transform to attach the bar.</param>
    /// <param name="background">The progress bar background image.</param>
    /// <returns>The fill image component.</returns>
    static Image CreateProgressBar(Transform parent, out Image background)
    {
        RectTransform rectTransform = CreateRectTransformObject("ProgressBar", parent);
        if (rectTransform == null)
        {
            background = null;
            return null;
        }
        rectTransform.sizeDelta = new Vector2(ProfessionProgressWidth, ProfessionProgressHeight);

        background = rectTransform.gameObject.AddComponent<Image>();
        background.sprite = ResolveProgressBackgroundSprite();
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        background.type = Image.Type.Sliced;
        background.raycastTarget = false;

        LayoutElement layout = rectTransform.gameObject.AddComponent<LayoutElement>();
        layout.minWidth = ProfessionProgressWidth;
        layout.preferredWidth = 0f;
        layout.flexibleWidth = 1f;
        layout.preferredHeight = ProfessionProgressHeight;
        layout.minHeight = ProfessionProgressHeight;

        RectTransform fillRect = CreateRectTransformObject("Fill", rectTransform);
        if (fillRect == null)
        {
            return null;
        }
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(1f, 1f);
        fillRect.offsetMax = new Vector2(-1f, -1f);

        Image fill = fillRect.gameObject.AddComponent<Image>();
        fill.sprite = ResolveProgressFillSprite();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0f;
        fill.raycastTarget = false;

        return fill;
    }

    /// <summary>
    /// Cycles the selected prestige leaderboard type.
    /// </summary>
    static void CyclePrestigeType()
    {
        if (_prestigeLeaderboardOrder.Count == 0)
        {
            return;
        }

        _prestigeLeaderboardIndex = (_prestigeLeaderboardIndex + 1) % _prestigeLeaderboardOrder.Count;
    }

    /// <summary>
    /// Resolves the active exoform entry.
    /// </summary>
    /// <returns>The active exoform entry or null if none is available.</returns>
    static ExoFormEntry ResolveActiveExoForm()
    {
        if (_exoFormEntries.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_exoFormCurrentForm))
        {
            ExoFormEntry match = _exoFormEntries.Find(entry =>
                entry.FormName.Equals(_exoFormCurrentForm, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return match;
            }
        }

        return _exoFormEntries[0];
    }

    /// <summary>
    /// Resolves a display name for an ability identifier.
    /// </summary>
    /// <param name="abilityId">The ability prefab GUID hash.</param>
    /// <returns>The display-friendly ability name.</returns>
    static string ResolveAbilityName(int abilityId)
    {
        PrefabGUID abilityGuid = new(abilityId);
        string abilityName = abilityGuid.GetLocalizedName();
        if (string.IsNullOrEmpty(abilityName) || abilityName.Equals("LocalizationKey.Empty"))
        {
            abilityName = abilityGuid.GetPrefabName();
        }

        if (string.IsNullOrEmpty(abilityName))
        {
            return $"Ability {abilityId}";
        }

        Match match = AbilitySpellRegex.Match(abilityName);
        if (match.Success)
        {
            return match.Value.Replace('_', ' ');
        }

        return abilityName;
    }

    /// <summary>
    /// Finds a familiar battle group by name.
    /// </summary>
    /// <param name="groupName">The group name to match.</param>
    /// <returns>The battle group or null if not found.</returns>
    static FamiliarBattleGroupData FindBattleGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return null;
        }

        return _familiarBattleGroups.Find(group =>
            group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Formats a familiar battle slot display string.
    /// </summary>
    /// <param name="slot">The slot data.</param>
    /// <param name="slotIndex">The 1-based slot index.</param>
    /// <returns>A formatted slot string.</returns>
    static string FormatFamiliarSlot(FamiliarBattleSlotData slot, int slotIndex)
    {
        if (slot.Id == 0)
        {
            return $"Slot {slotIndex}: <color=grey>Empty</color>";
        }

        string name = string.IsNullOrWhiteSpace(slot.Name) ? $"Familiar {slot.Id}" : slot.Name;
        string prestige = slot.Prestige > 0 ? $" P{slot.Prestige}" : string.Empty;
        return $"Slot {slotIndex}: {name} (Lv {slot.Level}{prestige})";
    }

    /// <summary>
    /// Returns the profession entries for display.
    /// </summary>
    /// <returns>Ordered profession entries.</returns>
    static IEnumerable<ProfessionEntry> GetProfessionEntries()
    {
        return new[]
        {
            new ProfessionEntry(Profession.Enchanting, _enchantingLevel, _enchantingProgress),
            new ProfessionEntry(Profession.Alchemy, _alchemyLevel, _alchemyProgress),
            new ProfessionEntry(Profession.Harvesting, _harvestingLevel, _harvestingProgress),
            new ProfessionEntry(Profession.Blacksmithing, _blacksmithingLevel, _blacksmithingProgress),
            new ProfessionEntry(Profession.Tailoring, _tailoringLevel, _tailoringProgress),
            new ProfessionEntry(Profession.Woodcutting, _woodcuttingLevel, _woodcuttingProgress),
            new ProfessionEntry(Profession.Mining, _miningLevel, _miningProgress),
            new ProfessionEntry(Profession.Fishing, _fishingLevel, _fishingProgress)
        };
    }

    /// <summary>
    /// Ensures the entry list has enough UI objects for the requested count.
    /// </summary>
    /// <param name="count">The number of entries required.</param>
    static void EnsureEntries(int count)
    {
        if (entriesRoot == null || entryTemplate == null)
        {
            return;
        }

        while (entries.Count < count)
        {
            int index = entries.Count;
            GameObject entryObject = UnityEngine.Object.Instantiate(entryTemplate, entriesRoot, false);
            entryObject.name = $"BloodcraftEntry_{index + 1}";
            entryObject.SetActive(true);

            TextMeshProUGUI text = entryObject.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                UnityEngine.Object.Destroy(entryObject);
                return;
            }

            SimpleStunButton button = entryObject.GetComponent<SimpleStunButton>() ?? entryObject.AddComponent<SimpleStunButton>();

            entries.Add(text);
            entryButtons.Add(button);
        }

        for (int i = 0; i < entries.Count; i++)
        {
            bool isActive = i < count;
            entries[i].gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// Configures a button to send a chat command when clicked.
    /// </summary>
    /// <param name="button">The button to configure.</param>
    /// <param name="command">The command to send.</param>
    /// <param name="enabled">Whether the command is enabled.</param>
    static void ConfigureCommandButton(SimpleStunButton button, string command, bool enabled)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();

        if (!enabled || string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        button.onClick.AddListener((UnityAction)(() => Quips.SendCommand(command)));
    }

    /// <summary>
    /// Configures a button to execute a local action when clicked.
    /// </summary>
    /// <param name="button">The button to configure.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="enabled">Whether the action is enabled.</param>
    static void ConfigureActionButton(SimpleStunButton button, Action action, bool enabled)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();

        if (!enabled || action == null)
        {
            return;
        }

        button.onClick.AddListener((UnityAction)(() => action()));
    }

    /// <summary>
    /// Represents a single line entry in the Bloodcraft tab.
    /// </summary>
    readonly struct BloodcraftEntry
    {
        public string Text { get; }
        public string Command { get; }
        public Action Action { get; }
        public bool Enabled { get; }
        public FontStyles Style { get; }

        /// <summary>
        /// Initializes a new Bloodcraft entry.
        /// </summary>
        /// <param name="text">The display text for the entry.</param>
        /// <param name="style">The font style to apply.</param>
        /// <param name="command">An optional chat command to send when clicked.</param>
        /// <param name="action">An optional local action to invoke when clicked.</param>
        /// <param name="enabled">Whether the entry is clickable.</param>
        public BloodcraftEntry(string text, FontStyles style, string command = "", Action action = null, bool enabled = false)
        {
            Text = text;
            Command = command;
            Action = action;
            Enabled = enabled;
            Style = style;
        }
    }

    /// <summary>
    /// Holds UI references for a profession row entry.
    /// </summary>
    sealed class ProfessionRow
    {
        public GameObject Root { get; }
        public Image Icon { get; }
        public TextMeshProUGUI NameText { get; }
        public TextMeshProUGUI LevelText { get; }
        public Image ProgressFill { get; }
        public Image ProgressBackground { get; }
        public TextMeshProUGUI ProgressText { get; }

        public ProfessionRow(GameObject root, Image icon, TextMeshProUGUI nameText, TextMeshProUGUI levelText,
            Image progressFill, Image progressBackground, TextMeshProUGUI progressText)
        {
            Root = root;
            Icon = icon;
            NameText = nameText;
            LevelText = levelText;
            ProgressFill = progressFill;
            ProgressBackground = progressBackground;
            ProgressText = progressText;
        }
    }

    /// <summary>
    /// Holds profession data for the Bloodcraft tab.
    /// </summary>
    readonly struct ProfessionEntry
    {
        public Profession Profession { get; }
        public int Level { get; }
        public float Progress { get; }

        public ProfessionEntry(Profession profession, int level, float progress)
        {
            Profession = profession;
            Level = level;
            Progress = progress;
        }
    }

    /// <summary>
    /// Holds UI references for a stat bonus row entry.
    /// </summary>
    sealed class StatBonusRow
    {
        public GameObject Root { get; }
        public Image CheckBox { get; }
        public TextMeshProUGUI NameText { get; }
        public TextMeshProUGUI ValueText { get; }
        public SimpleStunButton Button { get; }
        public int StatIndex { get; set; }
        public bool IsSelected { get; set; }

        public StatBonusRow(GameObject root, Image checkBox, TextMeshProUGUI nameText,
            TextMeshProUGUI valueText, SimpleStunButton button)
        {
            Root = root;
            CheckBox = checkBox;
            NameText = nameText;
            ValueText = valueText;
            Button = button;
            StatIndex = -1;
            IsSelected = false;
        }
    }

    /// <summary>
    /// Holds stat bonus data for the Bloodcraft tab.
    /// </summary>
    readonly struct StatBonusEntry
    {
        public int StatIndex { get; }
        public string StatName { get; }
        public float Value { get; }
        public bool IsSelected { get; }

        public StatBonusEntry(int statIndex, string statName, float value, bool isSelected)
        {
            StatIndex = statIndex;
            StatName = statName;
            Value = value;
            IsSelected = isSelected;
        }
    }
}
