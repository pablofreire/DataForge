using DataForge.Modules.Processing.Contracts.Dtos;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Contracts;

/// <summary>A job row in the implementation console, labelled with the owning tenant.</summary>
public sealed record OperationsJobItem(Guid TenantId, string TenantName, JobSummaryDto Job);
