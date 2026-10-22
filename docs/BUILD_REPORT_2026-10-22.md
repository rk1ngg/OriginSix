# Windows local build and test report — 2026-10-22

Completed approximately 02:56 +08:00. This supplements the original static audit;
it does not replace its historical observations. User authorized dependency
downloads, required submodule initialization, builds and tests with “go”.

## Results

| Operation | Observed result |
|---|---|
| OpenMU NuGet restore | Successful |
| OpenMU Release solution build | Successful: 53 warnings, 0 errors; elapsed 1m 55s |
| OpenMU Release solution tests, without rebuilding | 3,693 passed, 11 skipped, 0 failed; 3,704 total |
| MuMain required submodules | SDL and RmlUi initialized at recorded gitlink pins |
| MuMain Windows x64 player configuration | Successful after session PATH and Python selection corrections |
| MuMain Release build, 8 parallel jobs | Successful; native Main and win-x64 Native AOT library produced |
| MuMain CTest Release | 976 passed, 1 skipped, 0 failed; 977 registered tests; elapsed 139.91s |
| Database initialization / game services / gameplay | Not performed |

Both repositories began clean. No manual source edits, gameplay changes,
commits, upstream branch changes, container creation or service startup occurred.
Unit tests exercised their own fixtures; no OriginSix PostgreSQL instance was created.

## Exact source revisions

- Server `master`: `151e3af43c8935915482f324f2538e097f2d8a9c`.
- Client `main`: `063169014914add35a777178b15d8c9fbaae4b51`.
- RmlUi: `afc25fecb70d1eff7753b15fc801835a4c0e155f`.
- SDL: `d9d5536704d585616d4db3c8ba3c4ff6fc2757e1`.
- ImGui remains uninitialized; the player build does not require it.

## Toolchain now verified

- Visual Studio Community 2026, installation version `18.11.12315.225`.
- MSVC compiler `19.51.36260`, x64; tool directory `14.51.36231`.
- Windows SDK `10.0.26100.0`.
- .NET SDK `10.0.402`.
- Visual Studio CMake `4.3.1-msvc1`, Ninja `1.13.2`.
- Visual Studio bundled vcpkg `2026-07-27-98d7cb0cf1f4686a3e43aa5672b6230c1d56bce8`.
- Python `3.14.2`, using vcpkg's downloaded embedded interpreter.

vcpkg downloaded its own CMake 4.4.0 and portable dependency-build utilities
(including Python, PowerShell, Perl, NASM, 7zip and jom). These are build-tool
caches, not new system-wide PATH configuration. Native libraries were installed
in `client/out/build/windows-x64/vcpkg_installed` using the repository manifest.
The host's .NET first run also reported creation of an ASP.NET development
certificate; no certificate trust command was run.

## Commands executed

Server working directory: `D:\OriginSix\server`:

```powershell
$env:PATH = 'C:\Program Files\dotnet;' + $env:PATH
dotnet restore src/MUnique.OpenMU.sln --verbosity minimal
dotnet build src/MUnique.OpenMU.sln -c Release --no-restore --verbosity minimal
dotnet test src/MUnique.OpenMU.sln -c Release --no-build --verbosity minimal
```

Client working directory: `D:\OriginSix\client`:

```powershell
git submodule update --init -- src/ThirdParty/SDL src/ThirdParty/RmlUi
$vs = 'C:\Program Files\Microsoft Visual Studio\18\Community'
Import-Module "$vs\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell -VsInstallPath $vs -SkipAutomaticLocation -DevCmdArguments '-arch=x64 -host_arch=x64'
$env:PATH = 'C:\Program Files\dotnet;' + $env:PATH
$env:VCPKG_ROOT = "$vs\VC\vcpkg"
$python = 'C:\Users\romki\AppData\Local\vcpkg\downloads\tools\python\python-3.14.2-x64-1\python.exe'
cmake --preset windows-x64 -DBUILD_TESTING=ON "-DPython3_EXECUTABLE=$python"
cmake --build --preset windows-x64-release --parallel 8
ctest --test-dir out/build/windows-x64 -C Release --output-on-failure
```

The Python cache path is specific to this PC; confirm it exists before reusing it.
No global PATH edits were made. Import the developer shell and set session
variables again in a new terminal if needed.

### Configuration failures resolved without source changes

1. Initial `cmake --preset windows-x64 -DBUILD_TESTING=ON` could not find dotnet
   in the inherited shell PATH. Added its installed directory to this session.
2. Next configuration restored the packet package but required a Python
   interpreter at `src/CMakeLists.txt:628`. Selected the interpreter vcpkg had
   already downloaded with `-DPython3_EXECUTABLE=...`.
3. Configuration then succeeded. Python is an actual requirement for the packet
   wire-size code generator in this revision, beyond optional developer scripts;
   the initial prerequisites audit understated that requirement.

## Test detail and limitations

| OpenMU test assembly | Passed | Skipped |
|---|---:|---:|
| ConnectServer | 8 | 0 |
| AttributeSystem | 49 | 0 |
| Pathfinding | 10 | 0 |
| Network.Packets | 642 | 0 |
| Network | 73 | 4 |
| PlugIns | 49 | 0 |
| ChatServer | 26 | 0 |
| Web | 281 | 0 |
| General Tests | 1,902 | 0 |
| Persistence.Initialization | 653 | 7 |

Skipped server cases include four network tests and seven persistence/backup
cases. PostgreSQL-backed initialization/loading and snapshot round trips remain
unverified. Tests reused the successful Release build with `--no-build`.

Client `editor_leak` and `control_socket_leak` checks passed. Cache confirms
`BUILD_TESTING=ON`, `ENABLE_EDITOR=OFF`, `ENABLE_CONTROL_SOCKET=OFF`.
`bitmap_gpu_upload_integration` was skipped: the fixture requires explicit
`MU_RUN_GPU_UPLOAD_TESTS=1`. No real GPU integration pass is claimed.
Build warnings include upstream/dependency diagnostics such as doctest's
`std::tuple` specialization warning; no warning-clean build is claimed.

## Outputs and generated-file review

Server startup artifact:
`D:\OriginSix\server\src\Startup\bin\Release\MUnique.OpenMU.Startup.dll`.

Client runtime directory:
`D:\OriginSix\client\out\build\windows-x64\src\Release`.
Inspected contents include Main.exe, MUnique.Client.Library.dll, libcrypto,
libcurl, zlib runtime DLLs, Data, fonts, shaders and seeded config.ini.
The full game executable was not launched; file presence and test success do not
prove a working login or rendered game session.

Client working tree remains clean. Server status marks
`src/Web/Map/wwwroot/js/MUnique.OpenMU.Web.Map.js` modified after the build,
but `git diff`/`--stat`/`--raw` show no content change. Git's filtered
`hash-object --path` equals the HEAD blob:
`684ace08e58fe88452b37747ba6df411706031ff`.
Git reports an LF/CRLF conversion warning. The generated file was left in place;
no checkout/reset/staging was used to clear the status.

## Next approval gate

Prepare a standalone loopback-only Compose configuration and dedicated OriginSix
PostgreSQL volume; then approve source-image build, database initialization and
service startup. Existing unrelated databases and occupied host port 5433 must
not be reused. Client/server smoke tests and vanilla acceptance remain pending.
See [LOCAL_SETUP.md](LOCAL_SETUP.md) and [VANILLA_COMPATIBILITY.md](VANILLA_COMPATIBILITY.md).
