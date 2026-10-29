# Agents guide

This guide applies to any coding agent (Claude Code, Copilot, Codex) working in this repository.

1. **Orient.** Read `STATUS.md`, `CLAUDE.md` and `docs/engineering/coding-standards.md`.
2. **Place code by layer.** Each module has the same five projects:

   | Project | What goes there |
   |---|---|
   | `Domain` | Entities, rules, repository interfaces. No framework dependencies |
   | `Application` | Use cases, options, job executors, integration event handlers |
   | `Infrastructure` | Repositories, `*Queries`, COPY writers, migrations |
   | `Contracts` | The only surface other modules may use |
   | `Presentation` | Blazor pages and MVC controllers, which call use cases only |

3. **Add a use case.** Create the folder `UseCases/{Aggregate}/{Action}/` with these subfolders:
   - `Contracts/` for the Request and the Response;
   - `Interfaces/` for `I{Name}UseCase`;
   - `Implementations/` for `{Name}UseCase`;
   - `Validators/` for `{Name}RequestValidator`.

   Then register the use case in the module's `DependencyInjection` extension and add unit tests that mirror its path. Keep one kind of artifact per folder, and never put an interface and a class in the same folder. `FolderConventionTests` enforces both rules.
4. **Change the database.** Add the next numbered SQL script in the module's `Persistence/Migrations`. Never edit an applied script.
5. **Verify.** Run `dotnet build` and `dotnet test --solution DataForge.slnx`. If you changed the UI, also run the E2E suite against Docker Compose.
6. **Report honestly.** List what was verified and what was not. Do not mark items done in `STATUS.md` without evidence.

**Quality gates (CI):**
- build with zero warnings;
- unit, architecture and integration tests;
- Playwright E2E;
- gitleaks;
- CodeQL;
- Docker build.

k6 runs only on demand (`performance.yml`).
