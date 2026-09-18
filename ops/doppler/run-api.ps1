#Requires -Version 5.1
$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $repoRoot

$project = if ($env:CONTENTOS_DOPPLER_PROJECT) { $env:CONTENTOS_DOPPLER_PROJECT } else { "epilogik-platform" }
$config = if ($env:CONTENTOS_DOPPLER_CONFIG) { $env:CONTENTOS_DOPPLER_CONFIG } else { "local_contentos" }

Write-Host "API via doppler -p $project -c $config (default http://localhost:5231)"
doppler run -p $project -c $config -- dotnet run --project src/backend/ContentOS.Api --launch-profile http
