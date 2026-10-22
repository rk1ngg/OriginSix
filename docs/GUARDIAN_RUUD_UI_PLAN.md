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

The official Guardian index currently exposes the 3rd through 7th Guardian
guides. The names and recipes for the earlier prerequisites must be verified
before they are added; no unverified names or recipes will be invented.

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

## Inventory and equipment UI

The target is the supplied S18-style inventory layout, limited to the surfaces
needed by this feature:

- Main inventory grid and existing S6 actions.
- Character equipment panel.
- Guardian/mount slot.
- Wing and ring slots as currently supported.
- Officially verified jewelry slots for the necklace/pendant and
  piercing/earring items.
- Item tooltip with official name, level, requirements, upgrade level, defense,
  Guardian options, elemental options, and resistance options.

The English item terminology for the two new jewelry slots must be verified
before naming client enums, localization keys, or database records. The user
term `piercing` is not treated as the canonical name automatically.

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
