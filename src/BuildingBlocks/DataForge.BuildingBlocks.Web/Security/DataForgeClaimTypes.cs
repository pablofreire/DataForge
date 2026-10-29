namespace DataForge.BuildingBlocks.Web.Security;

/// <summary>
/// Claim types issued in the authentication cookie. The cookie is encrypted and signed with ASP.NET Core
/// Data Protection, so the internal tenant key never becomes visible to the client.
/// </summary>
public static class DataForgeClaimTypes
{
    public const string UserPublicId = "df:user";
    public const string TenantId = "df:tenant_id";
    public const string TenantPublicId = "df:tenant";
    public const string TenantName = "df:tenant_name";
}
