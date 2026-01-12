using Eclipse.Services.CharacterMenu.Base;
using Eclipse.Services.CharacterMenu.Interfaces;
using Eclipse.Services.CharacterMenu.Shared;
using Eclipse.Services.HUD.Shared;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Eclipse.Services.CanvasService.DataHUD;
using static Eclipse.Services.DataService;

namespace Eclipse.Services.CharacterMenu.Tabs;

/// <summary>
/// Character menu tab for displaying class selection, spells, and stat synergies.
/// </summary>
internal class ClassTab : CharacterMenuTabBase, ICharacterMenuTabWithPanel
{
    #region Constants

    private const float HeaderBackgroundAlpha = 0.95f;
    private const float RowBackgroundAlpha = 0.35f;
    private const float RowSpacing = 4f;
    private const float RowHorizontalSpacing = 10f;
    private const int RowPaddingHorizontal = 12;
    private const int RowPaddingVertical = 8;
    private const float DividerOpacity = 0.7f;
    private const float ColumnSpacing = 12f;
    private const float CardSpacing = 8f;

    private const float HeaderFontScale = UIFactory.ProfessionHeaderFontScale;
    private const float RowFontScale = UIFactory.ProfessionFontScale;
    private const float SmallFontScale = 0.7f;
    private const float ChipFontScale = 0.6f;

    private const float RowHeight = 32f;
    private const float SpellRowHeight = 28f;
    private const float ClassIconSize = 12f;
    private const float SpellIndexSize = 18f;
    private const float ChipPadding = 4f;

    private static readonly Color HeaderBackgroundColor = new(0.1f, 0.1f, 0.12f, HeaderBackgroundAlpha);
    private static readonly Color RowBackgroundColor = new(0f, 0f, 0f, RowBackgroundAlpha);
    private static readonly Color ActiveRowBorderColor = new(0.5f, 0.05f, 0.06f, 0.7f);
    private static readonly Color ActiveStatusColor = new(0.62f, 0.95f, 0.71f, 1f); // #9ef2b5
    private static readonly Color LockedColor = new(0.82f, 0.35f, 0.35f, 0.8f);

    private static readonly string[] HeaderSpriteNames = ["Act_BG", "TabGradient", "Window_Box_Background"];
    private static readonly string[] CardSpriteNames = ["Window_Box", "Window_Box_Background"];
    private static readonly string[] DividerSpriteNames = ["Divider_Horizontal", "Window_Divider_Horizontal_V_Red"];

    // Class colors from Bloodcraft
    private static readonly Dictionary<PlayerClass, Color> ClassColors = new()
    {
        { PlayerClass.None, Color.gray },
        { PlayerClass.BloodKnight, new Color(1f, 0f, 0f, 1f) },           // #ff0000
        { PlayerClass.DemonHunter, new Color(1f, 0.8f, 0f, 1f) },         // #ffcc00
        { PlayerClass.VampireLord, new Color(0f, 1f, 1f, 1f) },           // #00ffff
        { PlayerClass.ShadowBlade, new Color(0.6f, 0.2f, 1f, 1f) },       // #9933ff
        { PlayerClass.ArcaneSorcerer, new Color(0f, 0.5f, 0.5f, 1f) },    // #008080
        { PlayerClass.DeathMage, new Color(0f, 1f, 0f, 1f) }              // #00ff00
    };

    // Class on-hit effect descriptions
    private static readonly Dictionary<PlayerClass, string> ClassOnHitEffects = new()
    {
        { PlayerClass.None, "" },
        { PlayerClass.BloodKnight, "Leech → Blood Curse" },
        { PlayerClass.DemonHunter, "Static → Storm Charge" },
        { PlayerClass.VampireLord, "Chill → Frost Weapon" },
        { PlayerClass.ShadowBlade, "Ignite → Chaos Heated" },
        { PlayerClass.ArcaneSorcerer, "Weaken → Illusion Shield" },
        { PlayerClass.DeathMage, "Condemn → Unholy Amplify" }
    };

    #endregion

    #region Fields

    private RectTransform _panelRoot;
    private TextMeshProUGUI _referenceText;
    
