namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;

/// <summary>
/// An upload request. <see cref="Content"/> is the raw request body; it is streamed to storage and never buffered in memory.
/// </summary>
public sealed record UploadDatasetRequest(Guid ProjectId, string FileName, Stream Content);
