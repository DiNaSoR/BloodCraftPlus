# Regression Checklist

Use this after relevant changes. Record what you ran (PASS/FAIL) in the journal entry.

## Always (every meaningful change)
- Build succeeds
- Lint/format passes (if applicable)
- Unit tests pass (if present)
- App/service starts successfully (if applicable)

## Critical user flows (customize)
- Flow 1: <describe> â€” PASS/FAIL
- Flow 2: <describe> â€” PASS/FAIL
- Flow 3: <describe> â€” PASS/FAIL

## Persistence / config
- Config loads correctly on startup
- Changes persist across restart/reload
- Migrations do not re-run destructively

## External integrations (if touched)
- Auth flow (token refresh, permissions)
- API schema compatibility
- Retries/backoff behave correctly (no tight loops)
- Error handling produces actionable logs

## Concurrency / async (if touched)
- No duplicate scheduling / double handlers
- Cancellation/shutdown is clean
- No unbounded queues/timers

## UI (if touched)
- Primary screens render
- Localization (if applicable)
- Accessibility checks (if applicable)

## Performance-sensitive areas (if touched)
- Hot path remains within budget
- No accidental N+1 queries
- No large allocations in loops
