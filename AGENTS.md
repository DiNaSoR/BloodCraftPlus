# AGENTS.md — Universal + Project-Aware Coding Principles

This document defines **mandatory operational rules** for AI agents contributing to this repository.
It combines **general software engineering best practices** with a **project-specific memory and learning system**
designed to prevent regressions and repeated mistakes.

---

## 🧠 Authority & Memory Model (CRITICAL)

This project uses a **multi-layer memory system**.
All agents MUST respect the following authority order:

1. `.cursor/memory/lessons.md` — hard rules learned from past mistakes (never violate)
2. `.cursor/memory/memo.md` — current truth and system state
3. `.cursor/memory/journal/` — historical context (append-only, monthly files)
4. Existing codebase
5. New code or refactors

If any conflict exists, **higher authority always wins**.

---

## 📚 Mandatory Memory Usage

### Quick navigation (memory)

- Entry point: `.cursor/memory/index.md`
- Hot rules: `.cursor/memory/hot-rules.md`
- Current truth: `.cursor/memory/memo.md`
- Never-break rules: `.cursor/memory/lessons.md`
- Change history (monthly): `.cursor/memory/journal/YYYY-MM.md` (or `THIS-MONTH.md` until renamed)
- Regression checklist: `.cursor/memory/regression-checklist.md`
- Architecture decisions: `.cursor/memory/adr/`
- Period digests: `.cursor/memory/digests/`

### Memory locations

- `.cursor/memory/memo.md`
  **Purpose:** Current system behavior and invariants  
  **Rule:** MUST be read before writing or modifying code.

- `.cursor/memory/journal/`
  **Purpose:** Append-only change history (monthly files)  
  **Rule:** MUST be updated after any meaningful feature or fix.

- `.cursor/memory/lessons.md`
  **Purpose:** Recorded mistakes, root causes, and permanent rules  
  **Rule:**
  - MUST be read before coding.
  - MUST NOT be violated.
  - MUST be updated whenever a non-obvious bug or incorrect assumption is discovered.

---

## 🔁 Mandatory Workflow (Non-Optional)

### Before writing any code

1. Read `.cursor/memory/index.md` (orientation + hotspots).
2. Read `.cursor/memory/hot-rules.md` (tiny do-not-break list).
3. Read `.cursor/memory/memo.md` to understand the current system state.
4. Read `.cursor/memory/lessons.md` and ensure no rule is violated.
5. Inspect the existing codebase for similar patterns, helpers, or owners.

### While writing code

- Do not introduce duplicate systems, hooks, or parallel logic.
- Do not force refresh external frameworks/APIs unless explicitly documented as safe.
- Prefer data-driven behavior over toggle- or assumption-driven logic.
- Use existing helpers and shared modules whenever possible.
- If behavior depends on external APIs/SDKs/framework versions, verify with primary sources (official docs/changelogs) or document assumptions in the journal.

### After completing a task (feature or fix)

1. Append an entry to the active monthly journal file in `.cursor/memory/journal/` (e.g. `YYYY-MM.md`) describing:
   - What changed
   - Why it changed
   - Key files involved
   - Verification performed (docs/tools) and assumptions
   - Regression checks run (PASS/FAIL)
2. If a bug, regression, or misunderstanding was involved:
   - Add a new entry to `.cursor/memory/lessons.md` documenting:
     - The symptom
     - The real root cause
     - The wrong approach
     - The correct rule going forward
3. If the change affects “current truth” (ownership, invariants, defaults):
   - Update `.cursor/memory/memo.md` and bump its `Last updated:` date.
4. Run the relevant items from `.cursor/memory/regression-checklist.md` and record results in the journal.

---

## 🧠 Structured Reasoning & Design

- **Single Responsibility Principle (SRP):**  
  Each class, module, or function must have one clear responsibility.

- **Explicit Intent:**  
  Code must explain *what it does* and *why*, not merely *how*.

- **Readability Over Cleverness:**  
  Prefer clarity and predictability over compact or “smart” solutions.

---

## 🎯 Clarity & Explicitness

- Use explicit, descriptive names for functions, variables, and modules.
- Avoid ambiguous or generic naming.
- Code intent must be obvious without external explanation.

---

## ♻️ Incremental & Safe Change

- Prefer small, incremental changes.
- Validate behavior after each step.
- Refactor only when behavior is fully understood and stable.
- For large refactors, prefer writing an ADR under `.cursor/memory/adr/`.

---

## 🛡️ Robustness & Regression Prevention

- Never reintroduce a bug documented in `lessons.md`.
- Avoid “quick fixes” that bypass state, timing, or lifecycle rules.
- Prefer re-application after state/layout changes over forced refreshes when dealing with UI lifecycle timing issues.
- Assertions and defensive checks are encouraged when assumptions exist.

---

## 🏗️ Architecture & Patterns

- Favor modular design and separation of concerns.
- Prefer composition over inheritance where practical.
- Use patterns (Factory, Strategy, Repository, Dependency Injection) only when they reduce complexity—not by default.
- Maintain clear ownership boundaries: each subsystem should have a single owning implementation.

---

## ♻️ Maintainability Rules

- Avoid magic numbers; use constants or configuration.
- Keep functions short and focused.
- Group related logic logically and consistently.

---

## 🚦 Consistency & Style

- Follow established naming and formatting conventions.
- Respect existing project conventions over personal preference.
- Use automated formatting and linting when available.

---

## 📈 Performance Awareness

- Be conscious of performance-critical paths.
- Avoid unnecessary allocations or repeated work.
- Choose data structures intentionally.

---

## 🧠 Learning Is Mandatory (Not Optional)

This project treats mistakes as **training data**, not failures.

- If an issue caused confusion, regression, or repeated fixes:
  → It MUST be recorded in `lessons.md`.
- Future agents are expected to **learn from past pain**, not rediscover it.

---

## 🧭 Final Principle

> The goal is not just to make the code work —  
> the goal is to make the system **hard to misuse and hard to regress**.
