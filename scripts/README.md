# Development automation

Local runtime creation/startup was approved and performed on 2026-10-22.
See [the runtime guide](../docs/LOCAL_RUNTIME_2026-10-22.md) for verified results.

- `local-server.ps1 -Action Start|Stop|Status`: manage only `originsix-local`.
- `local-server.ps1 -Action CopyAdminPassword`: copy the local admin password
  privately to the Windows clipboard; never paste it into chat.
- `test-local-server.ps1`: read-only Hello/server-list/endpoint protocol check.
- `play-local.cmd`: launch the built Windows client with the local connection.
- `compose.local.yml`: dedicated PostgreSQL and OpenMU with loopback-only host ports.

Keep `.local/credentials.clixml` and database volumes. Credentials are encrypted
for the current Windows user/machine. Never commit secrets, share expanded
Compose/container environment output, or use `down -v` as routine shutdown.
