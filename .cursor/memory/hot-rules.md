# Hot Rules (read this first)

- Do not duplicate ownership: each subsystem has one â€œownerâ€‌ module.
- Do not introduce new global state/singletons/hooks without confirming an established pattern.
- Prefer extending existing helpers and patterns over inventing new ones.
- If a change is version-sensitive or depends on external APIs, verify via primary docs/tools or implement the lowest-risk change and document assumptions.
- Lessons > Memo > Existing Codebase > New Code (authority order).
- After changes: journal entry + relevant regression checks; add a lesson if the bug was non-obvious; update memo if â€œcurrent truthâ€‌ changed.
