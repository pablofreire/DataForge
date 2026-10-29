namespace DataForge.Modules.Onboarding.Application.Jobs;

public static class OnboardingJobTypes
{
    public const string ImportDataset = "onboarding.dataset.import";

    public static string ImportIdempotencyKey(Guid datasetId) => $"{ImportDataset}:{datasetId:N}";
}
