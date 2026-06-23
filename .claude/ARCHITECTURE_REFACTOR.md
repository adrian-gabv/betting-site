# Phase 1 — Clean Architecture Refactor (Detailed Plan)

Companion to `PLAN.md` → **Phase 1**. This document is the *how*; `PLAN.md` is the *what/why*.
It does **not** change the Phase 1 goals (CQRS, `Result<T>`, FluentValidation, `IExceptionHandler`) —
it sequences them into reviewable steps and resolves the real-world decisions the move forces.

**Status:** Planning — nothing implemented yet.
**Prerequisite:** Phase 0 (build baseline) green — `dotnet build` + `dotnet run` succeed, migrations apply.

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
│   ├── BettingSite.Domain/            # entities + contracts. Almost no dependencies.
│   │   ├── Entities/                  AppUser, AppRole, AppUserRole, Photo
│   │   └── Interfaces/                IUserRepository, ITokenService, IPhotoService
│   │
│   ├── BettingSite.Application/       # the brain. Use-case-per-file (CQRS).
│   │   ├── Common/
│   │   │   ├── Result.cs              Result / Result<T> / Error
│   │   │   └── Behaviors/             ValidationBehavior (MediatR pipeline)
│   │   ├── Mappings/                  AutoMapperProfiles
│   │   ├── Abstractions/              IIdentityService, ICurrentUser  (wrap Identity/HTTP)
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
│   │   │   ├── Configurations/        AppUserConfig, PhotoConfig, ... (IEntityTypeConfiguration)
│   │   │   ├── Repositories/          UserRepository
│   │   │   └── Seed/                  Seed.cs, UserSeedData.json
│   │   ├── Identity/                  IdentityService (wraps UserManager/SignInManager)
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

- **Domain** references nothing in the solution (one external exception below).
- **Application** references **Domain** only.
- **Infrastructure** references **Application** (and Domain transitively).
- **Api** references **Application** + **Infrastructure** (Infrastructure only so `Program.cs` can wire DI —
  controllers must touch Application abstractions, never Infrastructure types).

`dotnet` will refuse to compile a cycle, so these references *are* the architecture.

---

## 4. The decisions this refactor forces

These are the parts that are genuinely "it depends." Each has a recommendation; flag any you disagree with
before we start coding.

### D1 — ASP.NET Identity makes Domain *not* dependency-free  ⚠️ most important

`AppUser : IdentityUser<int>`, `AppRole : IdentityRole<int>`, `AppUserRole : IdentityUserRole<int>`.
Those base types live in `Microsoft.Extensions.Identity.Stores`. So a Domain project that holds these
entities **must reference that package** — it cannot be a pure POCO library.

| Option | What it means | Cost |
|--------|---------------|------|
| **D1-a (recommended)** | Domain references `Microsoft.Extensions.Identity.Stores`. Entities stay exactly as they are. | One "impure" dependency in Domain. Universally accepted for Identity apps. |
| D1-b (purist) | Domain holds plain POCOs; separate `IdentityUser`-derived persistence entities live in Infrastructure; map between them. | A lot of boilerplate + a mapping layer, for little benefit on a solo learning project. |

