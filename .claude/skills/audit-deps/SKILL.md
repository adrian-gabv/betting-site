---
name: audit-deps
description: Run a local dependency-vulnerability audit across the .NET API and the Angular clients, then summarize findings by severity. Use when asked to check or scan dependencies for vulnerabilities/CVEs, run a security/dependency audit, or verify there are no high/critical vulns before a merge.
---

# audit-deps

Local dependency vulnerability scan. No GitHub/Dependabot PRs — this just runs the package auditors that ship with the toolchains and reports what they find.

The bar (PLAN.md non-functional targets): **zero high/critical vulnerabilities at any merge.** Any High or Critical in the API or the active client is a blocker that must be fixed or explicitly waived.

## Steps

1. **.NET API** — from the repo root:
   ```
   dotnet restore API/API.csproj
   dotnet list API/API.csproj package --vulnerable --include-transitive
   ```
   `no vulnerable packages` = pass. Otherwise capture each package, severity, and advisory URL.

2. **Active Angular client** (`client/`):
   ```
   npm --prefix client audit --package-lock-only
   ```
   `--package-lock-only` works without `node_modules`. Add `--omit=dev` to separate runtime vulns from dev-only tooling (vite/undici/esbuild are usually dev-only).

3. **`client-old/`** (legacy reference — report only, do **not** "fix"):
   ```
   npm --prefix client-old audit --package-lock-only
   ```
   These clear when `client-old/` is retired; never open changes there.

## Report

Print one summary table: `ecosystem | critical | high | moderate | low | action`. Then:
- Any **critical/high** in API or `client/` → flag as a merge blocker with the concrete fix (`npm audit fix`, a targeted bump, or `dotnet add API package <id> --version <fixed>`).
- Clean → state "no high/critical" explicitly.
- `client-old/` counts are informational; list the total but do not gate on them.

After applying any fixes, re-run the relevant command to confirm the counts dropped.
