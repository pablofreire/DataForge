using DataForge.BuildingBlocks.Domain.Results;

namespace DataForge.Modules.Validation.Domain.Errors;

public static class ValidationRunErrors
{
    public static readonly Error NotFound = Error.NotFound("validation_run.not_found", "No validation run was found for this dataset.");

    public static readonly Error DatasetNotFound = Error.NotFound("dataset.not_found", "The dataset was not found.");

    public static readonly Error DatasetNotImported = Error.Conflict("validation_run.dataset_not_imported", "The dataset must finish importing before it can be validated.");

    public static readonly Error NoRules = Error.Conflict("validation_run.no_rules", "The project has no validation rules. Add at least one rule first.");
}
