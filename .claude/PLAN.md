# Project Plan — Betting Site Modernization Roadmap

Casino-style betting site used as a **personal learning sandbox** for modern .NET 10, Angular 21,
Clean Architecture, modular monolith → microservices, DevOps, observability, and cloud-agnostic
Kubernetes delivery (Azure as the reference cloud).

> This file is the master **what / why**. The detailed **how** for individual phases lives in
> companion docs — currently `ARCHITECTURE_REFACTOR.md` for Phase 1. Write a new companion doc per
> phase when the scope gets deep enough to need one.

---

## Guiding principles

1. **Learning over shipping.** Every architectural decision is understood and recorded (see ADRs),
   not cargo-culted. We can afford to do things "the proper way" because there's no deadline.
2. **Best modern tech, end to end.** Latest stable runtimes/packages, cloud-native patterns,
   real observability, real CI/CD, real Kubernetes — the goal is to touch the whole 2026 stack.
3. **Cloud-agnostic core, Azure reference.** Manifests/Helm charts stay portable (AKS/EKS/GKE);
   Azure gets the one fully-wired reference deployment + runbook.
4. **Earn each layer of complexity.** Ship a great **modular monolith first** (Release A), then
   split into microservices only where the boundaries are real (Release B+). Never add distributed
   complexity faster than the learning justifies it.
5. **Every phase has a Definition of Done and stays green.** The app builds and runs after each step.

---

## Tech stack at a glance

| Area | Choices |
|---|---|
| **Backend runtime** | .NET 10, ASP.NET Core minimal hosting, C# primary constructors |
| **API patterns** | CQRS (MediatR), `Result<T>`/`Error`, FluentValidation, AutoMapper, `IExceptionHandler` + `ProblemDetails`, API versioning, OpenAPI-first contracts |
| **Data** | PostgreSQL, EF Core 10, Npgsql, `EFCore.NamingConventions` (snake_case), TestContainers for integration tests |
| **Identity / security** | ASP.NET Identity, JWT (issuer/audience/lifetime enforced) + refresh tokens + revocation, role-based authz, user-secrets / Key Vault, password policy |
| **Frontend** | Angular 21 (standalone components, signals, functional guards/interceptors, typed reactive forms, lazy routes), Tailwind v4 + SCSS hybrid, design-token-driven design system |
| **Media** | Cloudinary (validated uploads) |
| **Realtime** | SignalR (chat + private messaging) |
| **Testing** | xUnit + FluentAssertions + NSubstitute, TestContainers, `WebApplicationFactory`; Vitest (client unit), Playwright (e2e smoke); k6 / NBomber (load) |
| **Observability** | Serilog (structured logs + correlation IDs), OpenTelemetry (traces+metrics), Prometheus, Grafana, Jaeger, health/readiness/startup probes |
| **Inter-service** | YARP API gateway, gRPC (sync), RabbitMQ (local) / Azure Service Bus (cloud) with idempotency + retries |
| **Containers / CI-CD** | Docker multi-stage, docker-compose, GitHub Actions → GHCR, self-hosted runner, branch protection; SAST, dependency scanning (Trivy/Dependabot), SBOM (Syft/CycloneDX) |
| **Cloud / platform** | Kubernetes (kind/minikube local → AKS cloud), Helm, ArgoCD (GitOps), cert-manager + Let's Encrypt ingress/TLS, Azure Key Vault, Azure CDN, IaC (Bicep/Terraform), provider overlays for AKS/EKS/GKE |

---

## How we work together

**The goal is learning, not just shipping.** Every architectural decision should be understood, not just applied.

### Workflow per session
1. Pick an epic/phase from the roadmap below.
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
- Scope requests to one phase at a time. "Do everything" prompts produce shallow results.

---

## Cross-cutting governance

These run **alongside every phase** — they are not a phase you finish.

