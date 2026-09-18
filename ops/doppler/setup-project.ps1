#Requires -Version 7.0
<#
.SYNOPSIS
  Create dedicated Doppler project content-os when workplace has a free project slot.
#>
$ErrorActionPreference = "Stop"

$project = "content-os"
$envs = @(
    @{ Name = "Local"; Slug = "local" },
    @{ Name = "Development"; Slug = "dev" },
    @{ Name = "Production"; Slug = "prd" }
)

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

foreach ($e in $envs) {
    $slug = $e.Slug
    $name = $e.Name
    $envExists = $false
    try {
        doppler environments get $slug --project $project --json | Out-Null
        $envExists = $true
    } catch {
        $envExists = $false
    }
    if (-not $envExists) {
        Write-Host "Creating environment $slug ($name)..."
        doppler environments create $slug --name $name --project $project
    } else {
        Write-Host "Environment exists: $slug"
    }
}

Write-Host @"

Next:
  1. Copy secrets from epilogik-platform configs local_contentos / dev_contentos / prd_contentos
     into content-os configs local / dev / prd (dashboard or doppler secrets download/set).
  2. Update doppler.yaml setup.project to content-os and configs to local|dev|prd.
  3. Optionally delete the *_contentos branch configs after verifying.

"@
