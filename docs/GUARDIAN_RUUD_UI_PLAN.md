# Guardian, Ruud, and S18-style UI plan

Status: approved planning scope, no gameplay implementation yet.

## Goal

Add a Season 6 Episode 3-compatible Guardian progression through the official
5th Guardian `Ullr`, using official Webzen names and an S18-style inventory,
equipment, Trainer, and Ruud Shop presentation.

This is not an S18 ruleset port. The UI and item identity may follow the later
official presentation, but requirements, limits, stats, economy, and protocol
must fit the local S6 EP3 client/server pair.

## Locked scope

- Guardian progression stops at `Ullr`.
- Official spelling is used in code, localization, tooltips, and documentation.
- `Fierce Lion` is the official 3rd Guardian name.
- `Shining Tail` is the official 4th Guardian name.
- `Ullr` is the official 5th Guardian name. `UR` may be a player-facing alias
  only if it does not replace the canonical name.
- Existing S6 pets and mounts remain compatible.
- S18-only requirements such as level 1200 are not copied into S6.
- The feature is disabled by default until both client and server support it.

## Guardian facts verified from official Webzen guides

The official 3rd, 4th, and 5th Guardian guides describe the same upgrade flow:

- Trainer NPC: Lorencia `(122, 110)`.
- Previous-tier Guardian at `+11` through `+15`.
- Tier-specific Guardian Seal.
- Jewel of Bless x20.
- Jewel of Soul x20.
- Jewel of Chaos x1.
- Jewel of Creation x1.
- Combination chances: `+11 = 60%`, `+12 = 70%`, `+13 = 80%`,
  `+14 = 90%`, `+15 = 100%`.
- On failure, secondary materials are consumed and the base Guardian returns to
  `+0`.
- A Rare result has the same attributes as the normal result, a different color,
  and a lower acquisition probability.

Sources:

- https://muonline.webzen.com/en/gameinfo/guide/detail/247
- https://muonline.webzen.com/en/gameinfo/guide/detail/286
- https://muonline.webzen.com/en/gameinfo/guide/detail/320

## Guardian season placement & verified prerequisites

All 5 Guardian tiers and their prerequisites have been verified from official Webzen patch guides:

- **1st Guardian — `Ghost Horse` (Season 14):** Crafted at Trainer (`Lorencia 122, 110`) using `[Bound] Seal of Ghost Horse` (`50,000 Ruud` from Priest James at `Elveland 32, 239`) + Jewels.
- **2nd Guardian — `Ice Dragon` (Season 15):** Crafted from `+11~+15 Ghost Horse` + `[Bound] Seal of Ice Dragon` (`50,000 Ruud`).
- **3rd Guardian — `Fierce Lion` (Season 16):** Crafted from `+11~+15 Ice Dragon` + `[Bound] Seal of Fierce Lion` (`50,000 Ruud`).
- **4th Guardian — `Shining Tail` (Season 17):** Crafted from `+11~+15 Fierce Lion` + `[Bound] Seal of Shining Tail` (`50,000 Ruud`).
- **5th Guardian — `Ullr` / `Ur` (Season 18):** Crafted from `+11~+15 Shining Tail` + `[Bound] Seal of Ur` (`50,000 Ruud`).

## Extracted & verified 3D assets (`Data/Skill/`, `Data/Item/`, `Data/NPC/`)

Extracted from `MU_Red_1_20_61_Full.zip` (BMD Version `0x0F`, supported natively in `ZzzBMD.cpp` via `DecryptLea256Body`):