    // Class Selection
    private Transform _classSelectionCard;
    private TextMeshProUGUI _currentClassText;
    private TextMeshProUGUI _onHitEffectText;
    private readonly List<ClassRowUI> _classRows = [];
    
    // Class Spells
    private Transform _classSpellsCard;
    private TextMeshProUGUI _shiftSlotText;
    private readonly List<SpellRowUI> _spellRows = [];
    
    // Stat Synergies
    private Transform _statSynergiesCard;
    private TextMeshProUGUI _weaponSynergyLabel;
    private Transform _weaponSynergyChips;
    private TextMeshProUGUI _bloodSynergyLabel;
    private Transform _bloodSynergyChips;

    #endregion

    #region Properties

    public override string TabId => "Class";
    public override string TabLabel => "Class";
    public override string SectionTitle => "Class System";
    public override BloodcraftTab TabType => BloodcraftTab.Progression;

    #endregion

    #region ICharacterMenuTabWithPanel

    public Transform CreatePanel(Transform parent, TextMeshProUGUI reference)
    {
        Reset();
        _referenceText = reference;

        RectTransform rectTransform = CreateRectTransformObject("BloodcraftClass", parent);
        if (rectTransform == null)
        {
            return null;
        }

        UIFactory.ConfigureTopLeftAnchoring(rectTransform);

        // Create horizontal layout for two columns
        HorizontalLayoutGroup hLayout = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.UpperLeft;
        hLayout.spacing = ColumnSpacing;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = false;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;

        // Left Column: Class Selection
        Transform leftColumn = CreateColumn(rectTransform, "LeftColumn");
        _classSelectionCard = CreateClassSelectionCard(leftColumn, reference);

        // Right Column: Class Spells + Stat Synergies
        Transform rightColumn = CreateColumn(rectTransform, "RightColumn");
        _classSpellsCard = CreateClassSpellsCard(rightColumn, reference);
        _statSynergiesCard = CreateStatSynergiesCard(rightColumn, reference);

        rectTransform.gameObject.SetActive(false);
        _panelRoot = rectTransform;
        return rectTransform;
    }

    public void UpdatePanel()
    {
        if (_panelRoot == null)
        {
            return;
        }

        UpdateClassSelection();
        UpdateClassSpells();
        UpdateStatSynergies();
    }

    #endregion

    #region Lifecycle

    public override void Update()
    {
        UpdatePanel();
    }

    public override void Reset()
    {
        base.Reset();
        _panelRoot = null;
        _referenceText = null;
        _classSelectionCard = null;
        _currentClassText = null;
        _onHitEffectText = null;
        _classRows.Clear();
        _classSpellsCard = null;
        _shiftSlotText = null;
        _spellRows.Clear();
        _statSynergiesCard = null;
        _weaponSynergyLabel = null;
        _weaponSynergyChips = null;
        _bloodSynergyLabel = null;
        _bloodSynergyChips = null;
    }

    #endregion

    #region Private Methods - Layout

    private static Transform CreateColumn(Transform parent, string name)
    {
        RectTransform rectTransform = CreateRectTransformObject(name, parent);
        if (rectTransform == null)
        {
            return null;
        }

        UIFactory.ConfigureTopLeftAnchoring(rectTransform);

        VerticalLayoutGroup vLayout = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        vLayout.childAlignment = TextAnchor.UpperLeft;
        vLayout.spacing = CardSpacing;
        vLayout.childForceExpandWidth = true;
        vLayout.childForceExpandHeight = false;
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;

        LayoutElement layout = rectTransform.gameObject.AddComponent<LayoutElement>();
        layout.flexibleWidth = 1f;

        return rectTransform;
    }

    private Transform CreateCard(Transform parent, string name, string headerTitle)
    {
        RectTransform rectTransform = CreateRectTransformObject(name, parent);
        if (rectTransform == null)
        {
            return null;
        }

        UIFactory.ConfigureTopLeftAnchoring(rectTransform);

        // Card background
        Image background = rectTransform.gameObject.AddComponent<Image>();
        ApplySprite(background, CardSpriteNames);
        background.color = new Color(0.04f, 0.04f, 0.05f, 0.72f);
        background.type = Image.Type.Sliced;
        background.raycastTarget = false;

        VerticalLayoutGroup vLayout = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        vLayout.childAlignment = TextAnchor.UpperLeft;
        vLayout.spacing = RowSpacing;
        vLayout.childForceExpandWidth = true;
        vLayout.childForceExpandHeight = false;
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;
        vLayout.padding = UIFactory.CreatePadding(RowPaddingHorizontal, RowPaddingHorizontal, RowPaddingVertical, RowPaddingVertical);

        ContentSizeFitter fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Card header
        CreateCardHeader(rectTransform, headerTitle);

        return rectTransform;
    }

