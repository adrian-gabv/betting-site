# ADR-0003: Identity access and registration across context boundaries

- **Status:** Proposed — awaiting Adrian's decision
- **Date:** 2026-06-24
- **Proposal revised:** 2026-10-06 — clarify alternatives and failure semantics; no decision accepted
- **Deciders:** —
- **Tags:** architecture, identity, ports-and-adapters, consistency
- **Reversibility:** the port is relatively easy to change; persisted cross-context state is harder

## Context and problem statement

[ADR-0001](0001-ddd-and-clean-architecture.md) and
[ADR-0002](0002-authentication-as-separate-bounded-context.md) keep Identity outside the betting Domain.
The current account controller still directly uses `UserManager`, `SignInManager`, EF queries, and token
issuance. A future separate Player model adds another write to registration.

How should use cases call Identity without depending on its framework types, and how should registration
keep identity, roles, and the future Player consistent when part of the work fails?

An Application service coordinates work; a port describes a needed capability; an Infrastructure adapter
implements framework calls. A Domain Service expresses business policy that does not fit an entity.
Identity can have business policies of its own, but wrapping `UserManager` does not make password hashing
a betting Domain Service. These distinctions are about responsibilities, not naming conventions.

## Decision drivers

- Understand and test use cases without building a web host for every test.
- Keep Application contracts free of ASP.NET Identity types.
- Make incomplete registration, retry, and concurrency behavior explicit.
- Keep the implementation small enough to explain and maintain in a solo learning project.

## Options considered

### A — Intent-based port with direct application orchestration

Define the smallest Identity port required by the first handler; implement it in Infrastructure. The
registration application service coordinates identity, role assignment, and Player creation.

- **Benefits:** a visible call sequence; straightforward handler tests; an opportunity to study a local
  transaction without introducing a message transport.
- **Costs:** orchestration owns consistency. Multiple `SaveChanges` calls are not automatically one
  transaction. Verify the actual Identity store, DbContext, connection, and transaction enlistment. If
  the writes cannot share a transaction, define compensation or an explicit pending state.

### B — Intent-based port with an explicit event contract

Keep the port, and publish a registration event consumed by the Player context.

- **Benefits:** explicit notification contract; useful for studying decoupling and eventual consistency.
- **Costs:** define when publication occurs, delivery guarantees, retries, duplicate handling, and the
  user-visible state while processing is incomplete. An in-process callback is not durable delivery and
  does not by itself make the writes atomic. An outbox/consumer design adds work if durability is needed.

A domain event within one model and an event published across contexts serve different scopes. Choose
the contract deliberately; do not equate a C# event handler with a reliable distributed workflow.

### C — Keep direct Identity calls in API while deferring the refactor

- **Benefits:** no immediate change to the working baseline; useful while focusing on Angular.
- **Costs:** account behavior remains harder to test independently; this does not deliver the planned
  Application boundary. Moving `UserManager` into Application would conflict with the intended separation.

## Proposed direction, not accepted

Evaluate A first with one handler and failure-path tests because it exposes the mechanics with fewer
moving parts. Compare B as a separate experiment if event delivery is the learning objective. Adrian
chooses after explaining the trade-offs; this review does not authorize either design as final.

Before accepting, answer:

1. Which context owns each operation and what exact result/error contract does the first caller need?
2. Can the local writes actually share a transaction, and how will we demonstrate rollback on failure?
3. If identity succeeds and role/Player creation fails, what remains and how does a retry recover?
4. How are duplicate or concurrent registrations handled without relying on a pre-check alone?
5. When can the API report success or issue a token, and what response does an incomplete operation get?

Login currently updates lockout state on failed attempts. Treat it as a state-changing operation when
choosing a command/query name; do not infer “query” just because it returns a token.

## Consequences to evaluate

- **Benefit sought:** testable use cases and a visible boundary to Identity.
- **Cost accepted only after a decision:** maintaining the port and the chosen consistency mechanism.
- **Revisit when:** Identity is extracted into a separate service. Network calls introduce latency,
  timeouts, partial failure, and contract versioning; an in-process port is not automatically a finished
  RPC contract.

## Learning evidence

Pending: Adrian's explanation of both failure paths, a small experiment or test where useful, and the
actual decision. See [the technical backlog](../TECHNICAL_PLAN.md).
