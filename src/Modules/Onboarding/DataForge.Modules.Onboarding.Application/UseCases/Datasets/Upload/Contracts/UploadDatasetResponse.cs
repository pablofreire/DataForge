using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Shared;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;

public sealed record UploadDatasetResponse(DatasetItem Dataset, bool IsDuplicate, Guid? ImportJobId);
