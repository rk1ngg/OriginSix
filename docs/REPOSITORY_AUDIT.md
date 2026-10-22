# Repository and environment audit

> Historical audit recorded before the one-repository OriginSix snapshot was
> created. The source identity and upstream URLs remain valid; the current Git
> layout is documented in `README.md` and `docs/UPSTREAM_STRATEGY.md`.

Inspected **2026-10-21** using read-only local commands and public GitHub metadata.
This is an initial static assessment, not a complete code/security audit.

## Repository identity and status

| | Server | Client |
|---|---|---|
| Location | `D:\OriginSix\server` | `D:\OriginSix\client` |
| Official remote `origin` | `https://github.com/MUnique/OpenMU.git` | `https://github.com/sven-n/MuMain.git` |
| Current/default branch | `master` | `main` |
| HEAD | `151e3af43c8935915482f324f2538e097f2d8a9c` | `063169014914add35a777178b15d8c9fbaae4b51` |
| Commit date | 2026-10-20 | 2026-10-21 |
| Commit subject | Merge pull request #1259 from MUnique/dependabot/npm_and_yarn/docs-website/launch-editor-2.14.2 | Merge pull request #811 from eduardosmaniotto/fix/semantic-release-e2big-commit-message |
| Working tree at inspection | Clean | Clean |
| HEAD vs local tracking branch | 0 ahead / 0 behind | 0 ahead / 0 behind |
| Live remote check | `master` HEAD equals local HEAD | `main` HEAD equals local HEAD |

Verified with `git status --short --branch`, `git remote -v`, `git log -1`,
`git symbolic-ref refs/remotes/origin/HEAD`, `git rev-list --left-right --count`
and `git ls-remote --symref origin HEAD`, each in its repository. No fetch, pull,
checkout, remote change, merge or source modification was performed. Official
references already exist as `origin`; an additional remote name is not necessary.

## Structure and architecture

### OpenMU

- `src/MUnique.OpenMU.sln`: solution; `src/Startup/`: all-in-one host.
- `GameLogic`, `GameServer`, `Network/Packets`: game actions, protocol handlers/
  remote views, and generated packet structures respectively.
- `DataModel`, `AttributeSystem`, `Persistence/{EntityFramework,Initialization,InMemory}`:
  configuration/entities, formulas, PostgreSQL and demo persistence.
- `src/Web/AdminPanel`, `src/Interfaces`, `src/PlugIns`, `src/Telemetry`:
  operations UI, subsystem interfaces, extension system, instrumentation.
- `deploy/`: all-in-one, Traefik, distributed options; `tests/`: NUnit-style test
  projects and benchmarks; `docs/`: mechanics/protocol; `docs-website/`: user guide.
- Windows/Linux/macOS source operation documented; Linux container build supplied.
  Some solution tools target `net10.0-windows`; Linux server support does not imply
  every desktop utility builds on Linux.

Read: root README, QuickStart, AGENTS, getting-started pages, architecture,
startup parameters, progress/master-system docs, Startup project/Dockerfile,
packet project, initialization and representative plugin code.

### MuMain

- Root CMake files/presets and `toolchain-*.cmake`; `src/source/`: native client;
  `ClientLibrary/`: .NET Native AOT networking and generated C++ bindings.
- `src/bin/{Data,fonts}`: runtime assets; `src/shaders/compiled`: checked-in shaders;
  `src/ThirdParty`: submodules; `tests`, `cmake`, `scripts`, `tools`, `docs`.
- Documented player targets: Windows x64/x86, Linux x64; macOS arm64 preset and
  hosted-build documentation also exist. Windows/Linux/macOS release CI is
  described upstream; no current CI run was executed or independently certified.
- MinGW Linux cross-build alone cannot create the Windows Native AOT DLL;
  it is not a complete connectable Windows client build.
