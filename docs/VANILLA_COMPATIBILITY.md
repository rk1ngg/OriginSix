# Season 6 Episode 3 compatibility assessment

Initial static assessment, **2026-10-21**, at the full commits recorded in
[REPOSITORY_AUDIT.md](REPOSITORY_AUDIT.md). No gameplay, build or regression test
was executed. No row qualifies as **Implemented and verified** yet.

## Classification rules

- **Implemented and verified:** relevant behavior executed with recorded results
  for this exact client/server/configuration/data pair.
- **Implemented but untested:** implementation evidence exists, but this audit has
  not validated behavior or complete vanilla coverage.
- **Partially implemented:** explicit upstream gaps are documented; not a completion estimate.
- **Missing:** positive evidence of absent required implementation, not merely a failed search.
- **Unknown:** insufficient evidence to classify the required behavior.

Paths below are relative to `server/` unless prefixed `client/`. File existence,
upstream progress tables and existing tests are evidence to investigate, not test passes.

## Client and protocol alignment

| Requirement | Evidence / conclusion |
|---|---|
| S6 EP3 English | OpenMU README targets it; `VersionSeasonSix/DataInitialization.cs` creates English episode 3 client definitions |
| Original client | Definition Season=6, bytes `31 30 34 30 34` (ASCII `10404`), caption 1.04d; connect 44405 |
| MuMain client | Definition Season=106 (protocol discriminator), bytes `32 30 34 30 34` (ASCII `20404`); MuMain README says 2.04d; connect 44406 |
| Serial | Server initializer and client `src/source/Network/Server/WSclient.cpp` use `k1Pk2jcET48mxL3b` (public protocol identifier, not an account secret) |
| Packet package | Server `src/Network/Packets/MUnique.OpenMU.Network.Packets.csproj` and client `ClientLibrary/MUnique.Client.Library.csproj` both declare 0.9.11 |
| Extensions | MuMain documents larger damage/EXP fields and extended item/appearance serialization; use OpenMU's MuMain definition rather than assuming stock packet layouts |
| Runtime dependency | MuMain needs its matching .NET Native AOT library beside Main; OS/architecture must match |
| Game data | Local data/fonts exist; item/skill indices, terrain/collision, language files, master-tree layout and packet interpretation still need end-to-end checks |

Matching version/serial/package declarations is a favorable static check, **not**
proof that the NuGet package contents equal every current server source definition,
nor that all packets are compatible. Record paired SHAs and run packet and gameplay
regressions before accepting upgrades. No historical S6 EP3 reference data was supplied.

## Feature checklist

