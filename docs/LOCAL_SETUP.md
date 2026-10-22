# Local setup and verification procedure

Status: **planned, not executed**, 2026-10-21. See
[REPOSITORY_AUDIT.md](REPOSITORY_AUDIT.md) for installed/missing dependencies.
Commands below are reviewed against the checked-out files, not proven by execution.
Run each from its stated working directory; do not clone again.

## 1. Approve the dependency and build scope

For Windows source development, approve installation of .NET SDK 10 and, for
MuMain source builds, Visual Studio C++ desktop build tools + Windows SDK +
CMake >=3.25 + Ninja + vcpkg. Set `VCPKG_ROOT` in the build session rather than
changing system-wide PATH unnecessarily. Record exact versions after installation.
Node/npm and Docker already exist; no upgrade is indicated.

A source-built Linux server container can use the SDK in the upstream Dockerfile
without a host SDK installation. It does not replace the Windows Native AOT
toolchain required for a locally built Windows client. A verified official runtime
release may avoid client build prerequisites once its tag and rights are resolved.

After approval, validate `dotnet --info`, `dotnet --list-sdks`, `cmake --version`,
`ninja --version`, and the compiler from an x64 VS Developer PowerShell. Confirm
the vcpkg toolchain path, Windows SDK and C++ workload. Do not install every
optional dependency listed by the repositories.

## 2. Build OpenMU without starting services

Working directory: `D:\OriginSix\server`. After approval to restore/build and
execute repository generators:

```powershell
dotnet restore src/MUnique.OpenMU.sln
dotnet build src/MUnique.OpenMU.sln -c Release --no-restore
dotnet test src/MUnique.OpenMU.sln -c Release --no-build
git status --short
git diff --stat
```

Use explicit solution paths: upstream's bare `dotnet restore` example is ambiguous
from the root, where the solution is not located. Run the build and tests sequentially;
stop after a failing command. Build once, then test the matching configuration.
Report individual failures and generated changes without reverting user files.
Windows-targeted solution utilities may require additional targeting components.

For the proposed container runtime, the source build command (also approval-gated)
from the same directory is:

```powershell
docker build --file src/Startup/Dockerfile --tag originsix-openmu:151e3af43c89 src
```

This uses the upstream Dockerfile and its Linux .NET 10 SDK/runtime. It downloads
and executes build dependencies and creates an image, but does not start a service.
Record resolved base image digests; tags alone are mutable. Container build success
would not prove native solution/test success, nor vice versa.

## 3. Approve an isolated database and runtime configuration

Before creation/startup, prepare a **new parent-workspace Compose file** for review;
do not alter upstream Compose files or reuse their existing volumes/container names.
It should have:

- Unique project `originsix-local`, dedicated PostgreSQL service/volume, and source-built
  `originsix-openmu:151e3af43c89` app image; no existing databases as targets.
- An explicitly selected PostgreSQL major/tag and resolved digest. Upstream says
  `postgres` without a version; **16-alpine is a candidate**, not a verified support
  guarantee. Validate migrations/tests before accepting the chosen major.
- App DB hostname `database` on an internal Compose network; database `openmu`;
  matching admin credentials on both services using local non-committed input.
  Do not print expanded Compose environment/connection strings containing credentials.
- No published database port needed for app-to-DB traffic. If host debugging later
  requires one, explicitly choose a free loopback port; **5433 is occupied**.
- Only explicit loopback mappings for 44406, needed 45901–45906 endpoints, 45980,
  and admin `127.0.0.1:8080:8080`. Bind game ports on `127.127.127.127` if the
  client requires that advertised address; confirm Docker Desktop accepts the bind.
  Do not substitute `0.0.0.0` on failure.
- `RESOLVE_IP=loopback`, `ASPNETCORE_URLS=http://+:8080` **inside the container**,
  DB readiness healthcheck/dependency, persisted admin key ring and logs as needed.
  Container-wide listening is isolated by loopback-only host publication.
- Admin bootstrap credentials handled privately; review first-run behavior before
  starting, since upstream can initially permit admin access without an account.

Do not run `docker compose up` in upstream `deploy/all-in-one` unchanged: automatic
overrides add occupied 5433, base ports publish on all interfaces, and images float.
Merely appending loopback ports in an override can retain broad mappings; prefer a
standalone reviewed file and inspect the effective port bindings.

After configuration approval and creation, from `D:\OriginSix`, proposed commands:

```powershell
docker compose --project-name originsix-local --file scripts/compose.local.yml config --quiet
docker compose --project-name originsix-local --file scripts/compose.local.yml up -d
docker compose --project-name originsix-local --file scripts/compose.local.yml ps
```

