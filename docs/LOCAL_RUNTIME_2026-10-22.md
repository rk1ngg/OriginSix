# Local runtime — 2026-10-22

User approved dedicated database creation and local server startup with “go”.
Both containers were left running. This supplements the original setup plan and
[build report](BUILD_REPORT_2026-10-22.md); no in-game login/gameplay pass is claimed.

## Current access

- Admin panel: **http://127.0.0.1:8080/** (redirects to `/login`).
- Bootstrap admin login name: **originsix-admin**.
- Copy its generated password privately in Windows PowerShell:

  ```powershell
  & D:\OriginSix\scripts\local-server.ps1 -Action CopyAdminPassword
  ```

  Paste directly into the local login page, not chat. This command changes the
  clipboard; the startup/verification did not execute it. After signing in, use
  the Users page to create a persistent personal admin account for routine use.
- Use Accounts in the admin panel to create your own game account. Admin users
  and game accounts are separate. The upstream initializer also created 20 test
  accounts; their well-known credentials are documented upstream, not stored here.
- Start the local Windows client by double-clicking
  **`D:\OriginSix\scripts\play-local.cmd`**. This launches the existing x64 Release
  output from the correct working directory with `127.127.127.127:44406`.
- Runtime `client/out/build/windows-x64/src/Release/config.ini` now uses that same
  loopback address. The tracked client template/source was not changed.

## Start, stop and status

Run with the same Windows user/machine that created the credentials:

```powershell
& D:\OriginSix\scripts\local-server.ps1 -Action Status
& D:\OriginSix\scripts\local-server.ps1 -Action Stop
& D:\OriginSix\scripts\local-server.ps1 -Action Start
& D:\OriginSix\scripts\test-local-server.ps1
```

Stop retains the database. Do not run `down -v`, delete the volumes or regenerate
passwords for this existing database. Start validates the Compose file and reuses
the existing image/data; it does not rebuild source. Keep `.local/credentials.clixml`:
it contains generated database/admin credentials encrypted with Windows DPAPI.
It is user/machine-bound, not a portable backup. Plaintext values are supplied to
Docker only at runtime; anyone with Docker-administrator access can inspect
container environment, so do not share full inspect/expanded Compose output.

## Configuration and source

- Compose: `scripts/compose.local.yml`, project **originsix-local**.
- Server source: `151e3af43c8935915482f324f2538e097f2d8a9c`.
- Client source: `063169014914add35a777178b15d8c9fbaae4b51`.
- Source image: `originsix-openmu:151e3af43c89`.
- Observed image ID:
  `sha256:16008b7a17fd04a9eaba2db4ae63b61c2c0e960ea37b0f313138fbe3b1021587`.
- PostgreSQL image pinned to:
  `postgres:16-alpine@sha256:721873c34ceb9f8d8fc265984940dc982404c105f19ad51be9fdc5970a6080ea`.
- Actual database server: **PostgreSQL 16.15**, Linux x86_64 musl.
- Dedicated containers: `originsix-local-database-1`, `originsix-local-openmu-1`.
- Dedicated volumes: `originsix-local_database-data`,
  `originsix-local_admin-keys`, `originsix-local_server-logs`.
- Dedicated network: `originsix-local_default`.
- DB name `openmu`; DB has no host-published port. Existing databases were not reused.
- `Database__AssumeExternallyProvisioned=true` built the schema in the new dedicated
  database without dropping/recreating it. No `-reinit` was used.

Source image built with the unmodified upstream Dockerfile:

```powershell
# Working directory: D:\OriginSix\server
docker build --file src/Startup/Dockerfile --tag originsix-openmu:151e3af43c89 src
```

## Observed verification

| Check | Result |
|---|---|
| Linux source image build/publish | Successful; build stage 42 warnings, 0 errors |
| Dedicated PostgreSQL readiness | Healthy; `SELECT current_database(), version()` confirmed `openmu`, PostgreSQL 16.15 |
| Database initialization | Log reports schema creation and initialization finished |
| All-in-one server | Healthy; three game servers, connect servers and chat started |
| Admin HTTP | HTTP 200 login page; root redirects to `/login` |
| Anonymous API access | `/api/status` returns 401, as intended |
| Host publications | Admin on 127.0.0.1:8080; game ports only on 127.127.127.127; DB not published |
| TCP reachability | 44406, 45901–45906 and 45980 reachable through alternate loopback |
| MuMain Hello / server list | Read-only socket probe passed; three game servers returned |
| Advertised endpoints | Server 0: 127.127.127.127:45902; Server 1: :45904; Server 2: :45906 |
| Client definitions | S6 EP3 English original `10404` and MuMain protocol discriminator 106 / `20404` present |
| EXP multiplier | 1.0 for all three servers; this alone does not prove vanilla progression |
| Character/stat reset features | Both inactive in persisted plugin configuration |
| Alternative shop currencies | Inactive in persisted plugin configuration |
| Bots | Plugin registered/active but configuration `Enabled=false`; bot population disabled |
| Admin password login | Not exercised by automated HTTP checks; interactive Blazor login is pending |
| Game login / creation / movement / combat | Not performed |
| Logout/restart persistence and reconnect | Not performed |

The first version of the socket probe did not consume the follow-up Hello that
`ServerInfoRequestHandler` sends after each connection-info response. That probe
failed on the second query; it was corrected to validate the Hello, then all
three advertised endpoints passed. No server source fix was required.

Docker mapping inspection confirmed no `0.0.0.0`/`::` host publications for this
project. Internally, upstream still listens on all container interfaces; host
loopback publication provides the local-only boundary. Original connect port
44405 exists inside the container but is not published.

## Remaining limitations

- Alpine emitted `Cannot load library libgssapi_krb5.so.2` during startup. Startup,
  database initialization and health checks subsequently succeeded. Kerberos/GSS
  authentication was not tested; the missing optional library was not patched.
- The default initializer creates three servers and test accounts. No gameplay
  rules or formulas were changed. Defaults and running services do not certify
  historical S6 EP3 fidelity; asset/license questions from the audit remain open.
- Credentials are protected at rest locally, but no database backup/restore test
  or persistent personal admin account has yet been established.
- Use the Windows client for the next smoke test: ordinary game account login,
  new level-1 character, movement, combat/loot/EXP, logout/relogin, then controlled
  restart persistence. Record observed results before promoting compatibility rows.
