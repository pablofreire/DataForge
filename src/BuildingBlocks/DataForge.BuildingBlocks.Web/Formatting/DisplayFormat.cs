using System.Globalization;

namespace DataForge.BuildingBlocks.Web.Formatting;

/// <summary>Consistent, culture-invariant formatting for numbers, sizes and timestamps in the UI.</summary>
public static class DisplayFormat
{
    public static string Count(long value) => value.ToString("N0", CultureInfo.InvariantCulture);

    public static string Percent(double value) => value.ToString("0.#", CultureInfo.InvariantCulture) + "%";

    public static string Bytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => (bytes / 1024d).ToString("0.#", CultureInfo.InvariantCulture) + " KB",
        < 1024L * 1024 * 1024 => (bytes / (1024d * 1024)).ToString("0.#", CultureInfo.InvariantCulture) + " MB",
        _ => (bytes / (1024d * 1024 * 1024)).ToString("0.##", CultureInfo.InvariantCulture) + " GB",
    };

    public static string Timestamp(DateTimeOffset? value) =>
        value?.ToUniversalTime().ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture) ?? "-";

    public static string Duration(DateTimeOffset? start, DateTimeOffset? end)
    {
        if (start is null || end is null)
        {
            return "-";
        }

        var elapsed = end.Value - start.Value;
        return elapsed.TotalSeconds < 60
            ? elapsed.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " s"
            : elapsed.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture);
    }

    public static string JobType(string jobType) => jobType switch
    {
        "onboarding.dataset.import" => "Import",
        "validation.dataset.validate" => "Validation",
        _ => jobType,
    };
}
