# Coding standards

The build and `tests/DataForge.ArchitectureTests` enforce most rules below. Breaking one fails CI.

## Structure

- **Module projects:** each module has five projects, `Domain`, `Application`, `Infrastructure`, `Contracts` and `Presentation`.
  - Allowed dependencies: Domain ← Application ← Infrastructure, and Application ← Presentation.
  - A module references another module only through its `Contracts` project.
- **One kind of artifact per folder.** An interface never shares a folder with a class. Namespaces mirror folders. `FolderConventionTests` enforces both rules.
- **Use cases:** each use case gets one folder, `UseCases/{Aggregate}/{Action}/`, split by artifact kind:

  ```
  UseCases/Datasets/Upload/
    Contracts/        UploadDatasetRequest, UploadDatasetResponse
    Interfaces/       IUploadDatasetUseCase
    Implementations/  UploadDatasetUseCase
    Validators/       UploadDatasetRequestValidator
  UseCases/Datasets/Shared/   output models reused by several use cases of the aggregate (DatasetItem)
  ```

- **Folders per project:**

  | Project | Folders |
  |---|---|
  | `Domain` | `Entities/`, `Enums/`, `Errors/`, `ValueObjects/`, `ReadModels/`, `Services/` (stateless domain logic), `Interfaces/Repositories/`. Validation also has `Engine/{Interfaces,Models,Services,Strategies}/` |
  | `Application` | `UseCases/`, `Interfaces/` (ports implemented by Infrastructure), `Models/`, `Services/`, `Jobs/`, `Execution/`, `Exceptions/`, `Catalogs/`, `IntegrationEventHandlers/`, `Mappings/`, `Options/`, `DependencyInjection/` |
  | `Infrastructure` | `Persistence/{Queries,Repositories,Readers,Writers,Migrations}/`, `Options/`, adapters such as `Csv/` or `Security/`, `DependencyInjection/` |
  | `Contracts` | `Interfaces/`, `Dtos/`, `IntegrationEvents/`, `Models/`, `Exceptions/` |
  | `Presentation` | `Controllers/` (with request and response bodies in `Controllers/Contracts/`), `Pages/`, `Components/`, `Models/` (form models) |

- **Unit tests** mirror the folder of the code they test, for example `UseCases/Datasets/Upload/Implementations/UploadDatasetUseCaseTests.cs`.
- **SQL:** lives only in `Infrastructure/Persistence/Queries/*Queries.cs` (internal constants). Repositories execute it; nothing else does.
- **Migrations:** `Infrastructure/Persistence/Migrations/{NNNN}_{schema}_{entity}_{action}.sql`. The 01xx range is Identity, 02xx Onboarding, 03xx Validation and 04xx Processing.

## Naming

| Artifact | Suffix |
|---|---|
| Entities | `Entity` |
| Repositories | `Repository` |
| SQL holders | `Queries` |
| Use cases | `UseCase` |
| Use case inputs and outputs | `Request` / `Response` |
| Validators | `RequestValidator` |
| Cross-module DTOs | `Dto` |
| Events | `IntegrationEvent` |
| Event handlers | `IntegrationEventHandler` |
| API controllers | `Controller` |
| Blazor pages | `Page` |
| Background services | `Worker` |
| Job executors | `JobExecutor` |
| Rule strategies | `RuleStrategy` |
| Options | `Options` |
| Error catalogs | `Errors` |
| Tests | `Tests`, with methods named `Method_Scenario_ExpectedResult` |
| E2E page objects | `PageObject` |

## Code

- Classes are `sealed` unless designed for inheritance.
- Requests, responses and DTOs are records.
- Use primary constructors.
- Use file-scoped namespaces, and keep one main type per file.
- Expected failures return `Result<T>` with an `Error`. Exceptions are reserved for unexpected faults.
- Use `TimeProvider` instead of `DateTime.Now`. Job leases and schedules use the database clock.
- Log with source-generated `LoggerMessage` methods (CA1848 and CA1873).
- Never expose internal `bigint` ids. APIs, routes and cross-module contracts use `PublicId` (UUID v7).
- Map objects by hand in `*MappingExtensions`. Do not use AutoMapper or MediatR.

## Analyzer exceptions

These are set in `.editorconfig`:

| Rule | Reason |
|---|---|
| CA1515 | Public types are required across module projects |
| CA1062 | Nullable reference types already cover it |
| CA2007 | ASP.NET Core has no synchronization context |
| CA1711 | `…EventHandler` is part of the naming convention |
| CA1716 | Module names may collide with VB keywords |
| CA1707 (tests only) | Underscored test method names |
| xUnit1051 (tests only) | False positives on NSubstitute verification calls |
