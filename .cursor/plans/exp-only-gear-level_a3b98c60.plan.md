---
name: Exp-only-gear-level
overview: Change server behavior so the base-game Equipment tab “Gear Level” shows the mod’s Exp level only (no expertise addition), while keeping expertise/class levels visible elsewhere via EclipsePlus. Also keep the previously verified fixes for RiftFrequency=0 and Familiar stats encoding/parsing.
todos:
  - id: server-gearlevel-exp-only
    content: Change `LevelingSystem.SetLevel()` so Equipment tab Gear Level reflects Exp level only (no expertise, no +1).
    status: completed
  - id: verify-gearlevel-patches
    content: Verify `GearLevelPatches` and related patches don’t re-add expertise into Equipment.WeaponLevel; keep expertise functionality intact.
    status: completed
  - id: docs-gearlevel-update
    content: Update docs + changelog to explain Gear Level now equals Exp and where WP/Class are shown.
    status: completed
  - id: fix-riftfrequency-zero
    content: Fix `RiftFrequency<=0` to disable rifts (avoid spam scheduling).
    status: completed
  - id: fix-familiarstats-format
    content: Fix familiar stats encoding/parsing overflow by switching to delimiter format; bump protocol version.
    status: completed
---

# Make Equipment-tab Gear Level show Exp only

## Goal

Make the base-game Equipment tab’s big **Gear Level** display equal the player’s **Exp level** (the mod’s leveling level), not “Exp + weapon expertise”.

## What controls the Equipment-tab Gear Level today (verified)

Server currently overwrites equipment levels in `LevelingSystem.SetLevel()`:

- Writes `equipment.ArmorLevel = 0` and `equipment.SpellLevel = 0`
- Writes `equipment.WeaponLevel = ExpLevel + WeaponExpertiseLevel (+optional +1 buff)`

File:

- `Server/Bloodcraftplus/Systems/Leveling/LevelingSystem.cs`

## Plan (implementation)

### Todo: Server — make WeaponLevel mirror Exp level only

- Update `Server/Bloodcraftplus/Systems/Leveling/LevelingSystem.cs` `SetLevel()`:
- Set `equipment.WeaponLevel._Value = ExpLevel` **only**.
- Do **not** add weapon expertise level.
- Do **not** apply `HandleExtraLevel` (+1 buff) since you want Gear Level == Exp exactly.
- Keep `equipment.ArmorLevel._Value = 0f` and `equipment.SpellLevel._Value = 0f` as-is unless testing shows it prevents the UI from matching Exp.

### Todo: Server — keep expertise system functional without affecting Gear Level

- Confirm `Server/Bloodcraftplus/Patches/GearLevelPatches.cs` continues to:
- Refresh stats on weapon spawn.
- Call `LevelingSystem.SetLevel(player)` at the same times as before.
- Ensure no other patch re-adds expertise into `Equipment.WeaponLevel`.

### Todo: Client UI (optional but recommended)

- `Client/EclipsePlus/Services/CharacterMenuService.cs` currently injects a summary under Gear Level:
- `WP:<expertise> | Cl:<classEnum> | Exp:<exp>`
- After Gear Level becomes Exp, consider changing this summary to reduce confusion, e.g.:
- `WP:<expertise> | Class:<name>` (omit Exp since the big number is Exp)
- or keep as-is if you want the breakdown visible.

### Todo: Docs + changelog (mandatory)

- Update docs to reflect the new invariant:
- `Docs/src/content/server/leveling.mdx`: explain that the vanilla Gear Level UI now shows **Exp level**, and where to find expertise/class.
- `Docs/src/content/reference/changelog.mdx`: add an entry for this behavior change.

## Verification / regression checks

- Build:
- `dotnet build -c Release` for `Server/Bloodcraftplus/Bloodcraftplus.csproj`
- `dotnet build -c Release` for `Client/EclipsePlus/EclipsePlus.csproj` (if we adjust the summary UI)
- In-game smoke checks:
- With varying weapon expertise, confirm Equipment tab Gear Level remains equal to Exp level.
- Confirm expertise progression still works and any expertise UI still shows the expertise level.
- Confirm no gear level “jumps” (+1) occur from set-bonus buffs (since we’re removing `HandleExtraLevel`).

## Carry-over fixes from the earlier verified report list

- Fix `RiftFrequency=0` to disable rifts (avoid 0-interval scheduling spam).
- Fix Familiar stats encoding/parsing (avoid fixed-width overflow misparsing) by switching to delimiter format and bumping protocol version.