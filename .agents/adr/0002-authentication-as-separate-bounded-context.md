# ADR-0002: Authentication/Identity is its own bounded context; the Domain stays framework-free

- **Status:** Accepted
- **Date:** 2026-06-24
- **Deciders:** Adrian (decided in working session)
- **Tags:** architecture, ddd, identity, security, foundational
- **Reversibility:** two-way door, but stickier the more data accrues — decide deliberately now.
- **Supersedes:** the earlier "D1-a" recommendation in `ARCHITECTURE_REFACTOR.md` (let Domain reference `Identity.Stores`). Rejected.

## Context and problem statement

The current `AppUser : IdentityUser<int>` (`api/Entities/AppUser.cs`) makes the central user entity inherit
~15 ASP.NET Identity fields — `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, `LockoutEnd`,
`AccessFailedCount`, `TwoFactorEnabled`, email-confirmation tokens, etc. None of that is the *betting*
domain; it's authentication mechanics. Putting it in the Domain layer pollutes the core model with auth
plumbing **and** forces the Domain project to reference an auth framework — breaking the Dependency Rule
from [[ADR-0001]].

The deeper question isn't "how do we tolerate Identity in Domain" — it's **"does authentication belong in
the betting domain at all?"** It does not. Authentication is a *supporting subdomain*. The betting domain
cares about a **Player** with a wallet, avatar, and activity; it does not care about password hashes. And
the project already plans an Auth/Identity microservice — so separating now is just starting where we
intend to finish.

## Decision drivers

- **Framework-free Domain** (the Dependency Rule from [[ADR-0001]]).
- **Separation of concerns / bounded contexts** — auth and betting are different languages.
- **Alignment with the microservices endgame** — the Auth service boundary should exist from day one.
- **Learning DDD boundaries** — modeling "same person, two contexts" is the lesson.

## Options considered

### Option 1 — Let Domain reference ASP.NET Identity (rejected)
Keep `AppUser : IdentityUser<int>` in Domain.
- Rejected: pollutes the core model, breaks the Dependency Rule, optimizes for delivery speed we don't need.

### Option 2 — POCO domain user + Identity persistence entity, same context, mapped
Pure `Player` in Domain; `ApplicationUser : IdentityUser<int>` in Infrastructure; map between them, one context.
- Pure Domain, but auth and betting still share a context/store and a lifecycle.

### Option 3 — Authentication is a separate bounded context (chosen)
- **Betting Domain:** pure `Player`/`Member` aggregate identified by a `UserId`, with betting state
  (display name, `Wallet`/`Money` value object, avatar, activity). No framework references.
- **Identity/Access context:** owns ASP.NET Identity (`ApplicationUser : IdentityUser<int>`, `AppRole`,
  credentials, roles, tokens) entirely within *its* Infrastructure. In the monolith it's the `Identity`
  module; in Phase 8 it becomes the Auth/Identity service.
- **Link:** same logical user, two representations, joined by a shared `UserId`. Registration **orchestrates
  both** contexts (create the auth user, then create the `Player`) — see [[ADR-0003]] for the mechanism.

## Decision

We choose **Option 3**. ASP.NET Identity never appears in the betting Domain; it is an implementation detail
of the Identity bounded context. The betting Domain is framework-free POCOs linked to identity by `UserId`.

> Note the elegance: once auth is its own context, the "must Domain reference `Identity.Stores`?" dilemma
> **disappears** — `IdentityUser` was never a domain entity. A trade-off that vanishes when you move the
> boundary means the boundary was the real problem.

## Consequences

- **Positive:** truly pure Domain; clean separation in ubiquitous language; the Auth service boundary
  exists from the start; models the canonical DDD "one person across contexts" pattern.
- **Negative / trade-offs accepted:** registration (and any user lifecycle change) must orchestrate two
  contexts and keep them consistent — handled via a domain event (`PlayerRegistered`/`UserRegistered`) or an
  application-service orchestration, plus the `UserId` link to maintain. This is real new complexity, taken
  on deliberately for the separation and the learning.
- **Revisit when:** we ever collapse auth back into the core (we won't here).

## Notes / links

- Follows from [[ADR-0001]]; mechanism continues in [[ADR-0003]].
- Code today: `api/Entities/AppUser.cs`, `api/Entities/AppRole.cs`, `api/Entities/AppUserRole.cs`, `api/Entities/Photo.cs`.
- Naming (`Player` vs `Member`) is a ubiquitous-language choice still to confirm.
- References: Khononov *Learning DDD* (bounded contexts, supporting subdomains); Newman *Building Microservices* (service boundaries).
