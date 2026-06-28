# Phase 1 — Clean Architecture Refactor (Detailed Plan)

Companion to `PLAN.md` → **Phase 1**. This document is the *how*; `PLAN.md` is the *what/why*.
It does **not** change the Phase 1 goals (CQRS, `Result<T>`, FluentValidation, `IExceptionHandler`) —
it sequences them into reviewable steps and resolves the real-world decisions the move forces.

The five "decisions this refactor forces" (D1–D5, §4) **are this project's first ADRs**
(ADR 0001–0005 in `PLAN.md` → *Architecture Decision Log*). Record the chosen option there once locked.

Adjacent Phase-1 items that `PLAN.md` now folds in but that don't change the mechanics below:
**API versioning** (`Asp.Versioning`), **OpenAPI-first** contracts, and **modular domain folders**
(Identity/Users/Media/Betting) so the future module boundaries are visible before any split. Layer these
on after 1B is green.

**Status:** Phase 1A complete (2026-06-28). The four-project solution builds green; all endpoints verified. Phase 1B (patterns) is next.

Previous state for reference: single `API/` project (`Microsoft.NET.Sdk.Web`). Split into `src/BettingSite.{Domain,Application,Infrastructure,API}` + three `tests/` skeletons. Old `API/` removed from solution.

---

## 1. Why split at all

Today everything lives in one `API/` project. That isn't "wrong" — it runs — but it blocks the
learning goals in `PLAN.md`:

- **No enforced boundaries.** A controller can `new DataContext()` or reach into Cloudinary directly.
  Nothing stops business rules leaking into HTTP code or EF leaking into DTOs. Project references are
  the only boundary the compiler actually enforces.
- **Hard to unit test.** `AccountController` depends on `UserManager`, `SignInManager`, `IMapper`,
  `ITokenService` and EF all at once. To test "register rejects a duplicate username" you currently
  need the whole web host.
- **No home for the patterns we want to learn.** CQRS handlers, validators, and `Result<T>` need an
  Application layer that doesn't exist yet.

The split is a means to those ends, not architecture for its own sake.

---

## 2. Current structure (single project)

```
API/                              API.csproj  (Microsoft.NET.Sdk.Web, net10.0)
├── Controllers/                  BaseApiController, Account, Users, Admin, Error
├── Data/                         DataContext, UserRepository, Seed, UserSeedData.json
├── DTOs/                         Login, Register, User, Member, MemberUpdate, Photo
├── Entities/                     AppUser, AppRole, AppUserRole, Photo
├── Errors/                       ApiException
├── Extensions/                   ApplicationServiceExtensions, IdentityServiceExtensions, ClaimsPrincipalExtensions
├── Helpers/                      AutoMapperProfiles, CloudinarySettings, JwtSettings
├── Interfaces/                   ITokenService, IPhotoService, IUserRepository
├── Middleware/                   ExceptionMiddleware
├── Services/                     TokenService, PhotoService
└── Program.cs
```

---

## 3. Target structure (matches `PLAN.md`)

