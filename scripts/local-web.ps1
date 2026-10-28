# Launches the local MU OriginSix Web CMS on loopback (http://127.0.0.1:3080)
# using DPAPI-encrypted credentials from .local/credentials.clixml without writing a plaintext .env file.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('Setup', 'Dev', 'Build', 'Start')]
    [string]$Action = 'Dev',

    [int]$Port = 3080
)

$ErrorActionPreference = 'Stop'
$workspace = Split-Path -Parent $PSScriptRoot
$webDir = Join-Path $workspace 'web'
$stateDirectory = Join-Path $workspace '.local'
$credentialPath = Join-Path $stateDirectory 'credentials.clixml'
$webSecretPath = Join-Path $stateDirectory 'web-secret.clixml'

if (-not (Test-Path -LiteralPath $credentialPath)) {
    throw 'No local credentials exist in .local/credentials.clixml. Run scripts/local-server.ps1 -Action Start first.'
}

$credentials = Import-Clixml -LiteralPath $credentialPath
if ($credentials.Database -isnot [System.Management.Automation.PSCredential]) {
    throw 'Unexpected credential format in .local/credentials.clixml.'
}

if (-not (Test-Path -LiteralPath $webSecretPath)) {
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $bytes = New-Object byte[] 32
        $rng.GetBytes($bytes)
        $secure = ConvertTo-SecureString ([Convert]::ToBase64String($bytes)) -AsPlainText -Force
        [System.Management.Automation.PSCredential]::new('nextauth', $secure) |
            Export-Clixml -LiteralPath $webSecretPath -NoClobber
    }
    finally {
        $rng.Dispose()
    }
}

$webSecret = Import-Clixml -LiteralPath $webSecretPath
$dbPassword = [System.Uri]::EscapeDataString($credentials.Database.GetNetworkCredential().Password)

$prevDbUrl = $env:DATABASE_URL
$prevNextAuthSecret = $env:NEXTAUTH_SECRET
$prevNextAuthUrl = $env:NEXTAUTH_URL
$prevNextPublicUrl = $env:NEXT_PUBLIC_URL
$prevGameServerUrl = $env:GAMESERVER_URL

try {
    $env:DATABASE_URL = "postgresql://postgres:$dbPassword@127.0.0.1:5434/openmu"
    $env:NEXTAUTH_SECRET = $webSecret.GetNetworkCredential().Password
    $env:NEXTAUTH_URL = "http://127.0.0.1:$Port"
    $env:NEXT_PUBLIC_URL = "http://127.0.0.1:$Port"
    $env:GAMESERVER_URL = "http://127.0.0.1:8080"

    Push-Location $webDir
    try {
        if (-not (Test-Path -LiteralPath (Join-Path $webDir 'node_modules'))) {
            Write-Host 'Installing web dependencies (npm ci --legacy-peer-deps)...'
            & npm ci --legacy-peer-deps
            if ($LASTEXITCODE -ne 0) { throw 'npm ci failed.' }
        }

        Write-Host 'Generating Prisma client...'
        & npx prisma generate
        if ($LASTEXITCODE -ne 0) { throw 'npx prisma generate failed.' }

        switch ($Action) {
            'Setup' {
                Write-Host 'Web dependencies and Prisma client are ready.'
            }
            'Build' {
                & npm run build
                if ($LASTEXITCODE -ne 0) { throw 'npm run build failed.' }
            }
            'Dev' {
                & npx next dev -H 127.0.0.1 -p $Port
            }
            'Start' {
                & npx next start -H 127.0.0.1 -p $Port
            }
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    $env:DATABASE_URL = $prevDbUrl
    $env:NEXTAUTH_SECRET = $prevNextAuthSecret
    $env:NEXTAUTH_URL = $prevNextAuthUrl
    $env:NEXT_PUBLIC_URL = $prevNextPublicUrl
    $env:GAMESERVER_URL = $prevGameServerUrl
}
