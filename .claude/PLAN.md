# Project Plan

Casino-style betting site used as a personal learning sandbox for modern .NET 10, Angular 21, Clean Architecture, microservices, DevOps, and cloud engineering (Azure).

---

## How we work together

**The goal is learning, not just shipping.** Every architectural decision should be understood, not just applied.

### Workflow per session
1. Pick an epic from the backlog below.
2. Ask me to plan it first if the scope is unclear — I'll lay out the approach and you approve before I write any code.
3. I implement. You review the **actual diff**, not just my description of it.
4. You run the app/tests to verify. I don't claim success on UI changes without visual confirmation.
5. Use `/clear` between unrelated sessions to reset context and save tokens.

### What I do with agents/subagents
- I spawn subagents for parallel file reads (exploring the codebase) and isolated research tasks.
- I keep complex multi-file implementations inline in the main context so you can follow the reasoning.
- Plan mode (`/plan` or just ask me to "plan X before coding") for architectural choices — I sketch, you approve, then I implement.

### What you do
- Approve architectural direction before implementation starts.
- Run the app yourself to test golden paths and edge cases.
- Push back when something feels like magic — ask me to explain the why, not just the what.
- Scope requests to one epic at a time. "Do everything" prompts produce shallow results.

---

## Phase 0 — Build Baseline
*Goal: verify the project runs cleanly end-to-end before touching architecture.*

### Server
- [ ] Collapse `Startup.cs` → top-level `Program.cs` (standard .NET 6+ minimal hosting)
- [ ] Fix any .NET 10 warnings/deprecations in the existing code
- [ ] Verify `dotnet build` and `dotnet run` succeed
- [ ] Confirm EF migrations apply cleanly against a local Postgres

### Client
- [ ] Verify `npm start` serves at `https://localhost:4200`
- [ ] Tailwind v4 PostCSS pipeline confirmed working
- [ ] Angular dev proxy to `https://localhost:5001` configured

### Infrastructure
- [ ] `docker-compose.yml` for local dev: API + PostgreSQL (so no manual Postgres install needed)
- [ ] `.env.example` with all required config keys documented
- [ ] Confirm secrets strategy: `dotnet user-secrets` for API, `.env.local` for client

---

## Phase 1 — API: Clean Architecture
*Goal: move from tutorial-style fat controllers to a maintainable, testable structure with proper boundaries.*

### Architecture
```
BettingSite.sln
├── src/
│   ├── BettingSite.Domain/          # Entities, Value Objects, Domain Events, Interfaces
│   ├── BettingSite.Application/     # CQRS Commands/Queries (MediatR), Validators (FluentValidation), Result<T>
│   ├── BettingSite.Infrastructure/  # EF Core, Repositories, JWT, Cloudinary, SignalR hubs
│   └── BettingSite.API/             # Controllers (or Minimal API endpoints), Middleware, DI wiring
└── tests/
    ├── BettingSite.Domain.Tests/
    ├── BettingSite.Application.Tests/
    └── BettingSite.Infrastructure.Tests/
```

### Server
- [ ] Create solution structure with four projects above
- [ ] Migrate `AppUser`, `AppRole`, `Photo` entities → `Domain` project as proper domain models
- [ ] Introduce `Result<T>` / `Error` pattern (replace exception-driven flow)
- [ ] Application layer: `RegisterCommand`, `LoginQuery`, `GetMembersQuery`, `UpdateMemberCommand` as first vertical slices
- [ ] FluentValidation validators for all commands
- [ ] Infrastructure: move EF `DataContext`, repositories, JWT service, Cloudinary service
- [ ] API: thin controllers calling MediatR — one action per endpoint, no business logic
- [ ] Replace constructor injection with `inject()` pattern (.NET 10 primary constructors where applicable)
- [ ] Audit EF Core: snake_case naming convention (already using `EFCore.NamingConventions`), owned types for `Photo`, explicit relationship configs
- [ ] Global exception handler modernized: `IExceptionHandler` (.NET 8+ interface) replacing custom middleware

---

## Phase 2 — Testing Foundation
*Goal: establish the test pyramid before adding more features.*

