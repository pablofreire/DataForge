using DataForge.BuildingBlocks.Domain.Results;

namespace DataForge.Modules.Identity.Domain.Errors;

public static class UserErrors
{
    /// <summary>Deliberately generic: never reveals whether the e-mail exists.</summary>
    public static readonly Error InvalidCredentials = Error.Unauthorized("auth.invalid_credentials", "Invalid e-mail or password.");
}
