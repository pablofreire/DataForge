using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Interfaces;

public interface IUploadDatasetUseCase : IUseCase<UploadDatasetRequest, UploadDatasetResponse>;
