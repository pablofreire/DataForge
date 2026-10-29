using DataForge.Modules.Processing.Contracts.Dtos;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Contracts;

public sealed record ListRecentJobsResponse(IReadOnlyList<JobSummaryDto> Jobs);
