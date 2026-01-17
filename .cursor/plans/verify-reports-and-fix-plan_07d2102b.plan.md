---
name: Verify-reports-and-fix-plan
overview: Verify which reported issues are supported by current BloodCraftPlus code, then fix the confirmed ones (Primal rift scheduling semantics and Familiar stat display overflow/parsing), plus document the “not-a-bug” misunderstandings to reduce user confusion.
todos:
  - id: verify-claims-mapping
    content: Map each user report to owning code paths and mark as confirmed/likely/not-supported (rifts, familiars stats, leveling display).
    status: pending
  - id: fix-riftfrequency-zero
    content: Implement ‘RiftFrequency <= 0 disables rifts’ safeguards in `Server/Bloodcraftplus/Systems/PrimalWarEventSystemBase.cs` and update config docs accordingly.
    status: pending
  - id: fix-familiarstats-format
    content: Switch FamiliarStats encoding/parsing from fixed-width concat to delimiter-based format; bump protocol version; update docs/changelog.
    status: pending
  - id: docs-clarify-gear-mirage
    content: Update leveling docs to clarify Gear Level Mirage (player level + weapon expertise) to reduce false bug reports.
    status: pending
---

# Verify reported issues + targeted fix plan

## What’s verified (from this repo’s code)

- **RiftFrequency “0 vs even numbers” confusion is real (bug/UX trap)**
- In `PrimalWarEventSystem`, the interval is computed as `DAY_SECONDS / PrimalRifts` where `PrimalRifts = Min(RiftFrequency, 24)`.
- If `RiftFrequency` is **0**, `RiftInterval` becomes **0**, and the scheduler will immediately trigger rifts repeatedly.
- Files:
- `Server/Bloodcraftplus/Systems/PrimalWarEventSystemBase.cs` (static `PrimalRifts`/`RiftInterval`, `OnSchedule()`)

- **“Elite Primal Rifts toggle doesn’t stop” is explainable**
- `EclipseService` caches `ElitePrimalRifts` into a `static readonly` field, so changing the config at runtime won’t change behavior until restart.
- File:
- `Server/Bloodcraftplus/Services/EclipseService.cs` (`static readonly bool _elitePrimalRifts = ConfigService.ElitePrimalRifts;` and `if (_elitePrimalRifts) PrimalWarEventSystem.OnSchedule();`)

- **“Familiar HP/PP/SP shows wrong or absurdly high” is a confirmed display/parsing bug when values exceed fixed widths**
- Server encodes familiar stats as a concatenated fixed-width string:
- `maxHealth:D4 + physicalPower:D3 + spellPower:D3`.
- Client parses by slicing fixed positions `[..4]`, `[4..7]`, `[7..]`.
- When **any value exceeds its width** (HP > 9999 or PP/SP > 999), the string grows and slicing becomes misaligned → wrong numbers.
- Files:
- `Server/Bloodcraftplus/Services/EclipseService.cs` (`GetFamiliarData()` builds `familiarStats`)
- `Client/EclipsePlus/Services/DataService.cs` (`FamiliarData` slices/`int.Parse`)
- `Client/EclipsePlus/Services/CharacterMenu/Tabs/FamiliarsTab.cs` (`BuildFamiliarStatsLine()` displays `HP/PP/SP`)

- **“Level 178” is not a bug in this repo (it’s the documented “Gear Level Mirage”)**
- Displayed weapon level = Player Level + Weapon Expertise level; armor level hidden.
- File:
- `Server/Bloodcraftplus/Systems/Leveling/LevelingSystem.cs` (`SetLevel()` adds expertise to level)

## Not supported / not directly verifiable from repo alone

- **Bloodmoon “massively inflated Spell Power”**: no explicit Bloodmoon stat-modifying logic found in server systems (only type registrations/prefab IDs). Could be base-game behavior, other mods, or a UI parsing/display symptom depending on where it’s observed.
- **Controller abilities not invoking properly**: this client mod doesn’t appear to own core combat ability invocation; likely outside scope or requires in-game reproduction.
- **Repairing gear broken**: no active repair hook is enabled in `CraftingSystemPatches.cs` (repair-related patch is commented out); likely game update / other mod conflict.
- **SLF/Kindred conflicts**: no explicit integration logic found; would need repro steps + mod list.

## Implementation plan (fix confirmed issues)

### Todo: Fix `RiftFrequency=0` to mean “disabled”

- Update `Server/Bloodcraftplus/Systems/PrimalWarEventSystemBase.cs`:
- Treat `ConfigService.RiftFrequency <= 0` as **disabled**.
- Early-return in `OnSchedule()` and `TryStartPrimalRifts()` when disabled.
- Ensure no division-by-zero / spam scheduling path exists.
- Update docs:
- `Docs/src/content/reference/config.mdx`: document **`RiftFrequency=0 disables rifts`**.
- Add a short troubleshooting note that changing `ElitePrimalRifts` and rift frequency requires restart (since config values are cached).

### Todo: Fix Familiar stat display overflow by switching to a delimited format (OK to require updating both mods together)

- Update server payload encoding:
- In `Server/Bloodcraftplus/Services/EclipseService.cs` `GetFamiliarData()`, change `familiarStats` from fixed-width concat to a safe delimiter format like: `"{maxHealth}|{physicalPower}|{spellPower}"`.
- Update client parsing:
- In `Client/EclipsePlus/Services/DataService.cs` `FamiliarData`, replace the substring slicing with `Split('|')` parsing and defensive fallbacks.
- Keep `FamiliarsTab.BuildFamiliarStatsLine()` unchanged (it already just renders strings).
- Update protocol version so mismatched installs fail fast instead of silently misparsing:
- Bump the `V1_3` marker in `Server/Bloodcraftplus/Services/EclipseService.cs` and corresponding client handling (registration/version checks) so both sides agree on the new format.
- Update docs/changelog:
- Add a fix note in `Docs/src/content/reference/changelog.mdx`.
- Add/adjust the Familiars docs section (likely `Docs/src/content/server/familiars.mdx` and/or client data flow docs) describing how stats are transported.

### Todo: Reduce “not-a-bug” confusion in docs

- Document the Gear Level Mirage clearly:
- `Docs/src/content/server/leveling.mdx`: explicitly call out that the displayed weapon level is `playerLevel + weaponExpertise`, explaining “178” reports.

## Verification / regression checks to run

- Build:
- `dotnet build -c Release` for `Client/EclipsePlus/EclipsePlus.csproj` and `Server/Bloodcraftplus/Bloodcraftplus.csproj`.
- In-game smoke checks (focused):
- Set `ElitePrimalRifts=true`, `RiftFrequency=0`: confirm no rift spam.
- Set `RiftFrequency=6`: confirm scheduling logs show sane countdown and rifts start.
- Force a familiar’s HP/PP/SP above 9999/999 temporarily (admin/buffs) and confirm Eclipse UI still displays correct numbers.