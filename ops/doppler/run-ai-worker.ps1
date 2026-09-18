#Requires -Version 5.1
$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$workerRoot = Join-Path $repoRoot "src\ai-worker"
Set-Location $workerRoot

$project = if ($env:CONTENTOS_DOPPLER_PROJECT) { $env:CONTENTOS_DOPPLER_PROJECT } else { "content-os" }
$config = if ($env:CONTENTOS_DOPPLER_CONFIG) { $env:CONTENTOS_DOPPLER_CONFIG } else { "local" }

if (-not (Test-Path ".venv")) {
    python -m venv .venv
    & .\.venv\Scripts\python -m pip install -e .
}

Write-Host "AI worker via doppler -p $project -c $config"
doppler run -p $project -c $config -- .\.venv\Scripts\python -m ai_worker.bootstrap.app
