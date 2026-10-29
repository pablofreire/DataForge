namespace DataForge.BuildingBlocks.Web.Security;

public static class AntiforgeryDefaults
{
    /// <summary>Header carrying the antiforgery request token on API calls made from the browser.</summary>
    public const string HeaderName = "X-CSRF-TOKEN";
}