### Architecture Decision Records (ADRs)
Every significant, hard-to-reverse decision gets a short ADR (context → options → decision → consequences).
They live in `.claude/adr/` — read `.claude/adr/README.md` first (it's also a self-teaching guide on *how*
to make and record architectural decisions), and copy `0000-template.md` for new ones. **No
architecturally-significant change without an ADR.** ADRs are `Proposed` until *you* actually decide, then
`Accepted`; they're immutable once accepted (supersede with a new one rather than editing). The planning
docs (`PLAN.md`, `ARCHITECTURE_REFACTOR.md`) are *drafts/input*, **not** decisions — the ADR log is the
source of truth for what's actually been chosen.

### Non-functional targets (learning SLOs)
Realistic targets for a solo project — aspirational, not contractual, but we measure against them.

- **Availability:** single-instance dev; ≥ 2 replicas + rolling updates once on Kubernetes.
- **Latency:** p95 < 200 ms for read endpoints, < 400 ms for writes (local/managed Postgres).
- **Security:** zero hardcoded secrets; **no high/critical** vulns at any merge; JWT fully validated; least-privilege roles.
- **Quality gates:** CI fails on build error, failing test, lint error, or high/critical dependency vuln.
- **Cost (cloud phase):** stay inside a defined monthly budget with alerts (FinOps phase).

### Risk register (small-project lens)
- **#1 risk: over-engineering past the project's size.** Mitigation: Release A/B checkpoints; cap service
  count at 2–3; defer mesh/multi-region/micro-frontends until scale demands them.
- **Secrets & git history.** Rotation + history scrubbing are mandatory security work, not optional.
- **Migration blockers.** Track Angular/EF/Identity major-version hops and contract changes as they surface.
- **Scope creep on the frontend rebuild.** Design system first, then page-by-page — don't free-style screens.

### Definition of Done (applies to every phase)
- [ ] App builds and runs (`dotnet build`/`dotnet run`, `npm start`) after the change.
- [ ] Relevant tests added/updated and green; CI gates pass.
- [ ] No new high/critical dependency vulnerabilities (`dotnet`/`npm` audit).
- [ ] Decisions worth keeping captured as an ADR; this `PLAN.md` checkboxes updated.
- [ ] No secrets in source; config externalized per environment.

---

## Release milestones

Staged so there's always a coherent thing to demo, even if later phases pause.

- **Release A — Secure Modern Modular Monolith** (Phases 0–7): hardened, Clean-Architecture .NET 10 API +
  rebuilt Angular 21 UI/UX, full test pyramid, CI/CD with security gates, observability, and an internal
  modular-monolith boundary (Identity/Wallet/Betting/Social). Runs via Docker + local Kubernetes.
- **Release B — Initial Microservices on Kubernetes** (Phases 8–9): 2–3 services behind a YARP gateway,
  database-per-service, deployed to local k8s and one managed cloud (AKS), cloud-agnostic manifests proven
  with provider overlays.
- **Release C — Production-grade Cloud Platform** (Phases 10–11): resilience (mTLS, autoscaling, backups,
  chaos), FinOps budgets, and full cutover with legacy retirement + post-migration audit.

---

## Phase 0 — Build Baseline, Discovery & Governance
*Goal: verify the project runs cleanly end-to-end and lock down scope/decisions before touching architecture.*

### Governance & discovery
- [ ] Freeze target end-state + intermediate Release A/B/C milestones (this file).
- [ ] Seed ADRs for the planned service boundaries (Auth/Identity, User/Profile+Media, Betting Core).
- [ ] Capture non-functional targets (above) and a living risk register.
- [ ] Capture dependency inventory (.NET/NuGet, Angular/npm) and a Definition of Done per phase.

