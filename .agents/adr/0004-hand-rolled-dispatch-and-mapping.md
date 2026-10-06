# ADR-0004: Hand-rolled CQRS dispatch and manual mapping instead of MediatR/AutoMapper

- **Status:** Accepted
- **Date:** 2026-07-02
- **Deciders:** Adrian (decided in working session)
- **Tags:** backend, application-layer, dependencies, licensing
- **Reversibility:** two-way door — the handler seams stay library-shaped, so adopting a framework later is a mechanical swap.

## Context and problem statement

Phase 1B introduces CQRS (one handler per use case, a validation pipeline) and entity↔DTO mapping. The
plans assumed **MediatR** for dispatch and **AutoMapper** for mapping — AutoMapper 16.x is already in the
code (`src/BettingSite.Infrastructure/Mappings/AutoMapperProfiles.cs`).

Two forces changed the calculus:

1. **Licensing.** MediatR (v13+) and AutoMapper (v15+) moved to commercial licensing in 2025. Free tiers
   cover a personal project today, but the terms are someone else's to change — and the alternative costs
   almost nothing here.
2. **The learning goal.** This project exists to *understand* patterns, not consume them. MediatR is
   essentially a DI-resolved dispatch call plus a decorator pipeline; AutoMapper is reflection-driven
   property copying. Both are small enough to build, and building them teaches exactly the
   mediator/decorator/mapping mechanics the frameworks hide.

The question: frameworks, free framework alternatives, or hand-rolled?

## Decision drivers

- **Learning value** — the project's objective function; understanding beats convenience.
- **Licensing independence** — no commercial license in the core architecture.
- **Carrying cost** — a solo codebase must afford whatever it owns.
- **Testability / explicitness** — handlers unit-testable; mapping errors caught at compile time, not runtime.

## Options considered

### Option A — MediatR + AutoMapper (the original plan)
- **Pros:** mature, documented everywhere, the .NET community default; zero plumbing to write.
- **Cons:** commercial licenses (MediatR v13+, AutoMapper v15+); reflection magic hides the two patterns we
  most want to learn; mapping errors surface at runtime; another vendor's roadmap in the dependency graph.

### Option B — Free / source-generated alternatives (`Mediator` (martinothamar), Wolverine; Mapperly)
- **Pros:** free licenses; source generators give compile-time wiring, speed, AOT-friendliness.
- **Cons:** still frameworks that hide the mechanics — swapping one dependency for another serves the
  licensing driver but not the learning one; Wolverine is far heavier than this project needs. Mapperly
  remains the strongest fallback if manual mapping boilerplate ever becomes a real tax.

### Option C — Hand-rolled dispatch + manual mapping (chosen)
Own `ICommand<TResult>` / `IQuery<TResult>` / handler interfaces in Application; a minimal dispatcher that
resolves handlers from DI; pipeline behaviors (validation first, logging later) as plain decorators.
Mapping via explicit mapper classes / `ToDto()` extension methods owned by each slice.
- **Pros:** every line understood; zero license exposure; compile-time-safe mapping; trivially AOT-clean;
  the dispatcher is tens of lines, not a framework.
- **Cons:** we own the plumbing (DI lifetimes, pipeline ordering, notifications/streaming if ever needed);
  no community documentation for it; mapping boilerplate must be kept in sync with DTOs by hand.

## Decision

We chose **Option C — hand-rolled dispatch and manual mapping**, because learning value and licensing
independence dominate this decision and the carrying cost is genuinely small at this scale. We keep
library-shaped seams (one handler interface per use case, behaviors as decorators) so Options A/B remain a
mechanical migration if the plumbing ever outgrows us.

Plan impact: `TECHNICAL_PLAN.md` (Phase 1B, Phase 7 domain events) and `ARCHITECTURE_REFACTOR.md` now say
"hand-rolled dispatcher / manual mapping" wherever they said MediatR/AutoMapper. The AutoMapper package
already in Infrastructure is removed during 1B as slices absorb the mapping. **FluentValidation stays** —
it remains free, and validation rule syntax is not a pattern we need to learn by writing it.

## Consequences

- **Positive:** first-hand understanding of the mediator, decorator, and mapping mechanics; no commercial
  license in the core; compile-time mapping safety; a smaller dependency graph.
- **Negative / trade-offs accepted:** we maintain the dispatcher and every pipeline feature ourselves;
  mapping boilerplate grows with each DTO and can drift from entities (mitigate with mapping unit tests in
  Phase 2); no Stack Overflow answers for our own plumbing.
- **Revisit when:** pipeline needs grow past validation/logging (notification fan-out, streaming,
  scheduling) or mapping boilerplate becomes a measurable tax — then adopt Mapperly and/or a source-gen
  mediator through the preserved seams, with a superseding ADR.

## Notes / links

- Related ADRs: [[ADR-0001]] (Clean Architecture / CQRS direction), [[ADR-0003]] (handlers depend on ports).
- Resolves the former "MediatR vs hand-rolled dispatch" open item in the ADR log. The broader
  **technology stack** ADR (the exercise in `README.md`) remains open.
- Code today: `src/BettingSite.Infrastructure/Mappings/AutoMapperProfiles.cs`; `AutoMapper 16.1.1` in
  `BettingSite.Infrastructure.csproj` — removal is a Phase 1B step.
