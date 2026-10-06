---
name: migrate
description: Create and review an EF Core migration for Betting Site. Use when asked to add a database migration in this repository; apply it to the local PostgreSQL database only when that step is authorized.
---

# Betting Site EF Core migration

Follow `AGENTS.md`. Explain the model/snapshot changes and flag unexpected data effects. Creating a
migration does not by itself authorize applying it.

## Prepare and generate

1. Identify the intended model change and migration name. Use a supplied name or suggest a descriptive
   one from a clear change; clarify if the change itself is ambiguous. Inspect existing migrations and
   the working diff so unrelated pending model changes are not silently bundled.
2. From the repository root, generate the migration, replacing `<Name>` with the selected name:
   ```sh
   dotnet ef migrations add <Name> -p src/BettingSite.Infrastructure -s src/BettingSite.API
   ```
3. Review the generated files in `src/BettingSite.Infrastructure/Migrations/`, including `Up`, `Down`,
   and the model snapshot diff. Explain the effect on existing rows, defaults, nullability, constraints,
   and indexes. Flag unexpected drops, rename-as-drop/add operations, and data-loss paths.

## Apply and verify when requested

Review the concrete migration with Adrian before applying it. Existing explicit authorization for this
migration's application counts; do not ask the same permission twice. Newly discovered unexpected or
destructive changes require resolving their meaning before application.

Confirm the intended target is the local development database without reading or displaying secrets.
Use the documented local configuration; if the target cannot be established, clarify rather than assume.

```sh
dotnet ef database update -p src/BettingSite.Infrastructure -s src/BettingSite.API
```

This applies all pending migrations, so establish which are pending before running it. If local Postgres
is unreachable, report that the migration was generated but application was not verified; the project
README documents `docker-compose up -d`. Do not start the API just to inspect a migration: the README
documents automatic development-startup migration, which can apply it before review.

Report the files changed, checks performed, and whether application succeeded. Use a relevant local
query or integration test to check the intended schema behavior when application was run. Explain why
`Down` is not a backup; data removed by a migration may not be recoverable by reversing the schema.
