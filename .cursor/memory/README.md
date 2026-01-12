# Project Memory System

This folder defines a structured, scalable â€œproject memoryâ€‌ that humans and AI tools can reliably use.

## The layers

- `hot-rules.md`:
  - Tiny file of the most important invariants (optimized for tight context).
- `index.md`:
  - Navigation: what to read, where things live, hotspots.
- `profile.md`:
  - Project identity, stack, commands, environments, critical paths.
- `memo.md`:
  - Current truth (curated, editable).
- `lessons.md`:
  - Hard rules from real mistakes (append-only).
- `journal/`:
  - Append-only monthly change logs.
- `regression-checklist.md`:
  - What to verify after changes to avoid regressions.
- `adr/`:
  - Architecture Decision Records (the â€œwhyâ€‌ behind bigger decisions).
- `digests/`:
  - Periodic summaries that compress history.

## What goes where (triage)

- If itâ€™s *true now* and people must know it: `memo.md`
- If itâ€™s a *repeatable mistake we must never reintroduce*: `lessons.md`
- If itâ€™s *what changed and why*: `journal/YYYY-MM.md`
- If itâ€™s a *major decision with alternatives and tradeoffs*: `adr/ADR-XXXX-...md`
- If itâ€™s *a summary of a time period*: `digests/`

## Scaling rules

- Journal is split by month.
- Lessons are append-only; supersede with a new lesson if needed.
- Memo stays short and high-signal.
- Add digests when history gets large.
