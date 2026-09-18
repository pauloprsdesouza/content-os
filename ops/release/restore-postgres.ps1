#Requires -Version 5.1
param(
    [Parameter(Mandatory = $true)]
    [string]$DumpFile,
    [Parameter(Mandatory = $true)]
    [string]$TargetDatabase
)
$ErrorActionPreference = "Stop"
if ($TargetDatabase -match '^(contentos|postgres)$') {
    Write-Error "BLOCKED: restore only into a disposable database name, never contentos or postgres."
}
if (-not (Test-Path $DumpFile)) { Write-Error "BLOCKED: dump not found." }
$meta = "$DumpFile.json"
if (Test-Path $meta) {
    $expected = (Get-Content $meta -Raw | ConvertFrom-Json).sha256
    $actual = (Get-FileHash $DumpFile -Algorithm SHA256).Hash
    if ($expected -ne $actual) { Write-Error "FAIL checksum mismatch." }
}
if (-not $env:CONTENTOS_PLATFORM_CONNECTION) {
    Write-Error "BLOCKED: set CONTENTOS_PLATFORM_CONNECTION without printing it."
}
Write-Host "Restore drill target=$TargetDatabase. Create that database first, then:"
Write-Host "pg_restore --dbname=<disposable> --no-owner $DumpFile"
Write-Host "PASS restore procedure is refuse-by-default for the live database."
