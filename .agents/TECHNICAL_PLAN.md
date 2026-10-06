# Technical backlog

Current structure and remaining work. Checkboxes describe implementation; use the source and relevant
checks to confirm behavior when changing it. Choose a small task at a time; no fixed schedule.

## Current baseline

- [x] Four backend projects: Domain, Application, Infrastructure, API; .NET 10 and PostgreSQL/EF Core.
- [x] Identity/JWT, roles, profile endpoints, and local photo storage implemented.
- [x] Angular 22 app scaffolded in `client/`; `client-old/` retained as the UI reference.
- [x] xUnit test projects and client Vitest setup exist. Backend tests are still placeholders.

The structural part of the architecture refactor is implemented. Controllers still call Identity/EF
and AutoMapper directly; the handler, mapping, error-handling, and domain changes below are pending.

## Backend

- [ ] Agree the Identity port and registration flow, including partial failure and retry behavior.
- [ ] Agree expected-error responses and whether to use `Result<T>`; keep unexpected failures separate.
- [ ] Move one endpoint at a time into an Application handler with a focused test.
- [ ] Add minimal hand-written dispatch and validation decorators where needed.
- [ ] Replace AutoMapper incrementally with explicit mapping; remove it after the last caller migrates.
- [ ] Thin controllers to Application calls and HTTP response mapping. Login is state-changing because of lockout tracking.
- [ ] Replace custom exception middleware with `IExceptionHandler`/`ProblemDetails` after agreeing the response contract.
- [ ] Clarify Player, profile, Wallet/Money, and Photo ownership before remodeling persistence.
- [ ] Extract EF configurations as useful; review migrations for existing-data effects.
- [ ] Add unit tests for behavior and integration/API tests for persistence and HTTP contracts.
- [ ] Review the Application project's ASP.NET framework reference and remove it if unnecessary.

## Client

- [ ] Build the app shell and login/register pages with typed forms.
- [ ] Add account state, HTTP calls, scoped auth/error interceptors, and route guards.
- [ ] Choose token/session handling deliberately; server authorization remains authoritative.
- [ ] Establish a small set of shared styles/components as pages need them.
- [ ] Port features from the legacy app without copying obsolete Angular patterns.
- [ ] Add useful component/service tests and a smoke test for the main user flows.
- [ ] Check responsive layouts and accessibility with automated and manual checks.

## Development and delivery

- [ ] Capture current dependency findings; address high/critical issues in the active API/client before merge, or record an explicit exception. Legacy-client findings are informational.
- [ ] Add CI for backend/client builds, tests, linting, and dependency checks.
- [ ] Containerize the active client and document running the app with PostgreSQL.
- [ ] Add useful structured logs, health checks, and basic request tracing.

## Later

- Refresh tokens, revocation, OAuth/OIDC, MFA, and API versioning as needed.
- Explicit Identity/Wallet/Betting/Social module boundaries and communication contracts.
- Performance/query analysis, caching, and load testing.
- One service-extraction experiment with retry, idempotency, and recovery understood.
- Local Kubernetes and an Azure deployment; explore further platform tooling only when useful.

These remain possible directions, not prerequisites for implementing the core features.
