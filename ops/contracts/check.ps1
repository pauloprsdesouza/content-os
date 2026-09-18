#Requires -Version 5.1
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

$schemas = @(
    "contracts/events/schemas/ai-research-requested.schema.json",
    "contracts/events/schemas/ai-content-generate-requested.schema.json",
    "contracts/events/schemas/ai-content-review-requested.schema.json"
)
foreach ($path in $schemas) {
    $doc = Get-Content $path -Raw | ConvertFrom-Json
    foreach ($field in @("specversion", "id", "source", "type", "time", "subject", "correlationid", "causationid", "idempotencykey", "schemaversion", "data")) {
        if ($doc.required -notcontains $field) {
            Write-Error "FAIL $path missing required $field"
        }
    }
}

$async = Get-Content "contracts/events/asyncapi.yaml" -Raw
if ($async -notmatch "ai.research") { Write-Error "FAIL asyncapi missing ai.research" }

$openapi = Get-Content "contracts/http/openapi.json" -Raw
if ($openapi -match "Phase 0") { Write-Error "FAIL openapi is still the stub" }
if ($openapi -notmatch "/api/v1/dashboard/summary") { Write-Error "FAIL openapi missing dashboard" }

$api = if ($env:CONTENTOS_API_URL) { $env:CONTENTOS_API_URL.TrimEnd("/") } else { "http://localhost:5231" }
try {
    $live = Invoke-WebRequest -Uri "$api/openapi/v1.json" -UseBasicParsing
    $normalized = $live.Content -replace '"url"\s*:\s*"http://localhost:\d+/"', '"url": "/"'
    $committed = Get-Content "contracts/http/openapi.json" -Raw
    if (($normalized.Trim() -replace "`r`n", "`n") -ne ($committed.Trim() -replace "`r`n", "`n")) {
        Write-Error "FAIL openapi drift. Re-export from $api/openapi/v1.json and set servers.url to /."
    }
    Write-Host "PASS contracts (openapi drift checked)"
}
catch {
    if ($_.Exception.Message -like "FAIL*") { throw }
    Write-Host "PASS contracts (schemas). SKIP openapi drift: API not reachable."
}
