using DataForge.BuildingBlocks.Application.Storage.Exceptions;
using DataForge.BuildingBlocks.Application.Storage.Interfaces;
using DataForge.BuildingBlocks.Application.Storage.Streams;
using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Application.Jobs;
using DataForge.Modules.Onboarding.Application.Mappings;
using DataForge.Modules.Onboarding.Application.Options;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Interfaces;
using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.Errors;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using DataForge.Modules.Processing.Contracts.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Implementations;

/// <summary>
/// Streams an upload to storage in a single pass (hashing and size-limiting on the fly), registers the
/// dataset and enqueues its import. Uploading identical content to the same project is idempotent and
/// returns the existing dataset.
/// </summary>
public sealed class UploadDatasetUseCase(
    IProjectRepository projectRepository,
    IDatasetRepository datasetRepository,
    IFileStorage fileStorage,
    IJobScheduler jobScheduler,
    ITenantContext tenantContext,
    IValidator<UploadDatasetRequest> validator,
    IOptions<OnboardingOptions> options,
    TimeProvider timeProvider) : IUploadDatasetUseCase
{
    public async Task<Result<UploadDatasetResponse>> ExecuteAsync(UploadDatasetRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var project = await projectRepository.GetByPublicIdAsync(tenantContext.TenantId, request.ProjectId, cancellationToken);
        if (project is null)
        {
            return ProjectErrors.NotFound;
        }

        var datasetId = Guid.CreateVersion7(timeProvider.GetUtcNow());
        var storagePath = $"tenants/{tenantContext.TenantPublicId:N}/datasets/{datasetId:N}.csv";
        var maxBytes = options.Value.MaxUploadBytes;

        string contentHash;
        long sizeBytes;
        await using (var hashing = new HashingReadStream(request.Content, maxBytes))
        {
            try
            {
                await fileStorage.WriteAsync(storagePath, hashing, cancellationToken);
            }
            catch (StreamLengthLimitExceededException)
            {
                await fileStorage.DeleteAsync(storagePath, CancellationToken.None);
                return DatasetErrors.FileTooLarge(maxBytes);
            }

            contentHash = hashing.GetHashHex();
            sizeBytes = hashing.BytesRead;
        }

        if (sizeBytes == 0)
        {
            await fileStorage.DeleteAsync(storagePath, CancellationToken.None);
            return DatasetErrors.EmptyFile;
        }

        var existing = await datasetRepository.GetByContentHashAsync(tenantContext.TenantId, project.Id, contentHash, cancellationToken);
        if (existing is not null)
        {
            return await ReturnExistingAsync(existing, storagePath, cancellationToken);
        }

        var dataset = DatasetEntity.Create(
            tenantContext.TenantId,
            project.Id,
            project.PublicId,
            datasetId,
            Path.GetFileName(request.FileName),
            contentHash,
            sizeBytes,
            storagePath,
            timeProvider.GetUtcNow());

        if (!await datasetRepository.AddAsync(dataset, cancellationToken))
        {
            // A concurrent upload of the same content won the race.
            existing = await datasetRepository.GetByContentHashAsync(tenantContext.TenantId, project.Id, contentHash, cancellationToken);
            return await ReturnExistingAsync(existing!, storagePath, cancellationToken);
        }

        var job = await jobScheduler.EnqueueAsync(OnboardingJobTypes.ImportDataset, dataset.PublicId, OnboardingJobTypes.ImportIdempotencyKey(dataset.PublicId), cancellationToken);

        return new UploadDatasetResponse(dataset.ToItem(), IsDuplicate: false, job.JobId);
    }

    private async Task<UploadDatasetResponse> ReturnExistingAsync(DatasetEntity existing, string discardedStoragePath, CancellationToken cancellationToken)
    {
        await fileStorage.DeleteAsync(discardedStoragePath, CancellationToken.None);
        var job = await jobScheduler.EnqueueAsync(
            OnboardingJobTypes.ImportDataset,
            existing.PublicId,
            OnboardingJobTypes.ImportIdempotencyKey(existing.PublicId),
            cancellationToken);
        return new UploadDatasetResponse(existing.ToItem(), IsDuplicate: true, job.JobId);
    }
}