### Server
- [ ] xUnit + FluentAssertions + NSubstitute setup in solution
- [ ] Unit tests: Application layer commands/queries (pure C#, no EF, no HTTP)
- [ ] Integration tests with **TestContainers** (real Postgres container per test run — no mocking the DB)
- [ ] `WebApplicationFactory` API tests: full HTTP request/response cycle
- [ ] Test data builders (not fixtures) for clean arrange steps
- [ ] CI gate: tests must pass before merge

### Client
- [ ] Vitest confirmed running with `npm test`
- [ ] First component unit test (auth service or app component)

---

## Phase 3 — Angular Client Core
*Goal: auth + shell working so features can be built on top.*

### UI
- [ ] HTTP client setup with `provideHttpClient(withInterceptors([...]))`
- [ ] JWT interceptor (attach token to requests) — functional interceptor style
- [ ] Error interceptor (handle 401, 500 globally)
- [ ] `AccountService` with signals: `currentUser = signal<User | null>(null)`
- [ ] Auth guard and role-based guard
- [ ] App shell: nav, layout skeleton, responsive breakpoints (Tailwind)
- [ ] Login / Register pages (reactive forms)

---

## Phase 4 — Angular Client Features
*Migrate from `client-old/` as reference.*

### UI
- [ ] Member list + member profile page
- [ ] Edit profile / avatar upload
- [ ] Private messaging (real-time via SignalR)
- [ ] Global chat (SignalR hub)
- [ ] Admin panel (role-gated)
- [ ] Wallet / balance display (read-only until Phase 7)

### Accessibility & Responsive
- [ ] All pages pass AXE checks
- [ ] Responsive layout: desktop / tablet / mobile breakpoints validated

---

## Phase 5 — CI/CD Pipeline
*Goal: automated build, test, and image publish on every push.*

### Infrastructure
- [ ] Docker multi-stage builds for API and client
- [ ] `docker-compose.yml` production-like composition (API + Postgres + Nginx)
- [ ] GitHub Actions workflow: `build → test → docker build → push to GHCR`
- [ ] Self-hosted runner setup (document the runner install steps)
- [ ] Branch protection: PR must pass CI before merge

---

## Phase 6 — Observability
*Goal: understand what the running system is doing.*

### Infrastructure / Server
- [ ] Serilog structured logging with console + file sinks
- [ ] OpenTelemetry SDK wired into API (traces + metrics)
- [ ] Prometheus metrics endpoint (`/metrics`)
- [ ] Grafana dashboard: request rate, error rate, latency
- [ ] Jaeger distributed tracing
- [ ] Health check endpoints (`/health`, `/health/ready`) + Grafana alert

---

## Phase 7 — Modular Monolith
*Goal: introduce explicit bounded contexts without the operational overhead of microservices.*

### Server
- [ ] Reorganize into modules: **Identity**, **Wallet**, **Betting**, **Social**
- [ ] Each module: own `IModule` registration, own EF `DbContext` (or separate schema)
- [ ] Cross-module communication via MediatR domain events (no direct project references between modules)
- [ ] Module contracts: shared `Contracts/` project for events and DTOs crossing boundaries
- [ ] Feature flags per module (simple config-based to start)

---

## Phase 8 — Microservices
*Goal: split bounded modules into independently deployable services.*

### Server
- [ ] Extract `IdentityService`, `WalletService`, `BettingService`, `SocialService`
- [ ] API Gateway: **YARP** reverse proxy (rate limiting, auth delegation, routing)
- [ ] Synchronous inter-service calls: **gRPC** (typed, fast)
- [ ] Async messaging: **RabbitMQ** (local) / **Azure Service Bus** (cloud)
- [ ] Distributed tracing across services (OpenTelemetry propagation)
- [ ] Each service has its own DB schema

---

## Phase 9 — Cloud & Kubernetes
*Goal: production-like deployment on Azure.*

### Infrastructure
- [ ] AKS cluster (Azure Kubernetes Service)
- [ ] Helm charts per service
- [ ] ArgoCD for GitOps CD
- [ ] Ingress + TLS (cert-manager + Let's Encrypt)
- [ ] Azure CDN for static client assets
- [ ] Azure Service Bus replacing RabbitMQ
- [ ] Explore Azure Container Apps for serverless-style workloads
- [ ] Azure Key Vault for secrets management

---

## Backlog / Later
- Performance testing: k6 or NBomber for API load tests
- Security review: OWASP checklist, dependency audit, penetration testing basics
- SSR for Angular (explore `@angular/ssr`)
- Multi-level cache: in-memory (IMemoryCache) → Redis distributed cache
- Database: read replicas, connection pooling tuning, query analysis
- Cost optimization on Azure
