using DataForge.E2ETests.Fixtures;

namespace DataForge.E2ETests.Scenarios;

/// <summary>E2E-002: a user of one organization cannot open another organization's project, even with its URL.</summary>
public sealed class E2E002_TenantIsolationTests : DataForgePageTest
{
    [Fact]
    public async Task ProjectUrlOfAnotherTenant_ShowsNotFound()
    {
        await SignInAsync("admin@acme.demo");
        await Page.GotoAsync("/projects");
        await ClickUntilVisibleAsync(Page.GetByTestId("new-project"), Page.GetByTestId("project-name"));
        await Page.GetByTestId("project-name").FillAsync($"Acme only {Guid.NewGuid().ToString("N")[..8]}");
        await Page.GetByTestId("project-create").ClickAsync();
        await Expect(Page.GetByTestId("project-rules-link")).ToBeVisibleAsync();
        var acmeProjectUrl = Page.Url;

        await Page.GetByTestId("logout").ClickAsync();
        await SignInAsync("admin@globex.demo");
        await Page.GotoAsync(acmeProjectUrl);

        await Expect(Page.GetByTestId("project-not-found")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("current-tenant")).ToHaveTextAsync("Globex Industries");
        await ScreenshotAsync("e2e-002-tenant-isolation");
    }
}
