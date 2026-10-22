# OriginSix project overview

Assessment date: **2026-10-21**. Evidence is tied to the commits in
[REPOSITORY_AUDIT.md](REPOSITORY_AUDIT.md).

## Goal and scope

Preserve classic MU Online Season 6 Episode 3 gameplay while establishing a
maintainable server/client workspace. Priorities: fidelity, reliable operation,
small upstream divergence, modular extension points, and low infrastructure cost.
The first milestone is login, character creation, movement, basic combat, and
persistent progress on an isolated local environment.

No custom feature development or engine refactoring is authorized in this phase.
An upstream-compatible development baseline is not automatically an authentic
vanilla baseline: differences must be recorded and evaluated.

## Layout

| Path | Responsibility |
|---|---|
| `../server/` | Independent OpenMU Git repository; C# server, persistence, admin UI, packet definitions |
| `../client/` | Independent MuMain Git repository; C++20 client, C# Native AOT networking, assets |
| This `docs/` directory | OriginSix setup, evidence, compatibility, decisions, roadmap |
| `../scripts/` | Development automation after its actions are reviewed/approved |
| `../AGENTS.md` | Workspace-wide agent boundaries |

The parent workspace has no Git repository. Documentation currently has no parent
version-control history; choose a separate metadata repository only with approval,
excluding `server/`, `client/`, secrets, downloads and runtime/database state.

## Verified observations

- Both official repositories exist and are clean; live upstream HEADs matched the
  checked-out commits when queried. No clone was necessary.
- OpenMU focuses on S6 EP3 English and explicitly describes itself as under development.
- MuMain is derived from Season 5.2 sources, modernized toward S6 EP3; it includes
  extended protocol, modern renderer/UI, reconnection and other differences.
- Both use .NET 10 for relevant C# projects. MuMain also needs native C++ tooling
  when built from source. Docker Linux containers are available on this PC.
- Client game data and fonts are already tracked locally; no claim about their
  completeness, legal provenance, or vanilla equivalence follows from availability.
- No builds, tests, game services, database initialization or gameplay were run.

## Proposed baseline decisions — not yet applied

- Keep the current server/client commits as a paired candidate baseline.
- Use Windows x64 player client, editor/control socket OFF, existing `legacy` UI
  theme and `Classic` render look. These settings reduce optional differences but
  do not reproduce the historical renderer or certify vanilla fidelity.
- Use one all-in-one server process plus dedicated PostgreSQL, isolated from
  existing containers. Prefer source-built Linux containers with loopback-only
  published ports for first runtime testing, using the existing Docker engine.
- Keep resets, alternative shop currencies, bots and other non-vanilla extensions
  disabled; inspect effective configuration before accepting gameplay results.
- Do not adopt distributed hosting, Dapr, a new control center, or monitoring
  infrastructure until the local baseline is working and measured.

## Unknowns and decisions still needed

Client/source and game-asset licensing; complete vanilla reference rules;
end-to-end protocol behavior; build success; database initialization and recovery;
exact supported PostgreSQL major; sustained resource usage and future VPS sizing.
See [LOCAL_SETUP.md](LOCAL_SETUP.md),
[VANILLA_COMPATIBILITY.md](VANILLA_COMPATIBILITY.md) and [ROADMAP.md](ROADMAP.md).
