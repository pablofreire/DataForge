#!/usr/bin/env pwsh
# Local quality gate: warning-free build, then unit, architecture and integration tests.
# Integration tests start PostgreSQL with Testcontainers, so Docker must be running.
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

Write-Host '==> Build (warnings are errors)' -ForegroundColor Cyan
dotnet build DataForge.slnx -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$suites = @(
    'tests/BuildingBlocks/DataForge.BuildingBlocks.UnitTests',
    'tests/Modules/Identity/DataForge.Modules.Identity.UnitTests',
    'tests/Modules/Onboarding/DataForge.Modules.Onboarding.UnitTests',
    'tests/Modules/Validation/DataForge.Modules.Validation.UnitTests',
    'tests/Modules/Processing/DataForge.Modules.Processing.UnitTests',
    'tests/DataForge.ArchitectureTests',
    'tests/DataForge.IntegrationTests'
)

foreach ($suite in $suites) {
    Write-Host "==> $suite" -ForegroundColor Cyan
    dotnet test --project $suite -c Release --no-build
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host 'All gates passed.' -ForegroundColor Green
