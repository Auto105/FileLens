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

# ADR-0008 — AI Provider Abstraction

**Status**

Accepted

**Date**

2026-07-07

**Decision**

FileLens must remain independent of any specific AI provider.

All AI functionality shall be accessed through the `IAIProvider` abstraction.

**Rules**

- Business logic must never depend on a vendor-specific SDK.
- Provider-specific implementations belong only in the Infrastructure layer.
- Every AI provider must implement the same abstraction.
- New providers can be added without modifying the Application or Domain layers.
- Users may freely choose their preferred AI provider.

**Reasoning**

The AI ecosystem evolves rapidly.

FileLens is designed to support multiple providers, including OpenAI, NVIDIA NIM, Anthropic, Google Gemini, Ollama, LM Studio, and future providers.

By introducing a provider abstraction from the beginning, FileLens remains vendor-independent, easier to maintain, and adaptable to future AI technologies.

---

# ADR-0009: Incremental Vertical Development

**Status:** Accepted

## Context

FileLens is intended to be a long-term project with a strong emphasis on maintainability, architecture, and explainable behavior.

Large features such as folder scanning, metadata extraction, duplicate detection, and AI recommendations should never be implemented in a single iteration.

Instead, every feature should evolve through small, independently verifiable steps.

## Decision

All major features shall be implemented using incremental vertical slices.

The preferred development sequence is:

```text
Contract
    ↓
DTO / Models
    ↓
Skeleton Implementation
    ↓
Working Implementation
    ↓
Refinement
    ↓
Optimization
```

Each stage must:

- Build successfully.
- Be independently reviewable.
- Preserve Clean Architecture boundaries.
- Avoid implementing unrelated functionality.

## Consequences

Benefits:

- Easier code reviews.
- Smaller implementation scope.
- Better architectural consistency.
- Simpler debugging.
- Lower regression risk.
- Easier collaboration with AI coding assistants.

Trade-offs:

- More commits.
- More planning.
- Some temporary placeholder implementations will exist until later sprint stages.

These trade-offs are considered acceptable for a long-term maintainable project.

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