```
BettingSite.sln
├── src/
│   ├── BettingSite.Domain/            # framework-free POCOs by bounded context (ADR-0001/0002). NO ASP.NET Identity.
│   │   ├── Betting/                   Player/Member (linked to identity by UserId), Wallet, Money (value object),
│   │   │                              Photo/avatar, domain events; domain services (settlement/payout) later
│   │   └── Common/                    base Entity / ValueObject; repository contracts e.g. IPlayerRepository
│   │
│   ├── BettingSite.Application/       # the brain. Use-case-per-file (CQRS).
│   │   ├── Common/
│   │   │   ├── Result.cs              Result / Result<T> / Error
│   │   │   └── Behaviors/             ValidationBehavior (MediatR pipeline)
│   │   ├── Mappings/                  AutoMapperProfiles
│   │   ├── Abstractions/              IIdentityService (port → Identity ctx), ICurrentUser, IPhotoStorage, ITokenIssuer
│   │   └── Features/                  vertical slices, grouped by use case
│   │       ├── Account/
│   │       │   ├── Register/          RegisterCommand + Handler + Validator
│   │       │   └── Login/             LoginQuery + Handler + Validator
│   │       └── Members/
│   │           ├── GetMembers/        GetMembersQuery + Handler
│   │           ├── GetMember/         GetMemberQuery + Handler
│   │           └── UpdateMember/      UpdateMemberCommand + Handler + Validator
│   │       (each slice owns its own DTOs, e.g. MemberDto, UserDto)
│   │
│   ├── BettingSite.Infrastructure/    # the "how". Implements Domain/Application contracts.
│   │   ├── Persistence/
│   │   │   ├── DataContext.cs
│   │   │   ├── Configurations/        PlayerConfig, WalletConfig, PhotoConfig, ApplicationUserConfig (IEntityTypeConfiguration)
│   │   │   ├── Repositories/          PlayerRepository
│   │   │   └── Seed/                  Seed.cs, UserSeedData.json
│   │   ├── Identity/                  Identity CONTEXT adapter: ApplicationUser:IdentityUser<int>, AppRole, IIdentityService impl
│   │   ├── Services/                  JwtTokenService, CloudinaryPhotoService
│   │   ├── Settings/                  CloudinarySettings, JwtSettings
│   │   └── DependencyInjection.cs     AddInfrastructure(config)
│   │
│   └── BettingSite.API/               # the edge. HTTP only.
│       ├── Controllers/               thin — dispatch to MediatR
│       ├── Extensions/                ClaimsPrincipalExtensions, AddApiServices
│       ├── Infrastructure/            GlobalExceptionHandler (IExceptionHandler)
│       ├── Program.cs
│       └── appsettings*.json
│
└── tests/                            # skeletons now; populated in Phase 2
    ├── BettingSite.Domain.Tests/
    ├── BettingSite.Application.Tests/
    └── BettingSite.Infrastructure.Tests/
```

### Dependency rule (the only rule that matters)

```
        Api  ──────────────┐
         │                 │
         ▼                 ▼
   Application ◄──── Infrastructure
         │                 │
         └──────► Domain ◄──┘
```

- **Domain** references nothing — no solution projects and **no framework** (pure POCOs). See [[ADR-0002]].
- **Application** references **Domain** only.
- **Infrastructure** references **Application** (and Domain transitively).
- **Api** references **Application** + **Infrastructure** (Infrastructure only so `Program.cs` can wire DI —
  controllers must touch Application abstractions, never Infrastructure types).

`dotnet` will refuse to compile a cycle, so these references *are* the architecture.

---

## 4. The decisions this refactor forces (ADR 0001–0005)

These are the parts that are genuinely "it depends." **D1 and D2 are now decided** (see the ADRs). D3–D5
remain open *proposals* to confirm before coding — they are **not** settled, and the recommendations below
are input, not decisions.

### D1 — Where does ASP.NET Identity live? → DECIDED ([[ADR-0002]])

**Decision: authentication is its own bounded context; the betting Domain is framework-free.** `IdentityUser`
is *not* a domain entity. `ApplicationUser : IdentityUser<int>`, `AppRole`, credentials and tokens live in
the **Identity context's Infrastructure**. The betting Domain holds a pure `Player`/`Member` POCO linked to
identity by a shared `UserId`. The old "Domain must reference `Identity.Stores`" problem disappears because
nothing in Domain derives from Identity. *(Option 1 — Identity in Domain — is rejected.)*

### D2 — How do use cases perform identity/auth operations? → PROPOSED ([[ADR-0003]])

Given D1, handlers must **not** touch `UserManager`/`SignInManager` directly. Proposed: a **port/adapter** at
the Identity-context boundary — an intent-revealing `IIdentityService`/`IAuthenticationService` in
`Application/Abstractions`, implemented by an ASP.NET Identity adapter in `Infrastructure/Identity`; handlers
depend on the interface and stay unit-testable with a fake. Registration **orchestrates both contexts**
(create auth user → emit `UserRegistered` → create `Player`). This is a *port*, **not** a Domain Service —
Domain Services are reserved for betting/wallet logic (settlement, payouts). Confirm before building the
Account slices.

### D3 — `Result<T>` vs exceptions (PLAN.md asks for `Result<T>`)

