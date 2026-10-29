# ADR-0002: Blazor Web App (Interactive Server) plus an ASP.NET Core MVC API in one web host

- Status: Accepted
- Date: 2026-10-29

## Context

DataForge needs a customer portal and an internal implementation console. It also needs a REST API for scripts, load tests and future integrations. The target environment is enterprise .NET, so the stack should be ASP.NET MVC and Blazor.

## Decision

- **UI:** a Blazor Web App with per-page Interactive Server rendering.
  - The login page is static SSR, because issuing the authentication cookie requires an HTTP response.
  - The data pages are interactive. They poll job progress and paginate grids without a JavaScript framework.
- **API:** ASP.NET Core MVC controllers that derive from `BaseApiController`. Every response uses the `ApiResponse<T>` envelope.
- **Location:** pages and controllers live in each module's `Presentation` project, a Razor Class Library. The web host only composes them.
- **Security:**
  - Cookie authentication for browser and API callers.
  - CSRF protection through the MVC `AutoValidateAntiforgeryToken` filter on unsafe API methods, plus Blazor antiforgery on forms.
  - Uploads go from the browser straight to the API with XHR, so large files never pass through the SignalR circuit.

## Consequences

- One deployable unit and one authentication mechanism.
- Interactive Server keeps state on the server. Sticky sessions are needed when the web tier scales out, and the data protection key ring is shared through a volume (`DataProtection:KeysPath`).
- Interactive pages are prerendered, so a click that happens before the circuit connects is lost. The end-to-end tests retry the click until the expected result appears instead of sleeping.
