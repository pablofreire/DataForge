namespace DataForge.Modules.Validation.Application.Jobs;

public static class ValidationJobTypes
{
    public const string ValidateDataset = "validation.dataset.validate";

    public static string ValidateIdempotencyKey(Guid runId) => $"{ValidateDataset}:{runId:N}";
}