Split responsibilities rather than picking one:
- **`Result<T>` / `Error`** for *expected* business outcomes — "username taken", "invalid credentials",
  "member not found". Handlers return these; controllers translate them to status codes.
- **`IExceptionHandler` + `ProblemDetails`** for *unexpected* failures — a thrown exception is a bug or an
  outage, not a business rule. This replaces the custom `ExceptionMiddleware`.

This is the modern .NET answer and teaches both patterns in their proper lane.

### D4 — `Photo`: owned type or separate entity? (PLAN.md floats "owned types")

`Photo` has its own `Id`, a Cloudinary `PublicId`, and an `AppUserId` FK; login `.Include`s it.
- An **owned type** is right *only if* a user will ever have exactly one avatar and we never query photos
  on their own.
- A **separate entity** (current shape) is right if a photo gallery / multiple images is plausible later
  (the social/profile features in Phase 4 suggest it is).

**Recommendation: keep `Photo` a separate entity** for now; revisit if the model stays single-avatar.
This intentionally defers the PLAN.md "owned types" idea rather than committing to it blind.

### D5 — Where does JWT *validation* wiring live?

Token **creation** is clearly Infrastructure (`JwtTokenService`). Token **validation** setup
(`AddAuthentication().AddJwtBearer(...)`) is pipeline config.

**Recommendation:** put it in `Infrastructure.AddInfrastructure(config)` alongside everything else that
reads `JwtSettings`, and have `Program.cs` call that one method. Keeps `Program.cs` to a composition root.

---

## 5. File-by-file move map

| Current | New project / folder | Change |
|---|---|---|
| `Entities/AppUser,AppRole,AppUserRole` | `Infrastructure/Identity/` | become `ApplicationUser:IdentityUser<int>` etc. — Identity context only ([[ADR-0002]]); **not** in Domain |
| `Entities/Photo` | `Domain/Betting/` | move into the framework-free betting Domain |
| *(new)* | `Domain/Betting/Player.cs` | pure POCO aggregate linked to identity by `UserId` (the domain "user"; replaces `AppUser` in the model). Move `Money` → a `Wallet`/`Money` value object |
| `Interfaces/IUserRepository` | `Domain/Common/` → `IPlayerRepository` | repository contract stays in Domain |
| `Interfaces/ITokenService,IPhotoService` | `Application/Abstractions/` | become app ports (token issuer, photo storage) implemented in Infrastructure |
| `DTOs/LoginDto,RegisterDto` | `Application/Features/Account/...` | move next to their slice |
| `DTOs/UserDto` | `Application/Features/Account/` | move (login/register response) |
| `DTOs/MemberDto,MemberUpdateDto,PhotoDto` | `Application/Features/Members/...` | move next to their slice |
| `Helpers/AutoMapperProfiles` | `Application/Mappings/` | move |
| `Errors/ApiException` | **delete** | replaced by `ProblemDetails` (D3) |
| `Data/DataContext` | `Infrastructure/Persistence/` | move; extract relationship config to `Configurations/` |
| `Data/UserRepository` | `Infrastructure/Persistence/Repositories/` | move |
| `Data/Seed, UserSeedData.json` | `Infrastructure/Persistence/Seed/` | move (json: `CopyToOutputDirectory`) |
| `Services/TokenService` | `Infrastructure/Services/JwtTokenService` | move + rename |
| `Services/PhotoService` | `Infrastructure/Services/CloudinaryPhotoService` | move + rename |
| `Helpers/CloudinarySettings,JwtSettings` | `Infrastructure/Settings/` | move |
| `Extensions/ApplicationServiceExtensions` | `Infrastructure/DependencyInjection.cs` + `Api/Extensions` | split: infra wiring → Infra; app wiring → Application |
| `Extensions/IdentityServiceExtensions` | `Infrastructure/DependencyInjection.cs` | move (D5) |
| `Extensions/ClaimsPrincipalExtensions` | `Api/Extensions/` | move (reads `ClaimsPrincipal` from HTTP) |
| `Middleware/ExceptionMiddleware` | `Api/Infrastructure/GlobalExceptionHandler` | rewrite as `IExceptionHandler` (1B) |
| `Controllers/*` | `Api/Controllers/` | move in 1A; thin out in 1B |
| `Program.cs` | `Api/Program.cs` | rewrite composition root |
| *(new)* | `Application/Common/Result.cs` | `Result`/`Result<T>`/`Error` |
| *(new)* | `Application/Abstractions/IIdentityService` | identity port at the context boundary ([[ADR-0003]]) |
| *(new)* | `Application/Common/Behaviors/ValidationBehavior` | FluentValidation in MediatR pipeline |

