# ADR-0006: Library choices

- Status: Accepted
- Date: 2026-10-29

Each version is pinned in `Directory.Packages.props` and was checked on nuget.org when it was added.

| Need | Choice | Why |
|---|---|---|
| Data access | Dapper + Npgsql | Explicit SQL that can be reviewed and tuned, access to PostgreSQL features (COPY, `SKIP LOCKED`, arrays, jsonb) and no ORM change tracking on bulk paths |
| Migrations | DbUp | Plain, numbered SQL scripts embedded per module and journaled |
| CSV | Sep (MIT) | SIMD-accelerated, low-allocation, detects the delimiter. Brazilian exports often use `;` |
| Validation of requests | FluentValidation | Declarative validators shared by the API and the UI |
| Passwords | BCrypt.Net-Next | Adaptive hashing with the work factor stored in the hash |
| Logging | Serilog | Structured logs, compact JSON in containers |
| Telemetry | OpenTelemetry (OTLP) | Vendor-neutral traces and metrics: ASP.NET Core, Npgsql, runtime and custom `DataForge.*` meters |
| Tests | xUnit v3 on Microsoft Testing Platform, NSubstitute, AwesomeAssertions | AwesomeAssertions is an Apache-2.0 fork of FluentAssertions. FluentAssertions 8+ needs a commercial licence |
| Integration tests | Testcontainers (PostgreSQL 17) | Real database behaviour; an in-memory database would hide COPY, locking and index semantics |
| Architecture tests | NetArchTest + assembly-reference checks | Module and layer rules enforced on every build |
| End-to-end tests | Playwright for .NET | Tests the real application in the browser, with traces and screenshots as evidence |
| Load tests | Grafana k6 (Docker image) | Scripted scenarios with thresholds; nothing to install locally |

**Rejected:**
- **MediatR:** use cases are called directly through their interfaces. The project convention is the Use Case pattern without a mediator.
- **AutoMapper:** mapping is written by hand, in `*MappingExtensions`.
- **EF Core:** bulk and queue semantics are central to the product.
