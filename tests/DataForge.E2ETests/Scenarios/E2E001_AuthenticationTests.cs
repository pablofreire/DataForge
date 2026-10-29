using DataForge.E2ETests.Fixtures;

namespace DataForge.E2ETests.Scenarios;

/// <summary>E2E-001: sign in, invalid credentials, sign out and access control.</summary>
public sealed class E2E001_AuthenticationTests : DataForgePageTest
{
    [Fact]
    public async Task ValidCredentials_OpenTheDashboardOfTheUsersOrganization()
    {
        await SignInAsync("admin@acme.demo");

        await Expect(Page.GetByTestId("page-title")).ToHaveTextAsync("Dashboard");
        await Expect(Page.GetByTestId("current-tenant")).ToHaveTextAsync("Acme Corporation");
        await ScreenshotAsync("e2e-001-dashboard");
    }

    [Fact]
    public async Task InvalidCredentials_ShowAGenericError()
    {
        await Page.GotoAsync("/account/login");
        await Page.GetByTestId("login-email").FillAsync("admin@acme.demo");
        await Page.GetByTestId("login-password").FillAsync("not-the-password");
        await Page.GetByTestId("login-submit").ClickAsync();

        await Expect(Page.GetByTestId("login-error")).ToHaveTextAsync("Invalid e-mail or password.");
        await ScreenshotAsync("e2e-001-invalid-credentials");
    }

    [Fact]
    public async Task SignOut_ReturnsToTheSignInPage()
    {
        await SignInAsync("admin@acme.demo");

        await Page.GetByTestId("logout").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/account/login"));
        await Page.GotoAsync("/projects");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/account/login\\?ReturnUrl="));
    }

    [Fact]
    public async Task CustomerUser_CannotOpenTheImplementationConsole()
    {
        await SignInAsync("analyst@acme.demo");

        await Page.GotoAsync("/operations");

        await Expect(Page.GetByTestId("access-denied")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("nav-operations")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ImplementationEngineer_SeesTheOperationsConsole()
    {
        await SignInAsync("engineer@dataforge.demo");

        await Page.GetByTestId("nav-operations").ClickAsync();

        await Expect(Page.GetByTestId("page-title")).ToHaveTextAsync("Operations");
        await ScreenshotAsync("e2e-001-operations-console");
    }
}