### Server
- [x] Collapse `Startup.cs` → top-level `Program.cs` (.NET minimal hosting) — *done; no `Startup.cs` present.*
- [x] Move backend to .NET 10 and align all NuGet packages to current stable — *done (commit 015a9ae).*
- [x] Fix any remaining .NET 10 warnings/deprecations.
- [x] Verify `dotnet build` and `dotnet run` succeed.
- [x] Confirm EF migrations apply cleanly against a local Postgres — *done; `InitialCreate` applied via `dotnet ef database update`.*

### Client
- [x] New Angular 21 client scaffolded; `client-old/` kept as the feature-migration reference — *done.*
- [x] Tailwind v4 PostCSS pipeline + Vitest present — *done.*
- [ ] Verify `npm start` serves at `https://localhost:4200`.
- [ ] Angular dev proxy to `https://localhost:5001` configured.

### Infrastructure
- [x] `docker-compose.yml` for local dev: API + PostgreSQL (no manual Postgres install needed).
- [x] `.env.example` with all required config keys documented.
- [x] Confirm secrets strategy: `dotnet user-secrets` for API, `.env.local` for client.

### Security & dependency baseline *(Phase 0 scope = obvious-vuln hygiene; deeper auth hardening deferred)*
- [x] No hardcoded secrets in source; previously committed credentials **rotated**. History scrub skipped — repo is already public and the values are dead, so it buys nothing.
- [x] JWT **issuer/audience/lifetime** validation enforced (`IdentityServiceExtensions`, `ClockSkew=30s`). Lifetime is configurable (`JwtSettings:AccessTokenExpirationMinutes`, 8h dev default); a short lifetime ships with refresh tokens.
- [ ] **Refresh-token** flow + revocation → **deferred to the auth-hardening pass** (OAuth2/OIDC); revocation needs server state, so it rides with the OAuth model, not Phase 0. See Backlog.
- [x] Admin/privileged endpoints have **role-based authz** (`AdminController` AdminRole policy; `UsersController` `[Authorize]`; register/login anonymous). Re-audit as the surface grows.
- [ ] DTO **validation** — partial (`[ApiController]` 400 `ProblemDetails`; `RegisterDto` annotated). FluentValidation lands in Phase 1.
- [x] Image upload **type/size validation** (`PhotoServiceBase`: `image/*`, ≤5 MB, GUID filename). Magic-byte sniff is a later nice-to-have; Cloudinary deferred by design.
- [x] **Password policy + lockout** (`IdentityServiceExtensions`: length 8, digit/upper/lower, unique email, lockout 5/15 min; login `lockoutOnFailure: true`). ⚠️ seed passwords in user-secrets must meet this; seed admin email via `SeedSettings:AdminEmail`.
- [~] **Audit gates** — `/audit-deps` skill runs `dotnet`/`npm` audits locally; CI enforcement is Phase 5. (2026-06-26: .NET 0 vulns; `client` npm 11 high — client work.)
- [x] **No SQL-injection surface** — all data access is EF Core LINQ (parameterized); no raw SQL.
- [x] **Build hygiene** — `Directory.Build.props`: `TreatWarningsAsErrors=true`. API builds clean.

---

