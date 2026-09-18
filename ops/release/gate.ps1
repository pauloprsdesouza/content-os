#Requires -Version 5.1
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root
$failed = $false

function Step($name, [scriptblock]$action) {
    Write-Host "== $name"
    & $action
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAIL $name"
        $script:failed = $true
    }
}

Step "dotnet test" { dotnet test src/backend/ContentOS.sln --nologo -v q }
Step "contracts" { powershell -File .\ops\contracts\check.ps1 }
Step "studio typecheck" { npm --prefix src/studio exec -- tsc --noEmit -p tsconfig.app.json }

$secretHit = git grep -n -E "ghp_[A-Za-z0-9]|AKIA[0-9A-Z]{16}" -- ":!ops/release" 2>$null
if ($secretHit) {
    Write-Host "FAIL secret scan"
    $failed = $true
} else {
    Write-Host "PASS secret scan"
}

if ($failed) { exit 1 }

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "BLOCKED image build: docker is not installed."
    exit 2
}

Write-Host "PASS gate checks that can run on this machine."
