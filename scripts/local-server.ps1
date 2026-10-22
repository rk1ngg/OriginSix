# Credentials are encrypted by Windows DPAPI for the current user and machine.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Start', 'Stop', 'Status', 'CopyAdminPassword')]
    [string]$Action
)

$ErrorActionPreference = 'Stop'
$workspace = Split-Path -Parent $PSScriptRoot
$stateDirectory = Join-Path $workspace '.local'
$credentialPath = Join-Path $stateDirectory 'credentials.clixml'
$composePath = Join-Path $PSScriptRoot 'compose.local.yml'

if (-not (Test-Path -LiteralPath $credentialPath)) {
    if ($Action -ne 'Start') {
        throw 'No local credentials exist. Run Start after approving database creation.'
    }

    $existingVolumes = & docker volume ls --filter 'label=com.docker.compose.project=originsix-local' --format '{{.Name}}'
    if ($LASTEXITCODE -ne 0) { throw 'Cannot check existing OriginSix volumes.' }
    if ($existingVolumes -contains 'originsix-local_database-data') {
        throw 'The database volume exists but credentials are missing. Restore the original credentials; do not regenerate them.'
    }

    if (-not (Test-Path -LiteralPath $stateDirectory)) {
        New-Item -ItemType Directory -Path $stateDirectory | Out-Null
    }

    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $passwords = @()
        foreach ($name in @('postgres', 'originsix-admin')) {
            $bytes = New-Object byte[] 32
            $rng.GetBytes($bytes)
            $secure = ConvertTo-SecureString ([Convert]::ToBase64String($bytes)) -AsPlainText -Force
            $passwords += [System.Management.Automation.PSCredential]::new($name, $secure)
        }
        [pscustomobject]@{ Database = $passwords[0]; Admin = $passwords[1] } |
            Export-Clixml -LiteralPath $credentialPath -NoClobber
    }
    finally {
        $rng.Dispose()
    }
}

$credentials = Import-Clixml -LiteralPath $credentialPath
if ($credentials.Database -isnot [System.Management.Automation.PSCredential] -or
    $credentials.Admin -isnot [System.Management.Automation.PSCredential]) {
    throw 'Unexpected credential format. Do not recreate credentials for an existing database.'
}

if ($Action -eq 'CopyAdminPassword') {
    Set-Clipboard -Value $credentials.Admin.GetNetworkCredential().Password
    'Local admin password copied to clipboard. Paste into the local login page, not chat.'
    return
}

$previousDatabasePassword = $env:DB_ADMIN_PW
$previousAdminPassword = $env:ORIGINSIX_ADMIN_PASSWORD
try {
    $env:DB_ADMIN_PW = $credentials.Database.GetNetworkCredential().Password
    $env:ORIGINSIX_ADMIN_PASSWORD = $credentials.Admin.GetNetworkCredential().Password
    $composeArgs = @('compose', '--project-name', 'originsix-local', '--file', $composePath)

    & docker @composeArgs config --quiet
    if ($LASTEXITCODE -ne 0) { throw 'Compose validation failed.' }

    switch ($Action) {
        'Start' { & docker @composeArgs up -d --no-build }
        'Stop' { & docker @composeArgs stop }
        'Status' { & docker @composeArgs ps }
    }
    if ($LASTEXITCODE -ne 0) { throw "Local server $Action failed." }
}
finally {
    $env:DB_ADMIN_PW = $previousDatabasePassword
    $env:ORIGINSIX_ADMIN_PASSWORD = $previousAdminPassword
}
