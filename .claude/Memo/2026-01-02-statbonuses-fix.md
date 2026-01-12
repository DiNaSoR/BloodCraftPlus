# StatBonusesTab Asset Fix - 2026-01-02

## Issue
The StatBonusesTab weapon icon was not showing because:
1. The weapon icon Image was created but no sprite was applied
2. The alpha was set to 0.2 (nearly invisible)

## Root Cause
In `CharacterMenuService.CreateStatBonusesWeaponHeader()`, the weapon icon was just a placeholder with:
- No sprite applied via `ApplySprite()`
- Very low alpha (0.2) making it nearly invisible

## Fix Applied

### Phase 1: Initial Fix
1. Added default sprite name array in `CharacterMenuService.cs`:
   ```csharp
   static readonly string[] StatBonusesDefaultWeaponIconSpriteNames = ["Poneti_Icon_Sword_v2_48", "strength_level_icon"];
   ```

2. Updated `CreateStatBonusesWeaponHeader()` to properly apply the sprite with correct visibility.

### Phase 2: Weapon Type Icons
Added weapon type to icon mapping for all 11 weapon types in `CharacterMenuService.cs`:
```csharp
static readonly Dictionary<string, string[]> WeaponTypeIconSpriteNames = new(StringComparer.OrdinalIgnoreCase)
{
    { "Sword", ["Poneti_Icon_Sword_v2_48", "Stunlock_Icon_BoneSword01"] },
    { "Axe", ["Poneti_Icon_Axe_v2_04", "Stunlock_Icon_BoneAxe01"] },
    { "Mace", ["Poneti_Icon_Hammer_30", "Stunlock_Icon_BoneMace01"] },
    { "Spear", ["Poneti_Icon_Spear_v2_01", "Poneti_Icon_Sword_v2_48"] },
    { "Crossbow", ["Poneti_Icon_Crossbow_v2_01", "Poneti_Icon_Bow_v2_01"] },
    { "GreatSword", ["Poneti_Icon_Greatsword_v2_01", "Poneti_Icon_Sword_v2_48"] },
    { "Slashers", ["Poneti_Icon_Dagger_v2_01", "Poneti_Icon_Sword_v2_48"] },
    { "Pistols", ["Poneti_Icon_Pistol_v2_01", "Poneti_Icon_Crossbow_v2_01"] },
    { "Reaper", ["Poneti_Icon_Scythe_v2_01", "Poneti_Icon_Axe_v2_04"] },
    { "Longbow", ["Poneti_Icon_Bow_v2_01", "Poneti_Icon_Crossbow_v2_01"] },
    { "Whip", ["Poneti_Icon_Whip_v2_01", "Poneti_Icon_Sword_v2_48"] }
};
```

3. Added `GetWeaponTypeIconSprites()` helper method.

4. Updated `UpdateStatBonusesPanel()` to dynamically update the icon based on `data.WeaponType`.

5. Added new sprite names to `HudData.SpriteNames`:
   - Poneti_Icon_Spear_v2_01
   - Poneti_Icon_Crossbow_v2_01
   - Poneti_Icon_Bow_v2_01
   - Poneti_Icon_Greatsword_v2_01
   - Poneti_Icon_Dagger_v2_01
   - Poneti_Icon_Pistol_v2_01
   - Poneti_Icon_Scythe_v2_01
   - Poneti_Icon_Whip_v2_01
   - Stunlock_Icon_BoneSword01
   - Stunlock_Icon_BoneAxe01
   - Stunlock_Icon_BoneMace01

## Notes
- Each weapon type has fallback sprites in case the primary sprite isn't found
- The icon updates dynamically when weapon data changes
- Some sprites (like Poneti_Icon_Spear_v2_01) may not exist in game assets; they will fallback to alternatives

## Files Modified
- `Services/CharacterMenuService.cs`
- `Services/HUD/Shared/HudData.cs`
