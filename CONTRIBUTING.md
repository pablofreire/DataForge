# Contributing

1. Open an issue describing the problem or the feature, and its acceptance criteria.
2. Branch from `main`.
3. Follow [docs/engineering/coding-standards.md](docs/engineering/coding-standards.md).
4. Run the gates locally:

   ```bash
   dotnet build DataForge.slnx                    # zero warnings
   dotnet test --solution DataForge.slnx          # needs Docker for Testcontainers
   ```

5. If you changed the UI, run the Playwright suite against `docker compose`, as described in the README.
6. Use [Conventional Commits](https://www.conventionalcommits.org) for commit messages, for example `feat(validation): add numeric range rule`.
7. Open a pull request. CI must be green before review.
