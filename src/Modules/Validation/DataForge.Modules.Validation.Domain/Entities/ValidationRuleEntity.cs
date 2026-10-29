using System.Text.RegularExpressions;
using DataForge.BuildingBlocks.Domain.Entities;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.Errors;
using DataForge.Modules.Validation.Domain.ValueObjects;

namespace DataForge.Modules.Validation.Domain.Entities;

/// <summary>
/// A rule that a column of every dataset in a project must satisfy. Rules are bound to column names,
/// so a corrected file re-uploaded to the same project is validated against the same rules.
/// </summary>
public sealed class ValidationRuleEntity : TenantOwnedEntity
{
    public const int ColumnNameMaxLength = 200;
    public const int PatternMaxLength = 500;
    public const int AllowedValuesMaxCount = 500;

    private ValidationRuleEntity()
    {
    }

    public Guid ProjectPublicId { get; private set; }

    public string ColumnName { get; private set; } = string.Empty;

    public RuleType RuleType { get; private set; }

    public RuleParameters Parameters { get; private set; } = RuleParameters.None;

    public static Result<ValidationRuleEntity> Create(
        long tenantId,
        Guid projectPublicId,
        string columnName,
        RuleType ruleType,
        RuleParameters? parameters,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tenantId);

        if (string.IsNullOrWhiteSpace(columnName) || columnName.Length > ColumnNameMaxLength)
        {
            return ValidationRuleErrors.InvalidParameters($"The column name is required and must have at most {ColumnNameMaxLength} characters.");
        }

        var normalized = Normalize(ruleType, parameters ?? RuleParameters.None);
        var error = ValidateParameters(ruleType, normalized);
        if (error is not null)
        {
            return ValidationRuleErrors.InvalidParameters(error);
        }

        var rule = new ValidationRuleEntity
        {
            TenantId = tenantId,
            ProjectPublicId = projectPublicId,
            ColumnName = columnName.Trim(),
            RuleType = ruleType,
            Parameters = normalized,
        };
        rule.Initialize(now);
        return rule;
    }

    /// <summary>Keeps only the parameters meaningful for the rule type.</summary>
    private static RuleParameters Normalize(RuleType ruleType, RuleParameters parameters) => ruleType switch
    {
        RuleType.MaxLength => new RuleParameters { MaxLength = parameters.MaxLength },
        RuleType.Pattern => new RuleParameters { Pattern = parameters.Pattern },
        RuleType.Numeric => new RuleParameters { Min = parameters.Min, Max = parameters.Max },
        RuleType.Date => new RuleParameters { Format = string.IsNullOrWhiteSpace(parameters.Format) ? null : parameters.Format.Trim() },
        RuleType.AllowedValues => new RuleParameters
        {
            AllowedValues = parameters.AllowedValues?
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
        },
        _ => RuleParameters.None,
    };

    private static string? ValidateParameters(RuleType ruleType, RuleParameters parameters)
    {
        switch (ruleType)
        {
            case RuleType.MaxLength when parameters.MaxLength is null or < 1:
                return "MaxLength must be a positive number.";
            case RuleType.Pattern when string.IsNullOrWhiteSpace(parameters.Pattern) || parameters.Pattern.Length > PatternMaxLength:
                return $"A pattern of at most {PatternMaxLength} characters is required.";
            case RuleType.Pattern when !IsValidRegex(parameters.Pattern!):
                return "The pattern is not a valid regular expression.";
            case RuleType.Numeric when parameters is { Min: not null, Max: not null } && parameters.Min > parameters.Max:
                return "Min must be less than or equal to Max.";
            case RuleType.AllowedValues when parameters.AllowedValues is null || parameters.AllowedValues.Count == 0:
                return "At least one allowed value is required.";
            case RuleType.AllowedValues when parameters.AllowedValues!.Count > AllowedValuesMaxCount:
                return $"At most {AllowedValuesMaxCount} allowed values are supported.";
            default:
                return Enum.IsDefined(ruleType) ? null : "Unknown rule type.";
        }
    }

    private static bool IsValidRegex(string pattern)
    {
        try
        {
            _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
