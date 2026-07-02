Add an EF Core migration named "$ARGUMENTS" to the Infrastructure project, then verify it applies cleanly against the local Postgres container.

If $ARGUMENTS is empty, stop immediately and ask for the migration name before doing anything else.

Steps:
1. Run `dotnet ef migrations add $ARGUMENTS -p src/BettingSite.Infrastructure -s src/BettingSite.API` from the repo root.
2. Read the generated migration file in `src/BettingSite.Infrastructure/Migrations/` and show it to the user.
3. Flag anything suspicious: unexpected table drops, missing columns, data-loss operations. Ask the user to confirm the migration looks correct before proceeding.
4. Run `dotnet ef database update -p src/BettingSite.Infrastructure -s src/BettingSite.API` to apply it. If Postgres isn't reachable, remind the user to run `docker-compose up -d` first.
5. Confirm the update succeeded.
