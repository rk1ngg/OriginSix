# Mid-rate server plan

Status: decided by the owner on 2026-10-29. This records the design; the drop and
option configuration is not changed yet and needs its own approval.

## Goal

A mid-rate, non-reset Season 6 Episode 3 server: normal experience, a slow master
level, and items whose excellent options are limited so that a normal build stays
the norm.

## Rates

| Rate | Plan |
| --- | --- |
| Experience | Moderate ("sakto lang"): a level 400 character is reachable without a reset system. |
| Master level | Hard: master experience is deliberately low. |
| Resets | None. Maximum level 400, maximum master level 200 (the OpenMU defaults). |

The exact experience and master experience multipliers are still open; they are set
after measuring the time to level on the local baseline (`docs/ROADMAP.md`).

## Items and excellent options

| Items | Excellent options |
| --- | --- |
| Weapons | Always 2 options. |
| Armor and jewelry other than the 380 items | Up to 3 options. |
| 380 items and 400 items | 1 option. |
| Socket items | None, as in official S6: a socket item has sockets (1 to 5) for seed spheres instead of excellent options. |

Open points:

- Whether the 380 items take 1 or 2 options was discussed; the plan is 1 option
  for 380 and 400 items, 2 options for weapons. Confirm before configuring.
- How many sockets dropped socket items get.

## Character builds

Normal stat builds are expected: points spread over the stats, plus fruits. Full
agility builds are possible but not the norm.

## Attack and magic speed cap

Players: **200** (the OpenMU default, `Stats.AttackSpeed` and `Stats.MagicSpeed`
`MaximumValue`, editable in the Admin Panel under Attributes; it applies to every
player).

Game masters: **300**, set by OriginSix code in `ItemAwareAttributeSystem`
(`GameMasterSpeedCap`) for characters with the game master status or game master
accounts; covered by `GameMasterSpeedCapTests`.

### Why 200 is enough for this plan

The speeds follow agility (server formulas in
`server/src/Persistence/Initialization/CharacterClasses`):

| Class | Attack speed | Magic speed |
| --- | --- | --- |
| Dark Knight, Magic Gladiator | agility / 15 | agility / 20 |
| Dark Wizard | agility / 20 | agility / 10 |
| Fairy Elf | agility / 50 | agility / 50 |
| Dark Lord | agility / 10 | agility / 10 |
| Summoner | agility / 20 | agility / 20 |
| Rage Fighter | agility / 9 | agility / 9 |

Items and buffs add about 30 to 55 in total: excellent attack speed +7 (weapon,
pendant), the fire socket option +7 to +11, some pets +10, the Dinorant option +5,
Jack O'Lantern Blessing +10, gloves, and the OriginSix Guardian +7.

A level 400 non-reset character has about 2,000 level-up points plus fruits. With a
normal build (around 600 to 900 agility), the speeds reach about 70 to 145, below
200 for every class. Only full agility Dark Wizards (magic), Dark Lords and Rage
Fighters reach 200 from agility alone.

### Why the cap exists (upstream)

- OpenMU caps attack speed at 200; magic speed got the same cap in
  [MUnique/OpenMU#659](https://github.com/MUnique/OpenMU/pull/659) (2025-11) for
  balance: physical skills stopped scaling at 200 while magic kept scaling.
- A configured maximum in the Admin Panel was ignored
  ([#663](https://github.com/MUnique/OpenMU/issues/663)); fixed in
  [#664](https://github.com/MUnique/OpenMU/pull/664), present locally
  (`server/src/AttributeSystem/AttributeSystem.cs`, `GetValueOfAttribute`).
- The speed hack check ([#823](https://github.com/MUnique/OpenMU/pull/823), merged
  2026-07-12, present locally) compares attack timing with the current attack
  speed, not with the cap. Whether the follow-up false positive fix
  ([#1322](https://github.com/MUnique/OpenMU/pull/1322), merged 2026-10-28) is in
  the local snapshot is not checked yet.

Not verified: whether attack animations of the S6 client look wrong above 200. The
client raises the animation speed with the attack speed (`SetAttackSpeed()` in
`client/src/source/Engine/Object/ZzzCharacter.cpp`); a game master at 300 is the
way to see it.
