namespace DataForge.Modules.Identity.Presentation.Controllers.Contracts;

/// <summary>The signed-in user as exposed by the API (public identifiers only).</summary>
public sealed record CurrentUserResponse(Guid UserId, string Email, string DisplayName, string Role, Guid TenantId, string TenantName);
