# ID Registry (to prevent collisions)

This file tracks the next IDs to use for append-only records.

## Next IDs
- Next Lesson ID: L-001
- Next ADR ID: ADR-0001
- Next Digest ID: DIGEST-0001

## How to allocate (manual, fast)
1) Pick the next ID for the record type.
2) Create the record file/entry using that ID.
3) Increment the â€œNext â€¦ IDâ€‌ here immediately.

## Notes
- Lessons live in `lessons.md` (append-only).
- ADRs live in `.cursor/memory/adr/ADR-XXXX-<slug>.md`
- Digests live in `.cursor/memory/digests/DIGEST-XXXX-<period>.md`
