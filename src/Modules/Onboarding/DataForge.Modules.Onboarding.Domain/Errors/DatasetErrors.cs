using DataForge.BuildingBlocks.Domain.Results;

namespace DataForge.Modules.Onboarding.Domain.Errors;

public static class DatasetErrors
{
    public static readonly Error NotFound = Error.NotFound("dataset.not_found", "The dataset was not found.");

    public static readonly Error EmptyFile = Error.Validation("dataset.empty_file", "The uploaded file is empty.");

    public static Error FileTooLarge(long maxBytes) =>
        Error.Validation("dataset.file_too_large", $"The file exceeds the maximum upload size of {maxBytes / (1024 * 1024)} MB.");
}
