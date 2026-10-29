using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DataForge.IntegrationTests.Fixtures;

namespace DataForge.IntegrationTests.Api;

/// <summary>
/// Security behaviour of the HTTP API: authentication, CSRF protection, role-based access and, above all,
/// that one tenant can never read or write another tenant's data, even with a valid identifier.
/// </summary>
public sealed class SecurityAndTenantIsolationTests(PostgresFixture database) : IAsyncLifetime
{
    private DataForgeApiFactory _factory = default!;

    public ValueTask InitializeAsync()
    {
        _factory = new DataForgeApiFactory(database);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task Api_WithoutSession_Returns401InsteadOfRedirect()
    {
        using var client = _factory.CreateCookieClient();

        using var response = await client.GetAsync("/api/projects", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsGenericError()
    {
        using var client = _factory.CreateCookieClient();
        await DataForgeApiFactory.RefreshAntiforgeryAsync(client);

        using var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin@acme.demo", password = "wrong-password" }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Contain("auth.invalid_credentials");
    }

    [Fact]
    public async Task UnsafeRequest_WithoutAntiforgeryToken_IsRejected()
    {
        using var client = await _factory.SignInAsync("admin@acme.demo");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");

        using var response = await client.PostAsJsonAsync("/api/projects", new { name = "CSRF attempt" }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OtherTenant_CannotReadOrWriteProjectByItsId()
    {
        using var acme = await _factory.SignInAsync("admin@acme.demo");
        using var globex = await _factory.SignInAsync("admin@globex.demo");
        var projectId = await CreateProjectAsync(acme, $"Acme confidential {Guid.NewGuid():N}");

        using (var read = await globex.GetAsync($"/api/projects/{projectId}", TestContext.Current.CancellationToken))
        {
            read.StatusCode.Should().Be(HttpStatusCode.NotFound, "existence of another tenant's project must not be disclosed");
        }

        using (var rules = await globex.GetAsync($"/api/projects/{projectId}/rules", TestContext.Current.CancellationToken))
        {
            rules.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        using (var upload = await globex.PostAsync(
            $"/api/projects/{projectId}/datasets?fileName=x.csv",
            new StringContent("id\n1\n", Encoding.UTF8, "text/csv"),
            TestContext.Current.CancellationToken))
        {
            upload.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        var globexProjects = await globex.GetStringAsync("/api/projects", TestContext.Current.CancellationToken);
        globexProjects.Should().NotContain(projectId.ToString());

        using var own = await acme.GetAsync($"/api/projects/{projectId}", TestContext.Current.CancellationToken);
        own.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OperationsApi_IsRestrictedToImplementationEngineers()
    {
        using var customer = await _factory.SignInAsync("admin@acme.demo");
        using var engineer = await _factory.SignInAsync("engineer@dataforge.demo");

        using var denied = await customer.GetAsync("/api/operations/jobs", TestContext.Current.CancellationToken);
        using var allowed = await engineer.GetAsync("/api/operations/jobs", TestContext.Current.CancellationToken);

        denied.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        allowed.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ApiResponses_NeverExposeInternalIdentifiers()
    {
        using var acme = await _factory.SignInAsync("admin@acme.demo");
        await CreateProjectAsync(acme, $"Envelope {Guid.NewGuid():N}");

        using var document = JsonDocument.Parse(await acme.GetStringAsync("/api/projects", TestContext.Current.CancellationToken));
        var project = document.RootElement.GetProperty("data").GetProperty("projects")[0];

        project.TryGetProperty("id", out _).Should().BeFalse();
        project.TryGetProperty("tenantId", out _).Should().BeFalse();
        Guid.TryParse(project.GetProperty("projectId").GetString(), out _).Should().BeTrue();
    }

    private static async Task<Guid> CreateProjectAsync(HttpClient client, string name)
    {
        using var response = await client.PostAsJsonAsync("/api/projects", new { name }, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return document.RootElement.GetProperty("data").GetProperty("projectId").GetGuid();
    }
}
