using DataForge.BuildingBlocks.Domain.Results;

namespace DataForge.Modules.Validation.Domain.Errors;

public static class ValidationRuleErrors
{
    public static readonly Error NotFound = Error.NotFound("rule.not_found", "The validation rule was not found.");

    public static Error InvalidParameters(string message) => Error.Validation("rule.invalid_parameters", message);
}
