namespace DataForge.BuildingBlocks.Web.Security;

public static class AuthorizationPolicies
{
    /// <summary>Any authenticated user bound to a tenant (customer portal).</summary>
    public const string TenantUser = "TenantUser";

    /// <summary>Internal implementation team (implementation console, cross-tenant operations).</summary>
    public const string ImplementationEngineer = "ImplementationEngineer";
}
