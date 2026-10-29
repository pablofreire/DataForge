namespace DataForge.BuildingBlocks.Application.Tenancy.Interfaces;

/// <summary>
/// The tenant the current unit of work runs for. Resolved from the authenticated principal in the web host
/// and from the job record in the worker host.
/// </summary>
public interface ITenantContext
{
    bool HasTenant { get; }

    /// <summary>Internal tenant key used to filter queries. Never leaves the process.</summary>
    long TenantId { get; }

    Guid TenantPublicId { get; }
}
