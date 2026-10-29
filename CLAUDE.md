# DataForge: instructions for Claude Code

Read these first:
- `STATUS.md`: current state and next steps;
- `docs/engineering/coding-standards.md`;
- the ADRs in `docs/adr`.

## Ground rules

- All code, comments, commit messages and documentation are written in English.
- Base every claim on code, tests or measurements. Never publish a performance number that was not measured. Record measurements in `docs/performance/` together with the environment they ran on.
- Follow spec-driven development. Before implementing a feature:
  1. Update or create its spec in `docs/specs/features/` with acceptance criteria.
  2. Implement the feature and its tests together.
  3. Update `STATUS.md`.
- Never weaken a quality gate to make a build pass. This covers analyzers, architecture tests and thresholds. Fix the cause instead.

## Conventions

These come from the house conventions and are enforced by the build and the architecture tests:
- Dapper and DbUp only, never EF Core.
- Use Case pattern, never MediatR, CQRS libraries or AutoMapper.
- Every API response uses `ApiResponse<T>`.
- Dual ids: a `bigint` id that stays internal and a `uuid` `public_id` for routes and APIs.
- Soft delete through `is_active`.
- One schema per module, with snake_case names.
- Indexes `ix_{table}_public_id` and `ix_{table}_is_active`. ADR-0004 lists the exceptions.
- Passwords are hashed with BCrypt.
- SQL lives only in `*Queries` classes, and every query filters by `tenant_id`.
- Each folder holds one kind of artifact. Interfaces live in `Interfaces/` and never share a folder with classes.
  - Use cases are split into `Contracts/`, `Interfaces/`, `Implementations/` and `Validators/`.
  - The full layout is in `docs/engineering/coding-standards.md`.

## Commands

```bash
dotnet build DataForge.slnx                    # must be warning-free
dotnet test --solution DataForge.slnx          # unit + architecture + integration (Docker required)
docker compose -f deploy/compose/docker-compose.yml up --build -d
dotnet test --project tests/DataForge.E2ETests # against the compose stack (http://localhost:8080)
```

## Definition of done for a change

- The build and every test suite pass.
- A new use case has unit tests.
- New SQL has an integration test.
- A new page flow has a Playwright scenario.
- `STATUS.md` reflects the change.
