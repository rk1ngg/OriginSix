# Three-character test loadouts — 2026-10-22

This is the current snapshot, supplementing the earlier OpenCodeX creation report.
The user authorized maximum Zen/fruit, Summoner equipment/skills, a new PvM Blade
Master named `OpenCodeXx`, and upgrading the existing first-slot RF `RKINGG`.
All updates are local test-account data; global item definitions, formulas,
rates, socket limits and upstream source remain unchanged.

## Saved account state

Account: `rkingg`. All three are level **400 / master 200**, have **127 positive
fruit points**, zero negative-fruit usage, **200 allocated master points**, no
unspent normal/master points, and **2,147,483,647 inventory Zen**.

| Character | Slot | Class | STR | AGI | VIT | ENE | Regular class skills |
|---|---:|---|---:|---:|---:|---:|---:|
| RKINGG | 0 | Fist Master (RF third class) | 697 | 700 | 1,283 | 404 | 12 |
| OpenCodeX | 1 | Dimension Master (Summoner third class) | 411 | 300 | 200 | 1,554 | 16 |
| OpenCodeXx | 2 | Blade Master | 1,305 | 860 | 200 | 100 | 13 |

RF was upgraded in place, not duplicated; its creation date, key configuration,
current map/position and pre-existing bag items were preserved by the operation.
The new BM occupies the first unused character slot. Existing class skills were
retained; missing normal skills were added. Castle Siege/event/GM and mount-only
abilities were excluded from the normal class-skill acquisition list.

### Normal and fruit accounting

- Summoner/BM: 1,995 level-up points + 80 class-quest points + 180 compensated
  Marlon/hero points = **2,255** ordinary points, plus **127** fruit points.
  Base stats total 83; final allocated base stats total **2,465**.
- RF: 399 levels × 7 = 2,793, plus 60 third-class quest points = **2,853** ordinary
  points, plus **127** fruit points. Base stats total 104; final total **3,084**.
- Fruit cap comes from `CharacterExtensions.GetMaximumFruitPoints` for the actual
  class/level. It is a total bonus-point budget, not 127 points per stat.
- RF allocates the remaining PvM budget mainly to vitality. BM allocates remaining
  points to strength after meeting both Titan and alternate Brave requirements.
- No full-stat values, resets, arbitrary extra points or negative-fruit trick were used.

## Summoner STR correction

The user's red-weapon report exposed a client/server discrepancy in additional
option requirements:

- Server `GameLogic/ItemExtensions.GetRequirement`: weapon STR adds **4 per option level**.
- Client `Data/Items/Stats/ItemStats.json`, weapon profile: **5 per option level**.
- Eternal Wing Stick +15 with Excellent flags and option level 4: server computes
  **407 STR**, client computes **411 STR**.
- Moved **4 points from Energy into STR**, retaining the legal total. Energy
  changed from 1,558 after fruit to **1,554**, STR from 407 to **411**.

The maintenance validator now evaluates the actual staged client item JSON and
stat profile formulas as well as the server's existing `GetRequirement` method.
It uses the higher required allocation and includes the alternate Brave set.
This corrects the loadout allocation without changing either implementation;
the underlying fidelity/requirement discrepancy remains a separate assessment item.
Visual re-equip confirmation in the running client is still pending.

## Equipped gear

### OpenCodeX

- Existing Eternal Wing Stick +15 FO, Book of Lagle +15 FO and complete Aura +15 FO.
- **Wing of Dimension +15**, luck, all four existing third-wing special options,
  and one applicable maximum additional option.
- **Pendant of Lighting +4 FO**, **Ring of Ice +4 FO**, **Ring of Fire +4 FO**.
- Existing yellow/Harmony effects remain; no unsupported socket/Guardian effects
  were invented for the weapons.

### OpenCodeXx

- **Two Flamberges +15**, max supported sockets, luck, additional option and
  matching socket bonus. Dual Flamberges were chosen over Flamberge/Sword Breaker
  to fit the legal stat budget while also meeting the alternate Brave set.
- **Complete Titan +15 socket set equipped**.
- **Complete Brave +15 socket set in the inventory** at slots 12, 14, 16, 18, 28.
  No redundant spare Titan set was added.
- **Wing of Storm +15** with the existing full third-wing special options.
- **Pendant of Fire +4 FO**, **Ring of Ice +4 FO**, **Ring of Fire +4 FO**.

### RKINGG

- **Two Phoenix Soul Stars +15 FO**, weapon skills included.
- **Phoenix Soul Helmet, Armor, Pants and Boots +15 FO**, applicable 380 Guardian
  effects and one max yellow/Harmony effect per eligible piece.
- RF has no armor-glove piece in this set; its gloves are its two equipped weapons.
- **Cape of Overrule +15**, the existing RF third-class cape.
- **Pendant of Fire +4 FO**, **Ring of Ice +4 FO**, **Ring of Fire +4 FO**.
- Existing Wizard's Ring +1/+2 bag items were retained.

Jewelry is **+4**, its actual configured maximum, not a forced unsupported +15.
Normal additional options use the highest defined level (commonly level 4/+16).
FO means all existing Excellent or third-wing special effects requested for this
administrative test loadout, not a change to drop-generation limits.

