# DataForge MVP specification

## Product

**Problem.** Before a SaaS go-live, customers deliver data exports (customers, products, balances) of varying quality. Implementation teams lose weeks finding bad values row by row in spreadsheets.

**Users.**
- Customer data owners (TenantAdmin, TenantMember) upload files, define rules and fix their data.
- Implementation engineers watch processing across customers.

**MVP scope.**

| Step | Outcome |
|---|---|
| Login | The user is signed in to their organization |
| Project | An onboarding project exists |
| Rules | The project has validation rules |
| Upload | The CSV file is stored |
| Process | The file is imported in the background |
| Validate | Rows are checked automatically |
| Errors | Each issue is listed |
| Results | A quality summary shows the share of valid rows |

## Features and acceptance criteria

### F-001 Authentication and tenancy

| Id | Acceptance criterion | Verified by |
|---|---|---|
| AC1 | Valid credentials sign the user in to their tenant's dashboard. | E2E-001 |
| AC2 | Invalid credentials show "Invalid e-mail or password." for both unknown accounts and wrong passwords. | E2E-001; `AuthenticateUserUseCaseTests` |
| AC3 | Login is rate limited per IP. | Configured |
| AC4 | API calls without a session return 401; pages redirect to sign-in with `ReturnUrl`. | `SecurityAndTenantIsolationTests`; E2E-001 |
| AC5 | The operations console is limited to ImplementationEngineer. Others receive 403 from the API and an access-denied page in the UI. | Integration; E2E-001 |
| AC6 | A resource of another tenant is reported as not found, through the API and the UI. | Integration; E2E-002 |

### F-002 Projects and upload

| Id | Acceptance criterion | Verified by |
|---|---|---|
| AC1 | Project names are unique per tenant, ignoring case. | DB index; use case |
| AC2 | Uploads accept .csv, .tsv and .txt only, up to `Onboarding:MaxUploadBytes`. Names with traversal sequences are rejected. | Unit |
| AC3 | The file is streamed to storage with its SHA-256; it is never buffered in memory. | Unit |
| AC4 | Identical content uploaded to the same project returns the existing dataset (idempotent). | Unit; integration |
| AC5 | Every accepted upload enqueues exactly one import job. | Unit; integration |

### F-003 Import (processing)

| Id | Acceptance criterion | Verified by |
|---|---|---|
| AC1 | The delimiter is detected from the header. Quoted fields are unescaped. NUL characters are removed. | Implementation; smoke test (`;` file) |
| AC2 | A header that is empty, duplicated or has more than 1000 columns fails the dataset with a clear reason, without retries. | Unit; executor |
| AC3 | Rows are staged in batches with binary COPY. Row numbers are contiguous from 1. | Integration (25k rows) |
| AC4 | A resumed import discards partial batches and produces exactly the rows of the file. | `ImportResumeTests` |
| AC5 | Job progress is visible while importing. | Dataset page; E2E-006 |

### F-004 Validation and results

| Id | Acceptance criterion | Verified by |
|---|---|---|
| AC1 | The supported rules behave as follows. Blank values pass every rule except Required. | `RuleStrategyTests` |
| AC2 | A rule whose column is missing produces one file-level issue (row 0). | Unit; integration |
| AC3 | Validation starts automatically after an import when the project has rules, and can be re-run manually, creating a new run. | Integration |
| AC4 | The results show valid and invalid rows, issues per rule, and keyset-paginated issues with the offending value, filterable by column. | Integration; E2E-004 |
| AC5 | The run totals equal the number of distinct invalid rows and the number of issues. | Integration (exact expected issues) |

Rule behaviour for F-004 AC1:

| Rule | What fails |
|---|---|
| Required | A blank value |
| MaxLength | A trimmed value longer than the limit |
| Pattern | A value that does not match, or whose match times out (ReDoS protection) |
| Email | An invalid address |
| Numeric | Anything that is not an invariant number with a dot decimal separator and no thousands separator, or a value outside the range |
| Date | An invalid calendar date or a value in the wrong format (ISO 8601 by default) |
| AllowedValues | A value outside the list (case-insensitive) |
| Unique | Every row that shares a non-blank value with another row |

## Non-functional requirements

| Requirement | Target | Status |
|---|---|---|
| Memory | Constant with respect to file size: bounded channels and streaming | Designed |
| Scalability | Workers scale horizontally; a job is never run twice concurrently | Validated |
| Security | See [SECURITY.md](../../SECURITY.md) | Validated |
| Latency | API p95 below 300 ms at 10 concurrent users (baseline) | Measured: 3.1 ms |
| Large datasets | 1M rows measured; 100M is a stretch goal that needs suitable infrastructure | Planned |
