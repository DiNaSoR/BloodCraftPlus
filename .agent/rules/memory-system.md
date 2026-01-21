---
description: Mnemo memory system - structured AI memory in .cursor/memory/
alwaysApply: true
---

# Memory System (Mnemo)

This project uses Mnemo for structured AI memory. All memory lives in `.cursor/memory/`.

## Read Order (ALWAYS)
1. `.cursor/memory/hot-rules.md` - tiny invariants (read first)
2. `.cursor/memory/active-context.md` - current session state
3. `.cursor/memory/memo.md` - project truth + ownership

## Search First, Then Fetch
- `.cursor/memory/lessons/index.md` - searchable lesson index
- `.cursor/memory/digests/*.digest.md` - monthly summaries
- `.cursor/memory/journal/*.md` - raw history (last resort)

## Authority Order
1. Lessons override everything
2. active-context overrides memo/journal (but NOT lessons)
3. memo.md is long-term truth
4. Journal is history

## After Any Task
- Update active-context.md during work
- Add journal entry when done
- Create lesson if you discovered a pitfall
- Clear active-context.md when task is merged