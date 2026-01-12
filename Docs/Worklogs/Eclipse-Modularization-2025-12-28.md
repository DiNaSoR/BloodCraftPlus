# Eclipse Codebase Modularization - Work Log

## Overview
Refactored monolithic services (`CanvasService.cs` and `CharacterMenuService.cs`) into a modular, component-based architecture with interfaces for extensibility.

## Final Results (Phase 5 Complete)

| File | Original | After Phase 4 | After Phase 5 | Total Reduction |
|------|----------|---------------|---------------|-----------------|
| **CanvasService.cs** | 3,604 lines | 2,044 lines | 1,385 lines | **62% (-2,219 lines)** |
| **CharacterMenuService.cs** | 3,166 lines | 2,506 lines | 2,506 lines | **21% (-660 lines)** |
| **UIFactory.cs** | 0 lines | 712 lines | 712 lines | *shared component* |
| **Total** | 6,770 lines | 4,550 lines | 3,891 lines | **43% reduction** |

### New Files in Phase 5: 4 Configurators
- `Services/HUD/Configurators/ShiftSlotConfigurator.cs`
- `Services/HUD/Configurators/QuestWindowConfigurator.cs`
- `Services/HUD/Configurators/ClassWindowConfigurator.cs`
- `Services/HUD/Configurators/ProgressBarConfigurator.cs`

---

## New Folder Structure

```
Services/
├── HUD/
│   ├── Interfaces/
│   │   ├── IHudComponent.cs
│   │   ├── IHudProgressBar.cs
│   │   └── IHudWindow.cs
│   ├── Base/
│   │   ├── HudComponentBase.cs
│   │   └── ProgressBarComponentBase.cs
│   ├── Configurators/                    (NEW - Phase 5)
│   │   ├── ShiftSlotConfigurator.cs
│   │   ├── QuestWindowConfigurator.cs
│   │   ├── ClassWindowConfigurator.cs
│   │   └── ProgressBarConfigurator.cs
│   ├── ProgressBars/
│   │   ├── ExperienceBarComponent.cs
│   │   ├── LegacyBarComponent.cs
│   │   ├── ExpertiseBarComponent.cs
│   │   └── FamiliarBarComponent.cs
│   ├── QuestTracker/
│   │   └── QuestTrackerComponent.cs
│   ├── Shared/
│   │   ├── HudData.cs
│   │   ├── HudConfiguration.cs
│   │   └── HudUtilities.cs
│   ├── HudOrchestrator.cs
│   ├── HudIntegration.cs
│   ├── HudUpdateManager.cs
│   ├── HudToggleManager.cs
│   ├── HudConfigureManager.cs
│   └── InputAdaptiveManager.cs
│
├── CharacterMenu/
│   ├── Interfaces/
│   │   └── ICharacterMenuTab.cs
│   ├── Base/
│   │   └── CharacterMenuTabBase.cs
│   ├── Tabs/
│   │   ├── PrestigeTab.cs
│   │   ├── ExoformTab.cs
│   │   ├── BattlesTab.cs
│   │   ├── StatBonusesTab.cs
│   │   └── ProfessionsTab.cs
│   ├── Shared/
│   │   └── UIFactory.cs
│   ├── CharacterMenuOrchestrator.cs
│   └── CharacterMenuIntegration.cs
│
├── CanvasService.cs          (2,044 lines)
├── CharacterMenuService.cs   (2,506 lines)
└── [other services unchanged]
```

---

## Detailed Work Completed

### Phase 1: Foundation
- Created folder structure (`Services/HUD/`, `Services/CharacterMenu/`)
- Created interfaces:
  - `IHudComponent` - Base interface for HUD components
  - `IHudProgressBar` - Interface for progress bar components
  - `IHudWindow` - Interface for window components
  - `ICharacterMenuTab` - Interface for character menu tabs
- Created base classes:
  - `HudComponentBase` - Abstract base for HUD components
  - `ProgressBarComponentBase` - Abstract base for progress bars
  - `CharacterMenuTabBase` - Abstract base for menu tabs
- Extracted shared data to `HudData.cs`, `HudConfiguration.cs`, `HudUtilities.cs`

### Phase 2: HUD Components (Extracted from CanvasService)
- **HudUpdateManager.cs** (~900 lines) - All real-time HUD updates
  - `UpdateAttributeType()`, `UpdateBar()`, `UpdateQuests()`
  - Progress bar updates, ability state management
  - Tab panel updates, quest tracking
