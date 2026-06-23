# 🎲 Betting Site

A casino-style betting platform with social features — friend system, private messaging, user profiles, and a global chat — built on **ASP.NET Core (.NET 10)** and **Angular 21**.

> **Status: active rewrite / learning sandbox.** The backend API is functional (auth, identity, roles, profiles, avatar uploads). The `client/` Angular 21 app is a fresh migration skeleton; the full feature implementation lives in `client-old/` and is being ported over. This repo is used to practice modern .NET, Angular, Clean Architecture, and DevOps — see [Roadmap](#-roadmap).

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core (.NET 10), C# |
| **Database** | PostgreSQL via EF Core (Npgsql), snake_case naming convention |
| **Auth** | ASP.NET Identity + JWT bearer tokens, role-based authorization |
| **Mapping** | AutoMapper |
| **Media** | Cloudinary (avatar storage) |
| **Frontend** | Angular 21 (standalone components, signals), Tailwind CSS v4 |
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
│   ├── Services/        TokenService (JWT), PhotoService (Cloudinary)
│   ├── Extensions/      DI wiring & ClaimsPrincipal helpers
│   ├── Middleware/      Global exception handler
│   └── Program.cs       Minimal hosting entry point
├── client/              Angular 21 app (active migration target — skeleton)
├── client-old/          Legacy Angular app (full feature reference)
├── BettingSite.sln      Solution file
├── CLAUDE.md            Guidance for AI-assisted development
└── .claude/PLAN.md      Project roadmap & epics
```

> A clean-architecture refactor (splitting `API/` into Domain / Application / Infrastructure / API projects) is planned. See [`.claude/ARCHITECTURE_REFACTOR.md`](.claude/ARCHITECTURE_REFACTOR.md).

---

## Features

**Implemented (backend + legacy client):**
- 🔐 Registration & login with ASP.NET Identity (salted/hashed passwords) and JWT auth
- 👮 Role management — `Admin`, `Moderator`, `User` — with policy-based authorization
- 👤 User profiles with editable details and Cloudinary-backed avatar upload/delete
- 🛡️ Custom global error handling (server middleware + client guards in legacy app)
- 🌱 Database auto-migration and user seeding on startup

**Planned / in migration** (implemented in `client-old/`, being ported to `client/`):
- 🎮 Virtual betting games
- 💬 Real-time private messaging and a global group chat
- 💰 Wallet, currency, and withdrawals
- 🧰 Full admin tooling and moderator controls

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20+ and npm (for the Angular client)
- [PostgreSQL](https://www.postgresql.org/) running locally (or a connection string to one)
- A [Cloudinary](https://cloudinary.com/) account (for avatar uploads)

### 1. Configure secrets (API)

The API reads these settings (use **user secrets** in development — the `UserSecretsId` is already in `API/API.csproj`):

```bash
cd API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=betting;Username=postgres;Password=yourpassword"
dotnet user-secrets set "TokenKey" "a-long-random-signing-key-at-least-64-chars"
dotnet user-secrets set "CloudinarySettings:CloudName" "your-cloud-name"
dotnet user-secrets set "CloudinarySettings:ApiKey" "your-api-key"
dotnet user-secrets set "CloudinarySettings:ApiSecret" "your-api-secret"
```

### 2. Run the API

```bash
# From the repo root — applies EF migrations and seeds data automatically on startup
dotnet watch run --project API/API.csproj
```

The API listens on **https://localhost:5001** (and http://localhost:5000). In development, the OpenAPI document is served at `https://localhost:5001/openapi/v1.json`. CORS is configured to allow the Angular dev origin `https://localhost:4200`.

### 3. Run the client

```bash
cd client
npm install
npm start        # ng serve → https://localhost:4200
```

---

## Database & Seeding

Migrations are applied automatically at startup (`context.Database.MigrateAsync()` in `Program.cs`). On a fresh database, `Seed.cs` creates the `User`, `Moderator`, and `Admin` roles and loads sample users from `API/Data/UserSeedData.json`.

| Account | Username | Password | Role |
|---------|----------|----------|------|
| Admin | `ady4k` | `P@$$w0rd` | Admin |
| Seeded users | _(from JSON, e.g. `richard`, `phelps`)_ | `P@$$w0rd` | User |

> ⚠️ Seed credentials are for local development only. Never use these in any deployed environment.

### EF Core commands

```bash
# Add a migration
dotnet ef migrations add <MigrationName> -p API

# Apply migrations manually (also runs automatically on startup)
dotnet ef database update -p API
```

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

## 🗺️ Roadmap

This project follows a phased learning plan covering Clean Architecture, a testing pyramid, CI/CD, observability, a modular monolith, microservices, and Azure/Kubernetes deployment.

- **Full roadmap:** [`.claude/PLAN.md`](.claude/PLAN.md)
- **Phase 1 (Clean Architecture refactor) detail:** [`.claude/ARCHITECTURE_REFACTOR.md`](.claude/ARCHITECTURE_REFACTOR.md)

---

## License

See [LICENSE](LICENSE).