- `CMakeLists.txt`: C++20, CMake >=3.25; `CMakePresets.json`: Ninja Multi-Config.
  Player preset disables editor/control socket. Configuration downloads/builds
  dependencies and initializes missing submodules; it is not read-only.

Read: README, AGENTS, build overview and Windows console/Visual Studio guides,
root CMake, presets, manifest, ClientLibrary project, asset/config metadata.

## Licenses and assets

- **OpenMU:** root [MIT license](../server/LICENSE), copyright MUnique. Preserve
  notices; dependency licenses remain applicable. This does not license Webzen assets.
- **MuMain:** no root project license was found. `git ls-files '*LICENSE*'
  '*License*' '*license*'` returned font notices only; GitHub repository metadata
  returned no SPDX license. README attributes Webzen, Louis and contributors and
  describes Season 5.2 ancestry. Do not infer an open-source license from public access.
- Font notices exist under `client/src/bin/fonts/` for Cousine, DejaVuSans,
  LiberationSans, NanumGothic and NotoSansTC. Full dependency/license review remains open.
- `client/src/bin/Data/` exists with 13,581 tracked paths, including maps, models,
  items and local data; fonts are present. This is availability evidence only.
- `git lfs ls-files` returned no entries for the client; `.gitattributes` protects
  binary MU formats against line-ending conversion. Do not edit/re-encode assets.
- Client data tree: `93013751f40aceec3553c3e5025ca104f52be0f0`;
  fonts tree: `149f928e2547aa6658977c3f41c63abccfa471ed` (from `git ls-tree HEAD`).
- Public release metadata confirmed `data-0538a8b934856486`, published 2026-10-20,
  archive `MuMain-data-0538a8b934856486.tar.gz` (457,568,571 bytes), API digest
  `sha256:6dbc3a21b83b4e8740744257723c04fd014b0e701c789d71736a3328b7274604`.
  No archive was downloaded. This is not an independently computed checksum or a license.
- Runtime releases are documented separately from data. The latest-release endpoint
  returned a **data** release; the first 100 releases queried contained no non-data
  entry. This does not prove no runtime release exists. Locate a specific official
  runtime/tag and its exact compatible-data link before choosing the binary route.
- Appropriately licensed complete game data has **not** been established. Resolve
  upstream permissions/asset provenance or obtain demonstrably authorized assets;
  do not substitute arbitrary third-party packs or distribute bundled data by assumption.

## Submodules and fetched dependencies

Server: `git submodule status` empty; no tracked `.gitmodules` found.

Client: all three pinned submodules are **uninitialized** (`-` status):

| Path | Pin | Official configured source |
|---|---|---|
| `src/ThirdParty/SDL` | `d9d5536704d585616d4db3c8ba3c4ff6fc2757e1` | `https://github.com/libsdl-org/SDL.git` |
| `src/ThirdParty/RmlUi` | `afc25fecb70d1eff7753b15fc801835a4c0e155f` | `https://github.com/nitoygo/RmlUi.git` |
| `src/ThirdParty/imgui` | `b1bcb12a624af7509894c8e77dd47416997777fa` | `https://github.com/ocornut/imgui.git` |

SDL/RmlUi are needed for player builds; ImGui for editor builds. Use recorded gitlink
pins, not the RmlUi submodule's branch tip. CMake also fetches GLM 1.0.1,
spdlog v1.17.0, pinned miniaudio, SDL_ttf and SDL_image; SDL_ttf vendors FreeType.
Windows vcpkg baseline is `e03dc9b29710050cd1018bc5674688108658d327`, with curl/SSL,
OpenSSL, directx-dxc (Windows x64), glslang tools and spirv-cross. Shader compilation
is OFF by default and checked-in blobs are used, but these remain manifest dependencies.

## Host prerequisites report

Observed Windows 11 Pro 64-bit, version `10.0.26200`, AMD Ryzen 7 7700,
33,408,004,096 bytes reported physical memory (~31.1 GiB usable / 32 GB class),
NVIDIA RTX 4060 and AMD integrated graphics. GPU runtime behavior is untested.

