# ADR-0001: Adopt Domain-Driven Design + Clean Architecture

- **Status:** Accepted
- **Date:** 2026-06-24
- **Deciders:** Adrian (decided in working session)
- **Tags:** architecture, ddd, clean-architecture, foundational
- **Reversibility:** one-way-ish — this is the foundation the rest of the project is built on.

## Context and problem statement

Today the backend is a single `api/` project with tutorial-style fat controllers: a controller can touch
EF, Identity, Cloudinary and business rules all at once. That runs, but it blocks the project's actual
goal — using this codebase to grow from *writing code that works* to *engineering systems*: bounded
contexts, enforced boundaries, testable business logic, and a structure that scales toward the modular
monolith (Phase 7) and microservices (Phase 8) the project is heading for.

The question: what architectural style do we commit to as the backbone?

## Decision drivers

- **Learning value** — the explicit objective function of this project. The "extra" structure *is* the lesson.
- **Testability** — business logic must be unit-testable with no web host and no database.
- **Maintainability / evolvability** — boundaries the compiler enforces, so concerns can't leak.
- **Alignment with the endgame** — the chosen style should make the later modular-monolith → microservices
  path a continuation, not a rewrite.

## Options considered

### Option A — Keep a single project, organized by technical folders
Tidy the existing structure (Controllers/Services/Data) but stay one project, one layer.
- **Pros:** zero migration; simplest.
- **Cons:** no enforced boundaries; business logic stays entangled with framework code; teaches nothing new.

### Option B — Clean Architecture, single bounded context
Four layers (Domain / Application / Infrastructure / API) with the Dependency Rule, but one big domain.
- **Pros:** real boundaries; testable; modern .NET norm.
- **Cons:** without context boundaries, the "User = auth + betting all in one" blur persists (see [[ADR-0002]]).

### Option C — Clean Architecture organized by DDD bounded contexts (chosen)
The Dependency Rule (dependencies point inward; **Domain depends on nothing external**), *and* the model is
split into bounded contexts (Identity/Access, Betting Core, Wallet, Social) with their own ubiquitous
language. CQRS vertical slices inside the Application layer.
- **Pros:** the cleanest expression of the goal; contexts are the natural seams for later module/service
  extraction; forces real DDD modeling (aggregates, value objects, domain events, domain services).
- **Cons:** most upfront thinking and structure — which, for a learning project, is the point, not a cost.

## Decision

We adopt **Option C**: **Clean Architecture (Dependency Rule, framework-free Domain) organized by DDD
bounded contexts**, with CQRS vertical slices in the Application layer. "Dumb" ceremony that doesn't serve
clarity or learning is explicitly *not* adopted — every layer must justify itself.

## Consequences

- **Positive:** enforced boundaries; pure, testable Domain; contexts double as the future service split;
  the codebase becomes a teaching vehicle for the patterns the project exists to learn.
- **Negative / trade-offs accepted:** more projects and more moving parts than a CRUD app needs; some
  mapping/orchestration overhead between layers and contexts. Accepted deliberately.
- **Revisit when:** never wholesale, but individual *tactics* (e.g. CQRS vs plain services per slice) stay
  open and get their own ADRs as we hit them.

## Notes / links

- Enables: [[ADR-0002]] (auth as a separate context), and the Phase 7 modular monolith / Phase 8 microservices in `../PLAN.md`.
- References: R. C. Martin *Clean Architecture* (the Dependency Rule); Khononov *Learning DDD* (bounded contexts);
  Richards & Ford *Fundamentals of Software Architecture* (trade-offs, the two Laws).
