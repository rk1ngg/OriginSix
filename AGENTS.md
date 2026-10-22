# OriginSix workspace instructions

## Objective

Establish a playable, stable, local MU Online Season 6 Episode 3 baseline with
documented fidelity limitations. No custom gameplay, items, mounts, events,
resets, or visual changes during initial setup. Do not equate upstream defaults
with proven vanilla behavior.

## Workspace and repository boundaries

- Work from `D:\OriginSix` across independent `server/` (MUnique/OpenMU) and
  `client/` (sven-n/MuMain) Git repositories. Never combine their histories or
  create duplicate nested clones.
- Parent `docs/` contains OriginSix decisions and evidence; `scripts/` is for
  approved development automation. The parent is currently not a Git repository.
- Before changes, inspect each affected repository's status, branch and HEAD.
  Preserve existing work. Never delete or overwrite existing files without approval.
- Before diagnosing or implementing a bug fix, check the official upstream
  repository's current default branch, commit history, issues and pull requests
  for an existing fix or requested merge. Record whether the upstream fix is
  already present locally, should be applied selectively, or is still pending.
  Do not duplicate an upstream fix without explaining the difference.
- Read repository-local `AGENTS.md` and `docs/CODING_RULES.md` before source changes.
  Follow applicable deeper instructions. Preserve licenses and attribution.
- Keep official upstream remotes. No automatic merge, pull, reset, branch update,
  commit, push, or public deployment. Use the selective workflow in
  `docs/UPSTREAM_STRATEGY.md`; all applications of upstream patches need approval.

## Approval gates

Safe read-only inspection and creation of the requested initial parent documentation
are authorized. Explain the planned action and request approval before:

- Installing/upgrading dependencies or changing system settings, authentication,
  firewall rules, or public access.
- Modifying source, existing files, game data, formulas, or gameplay configuration.
- Creating/modifying database state, migrations, reinitialization, container/volume
  creation, or starting game services. Never reuse an unrelated database.
- Building/configuring when it executes repository hooks, generators, or downloaded
  tools; initializing submodules and downloading/executing client releases.

Do not log secrets or add credential/environment files to Git. Inspect secrets
by presence only. Do not use `-reinit` or remove database volumes as routine setup.

## Engineering policy

- Favor existing plugin/configuration/API boundaries; no engine refactor now.
- Avoid core combat/networking changes. Formula corrections require evidence and
  explicit approval. OriginSix custom modules, if approved later, default OFF.
- Keep client/server packet definitions and data compatibility paired and versioned.
- Use loopback-only published ports for local services. An advertised loopback IP
  is not a listener bind restriction; upstream listeners bind all interfaces.
- First establish the reproducible baseline, then measure before infrastructure
  expansion. Linux all-in-one hosting is a future candidate, not an approved deployment.

## Evidence and verification

Use full commit IDs, dates, file paths, exact commands and observed results.
Separate inspected implementation, upstream claims, assumptions, and executed tests.
Never infer a pass from file names, README badges, existing tests, or compilation.
No invented percentages or 1:1 compatibility claims. Record build/test failures and
unavailable checks honestly. Build once and reuse matching `--no-build` test outputs
when source/configuration has not changed. Follow `docs/ROADMAP.md` acceptance gates.
