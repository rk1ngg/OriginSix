# Authorized stat boost — rkingg account — 2026-10-22

## Request

Add **+5,000 to each main attribute** (STR, AGI, VIT, ENE) to the `rkingg`
account, and **+5,000 more on all stats for every character** on it.

Interpretation applied: one flat +5,000 on each of the four main attributes,
for each of the three characters of the account. No character receives the
boost twice. Leadership was not raised — see "Scope" below.

## Scope resolved from the data

Account `rkingg` has three characters:

| Character | Class | Role |
|---|---|---|
| `RKINGG` | 25 | Fist Master / Rage Fighter |
| `OpenCodeX` | 23 | Summoner |
| `OpenCodeXx` | 7 | Blade Master |

Twelve attribute rows were targeted: 4 attributes × 3 characters.

**Leadership was not touched.** `config."MonsterDefinition"`/`AttributeDefinition`
inspection confirmed `Base Leadership` is a base stat only for Dark Lord
(`ClassDarkLord.cs:41`). None of these three classes has a `Base Leadership`
row, so there is nothing to add. RF therefore receives no leadership change —
its leadership comes from equipment, unchanged by this operation.

## Execution

- Stopped only the app container `originsix-local-openmu-1` (the database
  container stayed up) so no live session could save over the change.
- Full custom-format PostgreSQL backup before the write:
  `.local/backups/originsix-before-statboost-20261022.dump`
  (1,442,029 bytes; archive listing verified, 982 TOC entries;
  SHA256 `F4BEC9909C0F440A34148F76CC681900A5B05D2FB541D06CB8820F5FD857BFCD`).
- Applied through `scripts/add-5000-test-stats.sql` in a single transaction
  with baseline guards: it asserts the current values equal the recorded
  baseline, refuses to run if the values differ (so a re-run cannot
  double-apply), and re-reads every row after the update to assert the saved
  delta is exactly +5,000.
- Restarted the app container; server healthy.

## Result — verified saved deltas

| Character | Attribute | Before | After | Delta |
|---|---|---|---|---|
| OpenCodeX | Base Strength | 411 | 5411 | +5000 |
| OpenCodeX | Base Agility | 300 | 5300 | +5000 |
| OpenCodeX | Base Vitality | 200 | 5200 | +5000 |
| OpenCodeX | Base Energy | 1554 | 6554 | +5000 |
| OpenCodeXx | Base Strength | 1305 | 6305 | +5000 |
| OpenCodeXx | Base Agility | 860 | 5860 | +5000 |
| OpenCodeXx | Base Vitality | 200 | 5200 | +5000 |
| OpenCodeXx | Base Energy | 100 | 5100 | +5000 |
| RKINGG | Base Strength | 697 | 5697 | +5000 |
| RKINGG | Base Agility | 700 | 5700 | +5000 |
| RKINGG | Base Vitality | 1283 | 6283 | +5000 |
| RKINGG | Base Energy | 404 | 5404 | +5000 |

Independent re-query after the restart confirms the same 12 values persisted.

## Not changed

Level, Master Level, master skill points, level-up points, fruit points,
Zen, equipment, item options, socket state, quests and skills are untouched.
`UsedFruitPoints` and `LevelUpPoints` were not modified, so the fruit cap and
the free-point counters still describe the same state as before.

## Why this is safe to send over the wire

The client receives base attributes as a plain `(ushort)` cast in
`UpdateCharacterStatsPlugIn.cs:48-51`, which **wraps** rather than clamps
above 65535. The largest base attribute after the boost is 6554, well under
that limit, so no value wraps.

Derived maxima are computed from these attributes
(`config."AttributeRelationship"`, e.g. Maximum Health =
`2 × Total Vitality + 1.3 × Total Level` for class 25) and are sent through
`ToUInt16Clamped()` (`AttributeValueExtensions.cs:21`), which clamps at 65535
rather than wrapping. The highest derived value after the boost is a maximum
shield of roughly 40.5k, still under the limit. No `ushort` overflow occurs
on any packet.

This is a **deliberate deviation from the vanilla legal point budget** — an
authorized local test-data exception, not a vanilla configuration.

## Verifier kept consistent

`scripts/CharacterProvisioner/CharacterUpgrades.cs` asserts the saved base
stats equal the legal budget plus the fruit delta. With the boost applied
that assertion is intentionally false, so the helper was updated to account
for it explicitly:

- a named constant `TestStatBoost = 5000`, documented as the authorized
  local exception;
- `Allocate` adds the boost to each of the four base stats, so a future
  re-run reproduces this state instead of silently wiping it;
- `Validate` adds `BaseStats.Length * TestStatBoost` to the expected total,
  so the check still validates the legal budget *and* the boost separately.

Helper rebuilt: **0 warnings, 0 errors**.

## Pre-existing defect found (not caused by this change)

The repo's own `--upgrade-all --check` run fails on `Item definition missing`
for `RKINGG`. Investigation:

- Two `data."Item"` rows have a NULL `DefinitionId`:
  `2760a101-0000-7987-5720-db9e93dd3c90` (in RKINGG's inventory, slot 14)
  and `0660a101-0000-7693-0234-0d594cd9b1d5` (in a separate storage, slot 20).
- Both are present with `DefinitionId = \N` in the **pre-change backup** taken
  before this operation, so they predate it.
- The stat budget assertion — the part this operation touches — **passed**
  for `OpenCodeX` before the run reached the item loop.

These are definition-less item rows: `DefinitionId` NULL, `Level` 0,
`SocketCount` 0, no option links. Two exist in the whole database; only the
one in RKINGG's inventory affects the verifier.

**Probable origin — not confirmed.** The server log shows an item dropped and
then expired (`Item dropped ... Great Dragon Gloves+2` followed by
`Item expired ... wasn't picked up and got deleted`) at 20:22–20:23 on
2026-10-21, which is close to when these rows would have been written. The
drop path (`DropItemAction.cs:109`) detaches the item from the player's
context, and the expiry path (`DroppedItem.cs:287-307`) deletes it through a
separate trade context. I did **not** confirm that this path is what wrote
these rows, so treat the link as a lead, not a finding.

The second row sits in storage `00001000-00f4-0000-0000-000000000000` — a
hand-crafted legacy GUID holding 13 NPC-shop items (Ale, Town Portal Scroll,
Blood Bone, Devil's Eye/Key, Armor of Guardsman, Horn of Fenrir). It is not
owned by any character.

**Not repaired here.** Deleting item rows is a destructive data change and was
not part of the request. It needs its own approval and its own backup.

## Verification status

| Check | Result |
|---|---|
| 12 rows changed, each exactly +5000 | Observed in-transaction |
| Values persisted after restart | Observed by independent re-query |
| Server healthy after restart | Observed (`Status=running Health=healthy`) |
| Helper rebuild | 0 warnings, 0 errors |
| Stat budget assertion under the helper | Passed for `OpenCodeX` |
| Full helper `--upgrade-all --check` | **Fails** on the pre-existing orphan item rows above |
| In-game displayed stats | **Not observed** — needs a client login; the database values are the evidence, not the rendered HUD |

## Files

| Path | Change |
|---|---|
| `scripts/add-5000-test-stats.sql` | New — the applied transaction with baseline guards |
| `scripts/CharacterProvisioner/CharacterUpgrades.cs` | `TestStatBoost` constant; `Allocate` and `Validate` updated |
| `.local/backups/originsix-before-statboost-20261022.dump` | New — pre-change backup (git-ignored) |