### NuGet ownership after the split

- **Domain:** *no external packages* — pure POCOs ([[ADR-0002]]). (Identity packages live in Infrastructure.)
- **Application:** `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `AutoMapper`.
- **Infrastructure:** `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`,
  `Npgsql.EntityFrameworkCore.PostgreSQL`, `EFCore.NamingConventions`,
  `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`,
  `CloudinaryDotNet`.
- **Api:** `Microsoft.AspNetCore.OpenApi` + `Asp.Versioning.Mvc.ApiExplorer` (it's `Sdk.Web`; no EF/Cloudinary refs).

> Pin each moved package to the version already in `api/API.csproj` (the .NET 10 train: EF Core / Identity /
> JwtBearer `10.0.x`, Npgsql `10.0.1`, `EFCore.NamingConventions` `10.0.1`, AutoMapper `16.x`,
> CloudinaryDotNet `1.28.0`, OpenApi `10.0.x`). Don't re-resolve versions during the move — keep 1A behavior-identical.

> EF migrations: `Microsoft.EntityFrameworkCore.Design` moves to Infrastructure, but the design-time tools
> need a startup project. Commands become:
> `dotnet ef migrations add <Name> -p src/BettingSite.Infrastructure -s src/BettingSite.API`
> The existing migration history moves with `DataContext` into Infrastructure.

---

## 6. Execution plan — two sub-phases

Split deliberately: **1A is a mechanical move that must stay green** (boring, low-risk, easy to review).
**1B is the interesting pattern work** (CQRS, Result, validation, exception handler). Don't mix them —
if 1B breaks something you want to know it wasn't the file shuffle.

### Phase 1A — Structural split (behavior-preserving)

Goal: identical runtime behavior, four projects instead of one. Controllers still use `UserManager` etc.

1. Use the existing `BettingSite.sln` (currently wraps only `api/`); create the four `src/` projects + three `tests/` skeletons, add them to the sln, and set project references per §3.
2. **Split entities by context ([[ADR-0002]]):** ASP.NET Identity entities → `Infrastructure/Identity`
   (`AppUser`→`ApplicationUser`, `AppRole`, `AppUserRole`); the betting **Domain** gets `Photo` + value
   objects, file-scoped `BettingSite.Domain.*`. **Keep `ApplicationUser` as the only user representation for
   now** — introducing a separate `Player` aggregate is a deliberate 1B remodel (1B step 4 below), *not* part
   of this mechanical move, so 1A stays behavior-preserving and low-risk. Build.
3. Move **DTOs + AutoMapper** into Application (flat for now; reshape into slices in 1B). Build.
4. Move **Infrastructure** (DataContext + relationship config, repository, services renamed, settings, seed).
   Write `Infrastructure/DependencyInjection.cs` consolidating the two old extension classes (incl. JWT — D5). Build.
5. Move **Api** (controllers, `ClaimsPrincipalExtensions`, `Program.cs`). `Program.cs` becomes:
   `AddApplication()` + `AddInfrastructure(config)` + controllers/CORS/OpenAPI. Keep the old
   `ExceptionMiddleware` for now. Build + **run**.
6. **Checkpoint:** `dotnet build` clean; `dotnet run` boots; migrations apply; smoke-test register / login /
   GET members / admin in Postman. Commit: `refactor: split monolith into clean-architecture projects (1A)`.

### Phase 1B — Patterns (the learning payload)

Introduce one concept at a time, each its own commit, app runnable after each.

1. **`Result<T>` + `Error`** in `Application/Common`. (no behavior change yet)
2. **MediatR**: register in Application DI; add the package; no handlers yet.
3. **First vertical slice — Register**: `RegisterCommand` + handler returning `Result<UserDto>`, using the
   `IIdentityService` **port** ([[ADR-0003]], confirm first). `AccountController.Register` becomes a 3-line
   dispatch. Run + test.
4. **`Player` aggregate + cross-context link — the DDD remodel ([[ADR-0002]]):** add `Player` to
   `Domain/Betting` keyed by `UserId`; move betting state *off* `ApplicationUser` (`Money` → a `Wallet`/`Money`
   value object, names, activity, avatar); registration emits `UserRegistered` → a betting handler creates the
   `Player`. Add the EF migration. Own commit; smoke-test register / login / profile. *(This is the real
   modeling work, deliberately separated from the 1A file move.)*
5. **Login** slice (`LoginQuery`), then **GetMembers / GetMember / UpdateMember** (now reading the `Player`).
   Delete `UserRepository` methods as handlers absorb them (or keep the repo behind the handler — your call).
6. **FluentValidation**: validators for `RegisterCommand`, `LoginQuery`, `UpdateMemberCommand`; wire
   `ValidationBehavior` into the MediatR pipeline so validation runs before every handler.
7. **`GlobalExceptionHandler : IExceptionHandler`** + `AddProblemDetails()`; delete `ExceptionMiddleware`
   and `ApiException` (D3 — confirm first).
8. **EF audit**: move relationship config from `OnModelCreating` into `IEntityTypeConfiguration<>` classes;
   confirm snake_case still applied; decide D4.
9. **Checkpoint:** full smoke test again; controllers contain no business logic. Commit per slice.

> Per `PLAN.md`, **tests come in Phase 2.** 1B leaves clean seams (handlers + `IIdentityService`) so that
> Phase 2 can unit-test handlers with fakes and integration-test repositories with TestContainers.

---

## 7. Definition of done for Phase 1

- [ ] Solution builds with the four-project dependency graph in §3; no reference cycles.
- [ ] `dotnet run` boots; migrations apply; register / login / members / admin all work unchanged from the client's view (no API contract change).
- [ ] Controllers contain **no** business logic — each action dispatches a MediatR request and maps `Result` → HTTP.
- [ ] Expected failures flow through `Result<T>`; unexpected ones through `IExceptionHandler`/`ProblemDetails`.
- [ ] Validators run via the MediatR pipeline for all commands/queries that take input.
- [ ] **Domain has no framework dependencies** — no EF, no ASP.NET Identity, no Cloudinary ([[ADR-0001]]/[[ADR-0002]]).
- [ ] No `Microsoft.EntityFrameworkCore` / `CloudinaryDotNet` / ASP.NET Identity reference reachable from a controller.
- [ ] Decisions recorded as ADRs in `.claude/adr/` (0001/0002 accepted; 0003 + D3–D5 confirmed before the slices that need them).

---

## 8. Rollback

Work happens on a branch with a commit per step (§6), so any step is independently revertible and we can
`git bisect` a regression to the exact slice. Worst case: `git reset --hard origin/master`.

---

## 9. Open decisions to confirm before coding

| # | Decision | Status |
|---|----------|--------|
| D1 | Where ASP.NET Identity lives | **Decided ([[ADR-0001]]/[[ADR-0002]])**: auth is its own bounded context; Domain framework-free; no Identity in Domain |
| D2 | How use cases reach identity/auth | **Proposed ([[ADR-0003]])**: port/adapter at the context boundary (not a Domain Service) |
| D3 | Errors | *Open proposal*: `Result<T>` for business outcomes + `IExceptionHandler` for the unexpected |
| D4 | `Photo`/avatar mapping | *Open proposal*: keep a separate entity; revisit if it stays single-avatar |
| D5 | JWT validation wiring + token/refresh | *Open proposal*: in `AddInfrastructure(config)`; also fix issuer/audience validation + add refresh tokens (PLAN Phase 0 security) |
| — | Folder convention | *Open proposal*: `Features/<UseCase>/` vertical slices (vs. tech-folders `Commands/`, `Queries/`) |