    private void CreateCardHeader(Transform parent, string title)
    {
        RectTransform headerRect = CreateRectTransformObject("CardHeader", parent);
        if (headerRect == null)
        {
            return;
        }

        UIFactory.ConfigureTopLeftAnchoring(headerRect);

        Image headerBg = headerRect.gameObject.AddComponent<Image>();
        ApplySprite(headerBg, HeaderSpriteNames);
        headerBg.color = HeaderBackgroundColor;
        headerBg.raycastTarget = false;

        HorizontalLayoutGroup hLayout = headerRect.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.spacing = 8f;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = false;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.padding = UIFactory.CreatePadding(8, 8, 4, 4);

        LayoutElement headerLayout = headerRect.gameObject.AddComponent<LayoutElement>();
        headerLayout.preferredHeight = 28f;
        headerLayout.minHeight = 28f;

        // Icon placeholder
        RectTransform iconRect = CreateRectTransformObject("Icon", headerRect);
        if (iconRect != null)
        {
            iconRect.sizeDelta = new Vector2(18f, 18f);
            Image icon = iconRect.gameObject.AddComponent<Image>();
            icon.color = new Color(1f, 1f, 1f, 0.9f);
            icon.raycastTarget = false;

            LayoutElement iconLayout = iconRect.gameObject.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 18f;
            iconLayout.minWidth = 18f;
            iconLayout.preferredHeight = 18f;
            iconLayout.minHeight = 18f;
        }

        // Title text
        if (_referenceText != null)
        {
            TextMeshProUGUI titleText = UIFactory.CreateTextElement(headerRect, "Title", _referenceText, SmallFontScale, FontStyles.Bold);
            if (titleText != null)
            {
                titleText.text = title.ToUpperInvariant();
                titleText.color = new Color(0.9f, 0.875f, 0.835f, 1f); // #e6dfd5
            }
        }
    }

    #endregion

    #region Private Methods - Class Selection

    private Transform CreateClassSelectionCard(Transform parent, TextMeshProUGUI reference)
    {
        Transform card = CreateCard(parent, "ClassSelectionCard", "CLASS SELECTION");
        if (card == null)
        {
            return null;
        }

        // Current class label
        _currentClassText = UIFactory.CreateTextElement(card, "CurrentClass", reference, RowFontScale, FontStyles.Normal);
        if (_currentClassText != null)
        {
            _currentClassText.text = "Current: None";
            _currentClassText.color = new Color(0.95f, 0.84f, 0.7f, 1f); // #f1d7b2
        }

        // On-hit effect label
        _onHitEffectText = UIFactory.CreateTextElement(card, "OnHitEffect", reference, SmallFontScale, FontStyles.Normal);
        if (_onHitEffectText != null)
        {
            _onHitEffectText.text = "On-Hit: ";
            _onHitEffectText.color = new Color(1f, 1f, 1f, 0.55f);
        }

        // Class list container
        Transform listRoot = UIFactory.CreateListRoot(card, "ClassList", RowSpacing);

        // Create class rows
        foreach (PlayerClass playerClass in Enum.GetValues(typeof(PlayerClass)))
        {
            if (playerClass == PlayerClass.None)
            {
                continue;
            }

            ClassRowUI row = CreateClassRow(listRoot, playerClass);
            if (row != null)
            {
                _classRows.Add(row);
            }
        }

        // Hint text
        TextMeshProUGUI hintText = UIFactory.CreateTextElement(card, "Hint", reference, ChipFontScale, FontStyles.Italic);
        if (hintText != null)
        {
            hintText.text = "Click a class to select or change. Changing class may require a special item.";
            hintText.color = new Color(1f, 1f, 1f, 0.48f);
        }

        return card;
    }