`scripts/compose.local.yml` **does not exist yet**. These are future commands, not
an immediately runnable setup. Configuration/startup requires separate approval.
Capture sanitized startup logs and actual listener bindings. Open the proposed
admin endpoint `http://127.0.0.1:8080/`, confirm Season 6 initialization, correct
client definitions and enabled services. Review effective plugin defaults and
test-account option. Verify resets/alternative currencies/bots/custom extensions
remain off and EXP/drop/shop/quest configuration before calling the baseline vanilla.

OpenMU initializes schemas/roles and may create/recreate the DB during first setup;
it can also auto-update schema. Restrict credentials to this disposable dedicated
instance. Never use `-reinit` against an existing database. Demo mode is documented
upstream but is not persistence testing and still needs service-start approval.

Native alternative (not the recommended first runtime here): upstream uses
`dotnet run --project src/Startup/MUnique.OpenMU.Startup.csproj -- -autostart`.
Before considering it, resolve all-interface game listener binding, bypass unsafe
launch profiles with `--no-launch-profile`, and explicitly review DB/admin settings.
Setting `-resolveIP:loopback` alone does not isolate the process.

## 4. Prepare the Windows x64 client

First resolve source/runtime and asset permissions. The checkout already contains
data/fonts; do not download another asset pack blindly or claim Webzen assets are
MIT licensed. Retain notices. Record asset trees and any approved replacements.

Working directory: `D:\OriginSix\client`, x64 VS Developer PowerShell. After
approval to initialize pinned submodules and build/download dependencies:

```powershell
git submodule update --init -- src/ThirdParty/SDL src/ThirdParty/RmlUi
cmake --preset windows-x64 -DBUILD_TESTING=ON
cmake --build --preset windows-x64-release
ctest --test-dir out/build/windows-x64 -C Release --output-on-failure
git status --short
git diff --stat
```

No editor/ImGui or control socket is needed for the initial player build. CMake
stages assets and builds `MUnique.Client.Library.dll`; confirm the DLL and required
native runtime DLLs, shaders, `Data/`, `fonts/`, and config exist beside `Main.exe`
in `out/build/windows-x64/src/Release`. Compilation without the AOT DLL cannot
establish a playable milestone. Build generators can change tracked bindings.

For a published runtime, locate its exact official tag, source commit, Windows x64
archive and matching **Compatible game data** release. Check SHA256 against the
publisher's checksum/API digest before extraction into a new directory. Do not
select `/releases/latest` blindly: it returned a data-only release in this audit.
No specific compatible runtime/data pair has been verified yet.

Keep the existing `RmlTheme=legacy` and `Look=Classic`; player builds disable
editor/control socket. Do not turn on enhanced visuals or custom content. Existing
upstream rendering/UI differences remain a fidelity limitation.

## 5. Connect and test — execution approval required

From the client executable's output directory, after server readiness and approval:

```powershell
.\Main.exe connect /u127.127.127.127 /p44406
```

Alternatively use `[CONNECTION SETTINGS]` in a separately prepared runtime
`config.ini` with `ServerIP=127.127.127.127`, `ServerPort=44406` (approval before
overwriting an existing config). Template defaults are `localhost:44406`.
Server docs warn original clients reject `127.0.0.1`; MuMain's current behavior
on that address was not confirmed. The explicit documented alternate loopback
avoids relying on that discrepancy. Ensure returned game-server endpoints resolve
to the same reachable loopback mapping; connect-screen success is not login success.

Record date, server/client commit, image digest, assets, configuration and expected/
actual results for each:

| Check | Required evidence | Current result |
|---|---|---|
| Build and unit tests | Exact command, exit status, sanitized log | Not run |
| DB bootstrap | Roles/schemas initialized in dedicated DB, no migration errors | Not run |
| Services | Admin + MuMain connect + game endpoint + chat readiness/listeners | Not run |
| Login | Account can log in; invalid credentials rejected | Not run |
| Character creation | New ordinary level-1 character persists; class rules checked | Not run |
| Movement | Walk, collision/safe zone, map transition | Not run |
| Basic combat | Melee/skill, mana, monster death, EXP, loot | Not run |
| Persistence | Logout/login and controlled server/DB restart preserve progress | Not run |
| Reconnection | Disconnect/reconnect, no duplicate session/items or lost state | Not run |

Optional isolated test accounts are described in
`server/docs-website/docs/getting-started/test-accounts.md`; enabling them is a DB
initialization choice, not a substitute for fresh ordinary-account testing. Avoid
logging credentials. Shutdown only the OriginSix project; retain its volume.
Never use `down -v` as routine cleanup. Back up the dedicated DB before any later
migration or approved upstream patch.
