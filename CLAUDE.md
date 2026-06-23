# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A casino-style betting website with social features (friend system, private messaging, profiles, global chat). Built with ASP.NET (.NET 10) backend and Angular 21 frontend. The `client-old/` directory is the legacy Angular app; the active frontend is `client/`.

**Project roadmap and epics:** `.claude/PLAN.md`

## Architecture

**Backend (`API/`)** — ASP.NET Core on .NET 10, PostgreSQL via EF Core (Npgsql), ASP.NET Identity for auth with JWT tokens, Cloudinary for avatar storage, AutoMapper for DTO mapping.

Key layers:
- `Entities/` — EF Core models (`AppUser` extends `IdentityUser<int>`, has `Avatar`, `Money`, `UserRoles`)
- `Data/` — `DataContext` (extends `IdentityDbContext`), `UserRepository`, `Seed` (seeds users/roles on startup)
- `DTOs/` — Data transfer objects for all API inputs/outputs
- `Services/` — `TokenService` (JWT), `PhotoService` (Cloudinary)
- `Extensions/` — `AddApplicationServices`, `AddIdentityServices` wire up DI in `Startup.cs`
- `Middleware/` — Global exception handler (`ExceptionMiddleware`)

Roles: `Admin`, `Moderator`, `User`. JWT `TokenKey` is read from configuration (use user secrets in dev).

**Frontend (`client/`)** — Angular 21, standalone components (no NgModules), Tailwind CSS v4, Vitest for tests.

The `client/` app is a fresh migration skeleton — routes and services are being migrated from `client-old/` (which has the full feature implementation for reference).

## Commands

### API
```bash
# Run with hot reload (from repo root)
dotnet watch run --project API/API.csproj

# Build
dotnet build API/API.csproj

# Add EF migration
dotnet ef migrations add <MigrationName> -p API

# Apply migrations
dotnet ef database update -p API
```

### Client (run from `client/`)
```bash
npm start          # ng serve — dev server at https://localhost:4200
npm run build      # production build
npm test           # vitest
```

## Configuration

The API needs:
- `ConnectionStrings:DefaultConnection` — PostgreSQL connection string
- `TokenKey` — JWT signing key
- `CloudinarySettings:CloudName`, `ApiKey`, `ApiSecret`

In development, store secrets with `dotnet user-secrets set "TokenKey" "..."` (UserSecretsId is in `API.csproj`).

The Angular dev server proxies to `https://localhost:5001` (API). CORS is configured to allow `https://localhost:4200`.

## Angular Conventions (from Copilot instructions)

- Standalone components only — **do not** set `standalone: true` (it's the default in Angular v20+)
- Use signals for state; `computed()` for derived state; `inject()` instead of constructor injection
- `input()` / `output()` functions, not `@Input`/`@Output` decorators
- `ChangeDetectionStrategy.OnPush` on all components
- Native control flow: `@if`, `@for`, `@switch` — not `*ngIf`, `*ngFor`
- `class` bindings instead of `ngClass`; `style` bindings instead of `ngStyle`
- `NgOptimizedImage` for static images (not inline base64)
- No `@HostBinding` / `@HostListener` — use `host` object in `@Component`/`@Directive` instead
- Reactive forms preferred over template-driven
- Must pass all AXE checks and WCAG AA minimums (focus management, color contrast, ARIA)
