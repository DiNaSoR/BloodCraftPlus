# UIInspectorService Enhancement - 2026-01-15

## Summary
Enhanced the VDebug UI Inspector to provide much more detailed and actionable information for UI development.

## Changes Made

### File Modified
- `Tools/VDebug/Services/UIInspectorService.cs`

### New Features Added

1. **Sibling Index Display** - Shows the element's sibling index in the GameObject section

2. **Anchor Preset Names** - Automatically identifies common anchor presets like "Top-Center", "Stretch-All", "Bottom-Left", etc.

3. **Screen Bounds** - Shows actual screen pixel coordinates of the element

4. **Canvas Scaler Info** - Displays scale mode, reference resolution, and scale factor

5. **Parent Layout Chain** (NEW SECTION)
   - Traverses parent hierarchy to find LayoutGroups
   - Shows VerticalLayoutGroup/HorizontalLayoutGroup/GridLayoutGroup settings
   - Displays spacing, padding, child control settings
   - Shows ContentSizeFitter settings
   - Indicates if no layout exists (manual positioning needed)

6. **Siblings Section** (NEW SECTION)
   - Lists all siblings in the same parent container
   - Shows sibling index, active state, component type, Y position
   - Highlights current element with "← YOU" marker
   - Useful for understanding element order in layouts

7. **Better Type Resolution**
   - Uses `GetIl2CppType()` to resolve actual Il2Cpp type names
   - Fixes issue where components showed as generic "Component"

8. **Primary Component Type Labels**
   - Children and siblings now show quick type labels like [Text], [Button], [Image], [Layout], etc.

9. **Nearby Text Style Reference** (NEW SECTION)
   - Finds closest TMP_Text in self, children, siblings, or parents
   - Shows font, size, style, color, alignment, overflow settings
   - Useful for matching existing text styles

10. **Smart Hints** (NEW SECTION)
    - Provides ready-to-use code snippets
    - "To add element AFTER this" - shows SetSiblingIndex code or manual positioning
    - "To find this element" - shows GameObject.Find path
    - "Interactivity" - shows if element is a raycast target

### Panel Size Updates
- Increased panel width: 400 → 450
- Increased panel height: 350 → 500
- Increased scroll view height: 250 → 400

## Why These Changes
User requested better inspector output for UI development tasks, specifically for adding elements relative to existing UI elements like the Divider in the main menu sidebar.