## BM socket choices

Actual item definitions allow **3 slots per item**. Each was filled with the
highest configured sphere level, **5**; no definition was changed to force 5 slots.

| Gear | Ordered slots | Matching bonus |
|---|---|---|
| Each Flamberge | Lightning: Excellent Damage Chance; Ice: Skill Damage; Fire: Base Damage | Skill Damage +11 |
| Each Titan/Brave piece | Water: Damage Reduction; Earth: Maximum Health; Wind: Maximum Health | Defense +24 |

The ordering follows `MountSeedSphereCrafting.GetPossibleBonusOption`. Socket gear
does not combine additional Excellent/Harmony effects in this loadout. All 12
socket items (2 swords, 5 Titan, 5 Brave) were independently queried: three slots,
all sphere levels exactly 5. Bag boundaries/overlap were also validated.

## Legal PvM master builds

Each is exactly **200 points**, with maximum levels, previous-rank unlocks and
explicit skill prerequisites checked against the loaded configuration.

### Summoner — preserved 200-point build

Attack Success 20; Lightning Tome Strengthener 10; Lightning Tome Mastery 10;
Chain Lightning Strengthener 20; Lightning Shock Strengthener 20; Magic Mastery 20;
PvP Attack Rate 10; Stick Strengthener 10; Other World Tome Strengthener 10;
Stick Mastery 20; Other World Tome Mastery 10; Monster Attack Life 10;
Berserker Strengthener 10; Berserker Proficiency 20.

### BM — 12 skills / 200 points

| Skill | Points |
|---|---:|
| Attack Success Rate Increase (325) | 20 |
| Cyclone Strengthener (326) | 10 |
| Twisting Slash Strengthener (330) | 20 |
| Rageful Blow Strengthener (331) | 10 |
| Twisting Slash Mastery (332) | 20 |
| Death Stab Strengthener (336) | 20 |
| PvP Attack Rate (347) | 10 |
| One-handed Sword Strengthener (349) | 20 |
| One-handed Sword Mastery (353) | 10 |
| Monster Attack Life Increase (359) | 20 |
| Minimum Attack Power Increase (361) | 20 |
| Monster Attack Mana Increase (362) | 20 |

### RF — 13 skills / 200 points

| Skill | Points |
|---|---:|
| Attack Success Rate Increase (599) | 20 |
| Killing Blow Strengthener (551) | 10 |
| Beast Uppercut Strengthener (552) | 10 |
| Killing Blow Mastery (554) | 10 |
| Beast Uppercut Mastery (555) | 10 |
| Weapon Mastery (557) | 20 |
| Dark Side Strengthener (559) | 20 |
| Dragon Roar Strengthener (560) | 20 |
| PvP Attack Rate (603) | 10 |
| Equipped Weapon Strengthener (568) | 20 |
| Equipped Weapon Mastery (571) | 20 |
| Recover HP from Monster Kills (606) | 10 |
| Minimum Attack Power Increase (607) | 20 |

PvP Attack Rate is a required first-rank path for the useful right-root skills;
it is not presented as directly improving PvM. These are validated usable test
builds, not a measured assertion of optimal DPS.

## Execution and verification

- Maintenance helper: `scripts/CharacterProvisioner`, referencing existing server
  Release assemblies. No upstream source build/change was needed.
- New mode: `--upgrade-all`, dry-run by default. `--apply` requires offline
  confirmation. `--upgrade-all --check` only re-loads and validates saved state.
- Read-only staged client data mounted at `/client-items` for requirements.
- Initial Flamberge/Sword Breaker draft exceeded the legal stat budget; dry-run
  validation stopped it before any write. Final dual-Flamberge plan passed.
- Final helper build: **0 warnings, 0 errors**.
- Stopped only the OriginSix app container and created a full dedicated DB backup:
  `.local/backups/originsix-before-fullgear-20261022-040115.dump`.
- Backup SHA256:
  `C5CB25CD931DA9A1D6D3A4D2EC9252DCBE38C22AC2B5D1EE616F909EF4BFC42E`.
- All edits saved through one final `IPlayerContext.SaveChangesAsync` after validation.
- Restarted server, then re-loaded all three characters in a separate maintenance
  process. Budget, skills/prerequisites, item options, socket maxima/indexes,
  inventory geometry and both client/server stat requirements passed.
- Independent PostgreSQL queries confirmed character slots/classes, fruit/Zen,
  master counts and sphere levels. Server healthy; connect-server Hello/list and
  current game endpoint `127.127.127.127:45902` passed.

No database backup files or credentials are printed in logs or committed. Keep
backups under `.local`; do not run the creation mode again to overwrite builds.
The earlier `--verify` mode describes the original seven-item Summoner snapshot;
use **`--upgrade-all --check`** for this current expanded loadout.

**Pending in-game checks:** relog; visually inspect/re-equip Summoner weapon and
wings/jewelry; inspect BM Titan/Brave inventory; inspect RF loadout and skill trees;
exercise actual PvM skills. Database checks do not certify rendered behavior or
historical vanilla parity. Current HP/mana were not artificially maximized.
