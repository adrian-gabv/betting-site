# Architecture Decision Records (ADRs) — and how to think like an engineer

This folder is two things at once:

1. **A log of the real decisions** made on this project (`0001`…), each one append-only and dated.
2. **A self-teaching guide** — because the point of this project is to turn a *Software Developer*
   (someone who makes code work) into a *Software Engineer* (someone who makes and defends decisions
   under constraints, and owns their consequences over time).

Read this whole file once. Then the worked examples (`0001`–`0003`) will make sense as *examples of the
skill*, not just project trivia — note `0001`/`0002` are `Accepted` (decided), `0003` is `Proposed` (still
open, your call). Then write your own (start with the exercise at the bottom).

---

## 1. What an ADR is (in one sentence)

> An ADR captures **one architecturally significant decision**: the context that forced it, the options
> you really considered, the option you chose, and the consequences you accepted by choosing it.

It is **not** documentation of *how the system works* (that's a README/architecture doc). It is a record
of *why the system is the way it is* — frozen at the moment the decision was made.

Origin: Michael Nygard, *"Documenting Architecture Decisions"* (2011). It's a 5-minute read; read it.

---

## 2. Why this is THE developer → engineer skill

A developer is handed a problem and writes code that solves it. An engineer is handed an *ambiguous*
problem with competing forces (time, cost, team size, performance, security, "we might need X later")
and has to **choose** — knowing every choice closes some doors and opens others, and that they'll be
answering for that choice in a year.

The two "Laws of Software Architecture" (Richards & Ford) are the whole job:

1. **Everything is a trade-off.** If you think you found an option with no downside, you haven't found
   the downside yet. Your job isn't to find the "right" answer — it's to find the *least-wrong* answer
   for *these* forces and to be honest about what you gave up.
2. **"Why" is more important than "how."** Anyone can read the code to see *how*. Almost no one can
   reconstruct *why* it was built that way — which means six months later (or a teammate, or future you)
   will "fix" a decision they don't understand and reintroduce the exact problem it solved.

**ADRs are the artifact of Law #2.** Writing them *forces* you to do Law #1 honestly, because the
template has a "consequences / trade-offs accepted" section you can't leave blank without feeling the lie.

If you do nothing else from this guide: **before any non-trivial choice, write down the options and what
each one costs.** That single habit is most of the gap between the two job titles.

---

## 3. Anatomy of a good ADR

Use `0000-template.md`. The fields, and what each is really asking:

| Field | The real question |
|---|---|
| **Title** | What is the *one* decision? If you need "and", it's probably two ADRs. |
| **Status** | Is this proposed, accepted, or has a later ADR replaced it? (lifecycle below) |
| **Context & problem** | What forces make this a decision at all? What breaks if we do nothing? |
| **Decision drivers** | Which quality attributes actually matter *here*? (the "-ilities", below) |
| **Options considered** | At least **2–3 genuine** options, each with cost. "Do nothing" is always an option. |
| **Decision** | What we chose, stated plainly, and the *because*. |
| **Consequences** | What gets better, **what gets worse** (this is the honest part), and when to revisit. |

The two sections people skip — *Options* and the *negative* Consequences — are the only two that prove you
actually engineered the decision instead of defaulting to it.

---

## 4. How to actually make an architectural decision

A repeatable loop. This is the thinking the ADR *records*; do the thinking first, write second.

### Step 1 — Frame the problem and surface the forces
Write the decision as a question. ("How do CQRS handlers talk to ASP.NET Identity without the Application
layer depending on Identity?") List the forces pulling on it: technical constraints, team size (here: 1),
the learning goal, performance/security needs, future plausibility. Naming the forces is half the work.

### Step 2 — Decide how reversible it is (one-way vs two-way doors)
Amazon's framing: a **two-way door** is cheap to undo — decide fast, don't over-agonize, you can walk back.
A **one-way door** is expensive/impossible to reverse (a public API contract, a data model thousands of
rows now depend on, a messaging technology three services are coupled to) — slow down, get more eyes,
prototype. *Most decisions are two-way doors and developers over-think them; the few one-way doors are the
ones worth an ADR and real deliberation.* Note which kind in the ADR.

### Step 3 — Generate genuine options (minimum 2–3)
If you only have one option, you have a foregone conclusion, not a decision. Always include the boring
option ("keep it as-is / do nothing") and the simplest thing that could work. Beware the option you
already emotionally picked — steel-man at least one alternative.

### Step 4 — Evaluate against the quality attributes that matter *here*
You cannot maximize everything — performance vs simplicity, flexibility vs cost, consistency vs
availability. Pick the 2–4 **architecture characteristics** ("-ilities") that dominate *this* decision and
score options against *those*, not all of them. Common ones:

> maintainability, testability, scalability, performance, security, availability, deployability,
> simplicity, evolvability, observability, cost, developer experience, time-to-learn.

This is a lightweight version of **ATAM** (Architecture Tradeoff Analysis Method): make the trade-offs
explicit instead of vibes.

### Step 5 — Weigh the true cost
Three costs, not one:
- **Cost to build now.**
- **Carrying cost** (complexity tax you pay every day it exists — the real killer in distributed systems).
- **Cost to reverse** (from Step 2).
Apply **YAGNI** and the **Last Responsible Moment**: don't pay for flexibility you can't yet justify, but
don't wait past the point where the decision gets expensive to change.

### Step 6 — Decide, and record the consequences honestly
State the choice and the *because*. Then write what you **gave up** and the conditions under which you'd
revisit. An ADR with an empty downsides section is a press release, not an engineering document.

---

## 5. When to write an ADR (and when not to)

**Write one when** the decision is *architecturally significant* — i.e. it's hard/expensive to reverse,
affects structure across modules, constrains future options, picks a core technology, or you found
yourself arguing the trade-offs (even with just yourself). Rule of thumb: *"will someone later wonder why
we did this, and be tempted to undo it?"* → ADR.

**Don't write one for** routine, easily-reversible choices: naming a variable, picking a date library,
formatting. That's noise. (Though for a *learning* project, erring toward writing a few extra is fine —
the practice itself is the goal.)

---

## 6. Lifecycle, immutability, numbering

- **Statuses:** `Proposed` → `Accepted` → later maybe `Deprecated` or `Superseded by ADR-NNNN`.
- **ADRs are immutable once Accepted.** You do **not** edit an old decision when you change your mind —
  you write a *new* ADR that supersedes it, and set the old one's status to `Superseded by ADR-XXXX`.
  The log is a history, not a wiki. (Fixing typos/links is fine; rewriting the decision is not.)
- **Numbering:** zero-padded, monotonic, in *creation* order — not logical order. It's normal for a
  foundational decision to be numbered *after* decisions that logically depend on it, if you happened to
  write it later. That's honest; don't renumber to make the log look tidy.
- **Filename:** `NNNN-kebab-case-title.md`.

---

## 7. How ADRs fit the bigger skill: system design

ADRs are the *recording* discipline. The *deciding* discipline is **software architecture / system
design**, and it's a deep field. The fastest way to get good is a loop:

> **Study a pattern → spot the decision it implies → make that decision on this project → ADR it →
> later, feel the consequence you predicted (or didn't) → update your judgement.**

This project is deliberately structured to give you that loop across the whole 2026 stack: clean
architecture, CQRS, modular monolith, microservices, messaging, Kubernetes, observability. Each phase in
`../PLAN.md` is a pile of latent decisions waiting to be ADR'd. **Conway's Law** ("systems mirror the
communication structure of the org that builds them") is worth internalizing early — it's *the* reason
service boundaries are an org decision as much as a technical one, even for a solo project where "the org"
is just you.

---

## 8. Study path (ordered, with the why)

You don't need all of these. Go top-to-bottom; stop when you're unblocked and come back.

### Start here (ADRs + the architect mindset)
- **Michael Nygard, "Documenting Architecture Decisions"** (blog, 2011) — the origin of ADRs.
- **adr.github.io** + **MADR (madr.dev)** — template collections; steal shamelessly.
- **ThoughtWorks Technology Radar** — search "Lightweight Architecture Decision Records" (rated *Adopt*),
  and skim the Radar generally to see how pros frame "adopt/trial/assess/hold" decisions.
- **Mark Richards & Neal Ford, *Fundamentals of Software Architecture*** — **read this first, cover to
  cover.** Architecture characteristics, styles, trade-off analysis, the two Laws, the architect's role.
  It is the single best on-ramp for Developer → Engineer.

### System design & data (the core)
- **Martin Kleppmann, *Designing Data-Intensive Applications* (DDIA)** — the most important systems book
  of the era. Replication, partitioning, consistency, the CAP/PACELC reality. Slow, worth it.
- **AWS Builders' Library** (aws.amazon.com/builders-library) — free, short, *exceptional* essays by
  people who run planet-scale systems. Read "Avoiding fallback in distributed systems", the timeout/retry
  /jitter ones, etc. Directly feeds your microservices phases.
- **ByteByteGo / Alex Xu, *System Design Interview* vol 1 & 2** — breadth, diagrams, the vocabulary.
- **The System Design Primer** (GitHub: donnemartin/system-design-primer) — free, broad index.

### Architecture styles & distributed systems
- **Richards & Ford, *Software Architecture: The Hard Parts*** — exactly your Phase 7→8 problem:
  how/when to decompose, data ownership, distributed transactions, the trade-offs of going distributed.
- **Sam Newman, *Building Microservices* (2nd ed)** and ***Monolith to Microservices*** — the strangler
  pattern, when *not* to, incremental extraction. Pairs with the Phase 7→8 path in `../PLAN.md`.
- **Chris Richardson, microservices.io + *Microservices Patterns*** — saga, outbox, API composition,
  CQRS, idempotency — the patterns named in `../PLAN.md` Phase 8.
- **Neal Ford et al., *Building Evolutionary Architectures*** — fitness functions; how to keep an
  architecture honest as it changes (your CI gates are baby fitness functions).

### Domain-Driven Design (boundaries & language)
- **Vlad Khononov, *Learning Domain-Driven Design*** — the best *modern* DDD intro; bounded contexts are
  literally how you'll choose your service splits.
- Eric Evans (blue book) / Vaughn Vernon (red book) — the classics, for later depth.

### Clean architecture & .NET specifics
- **Robert C. Martin, *Clean Architecture*** — the Dependency Rule that `0001` rests on.
- **Jason Taylor's Clean Architecture solution template** (GitHub) and **Steve "Ardalis" Smith's**
  clean-architecture/guard/specification repos — canonical .NET realizations of this exact structure.

### DevOps, cloud, SRE (your later phases)
- **Google SRE book + SRE Workbook** (sre.google/books) — free; SLOs, error budgets, toil. Your
  "non-functional targets" in `../PLAN.md` come straight from this thinking.
- **Forsgren, Humble, Kim, *Accelerate*** — the DORA metrics; *why* CI/CD and small batches win, with data.
- **Kim et al., *The Phoenix Project* / *The DevOps Handbook*** — the narrative + the practices.

### The career/role transition itself
- **Gregor Hohpe, *The Software Architect Elevator*** — about *being* the engineer who connects code
  decisions to systems and to the org. Also his *Enterprise Integration Patterns* for messaging.

### Keep-sharp habits (free, ongoing)
- Read engineering blogs: Netflix, Uber, Stripe, Cloudflare, Discord, and **The Pragmatic Engineer** /
  **ByteByteGo** newsletters. Don't just admire them — for each post ask *"what decision did they make,
  what were the options, what did it cost?"* and you're doing ADR reps on someone else's dime.
- Do **architecture katas** (search "Neal Ford architecture katas" / "architectural katas"): a fake
  requirement set, you design + defend an architecture in an hour. The highest-leverage practice there is.
- Draw systems with the **C4 model** (c4model.com, Simon Brown) — context/container/component/code. Cheap,
  shared vocabulary, pairs with ADRs (ADRs = decisions, C4 = structure).

---

## 9. Your turn — practice (do these)

The fastest way to learn ADRs is to write one where the decision *isn't* pre-chewed for you.

1. **Write `0004-technology-stack.md` yourself.** The stack (.NET 10 + Angular 22 + PostgreSQL) was
   partly *inherited* from the existing project — which makes it a great first ADR, because documenting an
   *inherited / constraint-driven* decision is its own real skill. Frame it honestly:
   - *Forces:* existing codebase already on this stack; solo learning project; want modern + employable +
     cloud-native; want to deeply learn one ecosystem, not dabble.
   - *Options to steel-man:* keep .NET; rewrite backend in Node/NestJS or Go; React/Vue/Svelte instead of
     Angular; MySQL/SQL Server/Mongo instead of Postgres.
   - *Drivers to score against:* time-to-learn, ecosystem maturity, cloud/k8s fit, employability,
     migration cost, "joy of learning."
   - End with the consequences you accept (e.g. Angular's steeper curve, .NET's Windows-history baggage).
2. **Find one decision already lurking in `../PLAN.md` and ADR it before you build it.** Good candidates:
   *MediatR vs hand-rolled dispatch*, *RabbitMQ vs Azure Service Bus (and why both)*, *YARP as the gateway*,
   *database-per-service*, *Tailwind+SCSS hybrid*, *Serilog+OpenTelemetry*.
3. **After you build something, come back and add a one-line "Consequences — observed" note** to its ADR.
   Comparing what you *predicted* vs what *happened* is the feedback loop that actually builds judgement.

> Rule for this project: **no architecturally-significant PR without an ADR.** It will feel slow at first
> and then it will feel like the thing that was missing.
