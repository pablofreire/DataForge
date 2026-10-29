using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Shared;
using DataForge.Modules.Processing.Contracts.Dtos;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Contracts;

public sealed record GetDatasetByIdResponse(DatasetItem Dataset, JobSummaryDto? ImportJob);
