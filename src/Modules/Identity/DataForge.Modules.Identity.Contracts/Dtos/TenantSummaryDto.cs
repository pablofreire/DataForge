namespace DataForge.Modules.Identity.Contracts.Dtos;

public sealed record TenantSummaryDto(Guid TenantId, string Name, string Slug);
