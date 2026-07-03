# Architecture Decisions

> This document records long-term architectural decisions for FileLens.
>
> The purpose is to explain **why** important decisions were made, not just **what** was implemented.
>
> Once a decision is recorded here, it should only change after careful discussion.

---

# ADR-0000 — Project Philosophy

**Status**

Accepted

**Date**

2026-07-03

## Vision

Build software that helps people understand their files instead of taking control away from them.

## Core Principles

- Explainability over automation.
- Transparency over convenience.
- Safety over speed.
- Maintainability over shortcuts.
- Human decision over AI autonomy.

These principles take precedence over feature implementation whenever trade-offs are required.

---

# ADR-0001 — Target Framework

**Status**

Accepted

**Date**

2026-07-03

**Decision**

FileLens targets **.NET 10**.

**Reasoning**

- Latest Windows desktop development platform.
- Long-term maintainability.
- Full compatibility with modern C# features.
- Native support for the latest Microsoft.Extensions ecosystem.

---

# ADR-0002 — Architecture

**Status**

Accepted

**Date**

2026-07-03

**Decision**

FileLens follows **Strict Clean Architecture**.

```text
        UI
        │
        ▼
 Application
        │
        ▼
     Domain

Infrastructure
        │
        └──── implements interfaces defined by Application
```


**Rules**

- Domain has no dependencies.
- Application depends only on Domain and Shared.
- Infrastructure depends on Application, Domain, and Shared.
- UI depends only on Application and Shared.
- Infrastructure must never be referenced directly by UI.

**Reasoning**

- High maintainability.
- Easy testing.
- Low coupling.
- Replaceable infrastructure.

---

# ADR-0003 — AI Philosophy

**Status**

Accepted

**Date**

2026-07-03

**Decision**

FileLens uses AI as a recommendation engine, **not an automation engine**.

**Principles**

- AI recommends.
- Users decide.
- Nothing happens automatically.

Every AI recommendation must include an explanation.

Every file operation requires explicit user approval.

**Reasoning**

Users should never lose control of their own files.

---

# ADR-0004 — Privacy

**Status**

Accepted

**Date**

2026-07-03

**Decision**

Privacy-first by default.

**Rules**

- Folder access is explicit.
- No administrator privilege unless required.
- Metadata analysis first.
- File content analysis requires user permission.
- OpenAI API usage is opt-in.

---

# ADR-0005 — Documentation First

**Status**

Accepted

**Date**

2026-07-03

**Decision**

Major features are designed before implementation.

Development order:

PRD

↓

Engineering Spec

↓

Implementation Plan

↓

Codex Review

↓

Implementation

↓

Human Review

↓

Git Commit

**Reasoning**

Architecture should evolve intentionally rather than organically.

---

# ADR-0006 — Git Workflow

**Status**

Accepted

**Date**

2026-07-03

**Decision**

Every Sprint follows the same workflow.

Plan

↓

Review

↓

Codex Implementation

↓

Human Review

↓

Git Commit

↓

GitHub Push


No feature is committed without review.

---

# ADR-0007 — Dependency Injection

**Status**

Accepted

**Date**

2026-07-03

**Decision**

All dependencies must be resolved through Microsoft.Extensions.DependencyInjection.

**Rules**

- No Service Locator.
- No static singleton services.

**Reasoning**

Using a single DI container keeps composition consistent, improves testability, and avoids hidden dependencies.

---

# Future Decisions

Additional architectural decisions will be appended here as the project evolves.
Future ADR numbers will be assigned sequentially as architectural decisions become permanent.

Examples:

- AI Provider abstraction
- Plugin system
- Database migration strategy
- Cross-platform support
- Local AI integration