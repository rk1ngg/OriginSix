# Selective upstream maintenance

## Current state

Both repositories retain their official URLs as remote `origin`. Keep those
references. The server was fast-forwarded to upstream `master` at
`cc0ccd268d70303b1b65bce58b178afcb1437fd5` on 2026-10-22 to include the recent
Castle Siege merges. The local anti-cheat changes remain uncommitted. No
OriginSix fork remote or push has been created. Default branches are server
`master` and client `main`; baseline full SHAs are in
[REPOSITORY_AUDIT.md](REPOSITORY_AUDIT.md).

When the personal GitHub repositories are ready, add them as separately named
push remotes such as `fork`; keep official upstream as `origin`. Do not silently
repoint `origin`, and never push to the official upstream. Never use `git pull`
as the update procedure.

## Personal fork and recurring checks

The intended layout is:

- `origin`: official `MUnique/OpenMU` or `sven-n/MuMain`, fetch only;
- `fork`: the user's GitHub repository, push only after explicit approval;
- local `master`/`main`: the working baseline, with OriginSix changes reviewed
  separately from upstream changes.

For a periodic review, record status and current heads, fetch only the official
reference, enumerate commits after the last reviewed upstream SHA, and inspect
each candidate before applying it. Apply only approved commits or a reviewed
fast-forward, run the paired tests, then push the resulting local branch to the
personal `fork` remote. Keep an upstream review ledger entry with the full SHA,
PR, reason for applying or skipping, and exact verification results.

## Review and approval workflow

Run each repository independently, with its working directory explicitly selected.

The upstream check is mandatory before a local bug fix. This applies regardless
of which automation tool or human is performing the work: inspect the official
default branch, search relevant commit history, issues and pull requests, and
compare the local implementation before proposing or writing a new fix. If an
upstream fix or merge request exists, prefer verifying or applying that work
before creating a duplicate local solution.

1. Record `git status --short --branch`, HEAD and remote URLs. Stop on unexplained
   local changes. Do not stash, reset, clean or overwrite them automatically.
2. On an authorized update review, fetch only official references, e.g. server
   `git fetch --no-tags origin master`, client `git fetch --no-tags origin main`.
   Fetch changes remote-tracking refs, not the checked-out branch. Record old and
   new upstream heads; do not automatically advance local branches.
3. Enumerate candidate commits with `git log --reverse --oneline BASE..origin/master`
   (server) or `BASE..origin/main` (client); substitute the recorded upstream review
   base SHA, not an assumed merge-base. Use `git show --stat SHA` and `git show SHA`
   for actual review. Inspect changed tests, parent commits and related PR discussion.
4. Prioritize relevant security fixes, correctness, data-loss prevention, then
   measured performance. Flag gameplay, packet, asset or default-configuration
   changes explicitly; do not import custom content merely because it is upstream.
5. For each candidate record rationale, dependencies, affected APIs/generated code,
   expected conflicts, schema/configuration/data changes, client/server coupling,
   licensing impact and proposed regression commands. Review source semantics,
   not just commit subjects. Check previous picks with `git cherry`/patch comparison
   where needed; adapted patches and merge commits require manual comparison.
6. Present a recommendation and obtain approval for the exact changes and any
   migration. Applying patches, commits and database changes are separate gates.
7. Create isolated `originsix/backport-<topic>` branches from the approved OriginSix
   baseline in each affected repository. After explicit commit authorization use
   `git cherry-pick -x SHA`. A source-only approval can instead use
   `git cherry-pick --no-commit SHA` for review, leaving commit creation pending.
   Cherry-pick merge commits only after understanding mainline/dependencies; never
   guess `-m`. Conflicts require manual adaptation, not automatic ours/theirs.
8. Review the resulting diff, build once and run relevant tests plus paired local
   smoke tests. Compare vanilla outcomes. A schema change needs a dedicated DB
   backup, migration/restore test and separate approval before existing-state use.
9. Record results, applied commit IDs and reasons for deviations. Keep failed
   candidates isolated. Promote only after acceptance; do not push without approval.

Before a runtime test, inspect `git status` again because build generators may
change tracked files. Never erase generated diffs just to produce a clean status.

## Regression selection

- Server: matching Release solution build/tests; targeted network/packet tests for
  protocol fixes, attribute/game-logic tests for combat, initialization and persistence
  checks for data/schema changes. Check the solution includes each required test
  project before relying on a solution-wide command.
- Client: x64 player configure/build, CTest with Release and tests enabled; verify
  Native AOT library and assets; editor/control-socket exclusion checks where registered.
- Pair: login, creation, movement/combat, map transitions, item operations and
  persistence/reconnect. Expand to affected feature rows and numeric formula
  comparisons; source tests alone cannot certify gameplay compatibility.
- Maintain packet package/version pairing and exact runtime/data release relation;
  a client-only update can require a server update and vice versa.

## Review ledger

Append one row per reviewed commit here (or split into a dedicated ledger once
large). Use full SHAs and statuses **reviewed**, **applied**, **skipped**, or
**deferred**. Preserve skipped/deferred reasons so future reviews do not repeat work.

| Date | Repository / upstream commit | Status | Relevance / dependencies | Decision / resulting commit | Verification |
|---|---|---|---|---|---|
| 2026-10-21 | Both: baseline SHAs in repository audit | reviewed (baseline identity only) | Live upstream heads match local heads | No upstream patches applied; individual historical fixes not reviewed | Read-only Git/source inspection; no regression run |

Future entry must include PR/reference, vanilla impact, prerequisite commits,
conflicts/adaptation, approval reference, local resulting SHA if committed, and
exact test results. No pending candidate commit is currently recommended without
individual review.