    private ClassRowUI CreateClassRow(Transform parent, PlayerClass playerClass)
    {
        RectTransform rectTransform = CreateRectTransformObject($"ClassRow_{playerClass}", parent);
        if (rectTransform == null)
        {
            return null;
        }

        UIFactory.ConfigureTopLeftAnchoring(rectTransform);

        Image background = rectTransform.gameObject.AddComponent<Image>();
        ApplySprite(background, CardSpriteNames);
        background.color = RowBackgroundColor;
        background.type = Image.Type.Sliced;
        background.raycastTarget = true;

        HorizontalLayoutGroup hLayout = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.spacing = RowHorizontalSpacing;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = false;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.padding = UIFactory.CreatePadding(RowPaddingHorizontal, RowPaddingHorizontal, 8, 8);

        LayoutElement rowLayout = rectTransform.gameObject.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = RowHeight;
        rowLayout.minHeight = RowHeight;

        // Class color icon
        RectTransform iconRect = CreateRectTransformObject("ClassIcon", rectTransform);
        Image icon = null;
        if (iconRect != null)
        {
            iconRect.sizeDelta = new Vector2(ClassIconSize, ClassIconSize);
            icon = iconRect.gameObject.AddComponent<Image>();
            icon.color = ClassColors.GetValueOrDefault(playerClass, Color.gray);
            icon.raycastTarget = false;

            // Make it circular (if needed, can use a circle sprite)
            LayoutElement iconLayout = iconRect.gameObject.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = ClassIconSize;
            iconLayout.minWidth = ClassIconSize;
            iconLayout.preferredHeight = ClassIconSize;
            iconLayout.minHeight = ClassIconSize;
        }

        // Class name
        TextMeshProUGUI nameText = null;
        if (_referenceText != null)
        {
            nameText = UIFactory.CreateTextElement(rectTransform, "ClassName", _referenceText, RowFontScale, FontStyles.Normal);
            if (nameText != null)
            {
                nameText.text = HudUtilities.SplitPascalCase(playerClass.ToString());
                nameText.color = ClassColors.GetValueOrDefault(playerClass, Color.white);

                LayoutElement nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
                nameLayout.flexibleWidth = 1f;
            }
        }

        // Status badge (will be shown for active class)
        TextMeshProUGUI statusText = null;
        if (_referenceText != null)
        {
            statusText = UIFactory.CreateTextElement(rectTransform, "Status", _referenceText, ChipFontScale, FontStyles.Bold);
            if (statusText != null)
            {
                statusText.text = "ACTIVE";
                statusText.color = ActiveStatusColor;
                statusText.gameObject.SetActive(false);

                LayoutElement statusLayout = statusText.gameObject.AddComponent<LayoutElement>();
                statusLayout.preferredWidth = 50f;
            }
        }

        return new ClassRowUI(rectTransform.gameObject, playerClass, icon, nameText, statusText, background);
    }

    private void UpdateClassSelection()
    {
        PlayerClass currentClass = _classType;

        // Update current class text
        if (_currentClassText != null)
        {
            Color classColor = ClassColors.GetValueOrDefault(currentClass, Color.white);
            string colorHex = ColorUtility.ToHtmlStringRGB(classColor);
            string className = currentClass == PlayerClass.None ? "None" : HudUtilities.SplitPascalCase(currentClass.ToString());
            _currentClassText.text = $"Current: <color=#{colorHex}>{className}</color>";
        }

        // Update on-hit effect text
        if (_onHitEffectText != null)
        {
            string effect = ClassOnHitEffects.GetValueOrDefault(currentClass, "");
            _onHitEffectText.text = string.IsNullOrEmpty(effect) ? "" : $"On-Hit: {effect}";
        }

        // Update class rows
        foreach (ClassRowUI row in _classRows)
        {
            bool isActive = row.PlayerClass == currentClass;
            
            if (row.StatusText != null)
            {
                row.StatusText.gameObject.SetActive(isActive);
            }

            if (row.Background != null)
            {
                Color bgColor = isActive 
                    ? new Color(0.5f, 0.05f, 0.06f, 0.18f) 
                    : RowBackgroundColor;
                row.Background.color = bgColor;
            }
        }
    }

