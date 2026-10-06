# Betting Site

A casino-style betting platform with social features — friend system, private messaging, user profiles, and a global chat — built on **ASP.NET Core (.NET 10)** and **Angular 22**.

> **Status: active rewrite / learning sandbox.** The backend API implements auth, identity, roles, profiles, and local photo uploads. PostgreSQL runs via Docker. The `client/` Angular 22 app is a migration skeleton; `client-old/` provides the legacy UI reference, not proof that every backend feature exists. This repo is used to practice modern .NET, Angular, Clean Architecture, and DevOps — see [Backlogs](#backlogs).

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core (.NET 10), C# |
| **Database** | PostgreSQL via EF Core (Npgsql), snake_case naming convention |
| **Auth** | ASP.NET Identity + JWT bearer tokens, role-based authorization |
| **Mapping** | AutoMapper |
| **Media** | Local file storage (Cloudinary deferred to a later phase) |
| **Frontend** | Angular 22 (standalone components, signals), Tailwind CSS v4 |
| **Containers** | Docker + docker-compose; multi-stage Dockerfile for the API |
| **Testing** | Vitest (client) |
| **API docs** | OpenAPI (`Microsoft.AspNetCore.OpenApi`) |

---

## Repository Layout

```
betting-site/
├── src/
│   ├── BettingSite.Domain/          Pure business entities — no framework dependencies
│   │   └── Betting/                 Photo (more domain types added in Phase 1B)
│   ├── BettingSite.Application/     Use-case contracts: interfaces, DTOs
│   │   ├── Abstractions/            ITokenService, IPhotoService, IUserRepository
│   │   ├── Common/                  PhotoUploadResult, PhotoDeleteResult
│   │   └── DTOs/                    Request/response shapes
│   ├── BettingSite.Infrastructure/  Framework implementations
│   │   ├── Identity/                ApplicationUser : IdentityUser<int>, ApplicationRole, ApplicationUserRole
│   │   ├── Mappings/                AutoMapperProfiles
│   │   ├── Migrations/              EF migrations and model snapshot
│   │   ├── Persistence/             DataContext, Repositories/UserRepository, Seed
│   │   ├── Services/                TokenService, LocalPhotoService
│   │   ├── Settings/                JwtSettings
│   │   └── DependencyInjection.cs   AddInfrastructure() — single DI entry point
│   └── BettingSite.API/             HTTP edge: controllers, middleware, Program.cs
│       ├── Controllers/             Account, Users, Admin, Error (thin — dispatch only in 1B)
│       ├── Errors/                  ApiException
│       ├── Extensions/              ClaimsPrincipalExtensions
│       └── Middleware/              ExceptionMiddleware
├── tests/
│   ├── BettingSite.Domain.Tests/
│   ├── BettingSite.Application.Tests/
│   └── BettingSite.Infrastructure.Tests/
├── client/              Angular 22 app (active migration target — skeleton)
├── client-old/          Legacy Angular app (full feature reference)
├── Dockerfile           Multi-stage API image (SDK build → ASP.NET runtime)
├── docker-compose.yml   Local dev: PostgreSQL container
├── .env.example         Required environment variable template
└── BettingSite.slnx     Solution file (all src/ and tests/ projects)
```

---

## Features

**Implemented (backend + legacy client):**
- Registration & login with ASP.NET Identity (salted/hashed passwords) and JWT auth
- Role management — `Admin`, `Moderator`, `User` — with policy-based authorization
- User profiles with editable details and local avatar upload/delete
- Custom global error handling (server middleware + client guards in legacy app)
- Database auto-migration and user seeding on startup (dev only)

**Planned / in migration** (implemented in `client-old/`, being ported to `client/`):
- Virtual betting games
- Real-time private messaging and a global group chat
- Wallet, currency, and withdrawals
- Full admin tooling and moderator controls

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 24 LTS (≥ 24.15.0) and npm (for the Angular client)
- [Docker](https://www.docker.com/) with docker-compose (for PostgreSQL)

### 1. Start PostgreSQL

```bash
# Copy the env template and fill in values
cp .env.example .env

# Start Postgres in the background
docker-compose up -d
```

### 2. Configure API secrets

The API reads sensitive config from user secrets in development (the `UserSecretsId` is already in `src/BettingSite.API/BettingSite.API.csproj`):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=<POSTGRES_DB>;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>" \
  --project src/BettingSite.API

dotnet user-secrets set "JwtSettings:TokenKey" \
  "a-long-random-signing-key-at-least-64-chars" \
  --project src/BettingSite.API

dotnet user-secrets set "SeedSettings:AdminPassword" "your-admin-password" --project src/BettingSite.API
dotnet user-secrets set "SeedSettings:AdminEmail" "admin@example.test" --project src/BettingSite.API
dotnet user-secrets set "SeedSettings:DefaultUserPassword" "your-dev-password" --project src/BettingSite.API
```

Replace `<POSTGRES_DB>`, `<POSTGRES_USER>`, `<POSTGRES_PASSWORD>` with the values you set in `.env`.

### 3. Run the API

```bash
# From the repo root — applies EF migrations and seeds data automatically on startup (dev only)
dotnet watch run --project src/BettingSite.API
```

The API listens on **https://localhost:5001** (and http://localhost:5000). In development, the OpenAPI document is served at `https://localhost:5001/openapi/v1.json`. CORS is configured to allow the Angular dev origin `https://localhost:4200`.

### 4. Run the client

```bash
cd client
npm install
npm start        # ng serve → https://localhost:4200
```

---

## Database & Seeding

EF migrations are applied automatically on startup in development (`AutoMigrateOnStartup: true` in `appsettings.Development.json`). This is **disabled by default** (`false` in `appsettings.json`) to avoid race conditions in multi-instance deployments — run migrations as a separate step in production.

On a fresh database, `Seed.cs` creates the `User`, `Moderator`, and `Admin` roles and loads sample users from `src/BettingSite.Infrastructure/Persistence/Seed/UserSeedData.json`. Passwords come from `SeedSettings:DefaultUserPassword` and `SeedSettings:AdminPassword` in local configuration/user-secrets; the admin email comes from `SeedSettings:AdminEmail`. Do not put credentials in the seed JSON.

### EF Core commands

```bash
# Requires the dotnet-ef tool: dotnet tool install -g dotnet-ef

# Add a migration (migrations live in Infrastructure; startup project is API)
dotnet ef migrations add <MigrationName> -p src/BettingSite.Infrastructure -s src/BettingSite.API

# Apply migrations manually (also runs automatically on startup in dev)
dotnet ef database update -p src/BettingSite.Infrastructure -s src/BettingSite.API
```

---

## Docker

The `Dockerfile` at the repo root is a multi-stage build for the API:
- **Build stage** — `mcr.microsoft.com/dotnet/sdk:10.0`: restores, compiles, and publishes
- **Runtime stage** — `mcr.microsoft.com/dotnet/aspnet:10.0`: minimal image, listens on port 8080

```bash
docker build -t betting-api .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=...;..." \
  -e JwtSettings__TokenKey="..." \
  betting-api
```

Note: use double underscores (`__`) as the hierarchy separator for .NET configuration keys in environment variables.

---

## API Reference

All routes are prefixed with `/api`. Endpoints marked 🔒 require a JWT bearer token; 👑 require the `Admin` role.

### Account
| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/account/register` | Register a new user, returns a JWT |
| `POST` | `/api/account/login` | Authenticate, returns a JWT |

### Users 🔒
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/users` | List all members |
| `GET` | `/api/users/{username}` | Get a member's profile |
| `PUT` | `/api/users` | Update the current user's profile |
| `POST` | `/api/users/add-photo` | Upload/replace avatar (multipart `file`) |
| `DELETE` | `/api/users/delete-photo` | Remove the current avatar |

### Admin 👑
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/admin/get-user-roles` | List users with their roles |
| `POST` | `/api/admin/edit-roles/{username}?roles=Admin,User` | Set a user's roles |

---

## Backlogs

- [Technical backlog](.agents/TECHNICAL_PLAN.md): remaining architecture, testing, client, and delivery work.
- [Feature backlog](.agents/FEATURE_PLAN.md): implemented functionality and upcoming features.

## AI assistants

[AGENTS.md](AGENTS.md) contains short repository instructions. `.agents/` holds the two backlogs and
shared task procedures in `skills/`. Ask an assistant to read the matching `SKILL.md` when it does not
discover it automatically.

Codex discovers `.agents/skills`. Claude Code can load the root `AGENTS.md` on supported versions, but
uses its own locations for native skill commands; it can follow our skill files directly from their paths.
No additional agent folders are needed for that workflow. See [Claude instructions](https://code.claude.com/docs/en/memory#agents-md)
and [skill discovery](https://code.claude.com/docs/en/skills#choose-where-skills-load).

Global settings are personal preferences applied across projects; skills are procedures for particular
tasks. The earlier global templates were installed in `~/.codex/AGENTS.md` and `~/.agents/skills` on this
machine. Those settings do not need duplicate templates inside this repository.

## License

See [LICENSE](LICENSE).