**Recommendation: D1-a.** Note the compromise in a comment and move on. Revisit only if Identity is ever
swapped out (it won't be here).

### D2 — How do CQRS handlers talk to Identity?

`UserManager`/`SignInManager` are Identity services. Handlers in Application can't reference them without
Application taking an Identity dependency.

| Option | What it means |
|--------|---------------|
| **D2-a (recommended)** | Define `IIdentityService` in `Application/Abstractions` (CreateUser, CheckPassword, AddToRole, FindByName…). Implement it in `Infrastructure/Identity` wrapping `UserManager`/`SignInManager`. Handlers depend on the interface. | 
| D2-b | Handlers depend on `UserManager<AppUser>` directly; Application references the Identity package. | 

**Recommendation: D2-a.** It's the single best clean-architecture exercise in this whole phase — it's
exactly the abstraction Identity *doesn't* give you out of the box, and it makes handlers unit-testable
with a fake. `ITokenService` already proves you know the pattern; this extends it.

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
| `Entities/AppUser,AppRole,AppUserRole,Photo` | `Domain/Entities/` | move + file-scoped namespace |
| `Interfaces/IUserRepository,ITokenService,IPhotoService` | `Domain/Interfaces/` | move |
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
| *(new)* | `Application/Abstractions/IIdentityService` | D2-a |
| *(new)* | `Application/Common/Behaviors/ValidationBehavior` | FluentValidation in MediatR pipeline |

### NuGet ownership after the split

- **Domain:** `Microsoft.Extensions.Identity.Stores` (D1-a) — nothing else.
- **Application:** `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `AutoMapper`.
- **Infrastructure:** `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`,
  `Npgsql.EntityFrameworkCore.PostgreSQL`, `EFCore.NamingConventions`,
  `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`,
  `CloudinaryDotNet`.
- **Api:** `Microsoft.AspNetCore.OpenApi` (it's `Sdk.Web`; no EF/Cloudinary refs).

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

1. Create `BettingSite.sln`; create the four `src/` projects + three `tests/` skeletons; set project references per §3.
2. Move **Domain** (entities, interfaces). File-scoped namespaces → `BettingSite.Domain.*`. Build.
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
3. **First vertical slice — Register**: `RegisterCommand` + handler returning `Result<UserDto>`, using
   `IIdentityService` (D2-a). `AccountController.Register` becomes a 3-line dispatch. Run + test.
4. **Login** slice (`LoginQuery`), then **GetMembers / GetMember / UpdateMember**. Delete `UserRepository`
   methods as handlers absorb them (or keep the repo behind the handler — your call per slice).
5. **FluentValidation**: validators for `RegisterCommand`, `LoginQuery`, `UpdateMemberCommand`; wire
   `ValidationBehavior` into the MediatR pipeline so validation runs before every handler.
6. **`GlobalExceptionHandler : IExceptionHandler`** + `AddProblemDetails()`; delete `ExceptionMiddleware`
   and `ApiException` (D3).
7. **EF audit**: move relationship config from `OnModelCreating` into `IEntityTypeConfiguration<>` classes;
   confirm snake_case still applied; decide D4.
8. **Checkpoint:** full smoke test again; controllers contain no business logic. Commit per slice.

> Per `PLAN.md`, **tests come in Phase 2.** 1B leaves clean seams (handlers + `IIdentityService`) so that
> Phase 2 can unit-test handlers with fakes and integration-test repositories with TestContainers.

---

## 7. Definition of done for Phase 1

- [ ] Solution builds with the four-project dependency graph in §3; no reference cycles.
- [ ] `dotnet run` boots; migrations apply; register / login / members / admin all work unchanged from the client's view (no API contract change).
- [ ] Controllers contain **no** business logic — each action dispatches a MediatR request and maps `Result` → HTTP.
- [ ] Expected failures flow through `Result<T>`; unexpected ones through `IExceptionHandler`/`ProblemDetails`.
- [ ] Validators run via the MediatR pipeline for all commands/queries that take input.
- [ ] No `Microsoft.EntityFrameworkCore` / `CloudinaryDotNet` reference reachable from a controller.
- [ ] `ARCHITECTURE_REFACTOR.md` decisions (D1–D5) recorded with the option chosen.

---

## 8. Rollback

Work happens on a branch with a commit per step (§6), so any step is independently revertible and we can
`git bisect` a regression to the exact slice. Worst case: `git reset --hard origin/master`.

---

## 9. Open decisions to confirm before coding

| # | Decision | Recommendation |
|---|----------|----------------|
| D1 | Identity coupling in Domain | **D1-a**: Domain references `Identity.Stores`; entities unchanged |
| D2 | Handlers ↔ Identity | **D2-a**: `IIdentityService` abstraction in Application, impl in Infrastructure |
| D3 | Errors | `Result<T>` for business outcomes + `IExceptionHandler` for the unexpected |
| D4 | `Photo` mapping | Keep as separate entity; revisit if it stays single-avatar |
| D5 | JWT validation wiring | In `AddInfrastructure(config)`; `Program.cs` stays a composition root |
| — | Folder convention | `Features/<UseCase>/` vertical slices (vs. tech-folders `Commands/`, `Queries/`). Recommended: vertical slices. |
