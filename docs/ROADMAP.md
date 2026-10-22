# Roadmap and architecture boundaries

As of **2026-10-21**. Statuses distinguish static inspection from runtime validation.

## Milestones

| Milestone | State | Exit evidence |
|---|---|---|
| Workspace/repository identity | Complete | Independent official clones; clean trees; paired SHAs/default branches recorded |
| Initial environment/static compatibility audit | Complete with open questions | Prerequisite report and source-based feature matrix; licensing, fidelity and runtime questions explicit |
| Approved reproducible build environment | Blocked on approval/tooling | Exact .NET/C++/CMake/Ninja/vcpkg versions; pinned dependencies; successful source builds and logs |
| Authorized client/data baseline | Open | Source/runtime permissions and asset provenance established; exact data trees or runtime/data tag pair |
| Isolated persistent local services | Blocked on approval | Reviewed loopback-only configuration, dedicated DB/volume, healthy services, no port conflicts |
| Playable local smoke milestone | Not run | Login, new character, movement, combat, loot/EXP, persistence and reconnect results |
| Vanilla feature validation | Static checklist only | Evidence per required feature; differences reviewed against reference S6 EP3 behavior |
| Stability/restore baseline | Not run | Extended session, restart/crash recovery, DB backup/restore, duplicate/loss checks, measured resources |
| Selective upstream maintenance | Workflow documented | Commit-by-commit ledger and approved isolated regression-tested backports |
| Future Linux VPS | Deferred | Local acceptance, rights resolved, measured capacity/cost and separate deployment approval |
| Optional OriginSix extensions | Deferred | Approved requirements, default-OFF gates, unchanged baseline regressions |

## Immediate sequence

1. Approve the exact dependency/build scope in [LOCAL_SETUP.md](LOCAL_SETUP.md).
   Install only missing requirements, verify versions, build OpenMU and MuMain.
2. Resolve source/assets rights and retain a reproducible client/data pairing.
3. Approve creation of a standalone parent-workspace local Compose configuration
   and dedicated DB state. Avoid occupied 5433 and all-interface publication.
4. Approve service/client execution; capture actual startup and smoke-test results.
5. Validate feature parity, then reliability and restore. Update the documentation
   with approval before overwriting the initial evidence snapshot.

## Existing architecture — observed extension points

OpenMU already separates protocol from game behavior:

`Network.Connection -> GameServer packet-handler plugins -> GameLogic.PlayerActions`

Responses pass through `GameLogic.Views` interfaces and version-specific remote
views. Persistence is abstracted through contexts/providers, with EF Core/PostgreSQL
and an in-memory demo implementation. The all-in-one Startup hosts subsystem
implementations; Dapr hosts exist for distributed deployment.

The existing `PlugIns` library supports plugin points, keyed strategies and custom
containers, generated registries, activation/configuration and external assemblies
in a plugins directory. `IFeaturePlugIn`/`FeaturePlugInContainer` and
`IDisabledByDefault` are existing gates; initialization turns marked plugins off.
There is no need to design a new plugin framework.

| Future need | Existing boundary to study first | Constraint |
|---|---|---|
| Optional events / resets | GameLogic plugin points, mini-game services; existing `Resets/` implementation | Explicitly approved, default OFF; do not reimplement existing reset machinery |
| Monitoring / notifications | `Interfaces.GameEvent`, `IEventPublisher`, `IGameEventListener`, telemetry and admin UI | Prefer passive event consumers; measure before adding infrastructure |
| Configuration/control center | Existing admin panel, subsystem interfaces, persisted configuration | Reuse current capabilities; review auth/API contracts before a separate application |
| Custom items / mounts | DataModel configuration, item power-ups and existing initializer/plugin mechanisms | Deferred; IDs, assets, client support and serialization can require coupled changes |
| Client presentation/extensions | Existing RmlUi/theme and networking boundaries | No initial visual changes; follow client UI policy before any future UI work |

Source evidence: `server/docs-website/docs/development/architecture.md`,
`server/src/PlugIns/Readme.md`, `server/src/GameLogic/FeaturePlugInContainer.cs`,
`server/src/GameLogic/PlugIns/`, `server/src/Persistence/Initialization/DataInitializationBase.cs`,
and client build/README documentation. These are candidate boundaries, not a proof
that arbitrary features require no core modifications.

## Future design policy — proposed, no implementation

Keep custom server logic in separate plugin assemblies/projects only when an
approved feature needs them. Give plugins stable unique IDs, explicit configuration,
default-OFF behavior and regression tests that compare enabled/disabled modes.
Keep baseline initial data separate from custom data; never silently change formulas
or replace core networking. Review data migrations and packet changes as coupled
server/client changes. Rollback must include data considerations, not only Git.

Use one process plus PostgreSQL initially. Reuse Docker on the existing PC. Estimate
a future Linux VPS only after measuring actual idle/session/event memory, CPU,
storage and bandwidth. No public deployment, distributed refactor, custom control
center, new event framework or custom gameplay is part of this initial milestone.
