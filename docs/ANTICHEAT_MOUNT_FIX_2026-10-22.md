# Walk-speed false positive on mounted players — 2026-10-22

## The report

A game master playing on the **Gold Fenrir** (movement speed 19) was flagged
with *"Warning: Unusual activity detected (speed check)"* and disconnected
while moving around Kanturu. The account was not banned (it is a game master
account, and `AutoBan` is `false` in the live configuration).

Server log, 2026-10-22 05:10–05:13:

```
Speedhack detected on walk for player "OpenCodeX": traveled 1 tiles in 97.6519ms
  (expected at least 175.43858333333333ms), exceeding the tolerance of
  1403.5086666666666ms. Score: 1.22, overshoot: 39ms.
...
Speedhack detected on walk for player "OpenCodeX": traveled 1 tiles in 0.284ms
  (expected at least 175.43858333333333ms), ... Score: 2.40, overshoot: 49ms.
```

The walk was one tile in **0.284 ms**. That is not a player moving fast; it
is a burst of walk requests arriving together.

## Root cause

`SpeedHackDetectPlugIn.GetMaxWalkCreditMs` scaled the configured walk
tolerance **down** with the player's step delay:

```csharp
double configuredCreditMs = config.WalkSpeedToleranceMs * (stepDelayMs / WalkSpeedValidator.NormalStepDelayMs);
```

`NormalStepDelayMs` is 300 ms. A Gold Fenrir's step delay is
`4000 / 19 = 210.5 ms`, so the scaling factor is `0.70` and the configured
2000 ms tolerance became **1403.5 ms** — exactly the tolerance in the log
line above.

The tolerance is a *duration*: a network stall or a burst of walk requests
costs the same wall-clock time whether the character walks or rides. The
number of walk requests a client sends per second does not depend on how fast
the character moves. Scaling the budget down by the mover's speed therefore
penalises exactly the players who have the most reasons to send requests
quickly.

The numbers reproduce the field report exactly:

| Quantity | Computed | In the log |
|---|---|---|
| Step delay (speed 19) | 210.53 ms | — |
| Scaled tolerance | 1403.51 ms | 1403.5086666666666 ms |
| Expected time per tile | 175.44 ms | 175.43858333333333 ms |

## The fix

`server/src/GameLogic/PlugIns/SpeedHackDetectPlugIn.cs` — the tolerance is no
longer scaled below its configured value. A faster mover keeps the full
tolerance; a slower mover keeps the existing, larger scaled tolerance.

```csharp
double scaling = stepDelayMs / WalkSpeedValidator.NormalStepDelayMs;
double configuredCreditMs = config.WalkSpeedToleranceMs * Math.Max(1.0, scaling);
double walkStartLeadCreditMs = Math.Max(0, config.MaxAllowedWalkStartOffset) * stepDelayMs;
return Math.Max(configuredCreditMs, walkStartLeadCreditMs);
```

The `Math.Max` with the walk-start-lead allowance is unchanged: a client which
runs more than `MaxAllowedWalkStartOffset` tiles ahead is still resynchronized
by the separate start-offset check, and the credit still covers that lead.

**Upstream check first.** Per the request, the two upstream repositories were
checked before changing anything:

- `MUnique/OpenMU` (server): the latest commit touching the anti-cheat files
  is `317786a02bc66fd0724a600fac4410e2f56c1a81` (2026-10-17, *"tolerate
  quick re-clicks on mounts and never ban game masters"*), which is **already
  in this tree**. The four relevant files were compared by blob SHA against
  upstream `main` and are **byte-identical** — there is nothing to import.
  26 open upstream PRs, none touching the anti-cheat.
- `sven-n/MuMain` (client): local `063169014914` is one commit behind
  `acfbe9ffb12f`; the difference is unrelated to walking.

So this is a local change on top of upstream, not a backport.

## Verification

| Check | Result |
|---|---|
| `SpeedHackAntiCheatTests` + `ViolationPolicyTests` | **36/36 passed** (35 pre-existing + 1 new) |
| Full server test suite | **1903/1903 passed** |
| `MUnique.OpenMU.Network.Tests` | **73 passed**, 4 skipped (pre-existing) |
| New regression test | `TestQuickShortWalksOnFastMountDoNotTriggerViolationAsync` |
| Docker image | built from the modified source, `originsix-openmu:statboost-20261022` |
| Deployed assembly | SHA-256 differs from the previous image's `MUnique.OpenMU.GameLogic.dll` |
| Container health | `healthy`; game server and connect server listening on 45901/45902 |
| `test-local-server.ps1` | 3/3 PASS (connect-server Hello, 1 game server, endpoint 127.127.127.127:45902) |

The new test asserts the contract directly: a player on an upgraded Fenrir
who clicks `MaxAllowedWalkStartOffset` short walks in quick succession is not
flagged, and the account stays normal.

### Measured behaviour

Tolerance sweep on the Gold Fenrir (speed 19), counting violations for the
upstream cheat scenario and the legitimate quick-click scenario:

| Configured tolerance | Cheat detected (banned)? | Legit quick clicks flagged? |
|---|---|---|
| 1400 ms (old effective value) | yes | 6 violations |
| 2000 ms (configured value) | yes | 0 violations |
| 2800 ms | yes | 0 violations |
| 3500 ms | **no** (not banned) | 0 violations |

The configured 2000 ms keeps cheat detection intact while removing the false
positive. Raising the tolerance further would start to hide real cheats, so
the code change — not a configuration change — is what fixes this.

## Not changed

- The `WalkSpeedValidator` math, the violation score, the warning history and
  the sanction policy are untouched.
- `AutoBan` remains `false` and `DisconnectOnViolation` remains `true` in the
  live plug-in configuration; the configuration was not altered.
- No client change was needed or made.

## Files

| Path | Change |
|---|---|
| `server/src/GameLogic/PlugIns/SpeedHackDetectPlugIn.cs` | The tolerance is not scaled below its configured value for a faster mover |
| `server/tests/MUnique.OpenMU.Tests/SpeedHackAntiCheatTests.cs` | New regression test for the mounted-player case |
| `scripts/compose.local.yml` | Points at the new image `originsix-openmu:statboost-20261022` |

Rollback: set `scripts/compose.local.yml` back to
`originsix-openmu:151e3af43c89` and restart. That image is still present.
