# Archived Original Services

This folder contains the original monolithic service files before the modularization refactoring.

## Archived Files

| File | Original Size | Description |
|------|---------------|-------------|
| `CanvasService_Original.cs` | 3,604 lines | HUD rendering with 7 nested static classes |
| `CharacterMenuService_Original.cs` | 3,166 lines | Character menu with 5 tabs in one class |

## Modularization Date
December 2024

## New Architecture

The monolithic services have been refactored into a modular component-based architecture:

### HUD Components (from CanvasService)
```
Services/HUD/
├── Interfaces/
│   ├── IHudComponent.cs
│   ├── IHudProgressBar.cs
│   └── IHudWindow.cs
├── Base/
│   ├── HudComponentBase.cs
│   └── ProgressBarComponentBase.cs
├── ProgressBars/
│   ├── ExperienceBarComponent.cs
│   ├── LegacyBarComponent.cs
│   ├── ExpertiseBarComponent.cs
│   └── FamiliarBarComponent.cs
├── QuestTracker/
│   └── QuestTrackerComponent.cs
├── Shared/
│   ├── HudData.cs
│   ├── HudConfiguration.cs
│   └── HudUtilities.cs
├── HudOrchestrator.cs
└── HudIntegration.cs
```

### CharacterMenu Tabs (from CharacterMenuService)
```
Services/CharacterMenu/
├── Interfaces/
│   └── ICharacterMenuTab.cs
├── Base/
│   └── CharacterMenuTabBase.cs
├── Tabs/
│   ├── PrestigeTab.cs
│   ├── ExoformTab.cs
│   ├── BattlesTab.cs
│   ├── StatBonusesTab.cs
│   └── ProfessionsTab.cs
├── Shared/
│   └── UIFactory.cs
├── CharacterMenuOrchestrator.cs
└── CharacterMenuIntegration.cs
```

## Benefits of Modularization

1. **Single Responsibility**: Each component handles one specific feature
2. **Testability**: Components can be tested in isolation
3. **Maintainability**: Smaller files are easier to understand and modify
4. **Extensibility**: New components can be added without modifying existing code
5. **Reusability**: Base classes and interfaces enable code reuse

## Integration

The original services now act as thin orchestrators that delegate to the new modular components via integration layers:
- `HudIntegration.cs` - Bridges CanvasService to HUD components
- `CharacterMenuIntegration.cs` - Bridges CharacterMenuService to tab components