- **HudToggleManager.cs** - Visibility toggling logic
- **InputAdaptiveManager.cs** - Input device adaptation
- **HudUtilities.cs** - Utility functions:
  - `ToRoman()`, `IntegerToRoman()` - Roman numeral conversion
  - `FormatClassName()`, `SplitPascalCase()` - String formatting
  - `FormatWeaponStatBar()`, `FormatAttributeValue()` - Value formatting
  - `ClassSynergy()` - Class synergy calculations
  - `FindSprites()`, `GetClassColor()`, `GetProfessionIcon()` - Resource helpers
- **HudOrchestrator.cs** - Component lifecycle management
- **HudIntegration.cs** - Integration layer
- Progress bar components (Experience, Legacy, Expertise, Familiar)
- **QuestTrackerComponent.cs** - Quest tracking UI

### Phase 3: CharacterMenu Tabs (Extracted from CharacterMenuService)
- **PrestigeTab.cs** - Leaderboard display with full logic
- **ExoformTab.cs** - Shapeshift configuration with full logic
- **BattlesTab.cs** - Familiar battle groups with full logic
- **StatBonusesTab.cs** - Weapon stat selection
- **ProfessionsTab.cs** - Profession progress panel
- **CharacterMenuOrchestrator.cs** - Tab management
- **CharacterMenuIntegration.cs** - Integration layer
- **UIFactory.cs** (712 lines) - UI creation helpers

### Phase 4: Delegation Wrappers
All nested static classes now delegate to their extracted managers:

```csharp
// CanvasService delegations
public static class UpdateHUD
{
    public static void UpdateBar(...) => HudUpdateManager.UpdateBar(...);
    public static void UpdateQuests(...) => HudUpdateManager.UpdateQuests(...);
}

public static class ToggleHUD
{
    public static void ToggleAllObjects() => HudToggleManager.ToggleAllObjects();
}
```

---

## UIFactory Methods (18 delegated methods)

### Core UI Creation
- `CreateRectTransformObject(string name, Transform parent)`
- `CreateSimpleRectTransform(string name, Transform parent)`
- `ConfigureTopLeftAnchoring(RectTransform rectTransform)`

### Layout Components
- `EnsureVerticalLayout(Transform target, padding...)`
- `CreateHorizontalLayout(RectTransform target, spacing, alignment)`
- `CreatePadding(int left, right, top, bottom)`
- `ClearChildren(Transform root)`

### Text Elements
- `CreateSectionHeader(Transform parent, TextMeshProUGUI reference, string text)`
- `CreateSectionSubHeader(Transform parent, TextMeshProUGUI reference, string text, bool applyAlphaFade)`
- `CreateText(Transform parent, TextMeshProUGUI reference, string text, float fontSize, TextAlignmentOptions alignment)`
- `CreateTextElement(Transform parent, string name, TextMeshProUGUI reference, float fontScale, FontStyles style)`
- `CreateEntry(Transform parent, TextMeshProUGUI reference, string text, FontStyles style)`
- `CopyTextStyle(TextMeshProUGUI source, TextMeshProUGUI target)`

### Visual Elements
- `CreateDividerLine(Transform parent, float height, Color? color)`
- `AddSpacer(Transform parent, float height)` - Vertical spacer
- `AddHorizontalSpacer(Transform parent, float width, float height)` - Horizontal spacer
- `CreateImage(Transform parent, string name, Vector2 size, Color? color)`
- `CreateProgressBar(Transform parent, string name, Vector2 size, Color backgroundColor, Color fillColor)`

### Container Elements
- `CreateContentRoot(Transform parent)`
- `CreatePaddedSectionRoot(Transform parent, string name)`
- `CreateEntriesRoot(Transform parent)`
- `CreateListRoot(Transform parent, string name)`

### Entry Templates
- `CreateEntryTemplate(Transform parent, TextMeshProUGUI reference)` - Returns `(GameObject, TextMeshProUGUI)`

### SubTab Utilities
- `StretchSubTabGraphics(GameObject buttonObject)`
- `ResolveSubTabFontSize(float targetHeight, float desiredFontSize)`
- `ApplySubTabTextSizing(TMP_Text[] templateLabels, TMP_Text fallbackLabel, float targetHeight, float desiredFontSize)`
- `ApplySubTabTextSizing(IReadOnlyList<TMP_Text> labels, float targetHeight, float desiredFontSize)`
- `ApplySubTabLabelStyle(TMP_Text label, float fontSize)`
- `ConfigureSubTabLabelRect(TMP_Text label)`
- `CreateSubTabLabel(Transform parent, TextMeshProUGUI reference, string label, float fontScale)`

---

## CharacterMenuService Delegations

