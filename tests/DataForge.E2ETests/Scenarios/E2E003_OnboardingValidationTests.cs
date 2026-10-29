using System.Text;
using System.Text.RegularExpressions;
using DataForge.E2ETests.Fixtures;
using Microsoft.Playwright;

namespace DataForge.E2ETests.Scenarios;

/// <summary>
/// E2E-003 (onboarding), E2E-004 (validation) and E2E-006 (processing) as one user journey:
/// create a project, define rules, upload a file with known errors, follow the background import and
/// validation, and review the issues.
/// </summary>
public sealed partial class E2E003_OnboardingValidationTests : DataForgePageTest
{
    private const string Csv = """
        customer_id,name,email,country
        C001,Alice Martins,alice@example.com,BR
        C002,Bruno Lima,bruno.example.com,BR
        C003,,carla@example.com,PT
        C001,Diego Souza,diego@example.com,XX
        """;

    [Fact]
    public async Task UploadedFile_IsImportedValidatedAndItsIssuesAreListed()
    {
        await SignInAsync("admin@acme.demo");

        // Create the project.
        await Page.GetByTestId("nav-projects").ClickAsync();
        await ClickUntilVisibleAsync(Page.GetByTestId("new-project"), Page.GetByTestId("project-name"));
        var projectName = $"E2E customers {Guid.NewGuid().ToString("N")[..8]}";
        await Page.GetByTestId("project-name").FillAsync(projectName);
        await Page.GetByTestId("project-create").ClickAsync();
        await Expect(Page.GetByTestId("page-title")).ToHaveTextAsync(projectName);

        // Define the rules.
        await Page.GetByTestId("project-rules-link").ClickAsync();
        await AddRuleAsync("name", "Required");
        await AddRuleAsync("email", "Email");
        await AddRuleAsync("customer_id", "Unique");
        await AddRuleAsync("country", "AllowedValues", allowed: "BR, PT, US");
        await Expect(Page.GetByTestId("rule-row")).ToHaveCountAsync(4);
        await ScreenshotAsync("e2e-004-rules");

        // Upload the file.
        await Page.GetByTestId("breadcrumb-project").ClickAsync();
        await Expect(Page.GetByTestId("upload-submit")).ToBeVisibleAsync();
        var upload = new FilePayload { Name = "customers.csv", MimeType = "text/csv", Buffer = Encoding.UTF8.GetBytes(Csv.ReplaceLineEndings("\n")) };
        var deadline = DateTime.UtcNow.AddSeconds(15);
        do
        {
            await Page.GetByTestId("upload-input").SetInputFilesAsync(upload);
        }
        while (await Page.GetByTestId("upload-file-name").InnerTextAsync() != "customers.csv" && DateTime.UtcNow < deadline);
        await Page.GetByTestId("upload-submit").ClickAsync();

        // Follow the background import (E2E-006).
        await Expect(Page).ToHaveURLAsync(DatasetUrl());
        await Expect(Page.GetByTestId("dataset-status")).ToHaveAttributeAsync("data-status", "Imported", new() { Timeout = 30_000 });
        await Expect(Page.GetByTestId("dataset-row-count")).ToHaveTextAsync("4");
        await ScreenshotAsync("e2e-006-dataset-imported");

        // Review the validation results (E2E-004).
        await Page.GetByTestId("validation-link").ClickAsync();
        await Expect(Page.GetByTestId("run-status")).ToHaveAttributeAsync("data-status", "Completed", new() { Timeout = 30_000 });
        await Expect(Page.GetByTestId("valid-rows")).ToHaveTextAsync("0");
        await Expect(Page.GetByTestId("invalid-rows")).ToHaveTextAsync("4");
        await Expect(Page.GetByTestId("metric-issues")).ToContainTextAsync("5");
        await Expect(Page.GetByTestId("issue-row")).ToHaveCountAsync(5);
        await Expect(Page.GetByTestId("issues-table")).ToContainTextAsync("bruno.example.com");
        await ScreenshotAsync("e2e-004-validation-results");
    }

    private async Task AddRuleAsync(string column, string ruleType, string? allowed = null)
    {
        var rows = await Page.GetByTestId("rule-row").CountAsync();
        await Page.GetByTestId("rule-column").FillAsync(column);
        await Page.GetByTestId("rule-type").SelectOptionAsync(ruleType);
        if (allowed is not null)
        {
            await Page.GetByTestId("rule-allowed").FillAsync(allowed);
        }

        await ClickUntilVisibleAsync(Page.GetByTestId("rule-add"), Page.GetByTestId("rule-row").Nth(rows));
    }

    [GeneratedRegex("/datasets/[0-9a-f-]{36}$")]
    private static partial Regex DatasetUrl();
}
