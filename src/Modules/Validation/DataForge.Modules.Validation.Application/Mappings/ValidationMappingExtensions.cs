using System.Globalization;
using DataForge.Modules.Validation.Application.UseCases.Issues.Shared;
using DataForge.Modules.Validation.Application.UseCases.Rules.Shared;
using DataForge.Modules.Validation.Application.UseCases.Runs.Shared;
using DataForge.Modules.Validation.Domain.Engine.Models;
using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.ReadModels;
using DataForge.Modules.Validation.Domain.ValueObjects;

namespace DataForge.Modules.Validation.Application.Mappings;

public static class ValidationMappingExtensions
{
    public static ValidationRuleItem ToItem(this ValidationRuleEntity rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        return new ValidationRuleItem(rule.PublicId, rule.ColumnName, rule.RuleType.ToString(), Describe(rule.RuleType, rule.Parameters), rule.CreatedAt);
    }

    public static ValidationRunItem ToItem(this ValidationRunEntity run)
    {
        ArgumentNullException.ThrowIfNull(run);
        return new ValidationRunItem(
            run.PublicId,
            run.DatasetPublicId,
            run.Status.ToString(),
            run.TotalRows,
            run.ValidatedRows,
            run.InvalidRows,
            Math.Max(0, run.TotalRows - run.InvalidRows),
            run.IssueCount,
            run.RuleCount,
            run.FailureReason,
            run.CreatedAt,
            run.CompletedAt);
    }

    public static RuleIssueStatItem ToItem(this RuleIssueStat stat)
    {
        ArgumentNullException.ThrowIfNull(stat);
        return new RuleIssueStatItem(stat.RuleId, stat.ColumnName, stat.RuleType, stat.IssueCount);
    }

    public static ValidationIssueItem ToItem(this ValidationIssue issue) =>
        new(issue.RowNumber, issue.ColumnName, issue.RuleType.ToString(), issue.Message, issue.Value);

    /// <summary>Human-readable summary of a rule, shown in the rules table.</summary>
    public static string Describe(RuleType ruleType, RuleParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return ruleType switch
        {
            RuleType.Required => "Value is required",
            RuleType.MaxLength => string.Create(CultureInfo.InvariantCulture, $"At most {parameters.MaxLength} characters"),
            RuleType.Pattern => $"Matches /{parameters.Pattern}/",
            RuleType.Email => "Valid e-mail address",
            RuleType.Numeric => (parameters.Min, parameters.Max) switch
            {
                (null, null) => "Numeric value",
                (not null, null) => string.Create(CultureInfo.InvariantCulture, $"Number ≥ {parameters.Min}"),
                (null, not null) => string.Create(CultureInfo.InvariantCulture, $"Number ≤ {parameters.Max}"),
                _ => string.Create(CultureInfo.InvariantCulture, $"Number between {parameters.Min} and {parameters.Max}"),
            },
            RuleType.Date => $"Date in format {parameters.Format ?? "yyyy-MM-dd"}",
            RuleType.AllowedValues => $"One of: {string.Join(", ", parameters.AllowedValues ?? [])}",
            RuleType.Unique => "Unique within the dataset",
            _ => ruleType.ToString(),
        };
    }
}
