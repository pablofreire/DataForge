# Requirements mapping: Senior Software Engineer IV (SaaS implementation tooling)

This document maps each requirement of the role to concrete evidence in this repository.

Status values:
- **Implemented:** the code exists.
- **Validated:** an automated test or a measurement proves it.
- **Planned:** in the backlog in [STATUS.md](../../STATUS.md); no claim is made yet.

| Requirement | Evidence | Status |
|---|---|---|
| C# / .NET | .NET 10 solution with warnings as errors and analyzers at `latest-recommended`: [`Directory.Build.props`](../../Directory.Build.props) | Validated (clean build) |
| Internal tooling for SaaS implementation | Implementation console [`OperationsPage.razor`](../../src/Modules/Processing/DataForge.Modules.Processing.Presentation/Pages/OperationsPage.razor), restricted to engineers | Implemented; access control validated (integration and E2E) |
| Data validation tools for customers | Rule engine using the Strategy pattern [`Engine/`](../../src/Modules/Validation/DataForge.Modules.Validation.Domain/Engine), results page with quality strip and issue grid | Validated (unit, integration, E2E-004) |
| Very large datasets | Constant-memory streaming upload, bounded channels, binary COPY, keyset pagination, checkpoints ([ADR-0005](../adr/0005-streaming-pipeline-copy-and-channels.md)) | Validated up to 25k rows in tests. 1M+ is planned. **100M is not claimed** |
| Scalable systems | Stateless workers scale horizontally through `FOR UPDATE SKIP LOCKED` ([ADR-0003](../adr/0003-postgres-job-queue.md)) | Validated: 8 concurrent claimers, every job claimed exactly once (`JobQueueTests`) |
| Query and data-access optimization | Dapper with SQL kept in `*Queries` classes, partial and composite indexes, row-value keyset pagination, set-based uniqueness, per-rule stats precomputed when a run completes | Implemented. BenchmarkDotNet suite planned |
| ASP.NET MVC | REST API built on MVC controllers with `BaseApiController` and the `ApiResponse<T>` envelope, plus the MVC antiforgery filter | Validated (integration tests over HTTP) |
| Blazor | Customer portal and console as Blazor Web App (Interactive Server) pages inside module Razor Class Libraries | Validated (Playwright E2E) |
| APIs and distributed services | Separate Web, Worker and Migrator processes. Leases, fencing, retries, graceful release on shutdown and idempotent enqueue | Validated (unit and integration) |
| Architecture good practices | Modular monolith, layered module projects, architecture tests, ADRs | Validated (29 architecture tests) |
| Diagnosing performance problems | Real bugs found and fixed through tests: clock skew between the app and the database queue, `"1,5"` parsed as 15, a missing MVC filter registration, the exception path rewritten to `/error`. k6 baseline with documented caveats | Validated |
| Observability | Serilog structured JSON, correlation id, OpenTelemetry traces and metrics (ASP.NET Core, Npgsql, runtime, `DataForge.Processing` meter), `/health/live` and `/health/ready` | Implemented. Grafana dashboards planned |
| Security | BCrypt, login rate limiting, user-enumeration timing equalization, CSRF protection, tenant isolation, ReDoS timeout on customer regexes, path-traversal-safe storage, no internal ids exposed, no secrets in the repository | Validated (unit and integration) |
| Testing | 101 unit, 29 architecture, 17 integration (Testcontainers), 7 E2E (Playwright), k6 baseline | Validated |
| Containers and CI/CD | Multi-stage Dockerfile, Compose stack, GitHub Actions workflows | Validated: Compose locally and the GitHub Actions CI on `main` (build, tests, E2E, gitleaks, Docker images, CodeQL) |
| Azure | Hosts are Azure-ready: Container Apps, Azure Database for PostgreSQL, and `IFileStorage` ready for Blob Storage | Planned (Bicep) |
| Claude Code and spec-driven development | Specs and acceptance criteria before code, AI-assisted implementation reviewed through automated gates. Conventions in [`CLAUDE.md`](../../CLAUDE.md) and [`AGENTS.md`](../../AGENTS.md) | Implemented |
| Engineering judgment and trade-offs | [ADRs 0001–0006](../adr), including the explicit exception to the conventions for high-volume tables | Documented |
