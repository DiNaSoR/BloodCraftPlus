# Memory Index

## Agent rules (repo-wide)

- Repository-wide agent rules live in `AGENTS.md` (read before coding).
- Project memory system entry point is this file: `.cursor/memory/index.md`.

## Read order (fast + reliable)

1) `hot-rules.md` (tiny "do not break" list)
2) `memo.md` (current truth)
3) `lessons.md` (hard rules from past failures)
4) `regression-checklist.md` (if touching relevant areas)
5) Latest journal month in `journal/` (what changed recently)

## Memory map

- Current truth: `memo.md`
- Never-break rules: `lessons.md`
- Change history (monthly): `journal/`
- Journal index: `journal.md`
- Regression checks: `regression-checklist.md`
- Architecture decisions: `adr/`
- Period digests: `digests/`
- IDs: `id-registry.md`

## Hotspots (customize for your project)

- Critical user flows:
  - `<path or module>`
- Persistence / config:
  - `<path or module>`
- Integrations / external APIs:
  - `<path or module>`
- Concurrency / async / queues:
  - `<path or module>`
- Security / auth / permissions:
  - `<path or module>`
- Performance-sensitive code:
  - `<path or module>`

## How to add new memory

- Bug fixed + non-obvious root cause -> add a lesson
- Any change worth "why later" -> add a journal entry
- Any stable truth changes -> update memo
- Any big refactor decision -> add an ADR
