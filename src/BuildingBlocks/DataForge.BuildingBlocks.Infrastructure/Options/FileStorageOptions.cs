namespace DataForge.BuildingBlocks.Infrastructure.Options;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Root directory for the local-disk provider. Shared by the web and worker hosts (Docker volume).</summary>
    public string RootPath { get; set; } = "data/files";
}