    #endregion

    #region Private Methods - Class Spells

    private Transform CreateClassSpellsCard(Transform parent, TextMeshProUGUI reference)
    {
        Transform card = CreateCard(parent, "ClassSpellsCard", "CLASS SPELLS");
        if (card == null)
        {
            return null;
        }

        // Shift slot status
        _shiftSlotText = UIFactory.CreateTextElement(card, "ShiftSlot", reference, SmallFontScale, FontStyles.Normal);
        if (_shiftSlotText != null)
        {
            _shiftSlotText.text = "Shift Slot: Ready";
            _shiftSlotText.color = new Color(1f, 1f, 1f, 0.55f);
        }

        // Spell list container
        Transform listRoot = UIFactory.CreateListRoot(card, "SpellList", RowSpacing);

        // Create placeholder spell rows (will be populated from server data)
        for (int i = 0; i < 4; i++)
        {
            SpellRowUI row = CreateSpellRow(listRoot, i);
            if (row != null)
            {
                _spellRows.Add(row);
            }
        }

        // Hint text
        TextMeshProUGUI hintText = UIFactory.CreateTextElement(card, "Hint", reference, ChipFontScale, FontStyles.Italic);
        if (hintText != null)
        {
            hintText.text = "Click a spell to set it as your Shift ability. (P#) indicates prestige requirement.";
            hintText.color = new Color(1f, 1f, 1f, 0.48f);
        }

        return card;
    }

    private SpellRowUI CreateSpellRow(Transform parent, int index)
    {
        RectTransform rectTransform = CreateRectTransformObject($"SpellRow_{index}", parent);
        if (rectTransform == null)
        {
            return null;
        }

        UIFactory.ConfigureTopLeftAnchoring(rectTransform);

        Image background = rectTransform.gameObject.AddComponent<Image>();
        ApplySprite(background, CardSpriteNames);
        background.color = RowBackgroundColor;
        background.type = Image.Type.Sliced;
        background.raycastTarget = true;

        HorizontalLayoutGroup hLayout = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.spacing = RowHorizontalSpacing;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = false;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.padding = UIFactory.CreatePadding(RowPaddingHorizontal, RowPaddingHorizontal, 8, 8);

        LayoutElement rowLayout = rectTransform.gameObject.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = SpellRowHeight;
        rowLayout.minHeight = SpellRowHeight;

        // Spell index badge
        RectTransform indexRect = CreateRectTransformObject("SpellIndex", rectTransform);
        TextMeshProUGUI indexText = null;
        if (indexRect != null && _referenceText != null)
        {
            indexRect.sizeDelta = new Vector2(SpellIndexSize, SpellIndexSize);
            
            Image indexBg = indexRect.gameObject.AddComponent<Image>();
            indexBg.color = new Color(0.31f, 0.31f, 0.35f, 0.6f);
            indexBg.raycastTarget = false;

            indexText = UIFactory.CreateTextElement(indexRect, "IndexText", _referenceText, ChipFontScale, FontStyles.Normal);
            if (indexText != null)
            {
                indexText.text = index.ToString();
                indexText.alignment = TextAlignmentOptions.Center;
                indexText.color = new Color(1f, 1f, 1f, 0.7f);
            }

            LayoutElement indexLayout = indexRect.gameObject.AddComponent<LayoutElement>();
            indexLayout.preferredWidth = SpellIndexSize;
            indexLayout.minWidth = SpellIndexSize;
            indexLayout.preferredHeight = SpellIndexSize;
            indexLayout.minHeight = SpellIndexSize;
        }

        // Spell name
        TextMeshProUGUI nameText = null;
        if (_referenceText != null)
        {
            nameText = UIFactory.CreateTextElement(rectTransform, "SpellName", _referenceText, SmallFontScale, FontStyles.Normal);
            if (nameText != null)
            {
                nameText.text = $"Spell {index + 1}";
                nameText.color = new Color(0.91f, 0.89f, 0.84f, 1f);

                LayoutElement nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
                nameLayout.flexibleWidth = 1f;
            }
        }

        // Prestige requirement
        TextMeshProUGUI reqText = null;
        if (_referenceText != null)
        {
            reqText = UIFactory.CreateTextElement(rectTransform, "Requirement", _referenceText, ChipFontScale, FontStyles.Normal);
            if (reqText != null)
            {
                reqText.text = "";
                reqText.color = new Color(1f, 1f, 1f, 0.5f);

                LayoutElement reqLayout = reqText.gameObject.AddComponent<LayoutElement>();
                reqLayout.preferredWidth = 40f;
            }
        }

        return new SpellRowUI(rectTransform.gameObject, index, nameText, reqText, indexText, background);
    }

