# CLAUDE.md

AI assistant instructions for this repository. **For project overview, setup, and commands — see [README.md](README.md).** This file only contains constraints and pointers that are not obvious from reading the code.

## Security rules

- **Never read secrets files.** Do not read `.env`, `.env.local`, `user-secrets` files (`%APPDATA%/Microsoft/UserSecrets/**`), or any file that may contain credentials, keys, or tokens. To understand what secrets are required, read `.env.example` or README.md only.

## Project context

Personal learning sandbox — not production. Learning over shipping; every architectural decision is understood and recorded. See the ADRs.

| Pointer | Path |
|---|---|
| Roadmap & epics | `.claude/PLAN.md` |
| Clean Architecture plan | `.claude/ARCHITECTURE_REFACTOR.md` |
| ADRs | `.claude/adr/` |

## Architecture pointers

- `API/Program.cs` — entry point; auto-applies EF migrations + seeds on startup via `MigrateAsync()`
- `API/Extensions/ApplicationServiceExtensions.cs` — DI wiring (DB, repos, AutoMapper, services)
- `API/Extensions/IdentityServiceExtensions.cs` — Identity + JWT setup
- `API/Data/DataContext.cs` — EF context (`IdentityDbContext`, snake_case naming via `EFCore.NamingConventions`)
- `API/Entities/` — `AppUser` extends `IdentityUser<int>`; roles: `Admin`, `Moderator`, `User`
- `client/` — active Angular 22 app (skeleton being migrated from `client-old/`)
- `client-old/` — legacy app; full feature reference only, do not add new code here

Photos are served from local storage (`LocalPhotoService`). Cloudinary integration is deferred to a later phase — do not add it back.

## Code style

- **No useless comments.** Don't narrate what the code already says (`// inject service`, `// secret`, `// short-lived token`). Let names carry meaning. Comment only the non-obvious *why* — a workaround, a spec/RFC reference, a deliberate trade-off — never the *what*.

## Angular conventions

- Standalone components only — **do not** set `standalone: true` (it is the default in Angular v20+)
- Signals for state; `computed()` for derived state; `inject()` instead of constructor injection
- `input()` / `output()` functions, not `@Input` / `@Output` decorators
- `ChangeDetectionStrategy.OnPush` on all components
- Native control flow: `@if`, `@for`, `@switch` — not `*ngIf`, `*ngFor`
- `class` / `style` bindings instead of `ngClass` / `ngStyle`
- `NgOptimizedImage` for static images (not inline base64)
- `host` object in `@Component` / `@Directive` — not `@HostBinding` / `@HostListener`
- Reactive forms preferred over template-driven
- Must pass AXE checks and WCAG AA minimums (focus management, color contrast, ARIA)
