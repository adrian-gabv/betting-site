# ADR-0003: How the betting context performs identity/auth operations across the boundary

- **Status:** Proposed — awaiting decision
- **Date:** 2026-06-24
- **Deciders:** — (proposed; your call)
- **Tags:** architecture, ddd, identity, ports-and-adapters
- **Reversibility:** two-way door.

## Context and problem statement

Given [[ADR-0002]] (auth is its own context, ASP.NET Identity lives only in Identity-context Infrastructure),
how do use cases perform auth-related work — register, login, "who is this user", role checks — without the
Application/Domain coupling to ASP.NET Identity (`UserManager`/`SignInManager`)?

You floated **Domain Services** for this. Worth pinning down the vocabulary, because it decides the answer:

- **Domain Service** (Domain layer, *is* domain logic): a domain verb spanning aggregates — e.g. bet
  settlement, payout math, wallet-to-wallet transfer. → the right tool for the **betting/wallet** domain.
- **Application Service** (Application layer, *orchestration*): the CQRS handlers — open a transaction,
  call ports/repos, return `Result`.
- **Infrastructure adapter behind a Port** (Infrastructure, *technical mechanism*): password hashing, token
  issuance, the ASP.NET Identity calls.

Authentication is a *technical/supporting* capability, not betting-domain logic — so it is **not** a Domain
Service. The fit is a **port/adapter (hexagonal)** at the Identity-context boundary.

## Decision drivers

- **Testability** — handlers must be unit-testable with a fake, no web host.
- **Dependency direction** — Application depends on an interface it owns, not on ASP.NET Identity.
- **Honest abstraction** — the port must express *intent*, not proxy `UserManager` 1:1.

## Options considered

### Option A — Port/adapter at the Identity-context boundary (recommended)
Define an intent-revealing port (e.g. `IIdentityService` / `IAuthenticationService` with
`RegisterUser`, `VerifyCredentials`, `IssueToken`, `AddToRole` → returning `Result`). ASP.NET Identity adapter
implements it in Infrastructure. Cross-context user creation flows via a `UserRegistered` domain event that a
betting-context handler turns into a `Player`.
- **Pros:** testable handlers; Application free of Identity; clean context seam; becomes the Auth-service RPC contract later.
- **Cons:** one port + adapter to design well (resist the 1:1-wrapper anti-pattern).

### Option B — Domain Service for auth
Model authentication as a Domain Service in the Identity context's domain.
- **Cons:** miscategorizes a technical concern as domain logic; the heavy lifting (hashing/tokens) is still
  infrastructure, so you end up with a port anyway. Save Domain Services for betting/wallet logic.

### Option C — Handlers use `UserManager` directly (rejected)
- Couples Application to ASP.NET Identity; handler tests need a real/heavily-mocked `UserManager`. Defeats [[ADR-0001]]/[[ADR-0002]].

## Decision

**TBD — proposed: Option A.** Confirm before we build the Account slices in Phase 1.

## Consequences (if Option A)

- **Positive:** unit-testable auth handlers; Application/Domain stay Identity-free; the port doubles as the
  future Auth-service contract.
- **Negative / trade-offs accepted:** must design the port around intent and own the cross-context
  orchestration (event/handler) for keeping auth-user and `Player` consistent.
- **Revisit when:** the Auth context becomes a real service — the port becomes a network contract (gRPC/HTTP).

## Notes / links

- Follows from [[ADR-0001]], [[ADR-0002]]. Domain Services revisited for the betting/wallet domain (future ADR).
- References: Cockburn (Hexagonal / Ports & Adapters); Khononov *Learning DDD* (service types); Vernon *Implementing DDD*.
