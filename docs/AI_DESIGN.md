# AI Design

## Current Status

The AI subsystem is design only. No `IAIProvider` interface or concrete provider implementation exists in the current source tree. ADR-0008 defines the accepted provider-independent direction; it does not mean those integrations are implemented.

## Planned Recommendation Flow

```text
Local metadata / analysis evidence
    -> Application recommendation use case / prompt
    -> IAIProvider (planned Application contract)
    -> Selected provider implementation (Infrastructure)
    -> Recommendations with evidence and explanations
    -> ViewModel / UI
```

This is a planned runtime data flow. Infrastructure providers implement the Application contract; Application and Domain must not depend on a provider-specific SDK. Providers are alternatives, not stages in a provider chain.

## Delivery Plan

Provisional Sprint 6 targets the first AI recommendation feature after usable local scanning, duplicate detection, and large-scale validation. Start with a minimal `IAIProvider` contract for that use case and one provider implementation. Provider selection, request / response design, and implementation files require separate planning and approval.

OpenAI, NVIDIA NIM, Anthropic, Gemini, Ollama, and other providers remain possible extensions under ADR-0008. This list is not a commitment to implement all providers in Sprint 6. Future sprint numbers may change; `../ROADMAP.md` defines the provisional sequence.

The natural language / chat assistant remains outside Version 1 and belongs to the PRD's Version 3.0 roadmap. It is distinct from the first recommendation feature.

## Boundaries

- AI receives approved metadata / analysis evidence and returns observations, recommendations, and explanations.
- AI never accesses the filesystem directly or performs file operations.
- Remote AI use is opt-in; metadata transmission follows the documented privacy rules.
- File operations remain a separate confirmation, execution, history, and undo / recovery flow controlled by the user.
- No AI contract or provider is implemented during Sprint 2 or this documentation alignment.
