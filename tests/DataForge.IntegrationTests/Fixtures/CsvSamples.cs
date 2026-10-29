using System.Globalization;
using System.Text;

namespace DataForge.IntegrationTests.Fixtures;

internal static class CsvSamples
{
    /// <summary>Five rows with ten known violations across six rules (see OnboardingPipelineTests).</summary>
    public const string CustomersWithErrors = """
        customer_id;name;email;country;credit_limit;signup_date
        C001;Alice Martins;alice@example.com;BR;1500.00;2024-01-15
        C002;Bruno Lima;bruno.example.com;BR;2000;2024-02-30
        C003;;carla@example.com;PT;-10;2024-03-01
        C001;Diego Souza;diego@example.com;XX;500;2024-04-10
        C005;Elena Rocha;elena@example.com;US;99999999;15/05/2024
        """;

    /// <summary>Generates <paramref name="rows"/> rows where every row whose number is divisible by 10 has an invalid e-mail.</summary>
    public static string Generated(int rows)
    {
        var builder = new StringBuilder("id,email,amount\n");
        for (var row = 1; row <= rows; row++)
        {
            var email = row % 10 == 0 ? $"user{row}.example.com" : $"user{row}@example.com";
            builder.Append(CultureInfo.InvariantCulture, $"{row},{email},{row % 1000}.50\n");
        }

        return builder.ToString();
    }

    public static MemoryStream ToStream(string content) => new(Encoding.UTF8.GetBytes(content.ReplaceLineEndings("\n")));
}
