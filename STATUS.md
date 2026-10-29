# DataForge — Status

Single source of truth for progress. Update it at the end of every work session.
Last updated: 2026-10-29.

## Verified

| Area | Evidence | Status |
|---|---|---|
| Build (.NET 10, warnings as errors, analyzers `latest-recommended`) | `dotnet build DataForge.slnx`: 0 warnings, 0 errors | Done |
| Unit tests: 101 across building blocks and 4 modules | `dotnet test --solution DataForge.slnx` | Passing |
| Architecture tests: 29 rules (module boundaries, layer direction, naming suffixes, one artifact kind per folder, sealed classes, Dapper only in Infrastructure) | `tests/DataForge.ArchitectureTests` | Passing |
| Integration tests: 17 against PostgreSQL 17 (Testcontainers), covering migrations, the full pipeline, 25k-row batching, resume idempotency, SKIP LOCKED concurrency, lease fencing, CSRF, tenant isolation and no internal ids in the API | `tests/DataForge.IntegrationTests` | Passing |
| Manual API smoke test: login, project, 7 rules, upload, import, automatic validation, 10 expected issues | Run on 2026-10-29 against the local web and worker hosts | Passing |
| End-to-end tests: 7 Playwright scenarios (E2E-001 sign-in and access control, E2E-002 tenant isolation, E2E-003/004/006 onboarding to validation results) | `tests/DataForge.E2ETests` against Docker Compose; screenshots and traces in `docs/evidence/e2e/` | Passing |
| Docker Compose stack: PostgreSQL, migrator with demo seed, web, worker | `deploy/compose/docker-compose.yml` | Running |
| k6 baseline, 10 virtual users for 1 minute: 266,018 requests, 4,860 req/s, p95 3.11 ms, 0% errors | `docs/performance/baseline-2026-10-29.md` | Measured |
| Release build and full suite: 154 tests | `dotnet test --solution DataForge.slnx -c Release` | Passing |

Bugs the tests caught and that are now fixed:
- **Numeric rule:** "1,5" was read as 15 because thousands separators were accepted.
- **Job queue clock:** `run_after` used the application clock while claiming used the database clock, so clock skew delayed jobs. The queue now uses only the database clock.
- **MVC antiforgery filter:** it required the ViewFeatures services, which were not registered.
- **Exception handler:** it evaluated the request path after it had been rewritten to `/error`.

## MVP flow (§19): Login → Project → Upload → Process → Validate → Errors → Results

| Step | Implemented | Automated test |
|---|---|---|
| Cookie login, logout, access denied, login rate limit | Yes | Unit and integration (API) |
| Projects | Yes | Integration |
| Streaming upload: SHA-256, size limit, idempotent per project | Yes | Unit and integration |
| Import worker: Sep CSV with auto-detected delimiter, bounded channel, binary COPY, checkpoints | Yes | Integration (including resume) |
| Rules (Strategy): Required, MaxLength, Pattern with ReDoS timeout, Email, Numeric, Date, AllowedValues, Unique (SQL) | Yes | Unit and integration |
| Automatic validation after import (integration event) and manual revalidation | Yes | Integration |
| Results: quality strip, per-rule stats, keyset-paginated issues | Yes | Integration and E2E-004 |
| Implementation console (`/operations`, engineer only) | Basic | Integration (API authorization) |

## Next (in order)

1. **Publish (needs owner approval):** create the remote repository `pablofreire/DataForge`, push, and confirm that the CI workflows are green. The CI is written but not yet proven.
2. Load profiles at 50, 100, 250 and 500 users, plus import/validation throughput (rows/s) for 1M rows, using the dataset generator CLI and BenchmarkDotNet.
3. UI flows for E2E-005 (correction and revalidation), E2E-007 (cancel and resume) and E2E-008 (export), plus E2E-010 (responsive).
4. Observability compose profile: OpenTelemetry Collector, Prometheus and Grafana dashboards.

## Backlog (Phase 3, not part of the MVP)

- Correction, revalidation and export UI flows (E2E-005, 007, 008).
- Full implementation console (E2E-009) and responsive E2E (E2E-010).
- Dataset generator CLI and BenchmarkDotNet suite (COPY vs INSERT, batch sizes, 1M+ rows).
- OpenTelemetry Collector, Prometheus and Grafana compose profile, plus an audit log module.
- PostgreSQL row-level security as defence in depth for tenant isolation.
- Azure deployment (Bicep): Container Apps, Azure Database for PostgreSQL, Blob Storage for `IFileStorage`.
- Transactional outbox when modules split into separate deployables.

## Local development quick reference

```bash
docker run -d --name dataforge-dev-pg -e POSTGRES_USER=dataforge -e POSTGRES_PASSWORD=dataforge_dev -e POSTGRES_DB=dataforge -p 55432:5432 postgres:17-alpine
export Database__ConnectionString="Host=localhost;Port=55432;Database=dataforge;Username=dataforge;Password=dataforge_dev"
DemoSeed__Password='<12+ chars>' dotnet run --project src/Hosts/DataForge.Migrator -- --seed-demo
dotnet run --project src/Hosts/DataForge.Web --launch-profile http     # http://localhost:5080
dotnet run --project src/Hosts/DataForge.Worker
dotnet test --solution DataForge.slnx                                  # needs Docker (Testcontainers)
```
