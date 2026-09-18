#Requires -Version 5.1
$ErrorActionPreference = "Stop"
Set-Location (Resolve-Path (Join-Path $PSScriptRoot "..\.."))

$status = git status --porcelain
if ($status) {
    Write-Error "BLOCKED: git tree is dirty."
}

$names = doppler secrets --project content-os --config prd --only-names
$blocked = $false
foreach ($line in $names) {
    if ($line -notmatch '^[A-Z0-9_]+$') { continue }
    $value = doppler secrets get $line --project content-os --config prd --plain
    if ($value -match 'REPLACE_ME') {
        Write-Host "FAIL $line contains placeholder"
        $blocked = $true
    }
}

if ($blocked) {
    Write-Error "BLOCKED: prd still has REPLACE_ME values."
}

Write-Host "PASS prd placeholder scan (values not printed)."