| Dependency | Observed state | Requirement / disposition |
|---|---|---|
| Git | Installed `2.54.0.windows.1` | Available; official remotes verified |
| Git LFS | Installed `3.7.1`; no client LFS entries | No LFS fetch indicated for this checkout |
| .NET SDK | Not on PATH; absent from standard Program Files locations | **Missing for native source builds**; both need SDK 10, not just runtime |
| PostgreSQL native tools | `psql`, `pg_isready` absent from PATH; no standard install/service detected | Native install unnecessary if dedicated Docker DB chosen |
| PostgreSQL containers | Existing unrelated PostgreSQL 16 / Supabase PostgreSQL 17 containers observed | Not an OriginSix DB; never repurpose implicitly |
| Docker engine/CLI | CLI and reachable engine `29.4.3`; Linux/x86_64 | Available; Windows helper service says Stopped but engine responds |
| Docker Compose | `v5.1.3` | Available; actual proposed configuration still needs validation |
| CMake | Not on PATH or standard `Program Files/CMake` path | Not detected; >=3.25 required for client source |
| Ninja | Not on PATH | Not detected; required by client presets |
| Visual Studio/MSVC/Windows SDK | `cl`, `msbuild` absent; `vswhere.exe` missing at standard installer path; no VS environment | Not detected; install/confirm C++ desktop workload and Windows SDK, plus Native AOT prerequisites |
| vcpkg | Not on PATH; `VCPKG_ROOT` unset; `C:\vcpkg` absent | Not detected; needed for Windows presets |
| Node.js/npm | `v24.15.0` / `11.12.1` | Meets documented Node >=16; build compatibility untested |
| Python | WindowsApps launcher found, interpreter not verified | Optional developer/test scripts; not a core player-build prerequisite |
| GitHub CLI | Found | Optional release/remote operations |
| Dapr, nginx, Traefik | Not audited as host installs | Not required for initial source all-in-one host; containers only if chosen |

Missing/not-detected software could exist in nonstandard locations; no whole-drive
search was performed. No installed version was proven incompatible. Missing .NET
and native tooling block host builds. Exact SDK patch, MSVC toolset, Windows SDK
and PostgreSQL major must be recorded when selected; no `global.json` was found
in the server scan. No software was installed or upgraded.

Node is relevant beyond documentation: `src/Network/Packets/*.csproj` invokes
`npx rimraf@3.0.2` and `npx xslt3` during non-CI generation. Builds can modify
tracked generated files and download/execute tools. Review diffs after each build;
never automatically revert them. Dockerfile uses `-p:ci=true` for builds/publish.

## Ports and operational limitations

- Sampled TCP listeners: **5433 already occupied** by unrelated PostgreSQL publishing
  on all interfaces. No listener observed on 80, 5432, 8080/8081, 44405/44406,
  45901–45906 or 45980 at inspection. Recheck immediately before runtime.
- Default new initialization: connect ports 44405 original / 44406 MuMain; three
  game servers have two version-specific endpoints each across 45901–45906;
  chat 45980. Existing DBs may retain legacy 559xx ports.
- Upstream Compose base publishes host-wide ports and unpinned images. Its automatic
  override adds 8081 and **5433**, which conflicts here. Do not run it unchanged.
- `Network/Listener.cs:64` binds `IPAddress.Any`; `-resolveIP:loopback` changes
  advertised addresses, not binding. Native startup is not automatically local-only.
- Startup launch profiles include `http://+:80` and a separate `-reinit` profile.
  Review/bypass profiles for controlled runs. Fresh system initialization sets
  `AutoStart` and `AutoUpdateSchema` true (`DataInitializationBase.cs:366–367`).
- No server/client build or test was attempted; no startup/login/gameplay success
  is claimed. Hardware capacity and production reliability remain unmeasured.
