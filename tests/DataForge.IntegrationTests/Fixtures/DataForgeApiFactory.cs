using System.Net.Http.Json;
using System.Text.Json;
using DataForge.Web.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DataForge.IntegrationTests.Fixtures;

/// <summary>
/// Hosts the real web application in memory against the test database. The type argument only identifies the
/// web assembly (both hosts use top-level statements, so <c>Program</c> is ambiguous).
/// </summary>
public sealed class DataForgeApiFactory(PostgresFixture database) : WebApplicationFactory<RateLimitingOptions>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Testing");
        foreach (var (key, value) in database.Settings)
        {
            builder.UseSetting(key, value);
        }
    }

    public HttpClient CreateCookieClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });

    /// <summary>Signs in and returns a client that sends the antiforgery header on every request.</summary>
    public async Task<HttpClient> SignInAsync(string email)
    {
        var client = CreateCookieClient();
        await RefreshAntiforgeryAsync(client);

        using var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = PostgresFixture.DemoPassword });
        response.EnsureSuccessStatusCode();

        // Antiforgery tokens are bound to the user, so a new token is required after signing in.
        await RefreshAntiforgeryAsync(client);
        return client;
    }

    public static async Task RefreshAntiforgeryAsync(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        using var document = JsonDocument.Parse(await client.GetStringAsync("/api/auth/antiforgery"));
        var data = document.RootElement.GetProperty("data");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add(data.GetProperty("headerName").GetString()!, data.GetProperty("token").GetString());
    }
}