## Phase 1 — API: Clean Architecture
*Goal: move from tutorial-style fat controllers to a maintainable, testable structure with proper boundaries.*
*Detailed step-by-step plan + decisions (D1–D5) live in `ARCHITECTURE_REFACTOR.md`.*

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
- [ ] Create solution structure with four projects above (see file-by-file move map in `ARCHITECTURE_REFACTOR.md`).
- [ ] Model the betting **Domain as framework-free POCOs** — `Player`/`Member` (linked to identity by `UserId`), `Wallet` (with a `Money` value object), `Photo`/avatar, domain events. No ASP.NET Identity, no EF (per [[ADR-0001]], [[ADR-0002]]).
- [ ] Keep ASP.NET Identity (`ApplicationUser : IdentityUser<int>`, roles, credentials, tokens) in the **Identity context's Infrastructure**, exposed to the app via an intent-revealing port ([[ADR-0003]], to confirm).
- [ ] Introduce `Result<T>` / `Error` pattern (replace exception-driven flow for *expected* outcomes).
- [ ] Application layer: `RegisterCommand`, `LoginQuery`, `GetMembersQuery`, `UpdateMemberCommand` as first vertical slices; registration **orchestrates both contexts** (create auth user → emit event → create `Player`).
- [ ] FluentValidation validators for all commands, wired via a MediatR `ValidationBehavior` pipeline.
- [ ] Infrastructure: move EF `DataContext`, repositories, JWT/identity adapter, Cloudinary service behind their ports.
- [ ] API: thin controllers calling MediatR — one action per endpoint, no business logic.
- [ ] Use primary constructors / modern DI patterns where they read cleanly.
- [ ] Audit EF Core: snake_case naming convention, `IEntityTypeConfiguration<>` per entity, explicit relationship configs.
- [ ] Global exception handling modernized: `IExceptionHandler` (.NET 8+) + `ProblemDetails`, replacing custom middleware.
- [ ] **API versioning** strategy (`Asp.Versioning`) and **OpenAPI-first** contracts published from the API.
- [ ] Reorganize into modular domain folders so module boundaries (Identity/Users/Media/Betting) are visible pre-split.

---

## Phase 2 — Testing Foundation
*Goal: establish the test pyramid before adding more features.*

