#!/usr/bin/env pwsh
# End-to-end gate: start the Docker Compose stack, wait for readiness, run Playwright and keep the evidence
# (screenshots and traces) in docs/evidence/e2e.
param(
    [string] $BaseUrl = 'http://localhost:8080',
    [switch] $KeepRunning
)
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
$compose = @('compose', '-f', 'deploy/compose/docker-compose.yml')

Write-Host '==> Starting the stack' -ForegroundColor Cyan
docker @compose up --build -d
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "==> Waiting for $BaseUrl/health/ready" -ForegroundColor Cyan
$deadline = (Get-Date).AddMinutes(3)
do {
    try { $ready = (Invoke-WebRequest "$BaseUrl/health/ready" -UseBasicParsing -TimeoutSec 5).StatusCode -eq 200 } catch { $ready = $false }
    if (-not $ready) { Start-Sleep -Seconds 2 }
} until ($ready -or (Get-Date) -gt $deadline)
if (-not $ready) { throw 'The application did not become ready in time.' }

Write-Host '==> Playwright' -ForegroundColor Cyan
dotnet build tests/DataForge.E2ETests -c Release
& ./tests/DataForge.E2ETests/bin/Release/net10.0/playwright.ps1 install chromium
$env:DATAFORGE_BASE_URL = $BaseUrl
dotnet test --project tests/DataForge.E2ETests -c Release --no-build
$exitCode = $LASTEXITCODE

if (-not $KeepRunning) { docker @compose down }
exit $exitCode