```csharp
// Methods that now delegate to UIFactory
static void ClearChildren(Transform root) => UIFactory.ClearChildren(root);
static RectOffset CreatePadding(...) => UIFactory.CreatePadding(...);
static Transform CreatePaddedSectionRoot(...) => UIFactory.CreatePaddedSectionRoot(...);
static RectTransform CreateRectTransformObject(...) => UIFactory.CreateRectTransformObject(...);
static Transform CreateEntriesRoot(...) => UIFactory.CreateEntriesRoot(...);
static TextMeshProUGUI CreateSectionHeader(...) => UIFactory.CreateSectionHeader(...);
static TextMeshProUGUI CreateSectionSubHeader(...) => UIFactory.CreateSectionSubHeader(...);
static TextMeshProUGUI CreateTextElement(...) => UIFactory.CreateTextElement(...);
static TextMeshProUGUI CreateText(...) => UIFactory.CreateText(...);
static TMP_Text CreateSubTabLabel(...) => UIFactory.CreateSubTabLabel(...);
static void StretchSubTabGraphics(...) => UIFactory.StretchSubTabGraphics(...);
static void ApplySubTabTextSizing(...) => UIFactory.ApplySubTabTextSizing(...);
static float ResolveSubTabFontSize(...) => UIFactory.ResolveSubTabFontSize(...);
static void ApplySubTabLabelStyle(...) => UIFactory.ApplySubTabLabelStyle(...);
static void ConfigureSubTabLabelRect(...) => UIFactory.ConfigureSubTabLabelRect(...);
static Transform CreateProfessionListRoot(...) => UIFactory.CreateListRoot(...);
static void AddSpacer(...) => UIFactory.AddHorizontalSpacer(...);

// CreateEntryTemplate uses tuple return
static GameObject CreateEntryTemplate(Transform parent, TextMeshProUGUI reference)
{
    var (template, text) = UIFactory.CreateEntryTemplate(parent, reference);
    entryStyle = text;
    return template;
}
```

---

## Backwards Compatibility

All public APIs remain unchanged. The nested static classes in CanvasService now delegate to their extracted managers, ensuring existing code continues to work:

```csharp
// External code still works
CanvasService.UpdateHUD.UpdateBar(...);  // Delegates to HudUpdateManager
CanvasService.ToggleHUD.ToggleAllObjects();  // Delegates to HudToggleManager
```

---

## Files Created (29 total)

### HUD Components (19 files)
1. `Services/HUD/Interfaces/IHudComponent.cs`
2. `Services/HUD/Interfaces/IHudProgressBar.cs`
3. `Services/HUD/Interfaces/IHudWindow.cs`
4. `Services/HUD/Base/HudComponentBase.cs`
5. `Services/HUD/Base/ProgressBarComponentBase.cs`
6. `Services/HUD/ProgressBars/ExperienceBarComponent.cs`
7. `Services/HUD/ProgressBars/LegacyBarComponent.cs`
8. `Services/HUD/ProgressBars/ExpertiseBarComponent.cs`
9. `Services/HUD/ProgressBars/FamiliarBarComponent.cs`
10. `Services/HUD/QuestTracker/QuestTrackerComponent.cs`
11. `Services/HUD/Shared/HudData.cs`
12. `Services/HUD/Shared/HudConfiguration.cs`
13. `Services/HUD/Shared/HudUtilities.cs`
14. `Services/HUD/HudOrchestrator.cs`
15. `Services/HUD/HudIntegration.cs`
16. `Services/HUD/HudUpdateManager.cs`
17. `Services/HUD/HudToggleManager.cs`
18. `Services/HUD/HudConfigureManager.cs`
19. `Services/HUD/InputAdaptiveManager.cs`

### CharacterMenu Components (10 files)
1. `Services/CharacterMenu/Interfaces/ICharacterMenuTab.cs`
2. `Services/CharacterMenu/Base/CharacterMenuTabBase.cs`
3. `Services/CharacterMenu/Tabs/PrestigeTab.cs`
4. `Services/CharacterMenu/Tabs/ExoformTab.cs`
5. `Services/CharacterMenu/Tabs/BattlesTab.cs`
6. `Services/CharacterMenu/Tabs/StatBonusesTab.cs`
7. `Services/CharacterMenu/Tabs/ProfessionsTab.cs`
8. `Services/CharacterMenu/Shared/UIFactory.cs`
9. `Services/CharacterMenu/CharacterMenuOrchestrator.cs`
10. `Services/CharacterMenu/CharacterMenuIntegration.cs`

### Complete Services Directory Structure
```
Services/
├── HUD/                          (23 files - 19 original + 4 configurators)
│   ├── Interfaces/               (3 files)
│   ├── Base/                     (2 files)
│   ├── Configurators/            (4 files - Phase 5)
│   ├── ProgressBars/             (4 files)
│   ├── QuestTracker/             (1 file)
│   ├── Shared/                   (3 files)
│   └── [6 manager files]
├── CharacterMenu/                (10 files)
│   ├── Interfaces/               (1 file)
│   ├── Base/                     (1 file)
│   ├── Tabs/                     (5 files)
│   ├── Shared/                   (1 file)
│   └── [2 orchestrator files]
├── AssetDumpService.cs
├── CanvasService.cs              (1,385 lines - reduced from 3,604, 62% reduction)
├── CharacterMenuService.cs       (2,506 lines - reduced from 3,166, 21% reduction)
├── DataService.cs
├── DebugService.cs
├── LayoutService.cs
├── LocalizationService.cs
└── SystemService.cs
```