| Tier | Guardian / NPC | Mount / World BMD (`Data/Skill/` or `Data/NPC/`) | Inventory BMD (`Data/Item/`) | Seal BMD (`Data/Item/`) |
|---|---|---|---|---|
| 1st | `Ghost Horse` | `ghost_horse.bmd` (`8` meshes, `108` bones, `8` actions) | `Ghosthorse_item_shield.bmd` | `Ghosthorse_item_mask01.bmd` |
| 2nd | `Ice Dragon` / `Rare` | `icedragon.bmd` / `icedragon_rare.bmd` (`3` meshes, `150` bones, `8` actions) | `hs_2GuadianiceCA.bmd` / `hs_2GuadianiceCB.bmd` | `hs_2GuadianiceD.bmd` |
| 3rd | `Fierce Lion` / `Rare` | `fiercelion.bmd` / `fiercelionR.bmd` (`4` meshes, `161` bones, `5` actions) | `hs_fiercelion_inB.bmd` / `hs_fiercelion_inR.bmd` | `hs_3Guadianice.bmd` |
| 4th | `Shining Tail` / `Rare` | `shiningtails.bmd` / `rare_shiningtails.bmd` (`4` meshes, `162`/`168` bones, `5` actions) | `shiningtails_in.bmd` / `rare_shiningtails_in.bmd` | `tail_inzangmdle.bmd` |
| 5th | `Ullr` (`Ur`) / `Rare` | `Ur.bmd` / `UrUp.bmd` (`7`/`8` meshes, `142` bones, `5` actions) | `ur_inven.bmd` / `urUp_inven.bmd` | `urseal.bmd` |
| NPC | `Priest James` (Ruud Shop) | `Data/NPC/james.bmd` (`1` mesh, `66` bones, `9` actions) + `NPCjames.OZJ` | `James_anvil.bmd` + `James_anvil.OZJ` | — |

## S6 adaptation rules

Each Guardian definition must keep the official identity separate from local
compatibility values:

- `OfficialName`
- `DisplayName`
- `GuardianTier`
- `RequiredLevelS6`
- `RequiredStatsS6`
- `AllowedClassesS6`
- `S6StatBonuses`
- `UpgradeMaterials`
- `RuudCost`
- `ClientModel`
- `RareVariantModelOrColor`
- `EnabledByDefault`

The S6 values must be chosen from the existing Season 6 level cap, class rules,
item rules, stat formulas, economy, item indices, and packet limits. S18 level,
stat, and balance values are reference evidence only, not defaults.

## OriginSix Guardian stats (fixed by the owner on 2026-10-29)

These are the stats of the OriginSix Guardians: the server sets them with
`AddGuardianMountsPlugIn` (version 8, `server/src/Persistence/Initialization/Updates`)
and the client shows them in `client/src/bin/Data/Items/Group13_Helper.json`
(items 13/146 to 13/154). They are OriginSix values, not official S18 ones.

| Guardian | Item | Life | Max HP | Armor | Attack speed | Running speed (ridden) |
| --- | --- | --- | --- | --- | --- | --- |
| Ghost Horse | 13/146 | 255 | +255 | 150 | +7 | 19 |
| Ice Dragon | 13/147 | 255 | +305 | 180 | +7 | 19.5 |
| Ice Dragon (Rare) | 13/148 | 255 | +330 | 195 | +7 | 19.5 |
| Fierce Lion | 13/149 | 255 | +355 | 210 | +7 | 20 |
| Fierce Lion (Rare) | 13/150 | 255 | +380 | 225 | +7 | 20 |
| Shining Tail | 13/151 | 255 | +405 | 240 | +7 | 20.5 |
| Shining Tail (Rare) | 13/152 | 255 | +430 | 255 | +7 | 20.5 |
| Ullr | 13/153 | 255 | +505 | 275 | +7 | 21 |
| Ullr (Rare) | 13/154 | 255 | +777 | 555 | +28 | 21 |

The Ullr (Rare) also adds attack +28 and wizardry +28.

- Max HP, armor and attack speed are added to the character while the Guardian is
  summoned. Max HP is added after the percentage bonuses, like the Gold Fenrir's, so
  a Guardian adds exactly its value. Measured on the server on 2026-10-29 (OpenCodeX,
  Ullr (Rare) summoned and put away): defense base +555, final defense +277.5. Armor is added to the character's base defense like an armor piece's
  defense. The attack speed counts toward the 200 cap of players (300 for game
  masters, see `docs/MID_RATE_PLAN.md`).
- The running speed applies while the Guardian is ridden outside the safezone
  (client `CharacterMoveSpeed()`); a Fenrir runs at 17, an excellent one at 19.
