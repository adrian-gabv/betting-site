# ADR-NNNN: <short, decision-shaped title>

- **Status:** Proposed | Accepted | Deprecated | Superseded by ADR-XXXX
- **Date:** YYYY-MM-DD
- **Deciders:** <name(s)>
- **Tags:** <area, e.g. backend / security / data / platform>
- **Reversibility:** two-way door (cheap to undo) | one-way door (expensive/irreversible)

## Context and problem statement

What situation forces a decision? State it as a question. What breaks or stays painful if we do nothing?
What are the forces in play — technical constraints, team size, the learning goal, performance, security,
future plausibility? (Naming the forces honestly is half the work.)

## Decision drivers

The 2–4 architecture characteristics ("-ilities") or constraints that actually dominate *this* decision —
not a generic wishlist. Score options against these.

- driver 1 (e.g. testability)
- driver 2 (e.g. simplicity / carrying cost)
- driver 3

## Options considered

### Option A — <name>
What it is. Pros. Cons. Cost (build / carry / reverse).

### Option B — <name>
…

### Option C — do nothing / keep as-is
Always consider it.

## Decision

We chose **<option>** because **<the because — tie it back to the drivers>**.

## Consequences

- **Positive:** what gets better.
- **Negative / trade-offs accepted:** what gets worse, what we gave up. (Don't leave this empty.)
- **Revisit when:** the condition that would make us reopen this (and supersede with a new ADR).

## Notes / links

- Related ADRs: [[ADR-XXXX]]
- Code: `path/to/relevant/file`
- References: <book/article/pattern that informed this>
