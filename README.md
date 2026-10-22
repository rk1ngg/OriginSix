# OriginSix

OriginSix is a local MU Online Season 6 Episode 3 baseline built from the
official OpenMU server and MuMain client sources.

The project goal is a reproducible, playable local baseline first. Custom
gameplay, items, mounts, events, resets, visual changes, and public deployment
are out of scope until they are separately approved and tested.

## Layout

The current workspace keeps the source repositories independent:

```text
OriginSix/
|-- server/   MUnique/OpenMU
|-- client/   sven-n/MuMain
|-- docs/     decisions, evidence, roadmap, and upstream review records
|-- scripts/  approved local runtime and verification helpers
|-- AGENTS.md repository maintenance rules for contributors and automation tools
`-- README.md this project overview
```

The planned personal GitHub repository is also named `OriginSix`. When the
one-repository migration is approved, `server/` and `client/` will live under
this root while their official source URLs remain documented and checked
periodically.

## Upstream maintenance

Before any bug fix, check the official upstream default branch, commits, issues,
and pull requests for an existing fix or requested merge. Prefer verifying or
applying relevant upstream work before writing a duplicate local fix.

Official sources:

- Server: https://github.com/MUnique/OpenMU
- Client: https://github.com/sven-n/MuMain

See [docs/UPSTREAM_STRATEGY.md](docs/UPSTREAM_STRATEGY.md) for the review,
approval, testing, and fork-push workflow. This rule applies to every
automation tool and human contributor.

## Current baseline

- Server upstream baseline: `cc0ccd268d70303b1b65bce58b178afcb1437fd5`
- Client baseline: `063169014914add35a777178b15d8c9fbaae4b51`
- Server includes the recent upstream Castle Siege merges.
- Local anti-cheat mount tolerance changes are preserved and require a local
  commit before they can be included in a GitHub push.

## Local runtime

Runtime instructions and observed results are in
[docs/LOCAL_RUNTIME_2026-10-22.md](docs/LOCAL_RUNTIME_2026-10-22.md).

Useful helpers:

- `scripts/local-server.ps1`: start, stop, and inspect the dedicated local
  runtime.
- `scripts/test-local-server.ps1`: run the read-only local protocol check.
- `scripts/play-local.cmd`: launch the client against the local server.

Do not reuse unrelated database containers or volumes. Do not commit secrets,
credentials, or expanded environment output.

## Evidence and limits

This is not a claim of 1:1 vanilla compatibility. Compatibility, asset rights,
runtime behavior, and feature parity are tracked as evidence in `docs/`.
Read [docs/ROADMAP.md](docs/ROADMAP.md) before expanding the baseline.

OpenMU is MIT-licensed. MuMain and bundled MU data require separate attribution
and rights review; public availability is not treated as a distribution license.
