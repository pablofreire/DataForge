using DataForge.Modules.Processing.Contracts.Dtos;
using DataForge.Modules.Validation.Application.UseCases.Runs.Shared;

namespace DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Contracts;

/// <summary>The latest run of a dataset; <see cref="Run"/> is null when the dataset was never validated.</summary>
public sealed record GetLatestValidationRunResponse(ValidationRunItem? Run, IReadOnlyList<RuleIssueStatItem> RuleStats, JobSummaryDto? Job);
