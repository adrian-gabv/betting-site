> **Historical reference:** This folder preserves earlier discussions and decisions. Its original process guidance is historical, not a required workflow. Use [AGENTS.md](../../AGENTS.md) and [the technical backlog](../TECHNICAL_PLAN.md) for current guidance; new ADRs are optional.

# Architecture decisions as learning exercises

An ADR records a significant choice, the alternatives considered, and the consequences accepted.
The useful skill is explaining a decision under this project's constraints. Job titles, the number of
patterns used, and the length of an ADR do not measure that skill.

## Current decisions

| ADR | Status | Meaning |
| --- | --- | --- |
| [0001](0001-ddd-and-clean-architecture.md) | Accepted | DDD and Clean Architecture are the chosen learning direction. |
| [0002](0002-authentication-as-separate-bounded-context.md) | Accepted | Identity is separate from the betting model; Domain stays framework-free. |
| [0003](0003-identity-access-across-context-boundary.md) | Proposed | The identity port and registration coordination still need Adrian's decision. |
| [0004](0004-hand-rolled-dispatch-and-mapping.md) | Accepted | Build minimal dispatch and manual mapping to study the mechanisms. |

Accepted records describe the context at their recorded date. Old file paths and phrases like “code today”
in those records are historical, not a current inventory. Use the source and current plans for that.

## When to write one

Write an ADR when a choice affects boundaries, data ownership, a public contract, a core dependency, or
another costly-to-reverse behavior. A short note is enough for a small learning experiment. Routine
formatting and easily reversible implementation details rarely need a separate record.

Use [the template](0000-template.md). Keep the record proportional to the choice:

1. State the problem as a question, including the constraints and learning objective.
2. Compare genuine feasible alternatives, including retaining the current behavior where reasonable.
   Do not invent weak alternatives to make a preferred option win.
3. Explain the cost to build, maintain, test, and reverse each option. Name a concrete failure case.
4. Where uncertainty matters, run a small experiment and record what it did and did not establish.
5. Adrian chooses and explains the trade-off. Record the decision and a condition for revisiting it.

AI can clarify a concept, find sources, challenge assumptions, and review a draft. By default Adrian
writes the reasoning and the decision. AI must not invent agreement or convert a suggestion to Accepted.

## Lifecycle

- Start as `Proposed`. The decision section can contain a recommendation while acceptance is pending.
- Mark `Accepted` only after Adrian decides; record the actual decision date and decider.
- Preserve accepted reasoning. Correcting a typo or link is fine; a changed decision gets a new ADR
  referencing the old one. Update the old status when the new decision supersedes it.
- Keep dated observations separate from the original reasoning, for example in learning session notes
  linked to the ADR. An observation can motivate a later proposal without rewriting history.
- Use the next available four-digit number in creation order; do not renumber existing records.

## Content review notes — 2026-10-06

The accepted decisions remain in force. These notes clarify limitations in their explanations; they do
not claim Adrian has accepted a replacement decision.

- **ADR-0001:** Project references can prevent cycles and missing dependencies; they do not enforce every
  namespace or controller boundary. Additional structure carries a maintenance cost even in a learning
  project. Treat it as an experiment with an outcome to demonstrate, not proof of quality by itself.
- **ADR-0002:** A bounded context is a modeling boundary, not automatically a separate service or database.
  The Player/Wallet/profile examples do not settle every aggregate's ownership. Revisit those details
  when implementing the corresponding use case. Service extraction still needs its own decision.
- **ADR-0003:** The proposal now compares orchestration with event coordination and includes the partial
  failure and retry questions omitted before. The choice remains open.
- **ADR-0004:** Explicit assignments provide type checking, but omitted or semantically wrong mappings
  can still compile. A hand-written dispatcher is not automatically AOT-compatible; its reflection,
  dynamic dispatch, and dependencies determine that. Test the selected deployment mode if it becomes a
  requirement. [Microsoft's AOT guidance](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/fixing-warnings)
  explains the analyzer warnings and runtime verification involved. No AOT claim was tested in this review.
- Package/license statements in historical records must be rechecked against the selected release's
  official terms before future adoption. The reason for learning dispatch/mapping does not require
  treating every third-party framework as trivial to reproduce.

## Next useful exercise

Start with [ADR-0003](0003-identity-access-across-context-boundary.md) when working on account slices:
draw the success path and the “identity created, Player creation failed” path. Explain what state is left
and what a retry does under each option. Then draft your recommendation. AI reviews it before acceptance.

A technology-stack ADR is an optional retrospective exercise. State honestly that the stack was
inherited; do not pretend a fresh competitive selection occurred or rewrite the app just to compare it.

For deeper reference, [MADR](https://adr.github.io/madr/) provides a structured decision format. Read only
what helps the current choice. Current work is tracked in [the technical backlog](../TECHNICAL_PLAN.md).
