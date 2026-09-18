#Requires -Version 7.0
<#
.SYNOPSIS
  Ensure Doppler project content-os exists with local / dev / prd environments.
#>
$ErrorActionPreference = "Stop"

$project = "content-os"

Write-Host "Checking for project '$project'..."
$exists = $false
try {
    doppler projects get $project --json | Out-Null
    $exists = $true
} catch {
    $exists = $false
}

if (-not $exists) {
    Write-Host "Creating project $project..."
    doppler projects create $project --description "Content OS (API, Migrations, AI worker, Studio)"
} else {
    Write-Host "Project already exists."
}

foreach ($pair in @(
    @{ Name = "Local"; Slug = "local" },
    @{ Name = "Development"; Slug = "dev" },
    @{ Name = "Production"; Slug = "prd" }
)) {
    $slug = $pair.Slug
    $name = $pair.Name
    try {
        doppler environments get $slug --project $project --json | Out-Null
        Write-Host "Environment exists: $slug"
    } catch {
        Write-Host "Creating environment $slug ($name)..."
        doppler environments create $name $slug --project $project
    }
}

Write-Host @"

Project ready. Pin this repo:
  doppler setup --project content-os --config local

Configs: local (laptop→homelab), dev (homelab host), prd (Hostinger).

"@
