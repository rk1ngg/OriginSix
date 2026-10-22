# Adding content to OriginSix — tutorial

As of **2026-10-22**. Written after the `rkingg` stat boost, to record what
content this workspace can actually add and the exact mechanism for each kind.
Everything here was verified by inspection of the working trees and the live
local database, not inferred from upstream documentation.

This is a research/tutorial document. It changes nothing by itself. Adding any
content still needs its own approval, and the rules in
[VANILLA_COMPATIBILITY.md](VANILLA_COMPATIBILITY.md) still apply: a custom
feature must not silently replace vanilla behaviour.

---

## 1. First: the "URUR / Rare Bear" question — ANSWERED

**"Ur" and "Ur (Rare)" are real MU Online items: the 5th Guardian mount.**
Found on the web and confirmed against multiple MU item databases.

| | |
|---|---|
| Names | `Ur` and `Ur (Rare)` |
| Item ID | 6237 (`Ur`); `Ur (Rare)` is a separate entry |
| Type | Mount-type guardian (an *inventory-mounted* mount, not a transformation) |
| Base defense | +290 |
| Required level | 1200 (older guides say 1450 — later-season rebalance) |
| Stat requirements | none (STR/DEX/ENE/VIT/CMD all 0) |
| Classes | all |
| `Ur` bonuses | +800 max HP, +800 max MP, +55 attack speed, 16% double damage, 2% triple damage, +35% EXP gain |
| `Ur (Rare)` bonuses | +900 max HP, +900 max MP, +60 attack speed, 18% double damage, 2% triple damage, +40% EXP gain |
| How obtained | Guardian upgrade combination at the **Trainer** NPC in Lorencia (122,110) — 4th Guardian +15 + Seal of Ur + jewels; `Ur (Rare)` at a low probability (≈20%) from the same combination |
| Where the "Rare" comes from | The Webzen S20 guide for the 5th Guardian "Ullr" says it plainly: *"The attributes of 'Ullr (Rare)' are the same as 'Ullr', but it has a different color and a lower probability of being acquired."* So **Rare = a different colour of the same model, same mechanic** |

The user remembered it as a "rare bear". The bear reading is reasonable — the
5th Guardian family is a bear-like creature, and the client in this workspace
already ships a bear-family model.

### Is it in this workspace? No.

| Check | Result |
|---|---|
| `config."ItemDefinition"` group 13 | 56 rows, max number 133 — no `Ur` |
| `client/src/bin/Data/Items/Group13_Helper.json` | 137 entries, max number 144 — no `Ur` |
| `client/src/bin/Data/Item/` model files | no `ur`/`guardian`/`ghost`/`twintail` model |
| `client/src/source/Core/Globals/_enum.h` | no `ITEM_UR` / `MODEL_UR` |

**`Ur` is a Season 18+ item.** S6 EP3 has no 5th Guardian, no
`Seal of Ur`, and no 5th-guardian upgrade combination. Adding it means
backporting a later season's content, not flipping a switch.

### What that costs

The `Ur` model file and its texture are **not in this workspace** and are
Webzen assets. There are two honest routes:

1. **Full backport** — obtain the S18+ client assets (model + texture +
   the `Seal of Ur` item art) and add the client entries, the server item
   definition, and the upgrade combination. This is a *later-season asset
   import*, and it is the only route that gives the real `Ur` visuals.
