namespace DataForge.BuildingBlocks.Application.Storage.Interfaces;

/// <summary>
/// Binary object storage. The local-disk implementation is used in development and Docker;
/// an Azure Blob Storage implementation can be registered without touching callers.
/// </summary>
public interface IFileStorage
{
    Task WriteAsync(string path, Stream content, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken);

    Task DeleteAsync(string path, CancellationToken cancellationToken);
}
