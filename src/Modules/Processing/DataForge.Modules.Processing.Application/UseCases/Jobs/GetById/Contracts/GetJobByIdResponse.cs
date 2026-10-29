using DataForge.Modules.Processing.Contracts.Dtos;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Contracts;

public sealed record GetJobByIdResponse(JobSummaryDto Job);
