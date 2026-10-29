using DataForge.BuildingBlocks.Application.Storage.Interfaces;
using DataForge.BuildingBlocks.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DataForge.BuildingBlocks.Infrastructure.Storage;

/// <summary>
/// Local-disk <see cref="IFileStorage"/>. Paths are relative and validated so a caller can never
/// escape the configured root directory (path traversal).
/// </summary>
public sealed class LocalFileStorage(IOptions<FileStorageOptions> options) : IFileStorage
{
    private const int BufferSize = 81_920;

    private readonly string _root = Path.GetFullPath(options.Value.RootPath);

    public async Task WriteAsync(string path, Stream content, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        var fullPath = Resolve(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, useAsync: true);
        await content.CopyToAsync(target, BufferSize, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken)
    {
        var fullPath = Resolve(path);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        var fullPath = Resolve(path);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string Resolve(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var fullPath = Path.GetFullPath(Path.Combine(_root, relativePath));
        var rootWithSeparator = _root.EndsWith(Path.DirectorySeparatorChar) ? _root : _root + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.Ordinal))
        {
            throw new ArgumentException("The path resolves outside of the storage root.", nameof(relativePath));
        }

        return fullPath;
    }
}
