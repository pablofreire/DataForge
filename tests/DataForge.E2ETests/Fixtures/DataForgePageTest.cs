using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace DataForge.E2ETests.Fixtures;

/// <summary>
/// Base class for end-to-end scenarios against the running application (no mocked APIs).
/// Records a Playwright trace per test and provides helpers that respect Blazor's interactive startup.
/// </summary>
public abstract class DataForgePageTest : PageTest
{
    private static readonly TimeSpan InteractiveTimeout = TimeSpan.FromSeconds(15);

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = E2ESettings.BaseUrl,
        ViewportSize = new ViewportSize { Width = 1440, Height = 900 },
        Locale = "en-US",
    };

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await Context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true });
    }

    public override async ValueTask DisposeAsync()
    {
        var name = TestContext.Current.Test?.TestDisplayName ?? "test";
        var safeName = string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(' ', '_');
        await Context.Tracing.StopAsync(new TracingStopOptions { Path = Path.Combine(E2ESettings.EvidenceDirectory, "traces", $"{safeName}.zip") });
        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected async Task SignInAsync(string email)
    {
        await Page.GotoAsync("/account/login");
        await Page.GetByTestId("login-email").FillAsync(email);
        await Page.GetByTestId("login-password").FillAsync(E2ESettings.DemoPassword);
        await Page.GetByTestId("login-submit").ClickAsync();
        await Expect(Page.GetByTestId("current-user")).ToBeVisibleAsync();
    }

    /// <summary>
    /// Interactive Blazor pages are prerendered, and clicks before the circuit connects are lost.
    /// Clicks until the expected outcome appears instead of sleeping for a fixed time.
    /// </summary>
    protected static async Task ClickUntilVisibleAsync(ILocator target, ILocator expected)
    {
        var deadline = DateTime.UtcNow + InteractiveTimeout;
        while (true)
        {
            await target.ClickAsync();
            try
            {
                await expected.WaitForAsync(new LocatorWaitForOptions { Timeout = 1_000 });
                return;
            }
            catch (TimeoutException) when (DateTime.UtcNow < deadline)
            {
            }
        }
    }

    protected Task ScreenshotAsync(string name) =>
        Page.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(E2ESettings.EvidenceDirectory, $"{name}.png"), FullPage = true });
}
