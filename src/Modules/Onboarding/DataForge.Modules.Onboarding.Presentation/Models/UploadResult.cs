namespace DataForge.Modules.Onboarding.Presentation.Models;

/// <summary>Raw outcome of the browser upload, as returned by datasetUpload.js.</summary>
public sealed record UploadResult(bool Ok, int Status, string? Body);
