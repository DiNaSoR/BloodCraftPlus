---
name: Verify-reports-and-fix-plan
overview: "Verify which reported issues are supported by current BloodCraftPlus code, then fix confirmed ones. NOTE: As of `Bloodcraftplus v1.12.21` + `EclipsePlus v1.4.0` (2026-01-17), the two main “confirmed fixes” in this plan (Primal rift scheduling semantics and Familiar stat overflow parsing) are already implemented and documented; this plan is now primarily a verification record + remaining docs polish."
todos:
  - id: docs-clarify-restart-required-config
    content: Add a short docs note that some config values are cached at startup (e.g. `ElitePrimalRifts`), so changing them requires a restart to take effect.
    status: pending
---

# Remaining fix(es)

## Docs: clarify restart-required config caching

### Why

- Some config values are cached at startup (example: `ElitePrimalRifts` is read into a `static readonly` field in the server), so changing the config file at runtime won’t affect behavior until a restart. This causes “toggle doesn’t stop” style reports.

### Do

- Update `Docs/src/content/reference/config.mdx`:
- Add a short troubleshooting note under **General** (or near `ElitePrimalRifts`) stating that some options are cached at startup and require a **server restart** to apply changes.
- Call out `ElitePrimalRifts` explicitly as an example.

### Verify

- Build docs (optional): `Docs/` build succeeds.