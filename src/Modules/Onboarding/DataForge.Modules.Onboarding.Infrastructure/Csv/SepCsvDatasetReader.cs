using DataForge.Modules.Onboarding.Application.Interfaces;
using nietras.SeparatedValues;

namespace DataForge.Modules.Onboarding.Infrastructure.Csv;

/// <summary>
/// <see cref="ICsvDatasetReader"/> backed by Sep, a SIMD-accelerated CSV parser (see ADR-0006).
/// The separator is auto-detected from the header, so comma, semicolon, tab and pipe files are all accepted.
/// </summary>
public sealed class SepCsvDatasetReader : ICsvDatasetReader
{
    public ICsvDocument Open(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            var reader = Sep.Reader(options => options with
            {
                HasHeader = true,
                Unescape = true,
                DisableColCountCheck = true,
            }).From(content);

            return new SepCsvDocument(reader);
        }
        catch (Exception exception) when (exception is not InvalidDataException and not OperationCanceledException)
        {
            throw new InvalidDataException("The file could not be read as delimited text.", exception);
        }
    }
}
