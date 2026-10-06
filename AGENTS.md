# Repository instructions

See [README.md](README.md) for the project, setup, and commands.

## Working style

- Keep changes small and explain non-obvious choices briefly.
- Default to explanations, hints, and review when helping Adrian work through code; implement when asked.
- Use the technical and feature backlogs for pending work. Existing `.agents/adr/` records are historical reference; new ADRs are optional.
- Run relevant checks and report what was actually verified.

## Project constraints

- Never read secrets files (`.env`, `.env.local`, user-secrets, credentials or token files). Use `.env.example` and documentation.
- `client/` is the active Angular app. `client-old/` is reference only; do not add code there.
- Keep Domain framework-free and ASP.NET Identity in Infrastructure. API references Application and Infrastructure; Infrastructure references Application; Application references Domain.
- The backend direction is small use-case handlers with hand-written dispatch and explicit mapping. AutoMapper remains in the current code until its callers are migrated; do not add MediatR.
- Photos use local storage. Cloudinary is deferred.
- Comment non-obvious reasons and trade-offs rather than narrating the code.

## Angular

Use standalone components, signals/computed for state, `inject()`, `input()`/`output()`, OnPush, native
`@if`/`@for`/`@switch`, class/style bindings, and reactive forms. Use component `host` metadata rather
than HostBinding/HostListener decorators. Check keyboard access, labels, contrast, and focus behavior.

## Shared resources

- [Technical backlog](.agents/TECHNICAL_PLAN.md)
- [Feature backlog](.agents/FEATURE_PLAN.md)
- [Dependency audit](.agents/skills/betting-site-audit-deps/SKILL.md)
- [EF migration](.agents/skills/migrate/SKILL.md)

Read the relevant `SKILL.md` when doing that task. These are ordinary shared workflow documents; if an
assistant does not discover them automatically, ask it to follow the file directly.