### Server
- [ ] xUnit + FluentAssertions + NSubstitute setup in solution.
- [ ] Unit tests: Application-layer commands/queries (pure C#, no EF, no HTTP) using `IIdentityService` fakes.
- [ ] Integration tests with **TestContainers** (real Postgres container per run — no mocking the DB).
- [ ] `WebApplicationFactory` API tests: full HTTP request/response cycle, including auth.
- [ ] Test data builders (not fixtures) for clean arrange steps.
- [ ] CI gate: tests must pass before merge.

### Client
- [ ] Vitest confirmed running with `npm test`.
- [ ] First component/service unit test (auth service or app component).

---

## Phase 3 — Angular Client Core
*Goal: auth + shell working so features can be built on top; modern Angular architecture + hardened frontend auth.*

### Architecture & auth
- [ ] HTTP client via `provideHttpClient(withInterceptors([...]))`.
- [ ] **Functional** JWT interceptor (attach token) and error interceptor (401/500 handled globally).
- [ ] `AccountService` with signals: `currentUser = signal<User | null>(null)`.
- [ ] Functional auth guard + role-based guard.
- [ ] Typed reactive forms; route-level **lazy loading** per feature area.
- [ ] Remove legacy/deprecated deps and any duplicate styling frameworks carried over from `client-old/`.

### Frontend security
- [ ] Replace unsafe token parsing/storage with hardened auth handling (no naive `localStorage` of raw JWT where avoidable; safe decode).
- [ ] App shell: nav, layout skeleton, responsive breakpoints (Tailwind).
- [ ] Login / Register pages (typed reactive forms).
- [ ] Unit tests for guards/services.

---

## Phase 4 — Angular Features + UI/UX Design System
*Goal: rebuild the experience design-system-first, then migrate features from `client-old/` onto it.*

### Design system (build before page-by-page rebuild)
- [ ] Define **design tokens**: color, spacing, typography, elevation, motion.
- [ ] Reusable component kit: buttons, inputs, cards, tables, dialogs, toasts.
- [ ] **Tailwind v4 + SCSS hybrid**: Tailwind for layout/utilities/responsive; SCSS for semantic component skins, theme maps, advanced composition.
- [ ] Light/dark theme tokens + responsive breakpoints.

### Features (migrate from `client-old/` as reference)
- [ ] Member list + member profile page.
- [ ] Edit profile / avatar upload (validated).
- [ ] Private messaging (real-time via SignalR).
- [ ] Global chat (SignalR hub).
- [ ] Admin panel (role-gated).
- [ ] Wallet / balance display (read-only until the Betting/Wallet domain lands).

### Accessibility & responsive
- [ ] All pages pass AXE checks (keyboard nav, contrast, focus management, ARIA labels).
- [ ] Responsive layout validated: desktop / tablet / mobile breakpoints.
- [ ] Playwright e2e smoke suite for core journeys (login, register, profile, chat).

> **Release A checkpoint** lands after Phases 5–7 complete on top of this.

---

## Phase 5 — CI/CD Pipeline + Supply-Chain Security
*Goal: automated build, test, scan, and image publish on every push.*

### Infrastructure
- [ ] Docker multi-stage builds for API and client.
- [ ] `docker-compose.yml` production-like composition (API + Postgres + Nginx).
- [ ] GitHub Actions workflow: `build → test → scan → docker build → push to GHCR`.
- [ ] Self-hosted runner setup (document the runner install steps).
- [ ] Branch protection: PR must pass CI before merge.

### Security gates
- [ ] Dependency scanning (Dependabot + Trivy) and **SAST** (CodeQL).
- [ ] **SBOM** generation (Syft / CycloneDX) attached to builds.
- [ ] Quality gates enforced: tests, lint, and high/critical vulnerability thresholds fail the build.

---

## Phase 6 — Observability
*Goal: understand what the running system is doing.*

### Infrastructure / Server
- [ ] Serilog structured logging with console + file sinks and **correlation IDs**.
- [ ] OpenTelemetry SDK wired into API (traces + metrics).
- [ ] Prometheus metrics endpoint (`/metrics`).
- [ ] Grafana dashboard: request rate, error rate, latency (RED metrics).
- [ ] Jaeger distributed tracing.
- [ ] Health/readiness/startup probe endpoints (`/health`, `/health/ready`) + a Grafana alert.

---

## Phase 7 — Modular Monolith
*Goal: introduce explicit bounded contexts without the operational overhead of microservices. This is the Release A architecture.*

### Server
- [ ] Harden the bounded-context boundaries already established in Phase 1 ([[ADR-0002]]) into explicit modules: **Identity**, **Wallet**, **Betting**, **Social**.
- [ ] Each module: own `IModule` registration, own EF `DbContext` (or dedicated schema); betting/wallet domain logic that spans aggregates lives in **Domain Services** (e.g. bet settlement, payouts, wallet transfers).
- [ ] Cross-module communication via MediatR domain events (no direct project references between modules).
- [ ] Module contracts: shared `Contracts/` project for events and DTOs crossing boundaries.
- [ ] Feature flags per module (simple config-based to start).

> **Release A** ✅ — secure modern monolith, rebuilt frontend, full test pyramid, CI/CD + security gates,
> observability, modular boundaries; runs via Docker + local Kubernetes profile.

---

## Phase 8 — Microservices (Service Extraction Wave 1)
*Goal: split bounded modules into independently deployable services — only where boundaries are real.*

### Server
- [ ] Extract in order: **Auth/Identity** → **User/Profile+Media** → **Betting Core** (shell + first endpoints).
- [ ] API Gateway: **YARP** reverse proxy (routing, rate limiting, auth delegation).
- [ ] **Strangler routing** + backward-compatible endpoints so the monolith and services coexist during migration.
- [ ] **Database-per-service from day one** (independent DB/schema per service).
- [ ] Synchronous inter-service calls: **gRPC** (typed, fast); define sync API contracts.
- [ ] Async messaging: **RabbitMQ** (local) / **Azure Service Bus** (cloud); define async event contracts.
- [ ] **Idempotency + retry** patterns for cross-service events.
- [ ] **Contract tests** between frontend↔services and service↔service.
- [ ] Distributed tracing propagated across services (OpenTelemetry context propagation).

---

## Phase 9 — Cloud & Kubernetes (Multi-Cloud Capable)
*Goal: production-like deployment on Kubernetes — cloud-agnostic core, Azure reference. This is Release B.*

### Platform
- [ ] Helm charts (or Kustomize base) per service; **cloud-agnostic** common manifests.
- [ ] Provider overlays for **AKS / EKS / GKE** (ingress, secrets, storage) over the shared base.
- [ ] Local Kubernetes profile (kind/minikube) kept at parity with managed clusters.
- [ ] Autoscaling (HPA), resource requests/limits, probes, and rollout policies.

### Azure reference deployment
- [ ] AKS cluster provisioned via IaC (**Bicep/Terraform**).
- [ ] ArgoCD for GitOps continuous delivery.
- [ ] Ingress + TLS (cert-manager + Let's Encrypt).
- [ ] Azure CDN for static client assets.
- [ ] Azure Service Bus replacing RabbitMQ in cloud.
- [ ] Azure Key Vault for secrets; explore Azure Container Apps for serverless-style workloads.
- [ ] Complete one reference cloud production deployment + runbook.

> **Release B** ✅ — 2–3 services behind the gateway, DB-per-service, validated locally and on AKS,
> cloud portability proven via provider overlays.

---

## Phase 10 — Resilience, Security & FinOps Hardening
*Goal: make the cloud platform production-grade.*

- [ ] Service-to-service **mTLS** + network/authorization policies.
- [ ] Gateway/API **rate limiting** and quotas.
- [ ] Autoscaling tuning under load; pod disruption budgets.
- [ ] **Backups + restore drills** for each service database.
- [ ] **Chaos tests** (pod/node failure, dependency latency) to validate resilience.
- [ ] **FinOps**: cost budgets, tagging, and alerts; right-size resources.

---

## Phase 11 — Cutover & Legacy Retirement
*Goal: finish the strangler migration and retire superseded paths. This completes Release C.*

- [ ] Execute phased traffic migration; monitor SLOs during each shift.
- [ ] Decommission monolith responsibilities as services take over.
- [ ] Finalize runbooks and incident workflows.
- [ ] Post-migration audit + backlog of follow-up optimizations.

> **Release C** ✅ — resilient, cost-governed, fully cut-over cloud platform.

---

## Backlog / Later
- **Auth hardening (deferred from Phase 0):** migrate to OAuth2/OIDC, add refresh tokens + revocation and MFA; short access-token lifetime drops in here. Pairs with the Auth/Identity service split.
- Performance testing: k6 or NBomber for API load tests.
- Security review: OWASP checklist, dependency audit, penetration-testing basics.
- SSR for Angular (explore `@angular/ssr` + hydration).
- Multi-level cache: in-memory (`IMemoryCache`) → Redis distributed cache.
- Database: read replicas, connection-pool tuning, query analysis.
- Advanced (defer until scale demands): full service mesh, multi-region active-active, micro-frontends.
- Cloud cost optimization deep-dive.

---

## Architecture Decision Log
The real decisions live as files in `.claude/adr/` — this table is just an index. `Accepted` = decided by you.

| ADR | Decision | Status |
|---|---|---|
| [0001](adr/0001-ddd-and-clean-architecture.md) | Adopt DDD + Clean Architecture (Dependency Rule, framework-free Domain, bounded contexts) | **Accepted** |
| [0002](adr/0002-authentication-as-separate-bounded-context.md) | Auth/Identity is its own bounded context; betting Domain is framework-free POCOs linked by `UserId` | **Accepted** |
| [0003](adr/0003-identity-access-across-context-boundary.md) | Identity access via port/adapter at the context boundary (not a Domain Service) | Proposed |
| — | *Open, not yet decided:* error strategy (`Result<T>` vs exceptions); avatar/`Photo` modeling; JWT validation wiring + token/refresh strategy; MediatR vs hand-rolled dispatch; messaging tech; cloud-secret strategy; **technology stack** (good first ADR to write yourself) | — |