- Life is the item's durability (255). It decreases when the owner is hit, about
  a third as fast as a Fenrir's; without life the Guardian returns to the
  inventory and must be repaired before it is summoned again.
- Guardians have no level or class requirement, take one inventory slot, and have
  no equipment slot (their item slot type has no slots, so they don't stack).
- The tooltips list Mount, Guardian Item, Life, the options and the usage lines;
  the Ullr (Rare)'s options are set apart by blank lines and sorted shortest line
  first: health (green), armor (purple, shown pink), attack and wizardry (blue),
  movement speed (blue), attack speed (purple).
- The owner checked in game on 2026-10-29 that, with version 8, the Ullr (Rare)
  adds exactly its Max HP and armor.
- In the inventory, every Guardian glows like a +15 item.
- For comparison, the official S18 Guardian defense (MUDevs `Data/Item/Item.xml`,
  group 12) is 30, 96, 162, 228 and 290 for the five tiers, and the official
  "HP: 255" of the tooltip is the Guardian's life.

## Inventory and equipment UI

The target is the supplied Season 7-style inventory layout, limited to the surfaces
needed by this feature:

- Main inventory grid and existing S6 actions.
- Character equipment panel.
- Guardian/mount slot.
- Wing and ring slots as currently supported.
- Existing S6 pendant slot and terminology.
- Earring slots are optional and out of the base S6/S7 scope; add them only if
  an obtained UI reference clearly includes them and the compatibility decision is
  made separately.
- Item tooltip with official name, level, requirements, upgrade level, defense,
  Guardian options, elemental options, and resistance options.

Pendant is existing S6 terminology. Earring terminology and behavior are not
part of the initial implementation and must not drive client enums, localization
keys, or database records yet.

Adding a slot requires more than drawing it: equip validation, drag/drop,
serialization, persistence, stat application, tooltip data, trade/store rules,
reconnect handling, and tests must agree.

## Ruud and WCoin separation

The currency model remains explicit:

- Zen: normal character currency.
- Ruud: character progression currency and Ruud Shop balance.
- WCoin-C: cash-shop currency.
- WCoin-P: premium/gift cash-shop currency.

Ruud and WCoin must not share balances, ownership, prices, packets, or storage.
The first Ruud Shop catalog is limited to the Guardian Seals and materials needed
for this progression. The broader S18 Ruud catalog is out of scope for the first
vertical slice.

## Implementation order

1. Verify earlier Guardian prerequisite names and official jewelry terminology.
2. Map the existing S6 inventory, equipment, pet, Ruud/WCoin, and RML UI paths.
3. Define the S6-compatible Guardian and currency data table.
4. Add server-side item, currency, Trainer, and Ruud Shop behavior.
5. Add paired client item data, slots, tooltips, packets, and models.
6. Add the S18-style inventory, Ruud Shop, and Trainer UI.
7. Test normal and Rare outcomes, failures, persistence, reconnect, trade, and
   malformed requests.

## Existing extension points found

- S6 pets: `server/src/Persistence/Initialization/VersionSeasonSix/Items/Pets.cs`
- S6 mount combinations: `server/src/Persistence/Initialization/VersionSeasonSix/ChaosMixes.cs`
- Existing pet Trainer NPC: `server/src/Persistence/Initialization/VersionSeasonSix/NpcInitialization.cs`
- Existing inventory UI: `client/src/source/UI/Inventory/`
- Existing NPC shop UI and RML bridge: `client/src/source/UI/NPCs/NPCShop.*`
- Existing S6 group-13 item data: `client/src/bin/Data/Items/Group13_Helper.json`
- Existing WCoin packet surface: `server/src/Network/Packets/`

Current baseline constraints observed in the source:

- The server has WCoin-C, WCoin-P, and Goblin Points account fields, but no Ruud
  balance or Ruud packet surface yet.
- The client equipment model uses the fixed `MAX_EQUIPMENT_INDEX` array and
  currently maps twelve slots: helper, helm, wing, both weapons, armor, gloves,
  pants, boots, both rings, and amulet.
- The client already has a native/RmlUi split for inventory windows: RmlUi owns
  presentation while live item grids and equipment renders remain native.
