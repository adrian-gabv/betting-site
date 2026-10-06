# Feature backlog

A play-money betting app with profiles, friends, messaging, games, and moderation. No real-money gambling.
Backend features below exist in the current source; the active Angular client is still a skeleton.
The legacy client is a UI reference, not proof that a backend feature is complete.

## Existing backend

- [x] Registration/login, JWT, password policy, and lockout.
- [x] User roles and admin role-management endpoints.
- [x] Member list/profile, profile edits, and local avatar upload/delete.
- [x] Balance value exposed in account responses; a dedicated Wallet model is still pending.

## Active client: first usable version

- [ ] App shell, navigation, login, and registration.
- [ ] Member list and profile page.
- [ ] Profile editing and avatar management.
- [ ] Admin role-management screen.
- [ ] Read-only balance display.

## Next features

- [ ] Private messaging and global chat with SignalR.
- [ ] Friend requests and friends list.
- [ ] Play-money Wallet operations with clear balance/transfer rules.
- [ ] One betting game: place a bet, settle it, and update the balance.
- [ ] Moderator controls for users and chat content.

## Later ideas

- More games, bet history, and activity views.
- Play-money redemption/withdrawal flow, if still useful.
- Additional admin tooling.

Choose one complete UI/API behavior at a time. Technical changes are tracked in
[the technical backlog](TECHNICAL_PLAN.md); features do not require microservices or cloud deployment.
