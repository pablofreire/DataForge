using DataForge.BuildingBlocks.Application.Events.Interfaces;
using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.Modules.Onboarding.Contracts.IntegrationEvents;
using DataForge.Modules.Validation.Application.Services;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;

namespace DataForge.Modules.Validation.Application.IntegrationEventHandlers;

/// <summary>
/// Starts a validation run as soon as a dataset is imported, when its project has rules.
/// </summary>
public sealed class DatasetImportedIntegrationEventHandler(
    IValidationRuleRepository ruleRepository,
    ValidationRunScheduler runScheduler,
    ITenantContext tenantContext) : IIntegrationEventHandler<DatasetImportedIntegrationEvent>
{
    public async Task HandleAsync(DatasetImportedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var rules = await ruleRepository.ListByProjectAsync(tenantContext.TenantId, integrationEvent.ProjectId, cancellationToken);
        if (rules.Count == 0)
        {
            return;
        }

        await runScheduler.ScheduleAsync(integrationEvent.DatasetId, integrationEvent.ProjectId, cancellationToken);
    }
}
