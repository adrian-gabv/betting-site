# Feature Plan — Betting Site Product Roadmap

Business/feature counterpart to `TECHNICAL_PLAN.md`. That file owns the *technical* phases (architecture,
testing, CI/CD, observability, cloud); this one owns **what the product does for its users** and tracks
feature delivery. Neither is a decision log — `.claude/adr/` stays the source of truth for decisions.

How the two plans relate:

- A feature ships when its full slice is done — UI + API + domain — which usually spans several technical
  phases; each feature below points at the phase(s) that unblock it.
- Features are grouped by **bounded context** ([[ADR-0002]]) so product scope and the module/service
  boundaries (`TECHNICAL_PLAN.md` Phases 7–8) stay aligned by construction.
- The same governance applies: the Definition of Done, ADRs for significant decisions, zero high/critical
  vulns at merge.

Legend: ✅ done · 🔄 in migration (implemented in `client-old/`, being ported to `client/`) · 🆕 not built.
"Backend" = the .NET API; "Client" = the active Angular 22 app.

---

## Product vision

A casino-style **virtual betting platform with a social core**: players register, manage a profile and a
virtual-currency wallet, place bets on games, and hang out — global chat, private messaging, friends.
Admins and moderators govern users, roles, and content. Everything runs on play money — there is no
real-money gambling.

---

## Feature areas

### 1. Identity & Access (Identity context)

| Feature | Backend | Client | Notes / unblocked by |
|---|---|---|---|
| Register / login with JWT | ✅ | 🔄 | Client auth core is Phase 3 |
| Roles (`Admin` / `Moderator` / `User`) + policy-based authorization | ✅ | 🔄 | Role-based guards land in Phase 3 |
| Password policy + account lockout | ✅ | — | Phase 0 hardening |
| OAuth2/OIDC, refresh tokens + revocation, MFA | 🆕 | 🆕 | Deferred auth-hardening epic (`TECHNICAL_PLAN.md` → Backlog) |

### 2. Player Profiles (Social context)

| Feature | Backend | Client | Notes / unblocked by |
|---|---|---|---|
| Member list + member profile page | ✅ | 🔄 | Phase 4 |
| Edit profile / avatar upload + delete (validated) | ✅ | 🔄 | Local storage; Cloudinary deferred |
| Friend system | 🆕 | 🆕 | Social-context modeling; scope with Phase 4/7 |

### 3. Messaging & Chat (Social context)

| Feature | Backend | Client | Notes / unblocked by |
|---|---|---|---|
| Private messaging (real-time) | 🆕 | 🔄 | SignalR hub; UI reference in `client-old/` |
| Global chat | 🆕 | 🔄 | SignalR hub; UI reference in `client-old/` |

### 4. Betting & Games (Betting context)

| Feature | Backend | Client | Notes / unblocked by |
|---|---|---|---|
| Virtual betting games | 🆕 | 🔄 | Games UI exists in `client-old/`; the real bet/odds domain model is the Phase 1B remodel + Phase 7 module |
| Bet settlement & payouts | 🆕 | — | Domain services (Phase 7) |

### 5. Wallet (Wallet context)

| Feature | Backend | Client | Notes / unblocked by |
|---|---|---|---|
| Balance display | 🆕 | 🆕 | Read-only first (Release A) |
| Virtual currency + wallet transfers | 🆕 | 🆕 | `Wallet` / `Money` value object — Phase 1B/7 domain work |
| Withdrawals | 🆕 | 🆕 | Play-money redemption flow; needs the wallet domain first |

### 6. Admin & Moderation (cross-context)

| Feature | Backend | Client | Notes / unblocked by |
|---|---|---|---|
| Role management (list users with roles, edit roles) | ✅ | 🔄 | `/api/admin/*`, Admin-role gated |
| Admin panel | ✅ (endpoints) | 🔄 | Role-gated UI — Phase 4 |
| Moderator controls (chat/content moderation) | 🆕 | 🔄 | Scope together with the chat features |

---

## Release A scope (feature view)

What Release A must ship on the new client — mirrors `TECHNICAL_PLAN.md` Phase 4, built on the design
system, migrated from `client-old/` as reference:

- [ ] Member list + member profile page.
- [ ] Edit profile / avatar upload (validated).
- [ ] Private messaging (real-time via SignalR).
- [ ] Global chat (SignalR hub).
- [ ] Admin panel (role-gated).
- [ ] Wallet / balance display (read-only until the Betting/Wallet domain lands).

Games, full wallet operations, withdrawals, and the friend system come after the betting/wallet domain
model exists (Phase 1B step 4, Phase 7) — feature-complete betting is Release B+ scope.

---

## Ideas / not committed

Feature proposals land here only after being discussed and approved — until then they live in
conversation. (Currently empty.)
