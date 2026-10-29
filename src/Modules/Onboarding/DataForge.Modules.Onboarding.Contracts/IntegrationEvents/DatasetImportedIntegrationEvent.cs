using DataForge.BuildingBlocks.Application.Events.Interfaces;

namespace DataForge.Modules.Onboarding.Contracts.IntegrationEvents;

/// <summary>
/// Published when all rows of a dataset are staged. The Validation module reacts by starting a validation run.
/// </summary>
public sealed record DatasetImportedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid DatasetId,
    Guid ProjectId,
    long RowCount) : IIntegrationEvent;
