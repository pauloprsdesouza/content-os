#Requires -Version 5.1
param(
    [string]$OutputDirectory = "ops/release/evidence"
)
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$target = Join-Path $OutputDirectory "contentos-$stamp.dump"
if (-not (Get-Command pg_dump -ErrorAction SilentlyContinue)) {
    Write-Error "BLOCKED: pg_dump is not on PATH. Install PostgreSQL client tools before a production backup."
}
if (-not $env:CONTENTOS_PLATFORM_CONNECTION) {
    Write-Error "BLOCKED: set CONTENTOS_PLATFORM_CONNECTION. This script does not print the connection string."
}
& pg_dump --format=custom --file=$target $env:CONTENTOS_PLATFORM_CONNECTION
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$hash = Get-FileHash $target -Algorithm SHA256
$meta = [pscustomobject]@{
    file = (Resolve-Path $target).Path
    sha256 = $hash.Hash
    bytes = (Get-Item $target).Length
    createdAt = (Get-Date).ToUniversalTime().ToString("o")
    retention = "Keep 7 daily and 4 weekly copies outside the container volume."
}
$meta | ConvertTo-Json | Set-Content "$target.json"
Write-Host "PASS backup $($hash.Hash)"