    private void UpdateClassSpells()
    {
        // Update shift slot status
        if (_shiftSlotText != null)
        {
            bool hasShift = _shiftSpellIndex >= 0;
            string statusColor = hasShift ? "#9ef2b5" : "#ffffff";
            string statusText = hasShift ? "Equipped" : _classShiftSlotEnabled ? "Ready" : "Disabled";
            _shiftSlotText.text = $"Shift Slot: <color={statusColor}>{statusText}</color>";
        }

        // Update spell rows with actual data
        PlayerClass currentClass = _classType;
        _classSpells.TryGetValue(currentClass, out List<int> classSpells);
        
        // Build spell entries (including default spell at index 0)
        List<(int Index, int SpellId)> spellEntries = [];
        if (_defaultClassSpell != 0)
        {
            spellEntries.Add((0, _defaultClassSpell));
        }
        if (classSpells != null)
        {
            for (int i = 0; i < classSpells.Count; i++)
            {
                spellEntries.Add((i + 1, classSpells[i]));
            }
        }

        // Update rows
        for (int i = 0; i < _spellRows.Count; i++)
        {
            SpellRowUI row = _spellRows[i];
            if (i < spellEntries.Count)
            {
                var (index, spellId) = spellEntries[i];
                row.Root.SetActive(true);

                // Get spell name from prefab
                PrefabGUID spellPrefab = new(spellId);
                string spellName = spellPrefab.GetLocalizedName();
                if (string.IsNullOrEmpty(spellName) || spellName.Equals("LocalizationKey.Empty"))
                {
                    spellName = spellPrefab.GetPrefabName();
                }

                if (row.NameText != null)
                {
                    row.NameText.text = spellName;
                }

                if (row.IndexText != null)
                {
                    row.IndexText.text = index.ToString();
                }

                // Get prestige requirement
                int requiredPrestige = index < _classSpellUnlockLevels.Count ? _classSpellUnlockLevels[index] : 0;
                if (row.RequirementText != null)
                {
                    if (requiredPrestige > 0)
                    {
                        row.RequirementText.text = $"(P{requiredPrestige})";
                        bool isLocked = _experiencePrestige < requiredPrestige;
                        row.RequirementText.color = isLocked ? LockedColor : new Color(1f, 1f, 1f, 0.5f);
                    }
                    else
                    {
                        row.RequirementText.text = "";
                    }
                }

                // Highlight active spell
                int activeIndex = _shiftSpellIndex >= 0 ? _shiftSpellIndex + 1 : -1;
                bool isActive = index == activeIndex;
                
                if (row.Background != null)
                {
                    row.Background.color = isActive 
                        ? new Color(0.6f, 0.2f, 0.1f, 0.1f)
                        : RowBackgroundColor;
                }

                if (row.NameText != null)
                {
                    row.NameText.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
                    row.NameText.color = isActive 
                        ? new Color(0.95f, 0.84f, 0.7f, 1f)
                        : new Color(0.91f, 0.89f, 0.84f, 1f);
                }
            }
            else
            {
                row.Root.SetActive(false);
            }
        }
    }

    #endregion

    #region Private Methods - Stat Synergies

