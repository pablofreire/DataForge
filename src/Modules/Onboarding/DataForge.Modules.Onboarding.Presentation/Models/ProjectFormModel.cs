namespace DataForge.Modules.Onboarding.Presentation.Models;

/// <summary>Form state only; validation runs in the use case so the API and the UI share the same rules.</summary>
public sealed class ProjectFormModel
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
