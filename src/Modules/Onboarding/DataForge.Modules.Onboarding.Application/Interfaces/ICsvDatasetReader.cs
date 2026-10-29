namespace DataForge.Modules.Onboarding.Application.Interfaces;

/// <summary>
/// Opens a delimited text file for streaming. The delimiter (comma, semicolon, tab or pipe) is detected
/// from the header row.
/// </summary>
public interface ICsvDatasetReader
{
    /// <exception cref="InvalidDataException">The content is not a readable delimited text file.</exception>
    ICsvDocument Open(Stream content);
}
