# Memory Index

Entry point for repo memory.

## Read order (token-safe)

ALWAYS READ (in order):
1) `hot-rules.md` (tiny invariants, <20 lines)
2) `active-context.md` (this session only)
3) `memo.md` (long-term current truth + ownership)

SEARCH FIRST, THEN OPEN ONLY WHAT MATCHES:
4) `lessons/index.md` -> find lesson ID(s)
5) `lessons/L-XXX-*.md` -> open only specific lesson(s)
6) `digests/YYYY-MM.digest.md` -> before raw journal
7) `journal/YYYY-MM.md` -> only for archaeology

## Files

- Hot rules: `hot-rules.md`
- Active context: `active-context.md`
- Memo: `memo.md`
- Lessons: `lessons/`
- Lesson index (generated): `lessons/index.md` + `lessons-index.json`
- Journal monthly: `journal/YYYY-MM.md`
- Journal index (generated): `journal-index.md` + `journal-index.json`
- Digests (generated): `digests/YYYY-MM.digest.md`
- Tag vocabulary: `tag-vocabulary.md`
- Regression checklist: `regression-checklist.md`
- ADRs: `adr/`

## Maintenance commands

Helper scripts:
- Add lesson: `scripts/memory/add-lesson.ps1 -Title "..." -Tags "..." -Rule "..."`
- Add journal: `scripts/memory/add-journal-entry.ps1 -Tags "..." -Title "..."`
- Rebuild indexes: `scripts/memory/rebuild-memory-index.ps1`
- Lint: `scripts/memory/lint-memory.ps1`
- Query (grep): `scripts/memory/query-memory.ps1 -Query "..."`
- Query (SQLite): `scripts/memory/query-memory.ps1 -Query "..." -UseSqlite`
- Clear session: `scripts/memory/clear-active.ps1`