2. **Reuse an existing S6 mount model** — implement the *Ur / Ur (Rare)
   mechanic* (mount-type helper, 290 defense, the stat line above, the rare
   colour variant, the 20% rare drop from an upgrade combination) on top of
   a mount model the S6 client already has.

   For route 2 the model must come from the **item** model namespace
   (`Data\Item\...`), because a mount is a group-13 item — not from
   `Data\Monster\`. The bear that qualifies is the **Panda**:
   `Data/Item/PandaPet.bmd` (item 80, `MODEL_PET_PANDA = MODEL_HELPER + 80`),
   with `PandaPet.OZJ`, `pandabody_R.OZJ`, `panda_body.OZJ`,
   `panda_cloth.OZJ`, `panda_face.OZJ` and `panda_R.OZJ` beside it.
   A panda is the closest thing the S6 client has to the Ur guardian's
   bear-like silhouette, and it needs **zero new binary assets**.

   The Grizzly is **not** usable this way: it is a monster
   (`Data\Monster\Monster135.bmd`, server monster 424/425) in the monster
   namespace, and a mount cannot point at it. `Bugbear*.OZJ` is likewise a
   monster texture set with no model and no server entity.

Route 2 is the practical one for this workspace. Route 1 is a licensing
question first: Webzen owns the assets, and `docs/REPOSITORY_AUDIT.md`
already tracks asset provenance as an open item, and
`docs/VANILLA_COMPATIBILITY.md` requires custom content to default OFF.

### The "Rare" colour mechanism — does the client support it?

Partly, and this is the key finding for implementing `Ur (Rare)`:

- The client has a **per-model glow colour** system: `glow.ancientColor`
  in the item model JSON (`client/src/bin/Data/Items/Models/Group08_Armor.json`
  and the other armour groups), read by `ItemModelGlowJson.cpp:169` and
  applied in `ZzzObject.cpp:5970`. That is the "gold ancient" tint.
- The client's item-index space is **512 per group**
  (`MAX_ITEM_INDEX = 512`, `_define.h:336`), and group 13 uses only ~144 of
  them. So there is ample room for new helper items.
- There is **no generic "rare" colour channel** in the client today. `Rare`
  is not a colour the item-model JSON can express. Making `Ur (Rare)` a
  visually distinct mount therefore needs either a second model file, or a
  new colour channel added to the glow system.

So `Ur (Rare)` as a *pure data* addition is not possible with the current
client: the rare variant must be distinguishable, and the client has no
"rare colour" concept. That is the one genuine piece of client work in the
whole task.

### The hard constraint for any new mount or monster

Mounts and monsters live in **two different model namespaces**, and this is
easy to get wrong:

- **Monsters** are numbered by the *monster number* the server sends, and
  their model file is named from a *dense, 0-based model index*:
  - `client/src/source/Core/Globals/_enum.h:4947` — `MONSTER_GRIZZLY = 424`
    (server `config."MonsterDefinition"."Number"` 424 → same number).
  - `client/src/source/Core/Globals/_enum.h:4479` —
    `MONSTER_MODEL_GRIZZLY = 134` (the dense model index).
  - `client/src/source/Engine/Object/ZzzOpenData.cpp:1329` —
    `gLoadData.AccessModel(Index, L"Data\\Monster\\", L"Monster", Type + 1);`
    so model index 134 loads `Data\Monster\Monster135.bmd` (which exists).
  - `client/src/source/Core/Globals/_enum.h:5254` —
    `MODEL_GRIZZLY = MODEL_MONSTER01 + 134;`.
  - The client also needs the render switch and the action/sound arms:
    `ZzzCharacter.cpp:5143` handles `MODEL_GRIZZLY`, and
    `ZzzOpenData.cpp:1722` / `:2710` give it its animation speeds and its
    `SE_Mon_beastwoo_*.wav` sounds.

- **Mounts/pets are items** (group 13) and their models are *item* models
  under `Data\Item\`, addressed by `MODEL_HELPER + itemNumber`:
  - `client/src/source/Core/Globals/_enum.h:2007` —
    `MODEL_PET_PANDA = MODEL_HELPER + 80;`
  - the model file comes from the item model JSON, not from the monster
    table: `client/src/bin/Data/Items/Models/Group13_Helper.json` maps
    item 80 → `Data/Item/PandaPet.bmd`, item 67 →
    `Data/Item/xmas/xmas_deer.bmd`, item 123 → `Data/Item/skeletonpet.bmd`,
    item 106 → `Data/Item/partcharge7/pet_unicorn.bmd`.

**So a Grizzly monster model cannot be used for a mount.** The Grizzly is
`Data\Monster\Monster135.bmd` in the monster namespace; a mount must point
at an `Data\Item\...` model through the group-13 model JSON. Mixing them
does not work.

---

## 2. What is already present (cheap to expose)

### 2.1 Invaders / boss invasions — the best-value addition

Server plugin family: `server/src/GameLogic/PlugIns/InvasionEvents/`.

| Plug-in | Config default | Monsters |
|---|---|---|
| `GoldenInvasionPlugIn` | `InvasionConfigurationDefaults.Golden` | Golden Budge Dragon, Goblin, Soldier, Titan, Vepar, Lizard King, Wheel, Tantallos, Dragon |
| `RedDragonInvasionPlugIn` | `.RedDragon` | Red Dragon (44) |
| `WhiteWizardInvasionPlugIn` | `.WhiteWizard` | White Wizard (135) + Destructive Ogre Soldier/Archer |

All three are **periodic** (`PeriodicInvasionConfiguration`), scheduled by a
timetable (`PeriodicTaskConfiguration.GenerateTimeSequence(4h, 00:30/01:30/02:30)`),
spawning on a map for 30 minutes, then ending with an in-game message.

The whole mechanism is data-driven after the monster exists. A new invasion is:

1. A `MonsterDefinition` + `MonsterAttribute` rows for the monster.
2. A `MonsterSpawnArea` if it is a permanent spawn, or nothing if it is
   invasion-only.
3. A `PeriodicInvasionConfiguration` with `Mobs = [new(monsterNumber, count,
   [maps], SpawnMapStrategy.RandomMap)]` and a `Timetable`.
4. A plug-in class deriving from `SimpleInvasionPlugIn` (see
   `RedDragonInvasionPlugIn.cs` — 34 lines including the header).
5. The monster's model must already exist client-side.

Note: the `Golden`/`RedDragon`/`WhiteWizard` monsters are numbered 43, 44, 53,
54, 78–83, 135, 136, 137 and **already have client models**. They are
configured but I did not verify they are enabled in the running server — that
needs a live check, not a config read.

### 2.2 Mini-games / event arenas

Server: `server/src/GameLogic/MiniGames/` (`Doppelganger`, `ImperialGuardian`,
`Kanturu`) plus `server/src/Persistence/Initialization/VersionSeasonSix/Events/`
(`BloodCastle`, `CastleSiege`, `ChaosCastle`, `Crywolf`, `DevilSquare`,
`Doppelganger`, `ImperialGuardian`, `Kanturu` initializers).

These are the classic MU event arenas. Each has a `MiniGameDefinition` config
row set in the `config` schema (`MiniGameDefinition`, `MiniGameSpawnWave`,
`MiniGameReward`, `MiniGameTerrainChange`, `MiniGameChangeEvent`).

### 2.3 The bear family already in the data

**As monsters** (not mounts — see the namespace note below):

| Server number | Designation | zh-CN | Level | HP | Where it spawns |
|---|---|---|---|---|---|
| 424 | Grizzly | 灰熊 (grey bear) | 43 | 2400 | Elvenland (`Elvenland.cs:74`), Aida (`Aida.cs:201`) |
| 425 | Captain Grizzly | 残暴的灰熊 (cruel grey bear) | — | — | Elvenland (`Elvenland.cs:75`), Aida (`Aida.cs:202`) |

Both are used as quest kill requirements (`Quests.cs:725-781` for quest 50,
`:1412-1470` for quest 100). Their attributes: Level 43, Attack Rate PvM 10,
Min/Max Phys Base Damage 130/145, Base Defense 65, Defense Rate PvM 65,
Maximum Health 2400.

Both already have client models (`Monster135.bmd`, `Monster136.bmd` exist) and
sounds (`SE_Mon_beastwoo_*.wav`). **A "rare bear" *boss* is therefore
achievable with no new client assets** — by adding a new monster definition
that reuses the existing bear model and scaling up the attributes, or by using the
invasion mechanism above with monster 424/425.

### 2.4 Already in the data but unused by the client: `Bugbear`

`client/src/bin/Data/Monster/` contains `Bugbear.OZJ`, `BugbearA.OZJ`,
`BugbearB.OZJ`, `Bugbearweapon.OZJ`, `BugbearWood.OZJ` — a second bear family
with weapon and wood variants. There is **no** `Bugbear` monster in
`config."MonsterDefinition"`, and no `Bugbear*.bmd` model, only textures.
This is client-side art with no server entity: exactly the shape of a
"not yet wired up" content drop.

### 2.5 The bear that is a *mount*: Panda

For a **mount** (a group-13 helper item), the model has to be an item model
under `Data\Item\`, not a monster model under `Data\Monster\`. The bear in
that namespace is the **Panda**:

| Asset | Path |
|---|---|
| Model | `Data/Item/PandaPet.bmd` (item 80) |
| Textures | `Data/Item/PandaPet.OZJ`, `pandabody_R.OZJ`, `panda_body.OZJ`, `panda_cloth.OZJ`, `panda_face.OZJ`, `panda_R.OZJ` |
| Model JSON | `client/src/bin/Data/Items/Models/Group13_Helper.json`, number 80 → `Data/Item/PandaPet.bmd`, texture folder `Item` |
| Client enum | `MODEL_PET_PANDA = MODEL_HELPER + 80` (`_enum.h:2007`) |
| Rendering | `ZzzCharacter.cpp:13046` → `ThePetProcess().CreatePet(...)` |

There is also `Data/Item/panda.bmd` + `panda_*.OZJ` (the player-side panda
transformation) and `PandaPetRing*.bmd/OZJ` (the ring) — same bear family,
different use.

This is the asset set to reuse for a bear-shaped `Ur` mount.

---

## 2.6 The two "Game Clients" in the admin panel — and which one is ours

The OpenMU admin panel shows two `GameClientDefinition` rows. They are
**not** two different games; they are two *protocol handshakes* for the same
Season 6 Episode 3 data, and they differ in one meaningful way: the
network encryption plugin each one is bound to.

| | GMO Client | Open Source Client ← **ours** |
|---|---|---|
| `Season` | 6 | **106** |
| `Episode` | 3 | 3 |
| `Language` | 1 (English) | 1 (English) |
| `Version` bytes | `0x31 0x30 0x34 0x30 0x34` = `"10404"` | `0x32 0x30 0x34 0x30 0x34` = `"20404"` |
| `Serial` | `k1Pk2jcET48mxL3b` | `k1Pk2jcET48mxL3b` (**same**) |
| Encryption plug-in | `Season6Episode3NetworkEncryptionFactoryPlugIn` (Key `6,3,English`) | `OpenSourceClientNetworkEncryptionFactoryPlugIn` (Key `106,3,English`) |
| GameServerEndpoint | `…-0000-…` | `…-0001-…` |
| ConnectServer | yes | yes |
| ChatServer | **yes** | — |

**Ours is the Open Source Client**, proven two ways:

1. The client binary sends `Version = {'2','0','4','0','4'}` and
   `Serial = "k1Pk2jcET48mxL3b"` (`client/src/source/Network/Server/WSclient.cpp:145-146`).
   `'2','0','4','0','4'` is hex `3230343034` — the **exact** `Version` of the
   Open Source Client row. The GMO row is `3130343034` = `"10404"`.
2. Both rows share the same `Serial`, so the serial does not disambiguate —
   only the version bytes do, and they match the open-source row.

**How the server routes it.** `ClientVersionResolver.Register`
(`server/src/GameServer/ClientVersionResolver.cs:29`) maps the client's
version bytes to a `ClientVersion`; `ClientVersionResolver.Resolve` (:59)
does the reverse for the login window
(`ShowLoginWindowPlugIn.cs:51`). The game server's
`DefaultTcpGameServerListener.cs:80` builds its `ClientVersion` from the
endpoint's `ClientId`, and `:101` then picks the matching
`INetworkEncryptionFactoryPlugIn` by that key.

**What actually differs between the two encryptors.** Reading both plug-ins
side by side, the two implementations are *identical in code* — same
`SimpleModulus` + `Xor32` pipeline, same `DefaultClientKey`. The only
difference is the `Key` they are registered under, i.e. which client they
are selected for. So this is a **version-routing distinction, not a
behavioural one**: the two S6 client flavours share the same encryption
scheme and differ only in the handshake bytes they present.

**One consequence that matters for this document.** Because the `ClientVersion`
carries the season, and the server branches on it, any new feature must
target **season 106** (our client), not season 6. Examples in the server:
`RemotePlayer.cs:99` — `IsPlayerStoreOpeningAfterEnterSupported =>
this.ClientVersion.Season > 100` — is true *only* for the open-source
client. A feature that is gated on the season would be off for the GMO
client and on for ours.

### Is the GMO client in this workspace?

**No.** The client repository is `sven-n/MuMain`, the open-source
reimplementation, and its data tree is the open-source layout:

- item data is JSON under `client/src/bin/Data/Items/`
  (`Group13_Helper.json`, `Models/Group13_Helper.json`, `Stats/ItemStats.json`),
  not the GMO `main.ini` / `data` table format;
- there is no `main.ini` anywhere under `client/src/bin/Data`;
- the `Data/Local/` folder holds open-source-format `.bmd` tables
  (`Quest.bmd`, `Skill.bmd`, `PetData.bmd`, `MasterSkillTreeData.bmd`, …);
- 13,581 files in `client/src/bin/Data`, none of them the GMO client's
  `GameGuard.csr`-era payload set or its `Interface`/`main.ini` structure.

`Data/Local/GameGuard.csr` and `webzenlogo.ozj` are the only Webzen-
branded files present, and they are carried by the open-source client
itself.

So: the **GMO client is not in the workspace**, and the assets it would
provide are not available here. Any "take the source from another MU"
step has to bring in files from outside this workspace, and those files
carry Webzen's licensing — see `docs/REPOSITORY_AUDIT.md` (asset provenance
is an open item) and `docs/VANILLA_COMPATIBILITY.md` (custom content must
default OFF).

---

## 3. How to add each kind of content

### 3.1 A new monster (needs client model)

Server side:

1. `config."MonsterDefinition"` row: `Number` (unique), `Designation`,
   `Attribute` (0 = normal, 2 = NPC), ranges, `ObjectKind`, `NpcWindow`.
2. `config."MonsterAttribute"` rows: Level, Maximum Health, Base Defense,
   Attack Rate, Min/Max Phys Base Damage, Defense Rate, and the element
   variants.
3. `config."MonsterSpawnArea"` rows to place it, or leave it unspawned if it
   is an event-only monster.
4. Client side: the model file `Data\Monster\Monster{index+1}.bmd` (plus any
   `.OZJ` texture), an `EMonsterModelType` entry in
   `client/src/source/Core/Globals/_enum.h`, a `MONSTER_*` number entry, a
   `MODEL_*` render entry, and the action/sound cases in `ZzzOpenData.cpp`
   and `ZzzCharacter.cpp`.

Both sides must agree on the monster number, and the client's dense model
enum must stay in sync with its name table (there is a `static_assert` at
`ZzzOpenData.cpp:1310` guarding that).

### 3.2 A new mount (helper item)

Mounts are **group 13** ("Helper") items in `config."ItemDefinition"`. Existing
examples: `Pet Panda` (13/80), `Pet Unicorn` (13/106), `Pet Rudolf` (13/67),
`Dark Horse` (13/4), `Panda Transformation Ring` (13/76).

Mount rendering is **client-side**. The client decodes the equipped helper
item and maps it to a model in `ZzzCharacter.cpp`:

- `ZzzCharacter.cpp:12694` / `:12700` — the helper's item type becomes the
  helper model slot: `c->Helper.Type = Data::Items::ToModelSlot(ITEM_HELPER + type)`.
- `:12703-12709` — a small `switch` turns the type into a mount model
  (`MODEL_HELPER`, `MODEL_UNICON`, `MODEL_PEGASUS`); anything else sets
  `bCreateHelper = FALSE`, i.e. **no mount is created**.
- `:13020-13049` — a second `switch` on `c->Helper.Type` handles the
  pet-style helpers: `MODEL_PET_RUDOLF`, `MODEL_PET_PANDA`,
  `MODEL_PET_UNICORN`, `MODEL_PET_SKELETON` and the horn/guardian mounts are
  each listed explicitly and routed to `CreateMount` / `ThePetProcess().CreatePet`.

**This is the key blocker for a new mount.** A helper item that is not in
those `switch` arms renders as nothing. So a new mount is not just "add an
item" — it needs a new `case` in **both** switches.

Server-side, the mount is only an inventory item plus one attribute:
`Stats.IsMuHelperActive` (`Stats.cs:1515`) and the `LeoHelperItemRequest`
packet (`ClientToServerPackets.cs:5643`).

Full checklist for a new mount:

1. **Client model**: a new `MODEL_*` entry in
   `client/src/source/Core/Globals/_enum.h` plus the model/texture assets.
2. **Client helper switch**: a `case` for the new helper type in
   `ZzzCharacter.cpp:12703` (to create the mount) and one in
   `ZzzCharacter.cpp:13020` (to route it to the right model/pet creation).
3. **Client item data**: an entry in
   `client/src/bin/Data/Items/Group13_Helper.json`, including the model and
   texture file names.
4. **Server item**: a group-13 `ItemDefinition` row (number, name, `Value`,
   `Width`/`Height`, `ItemSlotId`) whose number matches the client's
   `Group13_Helper.json` entry, plus the `ItemOptionLink`/power-up rows if
   the mount grants bonuses.

#### 3.2a Concrete recipe: `Ur` / `Ur (Rare)` on the S6 Panda model

This is the smallest honest version of the Ur mount that this workspace can
carry, with no new binary assets. It is a **custom local feature**, not
vanilla S6, and per `docs/ROADMAP.md` it must default OFF.

The model is the **Panda** (`Data/Item/PandaPet.bmd`, item 80) — the only
bear in the *item* model namespace, which is where a mount's model has to
come from. The Grizzly cannot be used: it is a monster model.

**The server side needs no new engine code.** Three existing mechanisms
cover it:

1. **The pet/mount item.** `server/src/Persistence/Initialization/VersionSeasonSix/Items/Pets.cs`
   has `CreatePet(number, skill, width, height, name, levelRequirement,
   …, (attribute, value, aggregateType) …)` — a mount-type group-13 item
   with its stat bonuses declared inline. `PetPanda` (`Pets.cs:101`) is the
   closest existing example: a group-13 item with `BonusExperienceRate` and
   `DefenseFinal` bonuses. The `Ur` stat line maps directly onto those
   tuples:
   - `Ur`: `MaximumHealth` +800, `MaximumMana` +800, `AttackSpeedAny` +55,
     `DoubleDamageChance` 0.16, `BonusExperienceRate` 0.35.
   - `Ur (Rare)`: +900 / +900 / +60 / 0.18 / 0.40.
   (The exact attribute names in this server are `DoubleDamageChance`
   (`Stats.cs:1256`), `AttackSpeedAny` (`Stats.cs:394`) and
   `BonusExperienceRate` (`Stats.cs:128`.) **The 2% triple-damage line has no
   matching attribute in this server** — `Stats.cs` has no
   `TripleDamageChance`; the only "Triple" attribute is
   `TripleShotExtraProjectiles` (`Stats.cs:553`), which is a different
   mechanic. Either add a new attribute definition or drop that line. Flag
   this before implementing rather than silently substituting one.)
   Use free group-13 numbers — the server's group 13 currently reaches 133,
   and the client's index space is 512 wide (`MAX_ITEM_INDEX = 512`).
2. **The upgrade combination with a rare outcome.** This is exactly what
   `server/src/GameLogic/PlayerActions/Craftings/FenrirUpgradeCrafting.cs`
   already does: it consumes a Horn of Fenrir plus jewels and *modifies the
   item in place*, attaching a different option set depending on the inputs
   (`BlueFenrir` vs `BlackFenrir` option types, lines 101-112). For `Ur`,
   the analogous craft consumes a 4th-guardian + `Seal of Ur` + jewels and
   returns either `Ur` or `Ur (Rare)` — the rare outcome is a probability in
   `CreateOrModifyResultItemsAsync`, exactly as the Fenrir craft picks
   between its two variants.
   The `TransientItemCraftingRequiredItem` / `CraftingRequiredItemLink`
   model and `successRateByItems` (line 78) are the levers.
3. **The "Rare" is a different colour, and the server already has the
   mechanism.** The Webzen guide says `Ullr (Rare)` has *"the same attributes
   as Ullr, but a different color"*. The server sends a pet's colour variant
   as **bit flags in the appearance preview bytes**:
   `server/src/GameServer/RemoteView/AppearanceSerializer.cs`.
   - `PetIndex` (line 29) maps the item number to a pet case:
     Angel 0, Imp 1, Unicorn 2, Dinorant 3, DarkHorse 4, DarkRaven 5,
     Fenrir 37, Demon 64, SpiritOfGuardian 65, Rudolph 66, Panda 80,
     PetUnicorn 106, Skeleton 123.
   - `preview[16]` bits select the **model**: Panda `|= 0xE0`,
     PetUnicorn `|= 0xA0`, Skeleton `|= 0x60`, Rudolph `|= 0x80`,
     SpiritOfGuardian `|= 0x40`, Demon `|= 0x20` (lines 398-421).
   - `preview[16]`/`preview[17]` bits select the **colour variant** for the
     Fenrir: `BlackFenrir` → `preview[16] |= 0x01`, `BlueFenrir` →
     `preview[16] |= 0x02`, `GoldFenrir` → `preview[17] |= 0x01`
     (lines 377-390). The option types are `ItemOptionTypes.BlackFenrir` /
     `.BlueFenrir` / `.GoldFenrir` (`ItemOptionTypes.cs:66,72`).
   So the `Ur`/`Ur (Rare)` colour split is the *same pattern*: a new
   `ItemOptionType` (e.g. `RareUr`) on the `Ur` definition, a new bit in
   the Fenrir-style branch, and a matching `case` in the client's pet
   rendering. This is precisely the "same item, different colour" mechanic
   `Ur (Rare)` uses.

**The client side is the real work**, and it is small:

4. A `MODEL_UR` entry in `client/src/source/Core/Globals/_enum.h` —
   `MODEL_UR = MODEL_HELPER + <itemNumber>`, matching how the existing
   mounts are declared (`MODEL_PET_PANDA = MODEL_HELPER + 80`).
5. A `case` in **both** helper switches — `ZzzCharacter.cpp:12703` (to
   create the mount) and `ZzzCharacter.cpp:13020` (to route it to
   `CreateMount` / `CreatePet`).
6. A `Group13_Helper.json` entry for each of the two items, plus a
   `Models/Group13_Helper.json` entry pointing at the model file
   (`Data/Item/PandaPet.bmd`) with the inventory/ground anchor, offset,
   rotation and scale — copy item 80's block.
7. **The "Rare" visual** — the server side is done (the colour bit above);
   the client has to act on that bit. The client has **no "rare colour"
   channel today** for pets, but it does have a per-model glow colour:
   `glow.ancientColor` in the item model JSON
   (`client/src/bin/Data/Items/Models/Group08_Armor.json` and the other
   armour groups), read by `ItemModelGlowJson.cpp:169` and applied in
   `ZzzObject.cpp:5970`. Two options:
   - tint the rare variant through the existing glow channel — a
     gold-tinted rare bear, no new assets; or
   - add a second model file for the rare colour, the faithful route but
     it needs new assets.

   The Fenrir colour variants are the reference implementation to copy: the
   server sets `preview[16] |= 0x01` for the black variant and the client
   switches model accordingly.

**Also needed for faithfulness:** the S6 client has no 5th-guardian
progression UI and no `Seal of Ur` art. The recipe above delivers the mount
item, its stats, and the rare-variant craft; the surrounding season content
(the 4th→5th guardian upgrade chain, the 5th-class quest that grants it) is
a much larger later-season backport and is out of scope here.

**Cost estimate:** the server side is configuration plus one small crafting
handler modelled on `FenrirUpgradeCrafting` (122 lines); the client side is
two `switch` arms and two JSON entries. The full-fidelity route is blocked
on assets, not on effort.

### 3.3 A new invasion / boss event

Copy the `RedDragonInvasionPlugIn` pattern (`RedDragonInvasionPlugIn.cs` is
34 lines; `SimpleInvasionPlugIn` + `InvasionConfigurationDefaults` do the
work). No new engine code. Needs: a monster that already exists server-side
*and* client-side, then a config entry.

### 3.4 A new map / gate

`server/src/Persistence/Initialization/VersionSeasonSix/Maps/` holds one file
per map with `CreateMonsterSpawn` calls; `GameMapsInitializer.cs` and
`Gates.cs` wire them up. The client also needs the map's `.att`/`.map` assets
under `client/src/bin/Data/World*` and the `Object*` texture folders. This is
the heaviest of the four because of the client asset volume.

---

## 4. Rules that apply to all of it

- `docs/VANILLA_COMPATIBILITY.md` requires: record expected vanilla behaviour
  and the reproducible discrepancy **first**, then seek approval for a
  correction. Do not add custom content to make a test pass.
- `docs/ROADMAP.md` keeps "Optional OriginSix extensions" **Deferred**, with
  the stated gate: *"Approved requirements, default-OFF gates, unchanged
  baseline regressions"*. Any custom feature must default to **OFF** and
  leave the baseline regressions unchanged.
- OpenMU's own extension classes implement `IDisabledByDefault`; the
  initialization honours that marker. Follow the same pattern for anything
  custom so it cannot turn itself on in a vanilla database.
- Client and server packet/data definitions must be **paired and versioned**
  together. A monster number on the server that the client cannot render is a
  silent failure, not a crash.
- Preserve upstream licensing and attribution for any asset that is added.

---

## 5. What I could not verify

- Whether the Golden / Red Dragon / White Wizard invasions are **enabled** in
  the running local server. The plugin classes exist and the monsters are
  configured; I did not observe an invasion running.
- Whether `Bugbear` client art is intended for a future server monster. No
  server-side reference exists.
- The `Ur` model file and its texture. `Ur` is confirmed to exist as an MU
  Online item (5th Guardian mount, item ID 6237, with a `Ur (Rare)` colour
  variant) from MU item databases, but the **binary assets are Webzen's and
  are not in this workspace**, so the full-fidelity route is blocked on
  assets, not on effort. The Panda-model route needs no assets.
- The exact level requirement of `Ur` in the version it would be ported
  from: sources disagree (1200 on the S20/21 item database, 1450 on older
  guides). Pick one and document it.
- Whether the client's pet renderer reads the colour-variant bit for a
  *new* pet type. The Fenrir path is confirmed server-side
  (`AppearanceSerializer.cs:377-390`); the matching client-side switch arm
  was not located in this pass.
- Any in-game rendering of a bear mount. The item/config side is verified by
  inspection only.

## 6. Files worth reading, in order

| Purpose | File |
|---|---|
| Invasion pattern to copy | `server/src/GameLogic/PlugIns/InvasionEvents/RedDragonInvasionPlugIn.cs` |
| Invasion defaults (monsters, maps, schedules) | `server/src/GameLogic/PlugIns/InvasionEvents/InvasionConfigurationDefaults.cs` |
| Monster numbers used by invasions | `server/src/GameLogic/PlugIns/InvasionEvents/InvasionMonsters.cs` |
| Client model loading by model index | `client/src/source/Engine/Object/ZzzOpenData.cpp:1317` |
| Client monster model enum (dense) | `client/src/source/Core/Globals/_enum.h:4342` |
| Client monster numbers (424/425 = bears) | `client/src/source/Core/Globals/_enum.h:4947` |
| Client render model entries | `client/src/source/Core/Globals/_enum.h:5254` |
| Client mount/helper model switch (create) | `client/src/source/Engine/Object/ZzzCharacter.cpp:12703` |
| Client mount/helper model switch (route) | `client/src/source/Engine/Object/ZzzCharacter.cpp:13020` |
| Server mount helper attribute | `server/src/GameLogic/Attributes/Stats.cs:1515` |
| Existing bear spawns | `server/src/Persistence/Initialization/VersionSeasonSix/Maps/Elvenland.cs:74` |
| Client group-13 (mount) item data | `client/src/bin/Data/Items/Group13_Helper.json` |
