# Betting Site

A casino-style betting platform with social features — friend system, private messaging, user profiles, and a global chat — built on **ASP.NET Core (.NET 10)** and **Angular 21**.

> **Status: active rewrite / learning sandbox.** The backend API is functional (auth, identity, roles, profiles, local photo uploads). PostgreSQL runs via Docker. The `client/` Angular 21 app is a fresh migration skeleton; the full feature implementation lives in `client-old/` and is being ported over. This repo is used to practice modern .NET, Angular, Clean Architecture, and DevOps — see [Roadmap](#️-roadmap).

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core (.NET 10), C# |
| **Database** | PostgreSQL via EF Core (Npgsql), snake_case naming convention |
| **Auth** | ASP.NET Identity + JWT bearer tokens, role-based authorization |
| **Mapping** | AutoMapper |
| **Media** | Local file storage (Cloudinary deferred to a later phase) |
| **Frontend** | Angular 21 (standalone components, signals), Tailwind CSS v4 |
| **Containers** | Docker + docker-compose; multi-stage Dockerfile for the API |
| **Testing** | Vitest (client) |
| **API docs** | OpenAPI (`Microsoft.AspNetCore.OpenApi`) |

---

## Repository Layout

```
betting-site/
├── API/                 ASP.NET Core Web API (.NET 10) — the active backend
│   ├── Controllers/     Account, Users, Admin, Error
│   ├── Data/            DataContext, UserRepository, Seed + UserSeedData.json
│   ├── DTOs/            Request/response objects
│   ├── Entities/        AppUser, AppRole, AppUserRole, Photo
│   ├── Migrations/      EF Core migrations
│   ├── Services/        TokenService (JWT), LocalPhotoService
│   ├── Extensions/      DI wiring & ClaimsPrincipal helpers
│   ├── Middleware/      Global exception handler
│   └── Program.cs       Minimal hosting entry point
├── client/              Angular 21 app (active migration target — skeleton)
├── client-old/          Legacy Angular app (full feature reference)
├── Dockerfile           Multi-stage API image (SDK build → ASP.NET runtime)
├── docker-compose.yml   Local dev: PostgreSQL container
├── .env.example         Required environment variable template
└── BettingSite.slnx     Solution file
```

> A clean-architecture refactor (splitting `API/` into Domain / Application / Infrastructure / API projects) is planned. See [`.claude/ARCHITECTURE_REFACTOR.md`](.claude/ARCHITECTURE_REFACTOR.md).

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
- [Node.js](https://nodejs.org/) 20+ and npm (for the Angular client)
- [Docker](https://www.docker.com/) with docker-compose (for PostgreSQL)

### 1. Start PostgreSQL

```bash
# Copy the env template and fill in values
cp .env.example .env

# Start Postgres in the background
docker-compose up -d
```

### 2. Configure API secrets

The API reads sensitive config from user secrets in development (the `UserSecretsId` is already in `API/API.csproj`):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=<POSTGRES_DB>;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>" \
  --project API

dotnet user-secrets set "JwtSettings:TokenKey" \
  "a-long-random-signing-key-at-least-64-chars" \
  --project API

dotnet user-secrets set "SeedSettings:AdminPassword" "your-admin-password" --project API
dotnet user-secrets set "SeedSettings:DefaultUserPassword" "your-dev-password" --project API
```

Replace `<POSTGRES_DB>`, `<POSTGRES_USER>`, `<POSTGRES_PASSWORD>` with the values you set in `.env`.

### 3. Run the API

```bash
# From the repo root — applies EF migrations and seeds data automatically on startup (dev only)
dotnet watch run --project API/API.csproj
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

On a fresh database, `Seed.cs` creates the `User`, `Moderator`, and `Admin` roles and loads sample users from `API/Data/UserSeedData.json`. Seeded users share a single dev password defined in that JSON file.

### EF Core commands

```bash
# Requires the dotnet-ef tool: dotnet tool install -g dotnet-ef

# Add a migration
dotnet ef migrations add <MigrationName> -p API

# Apply migrations manually (also runs automatically on startup in dev)
dotnet ef database update -p API
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

## Roadmap

This project follows a phased learning plan covering Clean Architecture, a testing pyramid, CI/CD, observability, a modular monolith, microservices, and Azure/Kubernetes deployment.

- **Full roadmap:** [`.claude/PLAN.md`](.claude/PLAN.md)
- **Phase 1 (Clean Architecture refactor) detail:** [`.claude/ARCHITECTURE_REFACTOR.md`](.claude/ARCHITECTURE_REFACTOR.md)

---

## License

See [LICENSE](LICENSE).