**Total component files created: 33** (23 HUD + 10 CharacterMenu)

---

## Phase 5 Details (ConfigureHUD Extraction)

Extracted from `CanvasService.ConfigureHUD` to `Services/HUD/Configurators/`:

| Configurator | Methods Extracted | Lines |
|--------------|-------------------|-------|
| `ShiftSlotConfigurator.cs` | `ConfigureShiftSlot()` | ~85 |
| `QuestWindowConfigurator.cs` | `ConfigureQuestWindow()` | ~134 |
| `ClassWindowConfigurator.cs` | `ConfigureClassWindow()`, `TryBindLocalizedText()`, `ApplyTransparentGraphic()` | ~250 |
| `ProgressBarConfigurator.cs` | `ConfigureHorizontalProgressBar()`, `ConfigureVerticalProgressBar()`, panel configs | ~189 |

**Result**: CanvasService reduced from 2,044 → 1,385 lines (659 lines, 32% additional reduction)

---

## Future Improvements (Optional)

The following could be extracted but require significant state refactoring:

1. **CharacterMenuService panel methods** (~300 lines)
   - `UpdateStatBonusesPanel()`, `UpdateProfessionPanel()`, `UpdateSubTabSizing()`
   - Tightly coupled to static state fields (rows, lists, text references)
   - Would require passing 10+ state parameters or full state object

2. **InitializeHUD** (~250 lines) - UI initialization
   - Called once during setup
   - Tied to ConfigureHUD

3. **DataHUD** (~400 lines) - State fields
   - Already accessed via `CanvasService.DataHUD`
   - Would need significant refactoring to move

4. **CharacterMenuService panels** (~500 lines)
   - `UpdateProfessionPanel`, `UpdateStatBonusesPanel`
   - Tightly coupled to Unity UI components

---

## Build Command

```bash
cd "h:\My gits\Eclipse" && "C:\Users\Asus\.dotnet8\dotnet.exe" build -c Release
```

**Build Status**: ✅ Successful (0 errors, 12 warnings)

---

## Bug Fixes Applied (Phase 5 Compilation)

The following fixes were required to achieve a clean build after Phase 5:

1. **Object ambiguity** - Changed `Object.Destroy()` to `UnityEngine.Object.Destroy()` in:
   - `HudComponentBase.cs`
   - `CharacterMenuTabBase.cs`
   - `UIFactory.cs`

2. **RectOffset constructor** - Il2Cpp doesn't support 4-arg constructor:
   ```csharp
   // Before (fails in Il2Cpp)
   new RectOffset(left, right, top, bottom)

   // After (works)
   RectOffset padding = new();
   padding.left = left; padding.right = right;
   padding.top = top; padding.bottom = bottom;
   ```

3. **HudUtilities.cs fixes**:
   - Updated `BloodStatTypeAbbreviations` dictionary to match current `BloodStatType` enum
   - Implemented `SplitPascalCase()` with regex (removed missing extension method)
   - Changed `Resources.FindObjectsOfTypeAll` to `UnityEngine.Resources.FindObjectsOfTypeAll`

4. **HudUpdateManager.cs** - Added missing imports:
   - `using Eclipse.Resources;` (for `PrefabGUIDs`)
   - `using Eclipse.Utilities;` (for `ModificationIds`)
   - `using static Eclipse.Patches.InitializationPatches;` (for `AttributesInitialized`)

5. **CharacterMenuOrchestrator.cs** - Changed `CurrentTabIndex` to `CurrentTab`

6. **CharacterMenuService.cs** - Removed duplicate `BloodcraftEntry` struct (using interface version)

7. **CanvasService.cs** - Added using alias for nested type:
   ```csharp
   using InputAdaptiveElement = Eclipse.Services.HUD.InputAdaptiveManager.InputAdaptiveElement;
   ```

8. **Eclipse.csproj** - Added exclusion for Docs folder:
   ```xml
   <Compile Remove="Docs\**\*.cs" />
   ```

---

## Archive Location

Original monolithic files backed up in:
- `Docs/Archive/OldServices/CanvasService_Original.cs`
- `Docs/Archive/OldServices/CharacterMenuService_Original.cs`
- `Docs/Archive/OldServices/README.md`

---

*Last Updated: December 28, 2025*
