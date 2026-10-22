# OpenCodeX — authorized local test character

Date: 2026-10-22. User requested a maximum-level Summoner on account `rkingg`,
level-380 FO equipment and Book of Lagle, maximum applicable yellow options,
and a legal PvM master-point allocation. User selected name `OpenCodeX` and
delegated the PvM allocation. This is an explicit local test-data exception,
not a change to global gameplay rules or proof of vanilla progression.

## Saved character

- Account: `rkingg`.
- Name: **OpenCodeX**, character slot **1**.
- Class: **Dimension Master**, Summoner's third class, configuration number 23.
- Normal level **400**, master level **200**, resets **0**.
- Base stats: **STR 407 / AGI 300 / VIT 200 / ENE 1431**.
- Normal points remaining: **0**. Master points remaining: **0**.
- `RKINGG` remains a Rage Fighter in character slot 0.
- No account password, global rates, formulas, class definitions or server source
  were modified by the provisioning operation.

### Normal-point accounting

Summoner base stats are 21/21/18/23, totaling 83.

| Source | Points |
|---|---:|
| Levels 2–400 at 5 points per normal level | 1,995 |
| Configured class-evolution quest rewards (10 + 10 + 20 + 10 + 30) | 80 |
| Gain Hero Status extra point for levels after 220 | 180 |
| **Allocated points above base stats** | **2,255** |

Final base-stat sum is 2,338 = 83 + 2,255. The configured advancement chain was
marked completed through quest 6 and its attribute rewards applied, including
Gain Hero Status and 6 points per subsequent normal level. Master levels grant
master points, not an invented second pool of normal stat points.

Strength was selected to meet the final equipment's calculated requirement;
agility 300 and vitality 200 provide a modest PvM baseline; remaining points go
to energy. Equipment requirements were checked against those base stats without
relying on circular equipment bonuses. This is a usable test build, not a
measured best-in-slot/meta recommendation.

## Equipment saved in equipped slots

| Slot | Item | Level | Durability |
|---:|---|---:|---:|
| 0 | Eternal Wing Stick | +15 | 162 |
| 1 | Book of Lagle, with its skill | +15 | 112 |
| 2 | Aura Helm | +15 | 137 |
| 3 | Aura Armor | +15 | 137 |
| 4 | Aura Pants | +15 | 137 |
| 5 | Aura Gloves | +15 | 137 |
| 6 | Aura Boots | +15 | 137 |

All seven items have the requested six existing Excellent options, luck, the
maximum **defined additional option level 4 (+16)**, and exactly one eligible
Harmony option at its configured maximum **level 13**:

- Stick/book: **Skill Damage Bonus +30**.
- Aura pieces: **Armor Damage Decrease +7%** each.
- Aura pieces also have their two existing 380 Guardian options.
- No invented Guardian option was attached to the stick/book where the actual
  item definition did not provide one. No sockets, Ancient groups, wings, jewelry
  or extra inventory items were added.

FO is the user's requested administrative test equipment: all six Excellent
effects are present, although random item generation's option group is configured
to roll at most two. That drop-generation setting was not changed.

The normal additional-option definitions in this checkout stop at level 4.
Forcing a level-7/+28 link would have no matching power-up definition and would
be ignored by `ItemPowerUpFactory`; the dry-run validation was corrected to use
the highest actual definition before anything was saved. Harmony options are
also selected from the actual allowed item definitions, not synthesized.

## Master build — exactly 200 points

Each skill was checked for class eligibility, minimum/maximum level, preceding
rank at level 10 and explicit master/base-skill prerequisites, following
`GameLogic/PlayerActions/Character/AddMasterPointAction.cs`.

| Skill number | Skill | Points |
|---:|---|---:|
| 325 | Attack Success Rate Increase | 20 |
| 450 | Lightning Tome Strengthener | 10 |
| 453 | Lightning Tome Mastery | 10 |
| 455 | Chain Lightning Strengthener | 20 |
| 456 | Lightning Shock Strengthener | 20 |
| 457 | Magic Mastery | 20 |
| 347 | PvP Attack Rate | 10 |
| 465 | Stick Strengthener | 10 |
| 466 | Other World Tome Strengthener | 10 |
| 467 | Stick Mastery | 20 |
| 468 | Other World Tome Mastery | 10 |
| 359 | Monster Attack Life Increase | 10 |
| 469 | Berserker Strengthener | 10 |
| 470 | Berserker Proficiency | 20 |
| | **Total** | **200** |

The 10-point PvP Attack Rate investment is the right root's required first-rank
path, not a claim it directly benefits PvM. The rest emphasizes offensive
spell/tome/stick performance, Berserker and monster-kill health sustain.

Regular learned skills: Drain Life, Chain Lightning, Damage Reflection, Berserker,
Sleep, Weakness, Innovation and Lightning Shock. Regular entries use the server's
normal level-0 representation; Pollution is supplied by the equipped Book of Lagle.

## Procedure and verification

1. Reviewed server source and live configuration using read-only queries.
2. Built `scripts/CharacterProvisioner/CharacterProvisioner.csproj` against existing
   Release server DLLs, without rebuilding/modifying the upstream solution.
3. Ran inspection and dry-run planning in temporary containers on the dedicated
   Docker network. Registered the same localized-string/binary JSON converters
   as Startup; the initial read attempt without those converters failed and made
   no writes. Final helper build: 0 warnings, 0 errors.
4. Stopped only `originsix-local-openmu-1` to avoid active-session save conflicts.
5. Created a PostgreSQL custom-format backup, checked its archive listing and copied
   it to `.local/backups/originsix-before-OpenCodeX-20261022.dump`.
   SHA256: `2761CACE83E1BE4326C1A9B76B4D9204C3FEAF09FE62C72542D289D0D119C541`.
6. Created the character through the existing persistence provider and
   `InitializeNewCharacter`, using one final `SaveChangesAsync` after validation.
   No direct SQL INSERT/UPDATE was needed. Duplicate names are refused.
7. Restarted the server in a finally block; confirmed healthy and passing
   connect-server Hello/list/endpoint checks.
8. Independently queried persisted class, stats, equipment, option links and skills.
   Re-loaded the account in a new helper process with `--verify`; all saved budget,
   master prerequisite and item option checks passed.

The existing database currently configures **one** game server, and the restarted
server advertises it at `127.127.127.127:45902`. This operation did not alter
server definitions. The earlier initial-runtime report recorded three servers;
that historical observation is not the current configured count.

## Re-running checks

The maintenance helper defaults to dry-run planning and refuses an existing
`OpenCodeX`. `--inspect` prints the relevant configured skills/items. `--verify`
reads and validates the saved character. `--apply` also requires the explicit
offline confirmation environment variable `ORIGINSIX_OFFLINE_APPLY=OpenCodeX`.
Do not apply it while the game server is running or use it to overwrite a character.
Supply DB credentials privately from the existing DPAPI credential file; do not
place passwords in command arguments or logs.

Source references: `GameLogic/CharacterInitialization.cs`,
`GameLogic/PlayerActions/Character/AddMasterPointAction.cs`,
`GameLogic/PlayerActions/Quests/QuestCompletionAction.cs`,
`Persistence/Initialization/VersionSeasonSix/Quests.cs`,
`Persistence/Initialization/CharacterClasses/ClassSummoner.cs`,
and the existing item definitions/Harmony/Guardian configuration.

**Pending:** user relog, visual equipment/master-tree checks and actual PvM
combat. Database verification does not prove rendered appearance or gameplay.
Resources begin with normal character-initialization values rather than a
fabricated full-health/mana state; recover resources in-game as needed.
