# Veil

**Universal V Rising Mod UI Framework**

Veil is a manifest-driven UI framework that allows multiple V Rising mods to display HUD elements without code changes. Simply create a `manifest.json` for your mod and Veil handles the rest.

## Features

- 🎯 **Manifest-Driven** - No code required, just JSON configuration
- 🔌 **Multi-Mod Support** - Multiple adapters can coexist
- 🎨 **Component Library** - Labels, Progress Bars, Panels, Buttons
- 📍 **Draggable Elements** - Users can customize positions
- 💾 **Position Persistence** - Saves layout preferences
- 🔄 **Data Binding** - Auto-updates from chat command responses

## Installation

1. Install [BepInEx 6](https://builds.bepinex.dev/projects/bepinex_be) for V Rising
2. Copy `Veil.dll` to `BepInEx/plugins/`
3. Copy adapter folders to `BepInEx/plugins/Adapters/`

## Creating an Adapter

1. Create a folder in `BepInEx/plugins/Adapters/YourMod/`
2. Add a `manifest.json`:

```json
{
  "id": "yourmod",
  "displayName": "Your Mod HUD",
  "version": "1.0.0",
  "hud": {
    "elements": [
      {
        "id": "example",
        "type": "label",
        "text": "Hello from Your Mod!"
      }
    ]
  }
}
```

## Controls

- **F1** - Toggle UI visibility
- **F2** - Status dump (debug)
- **F3** - Force show UI
- **Drag** - Hold and drag elements to reposition

## Supported Components

| Type | Description |
|------|-------------|
| `label` | Simple text display |
| `progressBar` | XP/health style bar with fill |
| `panel` | Container for child elements |
| `button` | Clickable button with action |

## Credits

Created by **DiNaSoR**

## License

MIT
