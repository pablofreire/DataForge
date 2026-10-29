using DataForge.Modules.Onboarding.Domain.Entities;

namespace DataForge.Modules.Onboarding.Domain.Services;

/// <summary>
/// Rules a CSV header must satisfy before any row is imported.
/// </summary>
public static class DatasetHeaderValidator
{
    public static string? Validate(IReadOnlyList<string> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        if (columns.Count == 0)
        {
            return "The file has no header row.";
        }

        if (columns.Count > DatasetEntity.MaxColumns)
        {
            return $"The file has {columns.Count} columns; the maximum is {DatasetEntity.MaxColumns}.";
        }

        var blank = columns.Select((name, index) => (name, index)).FirstOrDefault(column => string.IsNullOrWhiteSpace(column.name));
        if (blank.name is not null)
        {
            return $"Column {blank.index + 1} has an empty name.";
        }

        var duplicate = columns
            .GroupBy(name => name.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        return duplicate is null ? null : $"The column name '{duplicate.Key}' appears more than once.";
    }
}
