using DataForge.Modules.Processing.Contracts.Dtos;
using DataForge.Modules.Processing.Domain.Entities;

namespace DataForge.Modules.Processing.Application.Mappings;

public static class JobMappingExtensions
{
    public static JobSummaryDto ToSummaryDto(this JobEntity job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return new JobSummaryDto(
            job.PublicId,
            job.JobType,
            job.ResourceId,
            job.Status.ToString(),
            job.Attempt,
            job.MaxAttempts,
            job.ProcessedItems,
            job.TotalItems,
            job.LastError,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt);
    }
}