| Feature | Classification | Implementation evidence | Verification still required |
|---|---|---|---|
| Dark Knight, Dark Wizard, Fairy Elf | Implemented but untested | `src/Persistence/Initialization/CharacterClasses/CharacterClassInitialization.cs` creates classes/evolutions | Creation, advancement, starting stats, equipment and skill rules |
| Magic Gladiator, Dark Lord, Summoner | Implemented but untested | Same initializer; corresponding class files | Unlock restrictions, pets/leadership, curse/buff rules and advancement |
| Rage Fighter / Fist Master | Implemented but untested | Same initializer, `ClassRageFighter.cs`; MuMain README advertises support | Creation entitlement, appearance, gloves, skills and damage rules |
| Skills and damage formulas | Implemented but untested | `src/AttributeSystem`, `src/GameLogic/Attributes`, `PlayerActions/HitAction.cs`, `PlayerActions/Skills`, S6 `SkillsInitializer.cs` | Per-class PvM/PvP formulas, rounding, speed, buffs, combos; compare to evidenced vanilla rules |
| Master Skill Tree | Implemented but untested | `docs/MasterSystem.md`, S6 `MasterSkillPassivePowerUps.cs`, `PlayerActions/Character/AddMasterPointAction.cs`; client README | Every class tree/layout, prerequisites, point spending, replaced skills and effects |
| Character progression | Implemented but untested | `src/GameLogic/PlayerExperience.cs`, class initialization; `DataInitializationBase.cs` sets server EXP rate 1.0 | Level/master caps, EXP curves, quest progression and effective rates; 1.0 multiplier alone is not proof |
| Maps and monsters | Implemented but untested | S6 `Maps/`, `GameMapsInitializer.cs`, `NpcInitialization.cs`; client `src/bin/Data/World*`, `Monster/` | Spawns, gates, collision, safe zones, drops, all map/data identifiers |
| Base items | Implemented but untested | S6 `Items/{Weapons,Armors,Wings,Pets,ItemRules}.cs`; client item data | Stats, durability, requirements, drops and serialization |
| Excellent | Implemented but untested | S6 initializer invokes `ExcellentOptions`; item option model/logic | Option combinations, probabilities and effective bonuses |
| Ancient | Implemented but untested | S6 `Items/AncientSets.cs`, test-account Ancient initializer | Set membership, thresholds, stacking and client display |
| Socket | Implemented but untested | S6 `Items/SocketSystem.cs`; `PlayerActions/Craftings/{SeedSphere,MountSeedSphere,RemoveSeedSphere}Crafting.cs` | Creation, mounting/removal, slot restrictions, set bonuses and persistence |
| Chaos Machine | Implemented but untested | S6 `ChaosMixes.cs`, `PlayerActions/Craftings/`; `docs/Progress.md` lists mix handlers | Each recipe, costs, probability, failure destruction/downgrade and atomicity |
| Inventory and vault | Implemented but untested | `InventoryStorage.cs`, `PlayerStorages.cs`, `PlayerActions/{Items,Vault}`; client README extensions | Slot geometry/extensions, moving, stacking, money and persistence |
| Trading and personal shop | Implemented but untested | `PlayerActions/{Trade,PlayerStore}`, progress handler table | Confirm/cancel/disconnect atomicity, duplicate prevention; Zen-only vanilla shop |
| Quests | Implemented but untested | S6 `Quests.cs`, `Items/Quest.cs`, `PlayerActions/Quests` | All required quests, rewards, item drops and class advancement |
| Blood Castle | Implemented but untested | S6 `Events/BloodCastleInitializer.cs`, `MiniGames/BloodCastleContext.cs` | Entry levels/ticket, stages, archangel objective, timeouts/rewards |
| Devil Square | Implemented but untested | S6 `Events/DevilSquareInitializer.cs`, `MiniGames/DevilSquareContext.cs` | Entry, waves/scoring, EXP, failure and rewards |
| Chaos Castle | Implemented but untested | S6 `Events/ChaosCastleInitializer.cs`, `MiniGames/ChaosCastleContext.cs` | Disguise, PvP/knockback, shrinking terrain, win condition/rewards |
| Castle Siege | Partially implemented | S6 initializer + `src/GameLogic/CastleSiege`; numerous existing siege tests; `docs/Progress.md` explicitly records gaps | Full cycle with guilds, gates/statues, crown/switches, tax/ownership persistence; reconcile documented packet gaps with actual behavior |
| PvP and PK | Implemented but untested | class PvP relationships, `PlugIns/SelfDefensePlugIn.cs`, penalty interfaces, duel actions | SD/damage, self-defense, penalties, death losses and duel/guild-war exemptions |
| Guilds and parties | Implemented but untested | `PlayerActions/{Guild,Party}`, `PartyManager.cs`, guild server; progress table | Invite/kick/roles, alliances, EXP sharing, disconnect and persistence |
| MU Helper | Implemented but untested | `src/GameLogic/MuHelper/`, `PlayerActions/MuHelper`; client README says UI/core logic with improvements planned | Supported S6 settings, targeting, buffs, looting, Zen cost, stop/reconnect; exact missing behavior unknown |
| Persistence | Implemented but untested | EF Core PostgreSQL layer, `PlayerPersistence.cs`, `PeriodicSaveProgressPlugIn.cs` | Save/reload, restart/crash behavior, inventory/trade atomicity, migrations and backup restore |
| Reconnection | Implemented but untested | Client README advertises automatic reconnect; server lost-connection handling and persistence | Actual reconnect flow, duplicate-session prevention, state restoration and failure paths |
| Lucky Items (additional documented gap) | Partially implemented | MuMain README explicitly lists Lucky Items as remaining S6 work | Scope of missing client/server behavior is unknown; do not infer all item systems complete |

Castle Siege's upstream progress document lists missing status request `0xB200`,
guild command `0xB21D`, weapon explosion `0xB704` and castle-owner guild-mark request
`0xB902`. The code has substantial recent siege implementation/tests; treat these
as **documented gaps requiring reconciliation**, not proof that every note is current.
Cash shop also has documented event-list, storage deletion and expiry gaps. No
upstream percentages are adopted as OriginSix completion measurements.

## Vanilla deviations and baseline controls

- MuMain explicitly changes renderer, frame rate, UI, camera, Unicode/localization,
  network field widths, monster-health presentation and reconnect behavior.
  Existing `legacy`/`Classic` defaults do not remove every difference.
- OpenMU includes optional resets/stat resets, alternative shop currencies, bots,
  offline play and other extensions. Reset/stat-reset/shop-currency feature classes
  implement `IDisabledByDefault`; initialization honors that marker. Effective DB
  settings still need checking; existing DBs may differ from source defaults.
- Do not add custom content or modify formulas to make a test pass. Record expected
  vanilla behavior, reproducible discrepancy and supporting reference first, then
  seek approval for any configuration/source correction.
- Define a reference ruleset by region/version for rates, class unlocks, helper,
  item availability and event rules. S6 EP3 label alone does not settle these details.

## Acceptance evidence

For each promoted row record: date, paired SHAs, asset trees/hash, DB/configuration
version, exact scenario, expected/actual behavior, logs/screenshots, tester and
regression command where applicable. Start with the smoke matrix in
[LOCAL_SETUP.md](LOCAL_SETUP.md), then this checklist. A successful smoke test does
not verify an entire complex feature or certify production readiness.
