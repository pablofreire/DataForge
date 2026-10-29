# ADR-0001: Modular monolith with layered module projects

- Status: Accepted
- Date: 2026-10-29

## Context

DataForge has four bounded contexts, each with its own data and rules:
- **Identity**: tenants and users.
- **Onboarding**: projects, datasets and staged rows.
- **Validation**: rules, runs and issues.
- **Processing**: the durable job engine.

One engineer maintains the product today. Operating microservices would cost more than it returns. Still, the boundaries must be real so a module can be extracted later without a rewrite.

## Decision

- **Deployment:** a single deployable codebase with three hosts. `Web` serves the UI and API, `Worker` runs background jobs and `Migrator` applies database migrations.
- **Five projects per module:** `Domain`, `Application`, `Infrastructure`, `Contracts`, `Presentation`.
  - The compiler enforces the layer direction.
  - Modules reference each other only through `Contracts` projects, using interfaces, DTOs and integration events.
- **Database:** one schema per module (`identity`, `onboarding`, `validation`, `processing`).
  - There are no foreign keys across schemas.
  - Modules reference each other's data by public UUID.
- **Enforcement:** architecture tests (`tests/DataForge.ArchitectureTests`) fail the build on any cross-module reference that bypasses `Contracts`, and on violations of the layer direction.

## Consequences

**Benefits**
- A single process, one database and simple local development (`docker compose up`).
- Extracting a module later means replacing the in-process `Contracts` implementations with HTTP or messaging adapters.

**Trade-offs**
- Cross-module reads go through APIs instead of SQL joins. For example, validation reads staged rows through `IDatasetRowReader` and does not query `onboarding.dataset_rows` directly.
- More projects (about 24) than a layered monolith. Central Package Management and a shared `Directory.Build.props` keep the configuration in one place.