    private Transform CreateStatSynergiesCard(Transform parent, TextMeshProUGUI reference)
    {
        Transform card = CreateCard(parent, "StatSynergiesCard", "STAT SYNERGIES");
        if (card == null)
        {
            return null;
        }

        // Weapon Stats section
        _weaponSynergyLabel = UIFactory.CreateTextElement(card, "WeaponLabel", reference, ChipFontScale, FontStyles.Bold);
        if (_weaponSynergyLabel != null)
        {
            _weaponSynergyLabel.text = "WEAPON STATS (1.5x)";
            _weaponSynergyLabel.color = new Color(1f, 1f, 1f, 0.55f);
        }

        _weaponSynergyChips = CreateChipContainer(card, "WeaponChips");

        // Blood Stats section
        _bloodSynergyLabel = UIFactory.CreateTextElement(card, "BloodLabel", reference, ChipFontScale, FontStyles.Bold);
        if (_bloodSynergyLabel != null)
        {
            _bloodSynergyLabel.text = "BLOOD STATS (1.5x)";
            _bloodSynergyLabel.color = new Color(1f, 1f, 1f, 0.55f);
        }

        _bloodSynergyChips = CreateChipContainer(card, "BloodChips");

        return card;
    }

    private Transform CreateChipContainer(Transform parent, string name)
    {
        RectTransform rectTransform = CreateRectTransformObject(name, parent);
        if (rectTransform == null)
        {
            return null;
        }

        UIFactory.ConfigureTopLeftAnchoring(rectTransform);

        // Use GridLayoutGroup for chip wrapping
        GridLayoutGroup gridLayout = rectTransform.gameObject.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(100f, 20f); // Will adjust based on content
        gridLayout.spacing = new Vector2(4f, 4f);
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;

        ContentSizeFitter fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rectTransform;
    }

    private void CreateChip(Transform parent, string text)
    {
        RectTransform rectTransform = CreateRectTransformObject("Chip", parent);
        if (rectTransform == null || _referenceText == null)
        {
            return;
        }

        Image background = rectTransform.gameObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.35f);
        background.raycastTarget = false;

