# Security policy

## Reporting a vulnerability

Do not open public issues for security problems. Use GitHub's **Report a vulnerability** (private advisory) on this repository instead. You will receive an acknowledgement within 5 business days.

## Security controls in this codebase

| Area | Control |
|---|---|
| Authentication | Cookie authentication (HttpOnly, SameSite=Strict); passwords hashed with BCrypt; login rate limit per IP; uniform errors and equalized timing against user enumeration |
| Authorization | `TenantUser` and `ImplementationEngineer` policies; every query filters by tenant; another tenant's resources return 404 |
| CSRF | MVC `AutoValidateAntiforgeryToken` on unsafe API methods and Blazor antiforgery on forms |
| Input | FluentValidation on every request; upload extension and size limits; path-traversal-safe storage; a match timeout on customer-supplied regular expressions (ReDoS) |
| Data exposure | Internal ids are never returned; unexpected exceptions return a generic error with a trace id; job errors shown to users are sanitized |
| Headers | `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy`, `Permissions-Policy`, `Cross-Origin-Opener-Policy` |
| Secrets | Taken from configuration and environment only; `.env` is ignored by git; demo credentials are local-only defaults |
| Supply chain | Central Package Management with pinned versions; Dependabot, CodeQL and gitleaks in CI |

## Known limitations

- Tenant isolation is enforced in the application layer. PostgreSQL row-level security is planned as a second line of defence.
- TLS termination is expected at the ingress (reverse proxy or Azure Container Apps).
