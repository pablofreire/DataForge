using DataForge.Modules.Onboarding.Application.Interfaces;
using nietras.SeparatedValues;

namespace DataForge.Modules.Onboarding.Infrastructure.Csv;

internal sealed class SepCsvDocument(SepReader reader) : ICsvDocument
{
    private bool _enumerated;

    public IReadOnlyList<string> Columns { get; } = reader.Header.ColNames.Select(name => name.Trim()).ToArray();

    public IEnumerable<string[]> ReadRows()
    {
        if (_enumerated)
        {
            throw new InvalidOperationException("A CSV document can only be enumerated once.");
        }

        _enumerated = true;
        return new SepRowEnumerable(reader);
    }

    public void Dispose() => reader.Dispose();
}