        TextMeshProUGUI chipText = UIFactory.CreateTextElement(rectTransform, "ChipText", _referenceText, ChipFontScale, FontStyles.Normal);
        if (chipText != null)
        {
            chipText.text = text;
            chipText.color = new Color(1f, 1f, 1f, 0.75f);
            chipText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = chipText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(ChipPadding, ChipPadding);
                textRect.offsetMax = new Vector2(-ChipPadding, -ChipPadding);
            }
        }
    }

    private void UpdateStatSynergies()
    {
        // Clear existing chips
        if (_weaponSynergyChips != null)
        {
            UIFactory.ClearChildren(_weaponSynergyChips);
        }
        if (_bloodSynergyChips != null)
        {
            UIFactory.ClearChildren(_bloodSynergyChips);
        }

        // Get synergies for current class from DataService
        PlayerClass currentClass = _classType;
        
        if (currentClass == PlayerClass.None || !_classStatSynergies.TryGetValue(currentClass, out var synergies))
        {
            if (_weaponSynergyLabel != null)
            {
                _weaponSynergyLabel.text = "No class selected";
            }
            if (_bloodSynergyLabel != null)
            {
                _bloodSynergyLabel.gameObject.SetActive(false);
            }
            return;
        }

        // Show weapon synergies
        if (_weaponSynergyLabel != null)
        {
            float multiplier = _classStatMultiplier > 0 ? _classStatMultiplier : 1.5f;
            _weaponSynergyLabel.text = $"WEAPON STATS ({multiplier:0.#}x)";
        }

        if (synergies.WeaponStats != null)
        {
            foreach (WeaponStatType stat in synergies.WeaponStats)
            {
                string statName = HudUtilities.SplitPascalCase(stat.ToString());
                CreateChip(_weaponSynergyChips, statName);
            }
        }

        // Show blood synergies
        if (_bloodSynergyLabel != null)
        {
            _bloodSynergyLabel.gameObject.SetActive(true);
            float multiplier = _classStatMultiplier > 0 ? _classStatMultiplier : 1.5f;
            _bloodSynergyLabel.text = $"BLOOD STATS ({multiplier:0.#}x)";
        }

        if (synergies.BloodStats != null)
        {
            foreach (BloodStatType stat in synergies.BloodStats)
            {
                string statName = HudUtilities.SplitPascalCase(stat.ToString());
                CreateChip(_bloodSynergyChips, statName);
            }
        }
    }

    private void CreateDefaultWeaponSynergyChips()
    {
        if (_weaponSynergyChips == null)
        {
            return;
        }

        // Default weapon synergies per class
        var synergies = _classType switch
        {
            PlayerClass.BloodKnight => new[] { "Physical Power", "Primary Leech", "Phys Crit Dmg" },
            PlayerClass.DemonHunter => new[] { "Spell Power", "Attack Speed", "Movement Speed" },
            PlayerClass.VampireLord => new[] { "Spell Power", "Spell Crit Dmg", "Cooldown Reduction" },
            PlayerClass.ShadowBlade => new[] { "Physical Power", "Attack Speed", "Phys Crit Chance" },
            PlayerClass.ArcaneSorcerer => new[] { "Spell Power", "Spell Crit Chance", "Cooldown Reduction" },
            PlayerClass.DeathMage => new[] { "Spell Power", "Spell Leech", "Spell Crit Dmg" },
            _ => Array.Empty<string>()
        };

        foreach (string s in synergies)
        {
            CreateChip(_weaponSynergyChips, s);
        }
    }

    private void CreateDefaultBloodSynergyChips()
    {
        if (_bloodSynergyChips == null)
        {
            return;
        }

        // Default blood synergies per class
        var synergies = _classType switch
        {
            PlayerClass.BloodKnight => new[] { "Healing Received", "Dmg Reduction", "Phys Resistance" },
            PlayerClass.DemonHunter => new[] { "Movement Speed", "Attack Speed", "Crit Chance" },
            PlayerClass.VampireLord => new[] { "Spell Power", "Cooldown Reduction", "Spell Resistance" },
            PlayerClass.ShadowBlade => new[] { "Physical Power", "Crit Damage", "Attack Speed" },
            PlayerClass.ArcaneSorcerer => new[] { "Spell Power", "Max Health", "Spell Resistance" },
            PlayerClass.DeathMage => new[] { "Spell Leech", "Spell Power", "Cooldown Reduction" },
            _ => Array.Empty<string>()
        };

        foreach (string s in synergies)
        {
            CreateChip(_bloodSynergyChips, s);
        }
    }

    #endregion

    #region Helpers

    private static RectTransform CreateRectTransformObject(string name, Transform parent)
        => UIFactory.CreateRectTransformObject(name, parent);

    private static void ApplySprite(Image image, params string[] spriteNames)
    {
        if (image == null)
        {
            return;
        }

        Sprite sprite = ResolveSprite(spriteNames);
        if (sprite == null)
        {
            image.sprite = null;
            return;
        }

        image.sprite = sprite;
        image.type = Image.Type.Sliced;
    }

    private static Sprite ResolveSprite(params string[] spriteNames)
    {
        if (spriteNames == null || spriteNames.Length == 0)
        {
            return null;
        }

        foreach (string name in spriteNames)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (HudData.Sprites.TryGetValue(name, out Sprite sprite) && sprite != null)
            {
                return sprite;
            }
        }

        return null;
    }

    #endregion

    #region UI Data Classes

    private sealed class ClassRowUI
    {
        public GameObject Root { get; }
        public PlayerClass PlayerClass { get; }
        public Image Icon { get; }
        public TextMeshProUGUI NameText { get; }
        public TextMeshProUGUI StatusText { get; }
        public Image Background { get; }

        public ClassRowUI(GameObject root, PlayerClass playerClass, Image icon, TextMeshProUGUI nameText, TextMeshProUGUI statusText, Image background)
        {
            Root = root;
            PlayerClass = playerClass;
            Icon = icon;
            NameText = nameText;
            StatusText = statusText;
            Background = background;
        }
    }

    private sealed class SpellRowUI
    {
        public GameObject Root { get; }
        public int Index { get; }
        public TextMeshProUGUI NameText { get; }
        public TextMeshProUGUI RequirementText { get; }
        public TextMeshProUGUI IndexText { get; }
        public Image Background { get; }

        public SpellRowUI(GameObject root, int index, TextMeshProUGUI nameText, TextMeshProUGUI reqText, TextMeshProUGUI indexText, Image background)
        {
            Root = root;
            Index = index;
            NameText = nameText;
            RequirementText = reqText;
            IndexText = indexText;
            Background = background;
        }
    }

    #endregion
}
