using System.Collections;
using nietras.SeparatedValues;

namespace DataForge.Modules.Onboarding.Infrastructure.Csv;

/// <summary>
/// Materializes each Sep row (a ref struct that cannot cross an iterator boundary) into a string array.
/// NUL characters are removed because PostgreSQL text values cannot contain them.
/// </summary>
internal sealed class SepRowEnumerable(SepReader reader) : IEnumerable<string[]>
{
    public IEnumerator<string[]> GetEnumerator() => new Enumerator(reader);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Enumerator(SepReader reader) : IEnumerator<string[]>
    {
        public string[] Current { get; private set; } = [];

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            bool hasRow;
            try
            {
                hasRow = reader.MoveNext();
            }
            catch (Exception exception) when (exception is not InvalidDataException and not OperationCanceledException)
            {
                throw new InvalidDataException("The file contains a malformed row.", exception);
            }

            if (!hasRow)
            {
                return false;
            }

            var row = reader.Current;
            var values = new string[row.ColCount];
            for (var index = 0; index < values.Length; index++)
            {
                var span = row[index].Span;
                values[index] = span.Contains('\0') ? span.ToString().Replace("\0", string.Empty, StringComparison.Ordinal) : span.ToString();
            }

            Current = values;
            return true;
        }

        public void Reset() => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }
}
