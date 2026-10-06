---
name: betting-site-audit-deps
description: Audit Betting Site's .NET API and Angular clients for dependency vulnerabilities, with the active client gated and the legacy client reported separately. Use for dependency/CVE scans and pre-merge vulnerability checks in this repository.
---

# Betting Site dependency audit

Run the toolchain auditors and report their evidence. An audit request covers scanning and reporting;
change dependencies only when fixes are requested. Explain relevant findings and upgrade trade-offs briefly.

The project policy in `.agents/TECHNICAL_PLAN.md` is no high/critical findings in the API or active client
at merge unless Adrian explicitly records a waiver. The legacy client is informational. Never carry a
historical clean result forward as proof of the current dependency state.

## Scan from the repository root

1. **.NET solution, including transitive packages:**
   ```sh
   dotnet restore BettingSite.slnx
   dotnet list BettingSite.slnx package --vulnerable --include-transitive
   ```
   Record project, affected package/version, severity, and advisory URL. This solution also contains test
   projects; identify tooling/test findings separately rather than describing all packages as deployed.
2. **Active client:**
   ```sh
   npm --prefix client audit --package-lock-only
   ```
   Add a separate scan with `--omit=dev` when useful to distinguish runtime from development dependencies.
   Development-only findings still count under the active-client policy unless explicitly waived.
3. **Legacy reference client, report only:**
   ```sh
   npm --prefix client-old audit --package-lock-only
   ```
   Do not change `client-old/`. Its findings remain until addressed or the legacy code is retired.

Auditors consult package/advisory services over the network. A nonzero exit may mean vulnerabilities or
a failed scan: inspect the output. Missing toolchains, restore failures, or inaccessible registries make
the result incomplete, not clean. Do not update a roadmap baseline unless a successful scan supports it.

## Report and remediation

Use `project | ecosystem | critical | high | moderate | low | action`; use `unknown` for unavailable counts.
List concrete advisories and affected versions. Keep raw tool counts attributable to their ecosystem;
do not sum .NET/npm counts as if they used the same counting method. State “no high/critical” only when
every required active-project scan completed and supports it.

For requested fixes, inspect the dependency path first. Prefer a compatible direct/parent package update;
a transitive override requires checking constraints and explaining why it is appropriate. Avoid blind
`npm audit fix --force` upgrades. Rerun the affected auditor and relevant build/tests after changes.
Report unresolved findings and any explicit waiver with its scope, reason, and revisit condition. Do not
create external issues or pull requests unless requested.
